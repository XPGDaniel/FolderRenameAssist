using FolderRenameAssist.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FolderRenameAssist.Class
{
    public class GroupHandler
    {
        public static List<ItemToRename> ReplaceFolderName(List<ItemToRename> list, List<FolderRenameAssist.Objects.Group> groups)
        {
            if (list != null && groups != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    string defaultkeyword = GetTitleKeyword(list[i].Before);
                    string finalkeyword = "";
                    if (!string.IsNullOrEmpty(list[i].AlterKey))
                    {
                        finalkeyword = list[i].AlterKey;
                        Console.WriteLine();
                    }
                    else
                    {
                        finalkeyword = defaultkeyword;
                    }

                    if (!string.IsNullOrEmpty(finalkeyword))
                    {
                        FolderRenameAssist.Objects.Group listItem = groups.Find(x => x.Enable && x.Members.ToLowerInvariant().Contains("," + finalkeyword.ToLowerInvariant()));
                        if (listItem == null)
                        {
                            listItem = groups.Find(x => x.Enable && x.Members.ToLowerInvariant().Contains(finalkeyword.ToLowerInvariant()));
                        }
                        if (listItem != null)
                        {
                            //listItem.Presenter = listItem.Presenter; //.Replace(",", "][")
                            //list[i].Before = list[i].Before.Replace(defaultkeyword, listItem.Presenter);
                            list[i].Before = listItem.Presenter + list[i].Before; //prefix
                        }
                    }
                }
            }
            return list;
        }
        public static int IndexOfAny(string test, string[] values)
        {
            int first = -1;
            foreach (string item in values)
            {
                int i = test.IndexOf(item);
                if (i >= 0)
                {
                    if (first > 0)
                    {
                        if (i < first)
                        {
                            first = i;
                        }
                    }
                    else
                    {
                        first = i;
                    }
                }
            }
            return first;
        }

        public static string GetTitleKeyword(string before)
        {
            string header = "";
            string foldername = "";
            if (before.IndexOf('[') == 0 && before.Contains(']'))
            {
                header = before.Substring(0, before.IndexOf(']') + 1);
            }
            if (!string.IsNullOrEmpty(header))
            {
                if (header == before) //re-format first
                {
                    header = header.Remove(header.IndexOf(']') + 1);
                    before = before.Replace(header, "").Substring(1);
                    before = header + " " + before.Remove(before.IndexOf(']'));
                    if (before.IndexOf('[') != -1)
                        before = before + " " + before.Substring(before.IndexOf('['));
                }

                foldername = before.Replace(header, "").Trim();
                if (foldername[0] == '[') foldername = foldername.Substring(1);


                int cutindex = foldername.IndexOfAny(new char[] { '(', '[' });
                if (cutindex > 0)
                {
                    foldername = foldername.Remove(cutindex).Trim();
                }

                cutindex = IndexOfAny(foldername, new string[] { "TV", "OVA", "BD", "DVD" });
                if (cutindex > 0)
                {
                    foldername = foldername.Remove(cutindex).Trim();
                }

                if (foldername[foldername.Length - 1] == ']')
                {
                    foldername = foldername.Substring(0, foldername.Length - 1);
                    before = header + " " + foldername;
                }

                if (!string.IsNullOrEmpty(foldername))
                {
                    foldername = foldername.Replace("_", " ");
                }
            }
            return foldername;
        }

        public static AnidbResult SearchAniDB(ObservableCollection<AnimeTitle> anititles, string keyword)
        {
            AnidbResult ar = new AnidbResult();
            int aid = anititles.Where(x => x.title.ToLowerInvariant().StartsWith(keyword.ToLowerInvariant())).Select(x => x.aid).FirstOrDefault();
            string keywords = "";
            string presenter = "";
            if (aid > 0)
            {
                string[] candidates = anititles.Where(x => x.aid == aid).Select(x => x.title).Distinct(StringComparer.CurrentCultureIgnoreCase).ToArray();
                keywords = string.Join(",", candidates);
                string[] Pcandidates = anititles.Where(x => x.aid == aid && x.type != "short").Select(x => x.title).Distinct(StringComparer.CurrentCultureIgnoreCase).ToArray();
                presenter = string.Join(",", Pcandidates);
                if (!string.IsNullOrEmpty(keyword))
                {
                    ar.aid = aid.ToString();
                    ar.presenter = presenter;
                    ar.keywords = keywords;
                    return ar;
                }
            }
            return null;
        }
        public static AnidbResult SearchAniDBByID(ObservableCollection<AnimeTitle> anititles, string aid)
        {
            AnidbResult ar = new AnidbResult();
            string keywords = "";
            string presenter = "";
            if (!string.IsNullOrEmpty(aid))
            {
                string[] candidates = anititles.Where(x => x.aid == Convert.ToInt32(aid)).Select(x => x.title).Distinct(StringComparer.CurrentCultureIgnoreCase).ToArray();
                keywords = string.Join(",", candidates);
                string[] Pcandidates = anititles.Where(x => x.aid == Convert.ToInt32(aid) && x.type != "short").Select(x => x.title).Distinct(StringComparer.CurrentCultureIgnoreCase).ToArray();
                presenter = string.Join(",", Pcandidates);
                ar.aid = aid;
                ar.presenter = presenter;
                ar.keywords = keywords;
                return ar;
            }
            return null;
        }
    }
}
