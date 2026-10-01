using DevExpress.Web;
using PlayerVideo.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TVPlayerAPI.SocialPlugins.Client.Classes;
using TVPlayerAPI.SocialPlugins.Client.Interfaces;
using TVPlayerAPI;
using PlayerVideo.EntityDataModel;
using TVPlayerAPI.SocialPlugin.Client.Classes;

namespace PlayerVideo.Pages.Plugin
{
    public partial class FrmPluginSocialPlugin : System.Web.UI.Page
    {
        private string folderFinal = ConfigurationManager.AppSettings["PluginSocialPluginCollection"];
        private string folderPreview = "SocialPluginPreview";
        private string pluginInstagramCommand = ConfigurationManager.AppSettings["PluginInstagramCommand"];
        //private string folderFinalFacebook = ConfigurationManager.AppSettings["PluginFacebookFanpageCollection"];
        private string folderInstagramDownload = Path.Combine(@"D:\", "InstagramWebScraping");

        private string ERROR_MESSAGE = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int pluginID;

                if (int.TryParse(Request.QueryString["pluginID"], out pluginID))
                {
                    LoadContents(pluginID);
                }
                else
                {
                    spinQtdPosts.Value = 1;
                    cbxModoImagem.SelectedIndex = 0;
                }
            }
        }

        private void LoadContents(int pluginID)
        {
            using (VideoEntities dbContext = new VideoEntities())
            {
                SocialPlugin sp = dbContext.SocialPlugin.Where(s => s.ID == pluginID && s.Ativo == true).FirstOrDefault();
                cbxTipoPlugin.Value = sp.TipoMidia.HasValue ? sp.TipoMidia : null;
                txtNomePlugin.Text = sp.Nome;
                SearchParameters conf = JsonConvert.DeserializeObject<SearchParameters>(sp.Parametros);

                txtDuracao.Text = conf.TmpTransicao.ToString();
                txtProfile.Text = conf.Query;
                spinQtdPosts.Text = conf.QtdBusca.ToString();
                cbxModoImagem.Value = conf.TipoImagem;
            }
        }

        protected void callCallbackPreview_Callback(object source, CallbackEventArgs e)
        {
            if (e.Parameter == "preview")
            {
                string caminhoHTML = DoPreview();
                e.Result = caminhoHTML;
            }
        }

        protected void callCallbackSave_Callback(object source, CallbackEventArgs e)
        {
            if (e.Parameter == "save")
                Save(e);
        }

        private void Save(CallbackEventArgs e)
        {
            using (VideoEntities dbContext = new VideoEntities())
            {
                SocialPlugin oPlugin;
                int pluginID;
                string tipoPlugin;
                string htmlMaster, htmlMaster2;
                string pathPreview;
                SearchParameters parameters;

                FillForSearch(out tipoPlugin, out htmlMaster, out pathPreview, out parameters, out htmlMaster2);

                if (int.TryParse(Request.QueryString["pluginID"], out pluginID))
                    oPlugin = dbContext.SocialPlugin.Where(f => f.ID == pluginID && f.Ativo == true).FirstOrDefault();
                else
                {
                    oPlugin = new SocialPlugin();
                    oPlugin.DataCadastro = DateTime.Now;
                    oPlugin.UsuarioIDCadastro = GetUserInfo.ReturnID();
                    oPlugin.Ativo = true;
                }

                oPlugin.Nome = txtNomePlugin.Text;
                oPlugin.Parametros = JsonConvert.SerializeObject(parameters);
                oPlugin.UsuarioIDEdicao = GetUserInfo.ReturnID();
                oPlugin.TipoMidia = cbxTipoPlugin.Text.Contains("Facebook") ? 17 : 19;
                if (oPlugin.TipoMidia == 17)
                {
                    oPlugin.FaceCompatibilidadeXML = parameters.UserID.ToString() + "\\" + parameters.Query + ".xml";
                    oPlugin.FaceCompatibilidadeNome = parameters.Query;
                }

                if (dbContext.Entry(oPlugin).State == System.Data.Entity.EntityState.Detached)
                    dbContext.SocialPlugin.Add(oPlugin);

                try
                {
                    //if (oPlugin.TipoMidia  == 19 || RunSearch(tipoPlugin, parameters, htmlMaster, pathPreview, htmlMaster2))
                    if (RunSearch(tipoPlugin, parameters, htmlMaster, pathPreview, htmlMaster2))
                    {
                        dbContext.SaveChanges();
                        e.Result = "save_ok";
                        try
                        {
                            MoveFiles(parameters.Query);
                        }
                        catch { }
                    }
                    else
                        e.Result = "Erro ao buscar dados";
                }
                catch(Exception ex)
                {
                    e.Result = "Erro ao buscar dados";
                }
            }
        }

        private string DoPreview()
        {
            string tipoPlugin;
            string htmlMaster, htmlMaster2;
            string pathPreview;
            SearchParameters parameters;

            FillForSearch(out tipoPlugin, out htmlMaster, out pathPreview, out parameters, out htmlMaster2);
            try
            {
                Directory.Delete(Path.Combine(pathPreview, GetUserInfo.ReturnID().ToString()), true);
            }
            catch { }

            if (RunSearch(tipoPlugin, parameters, htmlMaster, pathPreview, htmlMaster2))
                return string.Format("{0}/{1}/{2}/{3}", Path.Combine(folderPreview, tipoPlugin), GetUserInfo.ReturnID().ToString(), parameters.Query, parameters.Query + ".html").Replace("\\", "/");
            else
                return "preview_error";
        }

        private void FillForSearch(out string tipoPlugin, out string htmlMaster, out string pathPreview, out SearchParameters parameters, out string htmlMaster2)
        {
            tipoPlugin = cbxTipoPlugin.Text.Contains("Facebook") ? "Facebook" : "Instagram";
            pathPreview = htmlMaster2 = htmlMaster = string.Empty;

            if (tipoPlugin.Contains("Facebook"))
            {
                htmlMaster = Path.Combine(MapPath(folderPreview), tipoPlugin, tipoPlugin.ToLower() + "_plugin_nova_versao.html");
                htmlMaster2 = Path.Combine(MapPath(folderPreview), tipoPlugin, tipoPlugin.ToLower() + "_plugin_old.html");
            }
            else
            {
                htmlMaster = Path.Combine(MapPath(folderPreview), tipoPlugin, tipoPlugin.ToLower() + "_plugin.html");
            }

            pathPreview = MapPath(Path.Combine(folderPreview, tipoPlugin));

            parameters = new SearchParameters()
            {
                Query = txtProfile.Text.Replace("http://www.facebook.com/", string.Empty)
                .Replace("https://www.facebook.com/", string.Empty)
                .Replace("https://www.instagram.com/", string.Empty)
                .TrimEnd('/').Trim('@'),
                TmpTransicao = Convert.ToInt32(txtDuracao.Text),
                QtdBusca = Convert.ToInt32(spinQtdPosts.Value),
                TipoImagem = cbxModoImagem.Value.ToString() == "contain" ? SearchParameters.ImageType.Contain : SearchParameters.ImageType.Cover,
                UserID = GetUserInfo.ReturnID()
            };
        }

        private bool RunSearch(string tipoPlugin, SearchParameters param, string pathHtml, string pathPreview, string htmlMaster2)
        {
            try
            {
                TVPlayerAPIClient oPlugin = new TVPlayerAPIClient();
                oPlugin.InitializeSocialClients(pathHtml, pathPreview, new List<SearchParameters>() { param }, htmlMaster2, 3000, false);
                bool ok = false;
                if (tipoPlugin.Contains("Instagram"))
                {
                    InstaSearchParameters isp = new InstaSearchParameters();
                    isp.Nome = txtNomePlugin.Text;
                    isp.Query = param.Query;
                    isp.QtdBusca = param.QtdBusca;
                    isp.TmpTransicao = param.TmpTransicao;
                    isp.TipoImagem = param.TipoImagem;
                    isp.UserID = param.UserID;
                    
                    oPlugin.InitializeInstagramSmartProxy(pathHtml, pathPreview, new List<InstaSearchParameters>() { isp }, 20000, false, false, folderInstagramDownload);
                    ok = oPlugin.RunInstagramSync();
                }
                if (tipoPlugin.Contains("Facebook"))
                    ok = oPlugin.RunFacebookSync();
                return ok;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void MoveFiles(string query)
        {
            string tipoPlugin = cbxTipoPlugin.Text.Contains("Facebook") ? "FacebookFanpage" : "Instagram";
            string userDestinationFinal = Path.Combine(string.Format(folderFinal, tipoPlugin), GetUserInfo.ReturnID().ToString());

            string queryDestinationFinal = Path.Combine(userDestinationFinal, query);
            string userSourcePreview = Path.Combine(MapPath(Path.Combine(folderPreview, tipoPlugin.Replace("Fanpage", string.Empty))), GetUserInfo.ReturnID().ToString());
            string userQuerySourcePreview = Path.Combine(userSourcePreview, query);
            string previewXmlFile = Path.Combine(userSourcePreview, query + ".xml");
            string finalXmlFile = Path.Combine(userDestinationFinal, query + ".xml");
            string previewJsonFile = Path.Combine(userSourcePreview, query + ".json");
            string finalJsonFile = Path.Combine(userDestinationFinal, query + ".json");

            if (!Directory.Exists(userDestinationFinal))
                Directory.CreateDirectory(userDestinationFinal);
            if (!Directory.Exists(queryDestinationFinal))
                Directory.CreateDirectory(queryDestinationFinal);

            //Cria todos os diretórios
            foreach (string dirPath in Directory.GetDirectories(userQuerySourcePreview, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(userQuerySourcePreview, queryDestinationFinal));

            //Copia todos os arquivos, substituindo os com mesmo nome
            foreach (string newPath in Directory.GetFiles(userQuerySourcePreview, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(userQuerySourcePreview, queryDestinationFinal), true);

            //copia o xml/json
            File.Copy(previewJsonFile, finalJsonFile, true);
            File.Copy(previewXmlFile, finalXmlFile, true);
        }
    }
}
