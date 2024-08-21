using System;
using System.Web;
using System.IO;
using System.Reflection;
using System.ServiceModel.Syndication;

namespace EFS.Syndication
{
    public static class SyndicationTools
    {
        public enum FeedFormatEnum
        {
            ALL, RSS20, Atom10
        }

        /// <summary>
        /// Ajoute un feed de syndication
        /// </summary>
        /// <param name="pTitle">Titre</param>
        /// <param name="pDescription">Description</param>
        /// <param name="pLink">Lien</param>
        /// <param name="pAuthor">Auteur</param>
        /// <param name="pCulture">Langue</param>
        /// <param name="pCopyright">Copyright</param>
        /// <param name="pGenerator">Générateur</param>
        /// <param name="pImageUrl">Média associé</param>
        /// <returns></returns>
        // EG [25500] New (Utilisé pour la gestion des flux RSS entre EFS Website et EFS CustomerPortal)
        public static SyndicationFeed AddFeed(string pTitle, string pDescription, string pLink, string pAuthor, 
            string pCulture, string pCopyright, string pGenerator, string pImageUrl)
        {
            SyndicationFeed feed = new SyndicationFeed(pTitle, pDescription, new Uri(pLink));
            feed.Authors.Add(new SyndicationPerson(pAuthor));
            feed.Description = new TextSyndicationContent(pDescription);
            feed.Language = pCulture.ToLower();
            feed.Copyright = new TextSyndicationContent(pCopyright);
            feed.Generator = pGenerator;
            feed.ImageUrl = new Uri(pImageUrl);
            return feed;
        }
        /// <summary>
        /// Ajoute un item à un Feed de syndication
        /// </summary>
        /// <param name="pId">Id</param>
        /// <param name="pTitle">Titre</param>
        /// <param name="pContent">Description</param>
        /// <param name="pLink">Lien</param>
        /// <param name="pPublishDate">Date de publication</param>
        /// <param name="pCategories">Catégories</param>
        /// <returns></returns>
        // EG [25500] New (Utilisé pour la gestion des flux RSS entre EFS Website et EFS CustomerPortal)
        public static SyndicationItem AddItem(object pId, object pTitle, object pContent, string pLink, object pPublishDate, object pCategories)
        {
            SyndicationItem item = new SyndicationItem(Convert.ToString(pTitle), Convert.ToString(pContent), 
                new Uri(pLink), Convert.ToString(pId), Convert.ToDateTime(pPublishDate));

            if (Convert.IsDBNull(pCategories) || String.IsNullOrEmpty(Convert.ToString(pCategories)))
                item.Categories.Add(new SyndicationCategory("N/A"));
            else
            {
                string[] categories = Convert.ToString(pCategories).Split(new char[] { ';' });
                foreach (string category in categories)
                {
                    item.Categories.Add(new SyndicationCategory(category));
                }
            }
            item.PublishDate = Convert.ToDateTime(pPublishDate);

            return item;
        }


        /// <summary>
        /// Ajoute un objet(image, video) à un item de syndication
        /// </summary>
        /// <param name="pItem">Item soource</param>
        /// <param name="pUri">Uri de l'objet</param>
        /// <param name="pMediaType">Type de l'objet</param>
        /// <param name="pLength">Longueur</param>
        // EG [25500] New (Utilisé pour la gestion des flux RSS entre EFS Website et EFS CustomerPortal)
        public static void SetEnclosure(SyndicationItem pItem, object pUri, object pMediaType, object pLength)
        {
            if (false == Convert.IsDBNull(pUri)) //&& (false == Convert.IsDBNull(pLength)) && (false == Convert.IsDBNull(pMediaType)))
            {
                SyndicationLink enclosure = SyndicationLink.CreateMediaEnclosureLink(new Uri(Convert.ToString(pUri)),
                    Convert.IsDBNull(pMediaType) ? "":Convert.ToString(pMediaType), Convert.IsDBNull(pLength) ? 0: Convert.ToInt32(pLength));
                pItem.Links.Add(enclosure);
            }
        }

        /// <summary>
        /// Ajoute un commentaire à un Item de syndication
        /// </summary>
        /// <param name="pItem">Item source</param>
        /// <param name="pComment">Commentaire</param>
        // EG [25500] New (Utilisé pour la gestion des flux RSS entre EFS Website et EFS CustomerPortal)
        public static void SetComment(SyndicationItem pItem, object pComment)
        {
            if (false == Convert.IsDBNull(pComment))
                pItem.ElementExtensions.Add(Convert.ToString(pComment));
        }

        /// <summary>
        /// Sauvegarde des flux RSS en mode Atom10 et/ou RSS20 pour chaque culture 
        /// </summary>
        /// <param name="pFeed">Flux Rss</param>
        /// <param name="pFeedFormat">Format</param>
        /// <param name="pCulture">Culture du flux</param>
        // EG [25500] New (Utilisé pour la gestion des flux RSS entre EFS Website et EFS CustomerPortal)
        public static void SaveSyndicationFeedFile(SyndicationFeed pFeed, FeedFormatEnum pFeedFormat, string pCulture)
        {
            string cultureFile = String.Format("/SyndicationFeed/RSS/{0}/", pCulture) + "News_{ALL}.xml";
            string write_File = string.Empty;
            string path = HttpContext.Current.Request.PhysicalApplicationPath;
            if (path.EndsWith(@"\"))
                path = path.Substring(0, path.Length - 1);
            try
            {
                DirectoryInfo dirInfo = Directory.GetParent(path);
                path = dirInfo.FullName + cultureFile;
                System.Xml.XmlWriterSettings xmlWriterSettings = new System.Xml.XmlWriterSettings
                {
                    Indent = true
                };
                System.Xml.XmlWriter feedWriter;

                switch (pFeedFormat)
                {
                    case FeedFormatEnum.ALL:
                    case FeedFormatEnum.Atom10:
                        // Use Atom 1.0        
                        Atom10FeedFormatter atomFormatter = new Atom10FeedFormatter(pFeed);
                        if (pFeedFormat == FeedFormatEnum.ALL)
                            write_File = path.Replace("{ALL}", "Atom10");
                        feedWriter = System.Xml.XmlWriter.Create(write_File, xmlWriterSettings);
                        atomFormatter.WriteTo(feedWriter);
                        feedWriter.Close();
                        break;
                }
                switch (pFeedFormat)
                {
                    case FeedFormatEnum.ALL:
                    case FeedFormatEnum.RSS20:
                        // Emit RSS 2.0
                        Rss20FeedFormatter rssFormatter = new Rss20FeedFormatter(pFeed);
                        if (pFeedFormat == FeedFormatEnum.ALL)
                            write_File = path.Replace("{ALL}", "RSS20");
                        feedWriter = System.Xml.XmlWriter.Create(write_File, xmlWriterSettings);
                        rssFormatter.WriteTo(feedWriter);
                        feedWriter.Close();
                        break;
                }

            }
            catch (DirectoryNotFoundException)
            {
                //Spheres® abandonne
            }
        }
    }
}
