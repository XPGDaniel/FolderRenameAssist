using FolderRenameAssist.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FolderRenameAssist.Class
{
    public class GroupHandler
    {
        private static readonly Regex cjkCharRegex = new Regex(@"\p{IsCJKUnifiedIdeographs}");
        private static Dictionary<string, string> _sanitizer = new Dictionary<string, string>();

        static GroupHandler()
        {
            //_sanitizer[" & "] = "";
            _sanitizer[","] = "";
            _sanitizer["~"] = "";
            _sanitizer["〜"] = "";
            _sanitizer["～"] = "";
            _sanitizer["！"] = "!";
            _sanitizer["："] = ":";
            _sanitizer["ova"] = " ";
            _sanitizer["oad"] = " ";
            _sanitizer["oav"] = " ";
            _sanitizer["tv"] = " ";
            _sanitizer["tvrip"] = " ";
            _sanitizer["bd"] = " ";
            _sanitizer["bdrip"] = " ";
            _sanitizer["dvd"] = " ";
            _sanitizer["dvdrip"] = " ";
            _sanitizer["the animation"] = " ";
            _sanitizer[" - "] = " ";
            _sanitizer[" -"] = " ";
            _sanitizer["- "] = " ";
            _sanitizer[" + "] = " ";
            _sanitizer[" +"] = " ";
            _sanitizer["+ "] = " ";
            _sanitizer[" sp "] = " ";
            _sanitizer[" sp"] = " ";
            _sanitizer["_"] = " ";
        }
        public static List<ItemToRename> ReplaceFolderName(List<ItemToRename> list, List<FolderRenameAssist.Objects.Group> groups, bool? presentOnly)
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
                            if (presentOnly == true)
                            {
                                list[i].Before = listItem.Presenter;
                            }
                            else
                            {
                                list[i].Before = listItem.Presenter + list[i].Before; //prefix
                            }
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
        public static string StringSanitizer(string input)
        {
            StringBuilder sb = new StringBuilder(input.ToLowerInvariant().Trim());
            string chunk = "";
            foreach (string to_replace in _sanitizer.Keys)
            {
                sb = sb.Replace(to_replace, _sanitizer[to_replace]);
            }
            input = sb.ToString();

            while (input.LastIndexOf('[') > -1 && input.Contains(']'))
            {
                chunk = input.Substring(input.LastIndexOf('[')).Trim();
                chunk = chunk.Substring(0, chunk.IndexOf(']') + 1).Trim();
                input = input.Replace(chunk, "").Trim();
            }
            while (input.LastIndexOf('(') > -1 && input.Contains(')'))
            {
                chunk = input.Substring(input.LastIndexOf('(')).Trim();
                chunk = chunk.Substring(0, chunk.IndexOf(')') + 1).Trim();
                input = input.Replace(chunk, "").Trim();
            }
            while (input.LastIndexOf('「') > -1 && input.Contains('」'))
            {
                chunk = input.Substring(input.LastIndexOf('「')).Trim();
                chunk = chunk.Substring(0, chunk.IndexOf('」') + 1).Trim();
                input = input.Replace(chunk, "").Trim();
            }

            return input;
        }
        public static string GetTitleKeyword(string before)
        {
            string original = before;
            string foldername = "";
            before = StringSanitizer(before);

            if (string.IsNullOrEmpty(before)) //[tag1][tag2][tag3][....][tagN]
            {
                return original;
            }

            foldername = before;

            if (foldername.Contains(" "))
            {
                HashSet<string> stringSet = new HashSet<string>(new string[] { "w", "d", "wd", "zd" });
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

            return foldername;
            //}
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
                }
                else
                {
                    string prepped_keyword = keyword;
                    if (!keyword.StartsWith("["))
                        prepped_keyword = "[" + prepped_keyword;
                    if (!keyword.EndsWith("]"))
                        prepped_keyword = prepped_keyword + "]";
                    gr = groups.Where(x => x.Enable && x.Presenter.ToLowerInvariant().Contains(prepped_keyword.ToLowerInvariant())).FirstOrDefault();
                    if (gr == null)
                    {
                        gr = groups.Where(x => x.Enable && x.Presenter.ToLowerInvariant().Contains(keyword.ToLowerInvariant())).FirstOrDefault();
                    }
                    if (gr == null)
                    {
                        gr = groups.Where(x => x.Enable && x.Members.ToLowerInvariant().Contains(keyword.ToLowerInvariant())).FirstOrDefault();
                    }
                }
                if (gr != null)
                {
                    ar.aid = "xxx";
                    ar.presenter = gr.Presenter;
                    ar.keywords = gr.Members;
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
