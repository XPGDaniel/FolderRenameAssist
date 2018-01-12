using FolderRenameAssist.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace FolderRenameAssist.Class
{
    public static class XMLHelper
    {
        public static bool SaveProfileXML(List<Group> list, string filename)
        {
            try
            {
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings()
                {
                    NewLineOnAttributes = false,
                    Indent = true
                };
                using (XmlWriter writer = XmlWriter.Create(filename, xmlWriterSettings))
                {
                    writer.WriteStartDocument();

                    writer.WriteStartElement("Group");
                    foreach (Group ru in list)
                    {
                        writer.WriteStartElement("Group");
                        writer.WriteAttributeString("Enable", ru.Enable ? "true" : "false");
                        writer.WriteAttributeString("AnidbId", string.IsNullOrEmpty(ru.AnidbId) ? "" : ru.AnidbId);
                        writer.WriteAttributeString("Presenter", string.IsNullOrEmpty(ru.Presenter) ? "" : ru.Presenter);
                        writer.WriteAttributeString("Members", string.IsNullOrEmpty(ru.Members) ? "" : ru.Members);
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static ObservableCollection<Group> LoadGroupXML(string filename)
        {
            try
            {
                XDocument xdoc = XDocument.Load(filename);
                ObservableCollection<Group> rulist = new ObservableCollection<Group>((
                from e in xdoc.Root.Descendants("Group")
                select new Group
                {
                    Enable = Convert.ToBoolean(e.Attributes("Enable").Single().Value),
                    AnidbId = (e.Attributes("AnidbId") == null) ? "" : e.Attributes("AnidbId").Single().Value,
                    Presenter = e.Attributes("Presenter").Single().Value,
                    Members = e.Attributes("Members").Single().Value
                }).ToList());
                return rulist;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static ObservableCollection<AnimeTitle> LoadAnimeTitlesXML(string filename)
        {
            try
            {
                string[] validtype = new string[] {"main","official","short" };
                string[] validlang = new string[] { "x-jat", "ja", "en" };
                ObservableCollection<AnimeTitle> titlelist = new ObservableCollection<AnimeTitle>();
                XDocument xdoc = XDocument.Load(filename);
                foreach (var anime in xdoc.Root.Descendants("anime"))
                {
                    int aid = Convert.ToInt32(anime.Attributes("aid").Single().Value);
                    foreach (var title in anime.Descendants("title"))
                    {
                        string type = title.Attributes("type").Single().Value;
                        string lang = title.Attributes(XNamespace.Xml + "lang").Single().Value;
                        if (validtype.Contains(type) && validlang.Contains(lang))
                        {
                            titlelist.Add(new AnimeTitle
                            {
                                aid = aid,
                                type = type,
                                lang = lang,
                                title = title.Value
                            });
                        }
                    }
                }
                return titlelist;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
