using SerializableObjects.PluginsTVPlayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using TVPlayerAPI.Interfaces;
using TVPlayerAPI.RSS.Classes;

namespace TVPlayerAPI.RSSCustom
{
    public class RSSCustomClient : IPlugin
    {
        public List<string> urlList { get; }
        public RSSCustomClient(List<string> urls)
        {
            urlList = urls;
        }

        public bool RunSync()
        {
            return RunAsync().GetAwaiter().GetResult();
        }

        public async Task<bool> RunAsync()
        {            
            foreach (var item in urlList)
            {
                KeyValuePair<string, RssRecoverType> rssContent = await RSSRecover.RecoverRSS(RSSType.RSSCustom, item);
                if (!string.IsNullOrEmpty(rssContent.Key))
                    SaveOnDisk(rssContent.Key);                
            }
            return true;
        }

        public static async Task<string> ProcessRssXml(string xmlContent)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlContent);

                XmlNode channelNode = doc.SelectSingleNode("//channel");
                if (channelNode == null)
                    throw new ArgumentException("Formato RSS inválido.");

                XmlDocument simplifiedDoc = new XmlDocument();
                XmlElement root = simplifiedDoc.CreateElement("rss");
                root.SetAttribute("version", "2.0");
                simplifiedDoc.AppendChild(root);

                XmlElement newChannel = simplifiedDoc.CreateElement("channel");
                root.AppendChild(newChannel);

                string[] channelFields = new string[] { "title", "description", "link", "language" };
                foreach (string field in channelFields)
                {
                    XmlNode node = channelNode.SelectSingleNode(field);
                    if (node == null) continue;

                    XmlElement newField = simplifiedDoc.CreateElement(field);
                    string cleanedText = (field == "title" || field == "description")
                        ? RemoveHtmlTags(node.InnerText)
                        : node.InnerText.Trim();
                    newField.InnerText = cleanedText;
                    newChannel.AppendChild(newField);
                }

                XmlElement lastBuildDate = simplifiedDoc.CreateElement("lastBuildDate");
                lastBuildDate.InnerText = DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss +0000");
                newChannel.AppendChild(lastBuildDate);

                XmlNodeList itemNodes = channelNode.SelectNodes("item");
                List<XmlNode> selectedItems = SelectItemsWithImages(itemNodes, doc);

                foreach (XmlNode itemNode in selectedItems)
                {
                    XmlElement newItem = simplifiedDoc.CreateElement("item");
                    newChannel.AppendChild(newItem);

                    string[] itemFields = new string[] { "title", "description", "pubDate", "author" };
                    foreach (string field in itemFields)
                    {
                        XmlNode node = itemNode.SelectSingleNode(field);
                        XmlElement newField = simplifiedDoc.CreateElement(field);

                        if (node != null)
                        {
                            if (field == "description")
                                newField.InnerText = RemoveHtmlTags(node.InnerText);
                            else if (field == "pubDate")
                                newField.InnerText = DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss.fff +0000");
                            else
                                newField.InnerText = node.InnerText.Trim();
                        }
                        else
                        {
                            if (field == "pubDate")
                                newField.InnerText = DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss.fff +0000");
                            else if (field == "author")
                                newField.InnerText = "id-" + DateTime.UtcNow.Ticks.ToString();
                            else
                                newField.InnerText = "Indisponível";
                        }
                        newItem.AppendChild(newField);
                    }

                    XmlElement guid = simplifiedDoc.CreateElement("guid");
                    guid.SetAttribute("isPermaLink", "true");
                    XmlNode linkNode = itemNode.SelectSingleNode("link");
                    guid.InnerText = linkNode != null ? linkNode.InnerText : "https://www.exemplo.com/default-guid";
                    newItem.AppendChild(guid);

                    string imageLink = GetImageUrl(itemNode, doc);
                    if (!string.IsNullOrEmpty(imageLink))
                    {
                        XmlElement enclosure = simplifiedDoc.CreateElement("enclosure");
                        enclosure.SetAttribute("url", imageLink);
                        enclosure.SetAttribute("type", "image/jpeg");
                        enclosure.SetAttribute("length", "12345");
                        newItem.AppendChild(enclosure);

                        XmlElement linkfoto = simplifiedDoc.CreateElement("linkfoto");
                        linkfoto.InnerText = imageLink;
                        newItem.AppendChild(linkfoto);
                    }
                }

                StringWriter stringWriter = new StringWriter();
                XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter);
                xmlWriter.Formatting = System.Xml.Formatting.Indented;
                simplifiedDoc.WriteTo(xmlWriter);
                xmlWriter.Close();
                string result = stringWriter.ToString();
                stringWriter.Close();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao processar o XML RSS: " + ex.Message, ex);
            }
        }

        private static string RemoveHtmlTags(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            string cleanedText = Regex.Replace(text, @"<[^>]+>", "");
            cleanedText = Regex.Replace(cleanedText, @"&[^;]+;", "");
            cleanedText = Regex.Replace(cleanedText, @"\s+", " ").Trim();
            return cleanedText;
        }

        private static List<XmlNode> SelectItemsWithImages(XmlNodeList itemNodes, XmlDocument doc)
        {
            List<XmlNode> itemsWithImages = new List<XmlNode>();
            foreach (XmlNode itemNode in itemNodes)
            {
                if (!string.IsNullOrEmpty(GetImageUrl(itemNode, doc)))
                {
                    itemsWithImages.Add(itemNode);
                    if (itemsWithImages.Count >= 15)
                        break;
                }
            }

            if (itemsWithImages.Count == 0)
                throw new Exception("Esse feed não possui imagens.");

            return itemsWithImages;
        }

        private static readonly string[] IMAGE_EXTENSIONS = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private static bool IsImageMedia(XmlNode media)
        {
            string medium = media.Attributes["medium"]?.Value ?? "";
            string type = media.Attributes["type"]?.Value ?? "";

            if (medium == "image" || type.StartsWith("image/"))
                return true;

            bool declaresOtherMedia = medium != "" || type != "";
            return !declaresOtherMedia && HasImageExtension(media.Attributes["url"]?.Value);
        }

        private static bool HasImageExtension(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
                return false;

            string extension = Path.GetExtension(uri.AbsolutePath).ToLowerInvariant();
            return IMAGE_EXTENSIONS.Contains(extension);
        }

        private static string GetImageUrl(XmlNode itemNode, XmlDocument doc)
        {
            List<(string url, int width, int height)> imageList = new List<(string url, int width, int height)>();

            void ExtractImage(XmlNode node, string urlAttribute, string widthAttribute, string heightAttribute)
            {
                string url = node.Attributes[urlAttribute]?.Value ?? "";
                int width = int.TryParse(node.Attributes[widthAttribute]?.Value, out int w) ? w : 0;
                int height = int.TryParse(node.Attributes[heightAttribute]?.Value, out int h) ? h : 0;

                if (!string.IsNullOrEmpty(url))
                    imageList.Add((url, width, height));
            }

            var nsmgr = XmlNamespaceManager(doc);

            foreach (XmlNode thumb in itemNode.SelectNodes("media:thumbnail", nsmgr))
            {
                ExtractImage(thumb, "url", "width", "height");
            }

            foreach (XmlNode mediaGroup in itemNode.SelectNodes("media:group", nsmgr))
            {
                foreach (XmlNode mediaContent in mediaGroup.SelectNodes("media:content", nsmgr))
                {
                    if (IsImageMedia(mediaContent))
                    {
                        ExtractImage(mediaContent, "url", "width", "height");
                    }
                }
            }

            foreach (XmlNode media in itemNode.SelectNodes("media:content", nsmgr))
            {
                if (IsImageMedia(media))
                {
                    ExtractImage(media, "url", "width", "height");
                }
            }

            XmlNode enclosure = itemNode.SelectSingleNode("enclosure");
            if (enclosure != null && enclosure.Attributes["type"]?.Value.StartsWith("image/") == true)
            {
                ExtractImage(enclosure, "url", "width", "height");
            }

            XmlNode linkFotoNode = itemNode.SelectSingleNode("linkfoto");
            if (linkFotoNode != null)
            {
                ExtractImage(linkFotoNode, "url", "width", "height");
            }

            XmlNode imageNode = itemNode.SelectSingleNode("image");
            if (imageNode != null)
            {
                XmlNode urlNode = imageNode.SelectSingleNode("url");
                if (urlNode != null && !string.IsNullOrEmpty(urlNode.InnerText))
                {
                    imageList.Add((urlNode.InnerText, 0, 0));
                }
            }

            XmlNode imageSingleNode = itemNode.SelectSingleNode("image");
            if (imageNode != null)
            {
                string url = imageSingleNode.InnerText.Trim();
                if (!string.IsNullOrEmpty(url))
                {
                    imageList.Add((url, 0, 0));
                }
            }

            string[] genericImageTags = { "img", "photo", "picture", "imageurl" };
            foreach (string tag in genericImageTags)
            {
                XmlNode genericNode = itemNode.SelectSingleNode(tag);
                if (genericNode != null)
                {
                    ExtractImage(genericNode, "src", "width", "height");
                }
            }

            if (!imageList.Any())
            {
                string[] textFields = { "description", "summary", "content", "subtitle" };
                foreach (string field in textFields)
                {
                    XmlNode textNode = itemNode.SelectSingleNode(field);
                    if (textNode != null)
                    {
                        string extractedImage = ExtractImageUrlFromText(textNode.InnerText);
                        if (!string.IsNullOrEmpty(extractedImage))
                            return extractedImage;
                    }
                }

                return string.Empty;
            }

            var bestImage = imageList
                .OrderByDescending(img => Math.Abs(img.width * img.height - 1920 * 1080))
                .ThenByDescending(img => img.width * img.height)
                .First();

            return bestImage.url;
        }

        private static XmlNamespaceManager XmlNamespaceManager(XmlDocument doc)
        {
            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("media", "http://search.yahoo.com/mrss/");
            return nsmgr;
        }

        private static string ExtractImageUrlFromText(string text)
        {
            Match match = Regex.Match(text, @"https?://[^\s""']+\.(jpg|jpeg|png|gif|webp)", RegexOptions.IgnoreCase);
            return match.Success ? match.Value : string.Empty;
        }

        private void SaveOnDisk(string content)
        { }
    }
}
