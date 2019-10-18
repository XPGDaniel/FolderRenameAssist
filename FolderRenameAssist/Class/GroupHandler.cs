using FolderRenameAssist.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FolderRenameAssist.Class
{
    public class GroupHandler
    {
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
        public static List<ItemToRename> ReplaceFolderName(List<ItemToRename> list, List<FolderRenameAssist.Objects.Group> groups, bool? presentOnly, bool? UNC)
        {
            if (list != null && groups != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    string defaultkeyword = Path.GetFileNameWithoutExtension(GetTitleKeyword(list[i].Before)).Trim();
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
                            listItem = groups.Find(x => x.Enable && x.Presenter.ToLowerInvariant().Contains("[" + finalkeyword.ToLowerInvariant()));
                        }
                        if (listItem == null)
                        {
                            listItem = groups.Find(x => x.Enable && x.Members.ToLowerInvariant().Contains(finalkeyword.ToLowerInvariant()));
                        }
                        if (listItem != null)
                        {
                            if (presentOnly == true)
                            {
                                list[i].Before = listItem.Presenter;
                            }
                            else
                            {
                                list[i].Before = listItem.Presenter + list[i].Before; //prefix mode
                            }
                            if (UNC == true)
                            {
                                list[i].Before = list[i].Before + "[UNC]";
                            }
                        }
                    }
                }
            }
            return list;
        }
        public static string StringSanitizer(string input)
        {
            StringBuilder sb = new StringBuilder(input.ToLowerInvariant().Trim());
            foreach (string to_replace in _sanitizer.Keys)
            {
                sb = sb.Replace(to_replace, _sanitizer[to_replace]);
            }
            input = sb.ToString();

            input = RemoveQuotation('[', ']', input);
            input = RemoveQuotation('(', ')', input);
            input = RemoveQuotation('「', '」', input);
            input = RemoveQuotation('{', '}', input);
            input = RemoveQuotation('【', '】', input);
            return input;
        }
        public static string RemoveQuotation(char front, char back, string input)
        {
            string chunk = "";
            while (input.LastIndexOf(front) > -1 && input.Contains(back))
            {
                chunk = input.Substring(input.LastIndexOf(front)).Trim();
                chunk = chunk.Substring(0, chunk.IndexOf(back) + 1).Trim();
                if (!string.IsNullOrEmpty(chunk))
                    input = input.Replace(chunk, "").Trim();
                else
                    break;
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
                    AnidbResult ar = new AnidbResult
                    {
                        aid = aid.ToString(),
                        presenter = presenter,
                        keywords = keywords
                    };
                    return ar;
                }
            }
            return null;
        }
        public static AnidbResult SearchGroups(ObservableCollection<Objects.Group> groups, string keyword, bool idsearch)
        {
            Objects.Group gr = new Objects.Group();
            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLowerInvariant();
                if (idsearch)
                {
                    gr = groups.Where(x => x.Enable && x.AnidbId == keyword.Replace("anidb-", "")).FirstOrDefault();
                }
                else
                {
                    string prepped_keyword = Utilities.Prepare_Keyword(keyword);
                    // Presenter full match
                    gr = groups.Where(x => x.Enable && x.Presenter.ToLowerInvariant().Contains(prepped_keyword)).FirstOrDefault();
                    // Presenter StartWith
                    if (gr == null)
                    {
                        gr = groups.Where(x => x.Enable && x.Presenter.ToLowerInvariant().StartsWith(prepped_keyword.Replace("]", ""))).FirstOrDefault();
                    }
                    // Presenter partial match
                    if (gr == null)
                    {
                        gr = groups.Where(x => x.Enable && x.Presenter.ToLowerInvariant().Contains(keyword)).FirstOrDefault();
                    }
                    // Members partial match
                    if (gr == null)
                    {
                        gr = groups.Where(x => x.Enable && x.Members.ToLowerInvariant().Contains(keyword)).FirstOrDefault();
                    }
                }
                if (gr != null)
                {
                    AnidbResult ar = new AnidbResult
                    {
                        aid = "xxx",
                        presenter = gr.Presenter,
                        keywords = gr.Members
                    };
                    return ar;
                }
            }
            return null;
        }
        public static AnidbResult SearchAniDBByID(ObservableCollection<AnimeTitle> anititles, string aid)
        {
            string keywords = "";
            string presenter = "";
            if (!string.IsNullOrEmpty(aid) && int.TryParse(aid, out int tryit))
            {
                string[] candidates = anititles.Where(x => x.aid == Convert.ToInt32(aid)).Select(x => x.title).Distinct(StringComparer.CurrentCultureIgnoreCase).ToArray();
                keywords = string.Join(",", candidates);
                string[] Pcandidates = anititles.Where(x => x.aid == Convert.ToInt32(aid) && x.type != "short").Select(x => x.title).Distinct(StringComparer.CurrentCultureIgnoreCase).ToArray();
                presenter = string.Join(",", Pcandidates);
                AnidbResult ar = new AnidbResult
                {
                    aid = aid,
                    presenter = presenter,
                    keywords = keywords
                };
                return ar;
            }
            return null;
        }
    }
}
