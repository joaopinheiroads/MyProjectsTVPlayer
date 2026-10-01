using NLog;
using SerializableObjects.PluginsTVPlayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TVPlayerAPI.Helpers;
using TVPlayerAPI.Interfaces;
using TVPlayerAPI.Domain.Schemas;
using TVPlayerAPI.Infrastructure.Factory;
using TVPlayerAPI.RSS.Classes;

namespace TVPlayerAPI.RSS.Client
{
    internal class RSSClient : IPluginRSS
    {
        private static readonly Dictionary<string, string> FONTES_COM_LOG_PROPRIO = new Dictionary<string, string>
        {
            { "tecmundo.com.br", "Tecmundo" },
            { "gazetadopovo.com.br", "GazetaDoPovo" },
            { "globo.com", "G1" }
        };

        private const int LIMITE_DOWNLOAD_IMAGEM_SEG = 20;
        private const int MAX_IMAGENS_QUE_NAO_BAIXAM_POR_RODADA = 2;

        private readonly Logger logger;

        private string channelTitle;
        private string url;
        private string encoding;
        private string pathToSave;
        private RSSType type;

        public RSSClient(string channelTitle, string url, string encoding, string pathToSave, RSSType type)
        {
            this.channelTitle = channelTitle;
            this.url = url;
            this.encoding = encoding;
            this.pathToSave = pathToSave;
            this.type = type;
            logger = LoggerDaFonte(url);
        }

        private static Logger LoggerDaFonte(string url)
        {
            string fonte = FonteComLogProprio(url);

            return fonte == null
                ? LogManager.GetLogger(typeof(RSSClient).FullName)
                : LogManager.GetLogger("TVPlayerAPI." + fonte + ".RSS");
        }

        private static string FonteComLogProprio(string url)
        {
            if (!Uri.TryCreate(url?.Trim(), UriKind.Absolute, out Uri uri))
                return null;

            string host = uri.Host.ToLowerInvariant();

            return FONTES_COM_LOG_PROPRIO
                .Where(fonte => host == fonte.Key || host.EndsWith("." + fonte.Key))
                .Select(fonte => fonte.Value)
                .FirstOrDefault();
        }

        private string Rotulo => channelTitle + " [" + Path.GetFileName(pathToSave) + "]";

        private static EnclosureData BaixarImagemComLimite(string url, string caminho)
        {
            using (var limite = new CancellationTokenSource(TimeSpan.FromSeconds(LIMITE_DOWNLOAD_IMAGEM_SEG)))
            {
                return WebHelper.DownloadMediaAsync(url, caminho, true, limite.Token).Result;
            }
        }

        private List<NewsData> MergeRSSContents(RSSData atualContents, RSSData atualRSS, RSSData newRSS)
        {
            RSSData mergedRSS;

            int entradaNovos = newRSS != null && newRSS.RSSChannel != null && newRSS.RSSChannel.RSSItem != null
                ? newRSS.RSSChannel.RSSItem.Count : 0;
            int entradaContents = atualContents != null && atualContents.RSSChannel != null && atualContents.RSSChannel.RSSItem != null
                ? atualContents.RSSChannel.RSSItem.Count : 0;

            if (atualRSS != null && atualRSS.RSSChannel != null && atualRSS.RSSChannel.RSSItem != null && atualRSS.RSSChannel.RSSItem.Count > 0)
            {
                foreach (var atualContent in atualRSS.RSSChannel.RSSItem)
                {
                    bool hasItem = false;
                    foreach (var content in newRSS.RSSChannel.RSSItem)
                    {
                        if (MesmoItem(content, atualContent))
                        {
                            hasItem = true;
                            break;
                        }
                    }
                    if (hasItem == false)
                        newRSS.RSSChannel.RSSItem.Add(atualContent);
                }
            }

            mergedRSS = newRSS;

            if (newRSS.RSSChannel.RSSItem.Count >= 15)
                mergedRSS.RSSChannel.RSSItem = newRSS.RSSChannel.RSSItem.Take(15).ToList();

            if (atualContents == null)
            {
                atualContents = new RSSData();
                atualContents.RSSChannel.Title = channelTitle;
            }

            mergedRSS.RSSChannel.RSSItem.Reverse();

            foreach (var newContent in mergedRSS.RSSChannel.RSSItem)
            {
                try
                {
                    bool hasItem = false;
                    foreach (var atualContent in atualContents.RSSChannel.RSSItem)
                    {
                        if (MesmoItem(atualContent, newContent))
                        {
                            hasItem = true;
                            break;
                        }
                    }
                    if (hasItem == false)
                        atualContents.RSSChannel.RSSItem.Insert(0, newContent);
                }
                catch { }
            }

            List<NewsData> semRepetidos = new List<NewsData>();

            foreach (var item in atualContents.RSSChannel.RSSItem)
            {
                if (!semRepetidos.Any(x => MesmoItem(x, item)))
                    semRepetidos.Add(item);
            }

            List<NewsData> resultado = semRepetidos.Take(15).ToList();

            int descartadosPorRepeticao = atualContents.RSSChannel.RSSItem.Count - semRepetidos.Count;

            logger.Debug(Rotulo + ": merge -> coletados " + entradaNovos
                + ", contents anterior " + entradaContents
                + ", repetidos descartados " + descartadosPorRepeticao
                + ", gravados " + resultado.Count);

            if (descartadosPorRepeticao > 0)
                logger.Info(Rotulo + ": merge descartou " + descartadosPorRepeticao + " item(ns) repetido(s).");

            return resultado;
        }

        private RssRecoverType SaveOnDisk(string rssContent)
        {
            List<string> savedFiles = new List<string>();

            IOHelper.CheckDirectory(pathToSave);
            string xmlData = Path.Combine(pathToSave, "data.xml");
            string jsonData = Path.Combine(pathToSave, "data.json");
            string xmlContents = Path.Combine(pathToSave, "contents.xml");

            string xmlFile = Path.Combine(pathToSave, "rss.xml");
            string xmlFileNew = Path.Combine(pathToSave, "rssNew.xml");
            string xmlFileOld = Path.Combine(pathToSave, "rssOld.xml");

            savedFiles.Add(jsonData);
            savedFiles.Add(xmlData);
            savedFiles.Add(xmlContents);
            savedFiles.Add(xmlFile);
            savedFiles.Add(xmlFileNew);
            savedFiles.Add(xmlFileOld);

            RSSLoteriasData rssLoteria = null;
            RSSData rss = null;
            int descartadasPorImagem = 0;

            if (type == RSSType.Loterias)
            {
                rssLoteria = RSSSearch.FeedRecover<RSSLoteriasData>(rssContent);
                IOHelper.SerializeXmlToFile(xmlFileNew, rssLoteria);
            }
            else
            {
                rss = RSSSearch.FeedRecover(rssContent, (type == RSSType.RSSCustom) || (type == RSSType.RSSUser), channelTitle, type != RSSType.UOLVideos);
                IOHelper.SerializeXmlToFile(xmlFileNew, rss);
            }

            if (rss != null && rss.RSSChannel.RSSItem.Count == 0)
            {
                logger.Warn(Rotulo + ": o feed veio sem itens; a pasta mantem o conteudo anterior.");
                File.Delete(xmlFileNew);
                return RssRecoverType.SemConteudo;
            }

            switch (IOHelper.CompareMD5_FileFile(xmlFile, xmlFileNew))
            {
                case true:
                    logger.Info(Rotulo + ": sem novidade desde a ultima coleta.");
                    File.Delete(xmlFileNew);
                    return RssRecoverType.SucessoSemModificacao;

                case false://xml diferente ou o 1º arquivo nao existe

                    if (type == RSSType.Loterias)
                    {
                    }
                    else if (type == RSSType.UOLHoroscopo)
                    { }
                    else
                    {
                        rss.RSSChannel.RSSItem = MergeRSSContents(IOHelper.DeserializeXmlFromFile<RSSData>(xmlContents), IOHelper.DeserializeXmlFromFile<RSSData>(xmlFileOld), rss);

                        List<NewsData> listToRemove = new List<NewsData>();
                        int imagensQueNaoBaixaram = 0;
                        try
                        {
                            foreach (var x in rss.RSSChannel.RSSItem)
                            {
                                string pathImage = string.Empty;
                                string pathImageTemp = string.Empty;

                                if (!string.IsNullOrEmpty(x.ImageLink))
                                {
                                    if (string.IsNullOrEmpty(x.Enclosure.Url))
                                    {
                                        if (imagensQueNaoBaixaram >= MAX_IMAGENS_QUE_NAO_BAIXAM_POR_RODADA)
                                        {
                                            listToRemove.Add(x);
                                            continue;
                                        }

                                        pathImageTemp = Path.Combine(pathToSave, RSSHelper.ReturnGeneratedFilename());
                                        pathImage = RSSHelper.ReturnGeneratedFilename();

                                        const int NumberOfRetries = 3;
                                        TimeSpan DelayOnRetry = new TimeSpan(2000);
                                        EnclosureData imgTemp = null;

                                        for (int j = 0; j < NumberOfRetries; j++)
                                        {                                            
                                            imgTemp = BaixarImagemComLimite(x.ImageLink, pathImageTemp);

                                            if (imgTemp == null)
                                            {
                                                Task.Delay(DelayOnRetry);
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }

                                        if(imgTemp == null)
                                        {
                                            imagensQueNaoBaixaram++;
                                            logger.Warn(Rotulo + ": noticia '" + x.Title + "' descartada, a imagem " + x.ImageLink + " nao baixou.");

                                            if (imagensQueNaoBaixaram == MAX_IMAGENS_QUE_NAO_BAIXAM_POR_RODADA)
                                                logger.Warn(Rotulo + ": " + imagensQueNaoBaixaram + " imagens nao baixaram; as demais noticias novas desta rodada ficam de fora sem tentar.");

                                            listToRemove.Add(x);
                                            continue;
                                        }
                                        
                                        string tempFile = string.IsNullOrEmpty(imgTemp?.Url) ? string.Empty : Path.Combine(pathToSave, imgTemp.Url);

                                        if (imgTemp?.TypeFormat == "image" && !ImagemLegivel(tempFile))
                                        {
                                            logger.Warn(Rotulo + ": noticia '" + x.Title + "' descartada, a imagem " + x.ImageLink + " nao abre como imagem.");
                                            listToRemove.Add(x);
                                            continue;
                                        }

                                        if (imgTemp?.TypeFormat == "image")
                                        {
                                            var fileExtension = new FileInfo(tempFile)?.Extension;
                                            switch (fileExtension)
                                            {
                                                case ".webp":
                                                    using (WebP webp = new WebP())
                                                    {
                                                        Bitmap bmp = webp.Load(tempFile);
                                                        TypeConverter converter = TypeDescriptor.GetConverter(bmp);
                                                        byte[] data = (byte[])converter.ConvertTo(null, CultureInfo.InvariantCulture, bmp, typeof(byte[]));

                                                        using (var ms = new MemoryStream(data))
                                                        {
                                                             SaveResizedImage(bmp, Path.Combine(pathToSave, pathImage + ".jpg"), 1920, 1080);
                                                        }
                                                    }
                                                    break;
                                                default:
                                                    using (Image original = Image.FromFile(tempFile))
                                                    {
                                                        try
                                                        {
                                                            Bitmap jpg = new Bitmap(original);
                                                            TypeConverter converter = TypeDescriptor.GetConverter(jpg);
                                                            byte[] data = (byte[])converter.ConvertTo(null, CultureInfo.InvariantCulture, jpg, typeof(byte[]));

                                                            using (var ms = new MemoryStream(data))
                                                            {
                                                                SaveResizedImage(jpg, Path.Combine(pathToSave, pathImage + ".jpg"), 1920, 1080);
                                                            }
                                                        }
                                                        catch (System.Exception ex)
                                                        {
                                                            string teste = ex.InnerException.ToString();
                                                        }
                                                    }
                                                    break;
                                            }
                                        
                                            imgTemp.Url = pathImage + ".jpg";
                                        }
                                        x.Enclosure = imgTemp;
                                        x.ImageLink = string.Empty;
                                        x.Description = string.IsNullOrEmpty(x.Description) ? string.Empty : RSSHelper.FormatDescription(x.Description);
                                        pathImage = string.IsNullOrEmpty(x.Enclosure?.Url) ? string.Empty : Path.Combine(pathToSave, x.Enclosure.Url);
                                    }
                                    else
                                    {
                                        string fileToVerify = Path.Combine(pathToSave, x.Enclosure?.Url);
                                        if (!File.Exists(fileToVerify))
                                        {
                                            if (!string.IsNullOrEmpty(WebHelper.DownloadImageAsync(x.ImageLink, fileToVerify, true).Result))
                                                pathImage = fileToVerify;
                                        }
                                        else
                                            pathImage = fileToVerify;
                                    }
                                }
                                else if (x.Enclosure != null && !string.IsNullOrEmpty(x.Enclosure.Url))
                                {
                                    pathImage = Path.Combine(pathToSave, x.Enclosure.Url);
                                }

                                if (!string.IsNullOrEmpty(pathImage))
                                {
                                    if (File.Exists(pathImage))
                                        savedFiles.Add(pathImage);
                                    else
                                    {
                                        logger.Warn(Rotulo + ": noticia '" + x.Title + "' descartada, o arquivo da imagem nao existe.");
                                        listToRemove.Add(x);
                                    }
                                }
                            }
                            foreach (var itemToRemove in listToRemove)
                            {
                                if (rss.RSSChannel.RSSItem.Contains(itemToRemove))
                                    rss.RSSChannel.RSSItem.Remove(itemToRemove);
                            }
                        }
                        catch (Exception err)
                        {
                            logger.Error(err, Rotulo + ": falha ao preparar as imagens; a pasta mantem o conteudo anterior.");
                            return RssRecoverType.ErroAoSalvar;
                        }
                        descartadasPorImagem = listToRemove.Count;
                        listToRemove = null;

                        if (rss.RSSChannel.RSSItem.Count == 0)
                        {
                            logger.Warn(Rotulo + ": todas as noticias foram descartadas; a pasta mantem o conteudo anterior.");
                            File.Delete(xmlFileNew);
                            return RssRecoverType.SemConteudo;
                        }
                    }

                    bool isOK = false;
                    do
                    {
                        try
                        {
                            IOHelper.DeleteFilesFromPath(pathToSave, savedFiles);

                            if (type == RSSType.Loterias)
                            {
                                IOHelper.SerializeXmlToFile(xmlContents, rssLoteria);
                                IOHelper.SerializeJsonToFile(Path.Combine(pathToSave, "contents.json"), rssLoteria.RSSChannel, true);
                                RSSRecover.ReturnToOldXDocument(new string[] { xmlData }, IOHelper.SerializeXmlToString(rssLoteria), type);
                            }
                            else
                            {
                                IOHelper.SerializeXmlToFile(xmlContents, rss);
                                IOHelper.SerializeJsonToFile(Path.Combine(pathToSave, "contents.json"), rss.RSSChannel, true);
                                RSSRecover.ReturnToOldXDocument(new string[] { xmlData }, IOHelper.SerializeXmlToString(rss), type);
                            }
                            isOK = true;
                        }
                        catch(Exception ex)
                        { isOK = false; }
                    } while (isOK == false);

                    if (File.Exists(xmlFile))
                        File.Replace(xmlFileNew, xmlFile, xmlFileOld);
                    else
                    {
                        File.Move(xmlFileNew, xmlFile);
                        File.Copy(xmlFile, xmlFileOld);
                    }

                    if (rss != null)
                        logger.Info(Rotulo + ": gravado com " + rss.RSSChannel.RSSItem.Count + " noticia(s); " + descartadasPorImagem + " descartada(s) por imagem.");
                    else
                        logger.Info(Rotulo + ": gravado.");

                    return RssRecoverType.Sucesso;

                case null://2º arquivo nao existe
                    return RssRecoverType.ErroAoSalvar;

                default:
                    return RssRecoverType.ErroAoSalvar;
            }
        }

        private static bool ImagemLegivel(string arquivo)
        {
            try
            {
                if (Path.GetExtension(arquivo) == ".webp")
                {
                    using (WebP webp = new WebP())
                    using (Bitmap imagem = webp.Load(arquivo))
                        return true;
                }

                using (Image imagem = Image.FromFile(arquivo))
                    return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
