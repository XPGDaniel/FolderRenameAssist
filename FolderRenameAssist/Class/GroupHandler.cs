using FolderRenameAssist.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace FolderRenameAssist.Class
{
    public class GroupHandler
    {
        private static readonly Regex cjkCharRegex = new Regex(@"\p{IsCJKUnifiedIdeographs}");
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
                        finalkeyword = list[i].AlterKey.ToLowerInvariant();
                        //Console.WriteLine();
                    }
                    else
                    {
                        finalkeyword = defaultkeyword;
                    }

                    if (!string.IsNullOrEmpty(finalkeyword))
                    {
                        FolderRenameAssist.Objects.Group listItem;
                        if (finalkeyword.StartsWith("anidb-") && finalkeyword.Length > 6)
                        {
                            listItem = groups.Find(x => x.Enable && x.AnidbId == finalkeyword.Replace("anidb-", ""));
                        }
                        else
                        {
                            listItem = groups.Find(x => x.Enable && x.Members.ToLowerInvariant().Contains("," + finalkeyword.ToLowerInvariant()));
                        }
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
            string original = "";
            string foldername = "";
            original = before;

            while (before.IndexOf('[') == 0 && before.Contains(']'))
            {
                before = before.Substring(before.IndexOf(']') + 1).Trim();
            }

            if (string.IsNullOrEmpty(before)) //[tag1][tag2][tag3][....][tagN]
            {
                original = original.Substring(1, original.Length - 2);
                original = original.Replace("][", ";");
                return original.Split(';')[1];
            }

            foldername = before; //.Replace("THE ANIMATION","")
            if (!string.IsNullOrEmpty(foldername))
            {
                foldername = foldername.Replace("_", " ");
            }

            if (foldername[0] == '[') foldername = foldername.Substring(1);

            if (foldername.Contains(" "))
            {
                HashSet<string> stringSet = new HashSet<string>(new string[] { "d", "wd", "zd" });
                string FirstSplitCheck = foldername.Split(' ')[0].ToLowerInvariant();
                if (stringSet.Contains(FirstSplitCheck))
                {
                    foldername = foldername.Substring(FirstSplitCheck.Length).Trim();
                }
                Regex date = new Regex(@"(\d\d\d\d)-(\d\d)-(\d\d)");
                if (date.Match(FirstSplitCheck).Success)
                {
                    foldername = foldername.Substring(FirstSplitCheck.Length).Trim();
                }
            }

            int cutindex = IndexOfAny(foldername.ToUpperInvariant(), new string[] { "~", "〜", "～", "(", "[", "TV", "OVA", "BD", "DVD", "THE ANIMATION" });
            if (cutindex > 0)
            {
                foldername = foldername.Remove(cutindex).Trim();
            }

            if (foldername[foldername.Length - 1] == ']')
            {
                foldername = foldername.Substring(0, foldername.Length - 1);
            }

            if (foldername.Split(' ')[foldername.Split(' ').Count() - 1][0] == '-')
            {
                foldername = foldername.Replace(foldername.Split(' ')[foldername.Split(' ').Count() - 1], "");
                //foldername = foldername.Remove(foldername.Length - 1, 1);
            }

            if (foldername[0] == ']') foldername = foldername.Substring(1);

            if (foldername.Contains(" ")) //prefix Title whatever_else
            {
                string FirstSplitCheck = foldername.Split(' ')[0].Trim();
                if (FirstSplitCheck.Any(z => IsChinese(z))&& foldername.Split(' ').Count() > 1)
                {
                    if (foldername.Split(' ')[1].Trim().Contains("～")) return foldername.Split(' ')[1].Trim().Substring(0, foldername.Split(' ')[1].Trim().IndexOf("～")).Trim();
                    return foldername.Split(' ')[1].Trim();
                }
                else
                {
                    if (FirstSplitCheck.Contains("～")) return FirstSplitCheck.Substring(0, FirstSplitCheck.IndexOf("～")).Trim();
                    return FirstSplitCheck;
                }
            }
            else if (foldername.StartsWith("[") && foldername.Contains("]") && foldername[foldername.Length - 1] != ']') //[groupname]Titles
            {
                if (foldername.Substring(foldername.LastIndexOf(']') + 1).Trim().Contains("～")) return foldername.Substring(foldername.LastIndexOf(']') + 1).Trim().Substring(0, foldername.Substring(foldername.LastIndexOf(']') + 1).Trim().IndexOf("～")).Trim();
                return foldername.Substring(foldername.LastIndexOf(']') + 1).Trim();
            }
            else
            {
                if (foldername.Contains("～")) return foldername.Substring(0,foldername.IndexOf("～")).Trim();
                return foldername;
            }
        }

        public static AnidbResult SearchAniDB(ObservableCollection<AnimeTitle> anititles, string keyword, bool idsearch)
        {
            AnidbResult ar = new AnidbResult();
            int aid = 0;

            if (idsearch)
            {
                aid = anititles.Where(x => x.aid == Convert.ToInt32(keyword.Replace("anidb-", ""))).Select(x => x.aid).FirstOrDefault();
            }
            else
            {
                aid = anititles.Where(x => x.title.ToLowerInvariant() == keyword.ToLowerInvariant()).Select(x => x.aid).FirstOrDefault();
                if (aid < 1)
                {
                    aid = anititles.Where(x => x.title.ToLowerInvariant().StartsWith(keyword.ToLowerInvariant())).Select(x => x.aid).FirstOrDefault();
                    if (aid < 1)
                    {
                        aid = anititles.Where(x => x.title.ToLowerInvariant().Contains(keyword.ToLowerInvariant())).Select(x => x.aid).FirstOrDefault();
                    }
                }
            }
            string keywords = "";
            string presenter = "";
            if (aid > 0)
            {
                string[] candidates = anititles.Where(x => x.aid == aid).Select(x => x.title).Distinct(StringComparer.CurrentCultureIgnoreCase).ToArray();
                keywords = string.Join(",", candidates);
                string[] Pcandidates = anititles.Where(x => x.aid == aid && x.type != "short").Select(x => x.title).Distinct(StringComparer.CurrentCultureIgnoreCase).ToArray();
                //Array.Sort(Pcandidates, (x, y) => x.Length.CompareTo(y.Length));
                Array.Sort(Pcandidates, (x, y) => String.Compare(x, y));
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
        public static AnidbResult SearchGroups(ObservableCollection<Objects.Group> groups, string keyword, bool idsearch)
        {
            AnidbResult ar = new AnidbResult();
            Objects.Group gr = new Objects.Group();
            if (!string.IsNullOrEmpty(keyword))
            {
                if (idsearch)
                {
                    gr = groups.Where(x => x.Enable && x.AnidbId == keyword.Replace("anidb-", "")).FirstOrDefault();
                    if (gr != null)
                    {
                        ar.aid = "xxx";
                        ar.presenter = gr.Presenter;
                        ar.keywords = gr.Members;
                        return ar;
                    }
                }
                else
                {
                    gr = groups.Where(x => x.Enable && x.Members.ToLowerInvariant().Contains(keyword.ToLowerInvariant())).FirstOrDefault();
                    if (gr != null)
                    {
                        ar.aid = "xxx";
                        ar.presenter = gr.Presenter;
                        ar.keywords = gr.Members;
                        return ar;
                    }
                }
            }
            return null;
        }
        public static AnidbResult SearchAniDBByID(ObservableCollection<AnimeTitle> anititles, string aid)
        {
            AnidbResult ar = new AnidbResult();
            string keywords = "";
            string presenter = "";
            if (!string.IsNullOrEmpty(aid) && int.TryParse(aid, out int tryit))
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
        public static bool IsChinese(char c)
        {
            return cjkCharRegex.IsMatch(c.ToString());
        }
    }
}
