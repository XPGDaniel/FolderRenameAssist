using FolderRenameAssist.Class;
using FolderRenameAssist.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.VisualBasic.FileIO;
using log4net;
using System.Windows.Input;
using System.Windows.Media;

namespace FolderRenameAssist
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly ILog log = LogHelper.GetLoggerRollingFileAppender(typeof(MainWindow).ToString(), "log.txt"); //@"D:\FolderRenameAssist\FolderRenameAssist\bin\Debug\
        private enum MoveDirection { Up = -1, Down = 1 };
        public ObservableCollection<Group> groups = new ObservableCollection<Group>();
        public ObservableCollection<AnimeTitle> anititles = new ObservableCollection<AnimeTitle>();
        public ObservableCollection<ItemToRename> Targets = new ObservableCollection<ItemToRename>();
        public string OriginalSearchWord = "";

        private void btn_Preview_Click(object sender, RoutedEventArgs e)
        {
            if (lView_Groups.Items.Count > 0 && lView_TargetList.Items.Count > 0)
            {
                lView_TargetList.ItemsSource = null;
                List<ItemToRename> newFilenameList = new List<ItemToRename>();
                foreach (ItemToRename vi in Targets)
                {
                    ItemToRename ftr = new ItemToRename()
                    {
                        Path = vi.Path,
                        AlterKey = vi.AlterKey,
                        Before = vi.Before
                    };
                    newFilenameList.Add(ftr);
                }
                if (groups.Any())
                {
                    newFilenameList = GroupHandler.ReplaceFolderName(newFilenameList, groups.ToList(), chkbox_PresenterOnly.IsChecked);
                    for (int i = 0; i < Targets.Count; i++)
                    {
                        int index = Targets.IndexOf(Targets.Where(X => X.Path == newFilenameList[i].Path).FirstOrDefault());
                        if (Targets[index].Before != newFilenameList[i].Before)
                        {
                            Targets[index].After = newFilenameList[i].Before;
                        }
                    }
                }
                newFilenameList.Clear();
                lView_TargetList.ItemsSource = Targets;
                lView_TargetList.Items.Refresh();
                tbx_TitleKeyword.Text = "";
                tbx_AnidbID.Text = "";
                tbx_GroupMembers.Text = "";
                tbx_Presenter.Text = "";
            }
        }

        private void btn_GO_Click(object sender, RoutedEventArgs e)
        {
            btn_Preview_Click(null, null);
            if (lView_Groups.Items.Count > 0 && lView_TargetList.Items.Count > 0)
            {
                lView_TargetList.ItemsSource = null;
                List<ItemToRename> RenameList = new List<ItemToRename>();
                foreach (ItemToRename vi in Targets)
                {
                    ItemToRename ftr = new ItemToRename()
                    {
                        Path = vi.Path,
                        Before = vi.Before,
                        AlterKey = vi.AlterKey,
                        After = vi.After
                    };
                    RenameList.Add(ftr);
                }
                if (RenameList.Any())
                {
                    foreach (ItemToRename fileCandidate in RenameList)
                    {
                        if (Directory.Exists(fileCandidate.Path))
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(fileCandidate.After))
                                {
                                    fileCandidate.Result = GlobalConst.EMPTY_STRING;
                                    continue;
                                }
                                if (!string.IsNullOrEmpty(Directory.GetParent(fileCandidate.Path).FullName + "\\" + fileCandidate.After))
                                {
                                    if (fileCandidate.Path != Directory.GetParent(fileCandidate.Path).FullName + "\\" + fileCandidate.After)
                                    {
                                        log.Info(Directory.GetParent(fileCandidate.Path).FullName + "\\" + fileCandidate.After);
                                        try
                                        {
                                            try
                                            {
                                                if (File.Exists(Path.Combine(fileCandidate.Path, Path.GetFileName(fileCandidate.Path) + ".md5")))
                                                    File.Move(Path.Combine(fileCandidate.Path, Path.GetFileName(fileCandidate.Path) + ".md5"), Path.Combine(fileCandidate.Path, fileCandidate.After + ".md5"));
                                            }
                                            catch (Exception) { throw; }
                                            try
                                            {
                                                if (Directory.Exists(Path.Combine(fileCandidate.Path.ToLowerInvariant(), "chi")))
                                                {
                                                    foreach (var file in Directory.EnumerateFiles(Path.Combine(fileCandidate.Path.ToLowerInvariant(), "chi")))
                                                    {
                                                        File.Move(file, Path.Combine(fileCandidate.Path, Path.GetFileName(file)));
                                                    }
                                                    Directory.Delete(Path.Combine(fileCandidate.Path.ToLowerInvariant(), "chi"));
                                                }
                                                if (Directory.Exists(Path.Combine(fileCandidate.Path.ToLowerInvariant(), "screenlists")))
                                                {
                                                    foreach (var file in Directory.EnumerateFiles(Path.Combine(fileCandidate.Path.ToLowerInvariant(), "screenlists")))
                                                    {
                                                        File.Move(file, Path.Combine(fileCandidate.Path, Path.GetFileName(file)));
                                                    }
                                                    Directory.Delete(Path.Combine(fileCandidate.Path.ToLowerInvariant(), "screenlists"));
                                                }
                                            }
                                            catch (Exception) { throw; }
                                            Directory.Move(fileCandidate.Path, Directory.GetParent(fileCandidate.Path).FullName + "\\" + fileCandidate.After);
                                            fileCandidate.Result = GlobalConst.RESULT_RENAME_OK;
                                        }
                                        catch (Exception)
                                        {
                                            try
                                            {
                                                FileSystem.MoveDirectory(fileCandidate.Path, Directory.GetParent(fileCandidate.Path).FullName + "\\" + fileCandidate.After, false /* Overwrite */);
                                                fileCandidate.Result = GlobalConst.RESULT_FOLDER_MERGED;
                                            }
                                            catch (Exception)
                                            {
                                                throw;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        fileCandidate.Result = GlobalConst.RESULT_NO_RENAME;
                                    }
                                }
                                else
                                {
                                    fileCandidate.Result = GlobalConst.RESULT_INVALID_NEW_FOLDERNAME;
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error(ex.ToString());
                                fileCandidate.Result = GlobalConst.RESULT_RENAME_FAIL;
                            }
                        }
                    }
                }
                for (int i = 0; i < RenameList.Count; i++)
                {
                    int index = Targets.IndexOf(Targets.Where(X => X.Path == RenameList[i].Path).FirstOrDefault());
                    Targets[index].Result = RenameList[i].Result;
                }
                RenameList.Clear();
                lView_TargetList.ItemsSource = Targets;
                lView_TargetList.Items.Refresh();
                btn_Undo.IsEnabled = true;
                tbx_TitleKeyword.Text = "";
                tbx_AnidbID.Text = "";
                tbx_GroupMembers.Text = "";
                tbx_Presenter.Text = "";
                MessageBox.Show("Rename Completed!");
            }
        }

        private void btn_Undo_Click(object sender, RoutedEventArgs e)
        {
            if (lView_TargetList.Items.Count > 0)
            {
                List<ItemToRename> UndoList = new List<ItemToRename>();
                foreach (ItemToRename vi in lView_TargetList.Items)
                {
                    ItemToRename ftr = new ItemToRename()
                    {
                        Path = vi.Path,
                        Before = vi.Before,
                        AlterKey = vi.AlterKey,
                        After = vi.After,
                        Result = vi.Result
                    };
                    UndoList.Add(ftr);
                }
                if (UndoList.Any())
                {
                    foreach (ItemToRename fileCandidate in UndoList)
                    {
                        if (string.IsNullOrEmpty(fileCandidate.After))
                        {
                            fileCandidate.Result = GlobalConst.EMPTY_STRING;
                            continue;
                        }
                        if (Directory.Exists(Directory.GetParent(fileCandidate.Path).FullName + "\\" + fileCandidate.After))
                        {
                            try
                            {
                                Directory.Move(Directory.GetParent(fileCandidate.Path).FullName + "\\" + fileCandidate.After, fileCandidate.Path);
                                fileCandidate.Result = GlobalConst.RESULT_UNDO_OK;
                            }
                            catch (Exception)
                            {
                                fileCandidate.Result = GlobalConst.RESULT_UNDO_FAIL;
                            }
                        }
                    }
                    for (int i = 0; i < UndoList.Count; i++)
                    {
                        int index = Targets.IndexOf(Targets.Where(X => X.Path == UndoList[i].Path).FirstOrDefault());
                        Targets[index].Result = UndoList[i].Result;
                    }
                }
                UndoList.Clear();
                lView_TargetList.Items.Refresh();
                btn_Undo.IsEnabled = false;
                tbx_TitleKeyword.Text = "";
                tbx_AnidbID.Text = "";
                tbx_GroupMembers.Text = "";
                tbx_Presenter.Text = "";
                MessageBox.Show("Undo Completed!");
            }
        }
        #region lView_TargetList
        private void lView_TargetList_Drop(object sender, DragEventArgs e)
        {
            foreach (string s in (string[])e.Data.GetData(DataFormats.FileDrop, false))
            {
                if (Directory.Exists(s))
                {
                    DirectoryInfo di = new DirectoryInfo(s);
                    if (Targets.IndexOf(Targets.Where(X => X.Path == di.FullName).FirstOrDefault()) < 0)
                        Targets.Add(new ItemToRename { Path = di.FullName, Before = di.Name, AlterKey = "", After = GlobalConst.EMPTY_STRING, Result = GlobalConst.EMPTY_STRING });

                }
            }
            if (Targets.Count > 0)
            {
                lbl_TargetCounts.Content = "No. of Items : " + Targets.Count;
                lView_TargetList.ItemsSource = Targets;
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lView_TargetList.ItemsSource);
                view.SortDescriptions.Add(new SortDescription("Path", ListSortDirection.Ascending));
                lView_TargetList.Items.Refresh();
            }
        }

        private void lView_TargetList_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void lView_TargetList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lView_TargetList.SelectedItems.Count > 0)
            {
                btn_Remove_Item.IsEnabled = true;
                if (lView_TargetList.SelectedItems.Count == 1)
                {
                    tbx_TitleKeyword.Text = string.IsNullOrEmpty(((ItemToRename)lView_TargetList.SelectedItems[0]).AlterKey) ?
                        GroupHandler.GetTitleKeyword(((ItemToRename)lView_TargetList.SelectedItems[0]).Before) : ((ItemToRename)lView_TargetList.SelectedItems[0]).AlterKey;

                    OriginalSearchWord = string.IsNullOrEmpty(tbx_TitleKeyword.Text) ? ((ItemToRename)lView_TargetList.SelectedItems[0]).Before : tbx_TitleKeyword.Text;
                    if (string.IsNullOrEmpty(tbx_TitleKeyword.Text)) tbx_TitleKeyword.Text = OriginalSearchWord;
                    AnidbResult ar = SearchMatchFromBothSources(OriginalSearchWord);
                    if (ar != null)
                    {
                        if (ar.aid == "xxx")
                        {
                            Group presetgroup = groups.Where(x => x.Presenter == ar.presenter).FirstOrDefault();
                            if (presetgroup != null)
                            {
                                lbl_GroupsMatch.Visibility = Visibility.Visible;
                                lView_Groups.SelectedItem = presetgroup;
                                lView_Groups.Items.Refresh();
                                lView_Groups.ScrollIntoView(presetgroup);
                            }
                        }
                        else
                        {
                            lbl_GroupsMatch.Visibility = Visibility.Hidden;
                            tbx_AnidbID.Text = ar.aid;
                        }
                        tbx_Presenter.Text = ar.presenter;
                        tbx_GroupMembers.Text = ar.keywords;
                    }
                    else
                    {
                        tbx_Presenter.Text = "";
                        tbx_GroupMembers.Text = "";
                    }
                }
            }
            else
            {
                btn_Remove_Item.IsEnabled = false;
            }
        }

        private void lView_TargetList_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.None;
                return;
            }

            // Set the effect based upon the KeyState.
            if ((e.KeyStates & GlobalConst.KEY_SHIFT) == GlobalConst.KEY_SHIFT &&
                (e.AllowedEffects & DragDropEffects.Move) == DragDropEffects.Move)
            {
                e.Effects = DragDropEffects.Move;

            }
            else if ((e.KeyStates & GlobalConst.KEY_CTRL) == GlobalConst.KEY_CTRL &&
                (e.AllowedEffects & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                e.Effects = DragDropEffects.Copy;
            }
            else if ((e.AllowedEffects & DragDropEffects.Move) == DragDropEffects.Move)
            {
                // By default, the drop action should be move, if allowed.
                e.Effects = DragDropEffects.Move;

                //switch (cbox_TargetType.SelectedItem.ToString())
                //{
                //    case GlobalConst.TARGETTYPE_FOLDERNAME:
                string[] folders = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (folders.Length > 0 && (e.AllowedEffects & DragDropEffects.Copy) == DragDropEffects.Copy)
                    e.Effects = DragDropEffects.Copy;
                //        break;
                //    default:
                //        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                //        if (files.Length > 0 && (e.AllowedEffects & DragDropEffects.Copy) == DragDropEffects.Copy)
                //            e.Effects = DragDropEffects.Copy;
                //        break;
                //}
            }
            else
                e.Effects = DragDropEffects.None;

            // This is an example of how to get the item under the mouse
            //Point pt = lView_TargetList.PointToClient(new Point(e.X, e.Y));
            //ListViewItem itemUnder = lView_TargetList.GetItemAt(pt.X, pt.Y);
        }

        private void btn_Reset_Filelist_Click(object sender, RoutedEventArgs e)
        {
            Targets.Clear();
            lbl_TargetCounts.Content = "No. of Items : 0";
            lView_TargetList.ItemsSource = null;
            lView_TargetList.Items.Clear();
            lView_TargetList.Items.Refresh();
            listView_CollectionChanged(lView_Groups, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            btn_Remove_Item.IsEnabled = false;
            btn_Undo.IsEnabled = false;
            tbx_TitleKeyword.Text = "";
            tbx_AnidbID.Text = "";
            tbx_GroupMembers.Text = "";
            tbx_Presenter.Text = "";
        }

        private void btn_Remove_Item_Click(object sender, RoutedEventArgs e)
        {

            if (lView_TargetList.SelectedItems.Count != 0)
            {
                IEditableCollectionView items = lView_TargetList.Items;
                while (lView_TargetList.SelectedItems.Count != 0)
                {
                    if (items.CanRemove)
                    {
                        items.Remove(lView_TargetList.SelectedItems[0]);
                        tbx_TitleKeyword.Text = "";
                        tbx_AnidbID.Text = "";
                        tbx_GroupMembers.Text = "";
                        tbx_Presenter.Text = "";
                    }
                }
            }
            lbl_TargetCounts.Content = "No. of Items : " + Targets.Count;
        }
        #endregion

        #region lView_Groups
        private void lView_Groups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (lView_Groups.SelectedItems.Count == 0)
            {
                btn_UpdateGroup.IsEnabled = false;
                //btn_MoveUp.IsEnabled = false;
                //btn_MoveDn.IsEnabled = false;
                btn_RemoveGroup.IsEnabled = false;
                return;
            }
            else if (lView_Groups.SelectedItems.Count == 1)
            {
                tbx_GroupMembers.Text = ((Group)lView_Groups.SelectedItem).Members;
                tbx_Presenter.Text = ((Group)lView_Groups.SelectedItem).Presenter;
                tbx_AnidbID.Text = ((Group)lView_Groups.SelectedItem).AnidbId;
                btn_UpdateGroup.IsEnabled = true;
                btn_RemoveGroup.IsEnabled = true;
                //lView_TargetList.SelectedItem = null;
                if (lView_TargetList.SelectedItem == null)
                    btn_SetAlterKey.IsEnabled = false;
            }
        }

        private void btn_AddGroup_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbx_GroupMembers.Text) && !string.IsNullOrEmpty(tbx_Presenter.Text))
            {
                string presenter = tbx_Presenter.Text.Replace(":", "：").Replace("/", "／").Replace("?", "？").Replace(",", "][").Trim();
                string members = tbx_GroupMembers.Text.Trim();
                if (!string.IsNullOrEmpty(OriginalSearchWord) && OriginalSearchWord != tbx_TitleKeyword.Text && cbox_AddKeywordToGroup.IsChecked == true)
                {
                    members = members + "," + OriginalSearchWord;
                }
                if (presenter.LastIndexOf(',') == presenter.Length - 1)
                {
                    presenter = presenter.Substring(0, presenter.Length - 2);
                }
                groups.Add(new Group
                {
                    Enable = true,
                    AnidbId = tbx_AnidbID.Text,
                    Presenter = "[" + presenter + "]",
                    Members = members
                });
                groups = new ObservableCollection<Group>(groups.OrderBy(i => i.Presenter));
                lView_Groups.ItemsSource = groups;
                lView_Groups.Items.Refresh();
                tbx_Presenter.Text = "";
                tbx_GroupMembers.Text = "";
                tbx_TitleKeyword.Text = "";
                tbx_AnidbID.Text = "";
                cbox_AddKeywordToGroup.IsChecked = false;
            }
        }

        private void btn_UpdateGroup_Click(object sender, RoutedEventArgs e)
        {
            if (lView_Groups.SelectedItems.Count == 1
                && !string.IsNullOrEmpty(tbx_AnidbID.Text)
                && !string.IsNullOrEmpty(tbx_Presenter.Text)
                && !string.IsNullOrEmpty(tbx_GroupMembers.Text))
            {
                ((Group)lView_Groups.SelectedItem).AnidbId = tbx_AnidbID.Text;
                ((Group)lView_Groups.SelectedItem).Members = tbx_GroupMembers.Text;
                ((Group)lView_Groups.SelectedItem).Presenter = tbx_Presenter.Text;
                btn_UpdateGroup.IsEnabled = false;
                groups = new ObservableCollection<Group>(groups.OrderBy(i => i.Presenter));
                lView_Groups.ItemsSource = groups;
                lView_Groups.Items.Refresh();
                tbx_GroupMembers.Text = "";
                tbx_Presenter.Text = "";
                tbx_AnidbID.Text = "";
            }
        }

        private void btn_MoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (lView_Groups.SelectedItems.Count == 1)
            {
                int seletedIndex = lView_Groups.SelectedIndex;
                MoveItems(lView_Groups, MoveDirection.Up);
                if (seletedIndex > 0)
                {
                    lView_Groups.SelectedIndex = seletedIndex - 1;
                }
                else
                {
                    lView_Groups.SelectedIndex = seletedIndex;
                }
                lView_Groups.Focus();
            }
        }

        private void btn_MoveDn_Click(object sender, RoutedEventArgs e)
        {
            if (lView_Groups.SelectedItems.Count == 1)
            {
                int seletedIndex = lView_Groups.SelectedIndex;
                MoveItems(lView_Groups, MoveDirection.Down);
                if (seletedIndex > 0 && seletedIndex < lView_Groups.Items.Count - 1)
                {
                    lView_Groups.SelectedIndex = seletedIndex + 1;
                }
                else
                {
                    lView_Groups.SelectedIndex = seletedIndex;
                }
                lView_Groups.Focus();
            }
        }

        private void btn_ResetGroups_Click(object sender, RoutedEventArgs e)
        {
            lView_Groups.ItemsSource = null;
            groups.Clear();
            lView_Groups.Items.Clear();
            lView_Groups.Items.Refresh();
            listView_CollectionChanged(lView_Groups, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void btn_RemoveGroup_Click(object sender, RoutedEventArgs e)
        {
            IEditableCollectionView items = lView_Groups.Items; //Cast to interface
            if (items.CanRemove)
            {
                items.Remove(lView_Groups.SelectedItem);
            }
            if (lView_Groups.Items.Count == 0)
            {
                listView_CollectionChanged(lView_Groups, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        private void SaveGroupSettings()
        {
            List<Group> Itemlist = lView_Groups.Items.OfType<Group>().ToList();
            //List<Group> Itemlist = new List<Group>();
            //foreach (Group vi in lView_Groups.Items)
            //{
            //    Itemlist.Add(vi);
            //}
            if (Itemlist.Any())
            {
                //Itemlist = Itemlist.OrderBy(i => i.Presenter).ToList();
                XMLHelper.SaveProfileXML(Itemlist, @"GroupSetting.xml");
            }
        }

        private void listView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //The projects ListView has been changed
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (lView_TargetList.Items.Count > 0)
                    {
                        btn_Preview.IsEnabled = true;
                        btn_GO.IsEnabled = true;
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    if (lView_TargetList.Items.Count == 0)
                    {
                        btn_Preview.IsEnabled = false;
                        btn_GO.IsEnabled = false;
                        btn_Reset_Filelist.IsEnabled = false;
                    }
                    else
                    {
                        btn_Preview.IsEnabled = true;
                        btn_GO.IsEnabled = true;
                        btn_Reset_Filelist.IsEnabled = true;

                    }
                    break;
            }
            SaveGroupSettings();
        }
        private void MoveItems(ListView sender, MoveDirection direction)
        {
            Group selectedfile = lView_Groups.SelectedItem as Group;
            int index = groups.IndexOf(selectedfile);
            bool valid = sender.SelectedItems.Count > 0 &&
                        ((direction == MoveDirection.Down && (index < sender.Items.Count - 1))
                        || (direction == MoveDirection.Up && (sender.SelectedIndex > 0)));

            if (valid)
            {
                if (direction == MoveDirection.Up)
                {
                    if (index > 0)
                    {
                        groups.Remove(selectedfile);
                        groups.Insert(index - 1, selectedfile);
                    }
                }
                else
                {
                    if (index < groups.Count - 1)
                    {
                        groups.Remove(selectedfile);
                        groups.Insert(index + 1, selectedfile);
                    }
                }
                lView_Groups.ItemsSource = null;
                lView_Groups.ItemsSource = groups;
                lView_Groups.Items.Refresh();
                lView_Groups.SelectedItem = selectedfile;
            }
        }
        #endregion

        public MainWindow()
        {
            try
            {
                //log4net.Config.XmlConfigurator.Configure();
                InitializeComponent();
                this.Title = this.Title + " v" + System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
                //log4net.GlobalContext.Properties["pid"] = Process.GetCurrentProcess().Id; 
                btn_Undo.IsEnabled = false;
                btn_UpdateGroup.IsEnabled = false;
                btn_RemoveGroup.IsEnabled = false;
                btn_Remove_Item.IsEnabled = false;
                btn_Preview.IsEnabled = false;
                btn_GO.IsEnabled = false;
                btn_Reset_Filelist.IsEnabled = false;
                ((INotifyCollectionChanged)lView_Groups.Items).CollectionChanged += listView_CollectionChanged;
                ((INotifyCollectionChanged)lView_TargetList.Items).CollectionChanged += listView_CollectionChanged;
                groups = XMLHelper.LoadGroupXML(@"GroupSetting.xml");
                if (groups != null)
                    if (groups.Count > 0) lView_Groups.ItemsSource = groups;

                CheckAniDBSource();
            }
            catch (Exception ex)
            {
                log.Error(ex);
                MessageBox.Show(ex.ToString());
            }
        }

        private void CheckAniDBSource()
        {
            FileInfo fi = new FileInfo(@"anime-titles.xml");
            DateTime sourcedate = fi.CreationTime;
            if (fi.LastWriteTime > sourcedate)
            {
                sourcedate = fi.LastWriteTime;
            }
            TimeSpan span = DateTime.Now.Subtract(sourcedate);
            if (span.TotalDays > 1)
            {
                log.Info("The anidb-titles.xml is " + span.TotalDays + " days(s) old, time for an update.");
                if (File.Exists(@"anime-titles.bak"))
                {
                    try
                    {
                        FileSystem.DeleteFile(@"anime-titles.bak", UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Trying to delete anime-titles.bak result in error.", ex);
                    }
                }
                if (File.Exists(@"anime-titles.xml"))
                {
                    try
                    {
                        File.Move(@"anime-titles.xml", @"anime-titles.bak");
                    }
                    catch (Exception ex)
                    {
                        log.Error("Trying to rename anime-titles.xml to anime-titles.bak result in error.", ex);
                    }
                }
                try
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile("http://anidb.net/api/anime-titles.xml.gz", "anime-titles.xml.gz");
                        FileInfo updategz = new FileInfo(@"anime-titles.xml.gz");
                        using (FileStream originalFileStream = updategz.OpenRead())
                        {
                            string currentFileName = updategz.FullName;
                            string newFileName = currentFileName.Remove(currentFileName.Length - updategz.Extension.Length);

                            using (FileStream decompressedFileStream = File.Create(newFileName))
                            {
                                using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                                {
                                    decompressionStream.CopyTo(decompressedFileStream);
                                    Console.WriteLine("Decompressed: {0}", updategz.Name);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Trying to download anime-titles.xml from anidb.net result in error.", ex);
                    throw;
                }
            }
            else
            {
                log.Info("The anime-titles.xml is still fresh.");
            }
            anititles = XMLHelper.LoadAnimeTitlesXML(@"anime-titles.xml");
            if (File.Exists(@"anime-titles.xml.gz"))
            {
                try
                {
                    FileSystem.DeleteFile(@"anime-titles.xml.gz", UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }
                catch (Exception ex)
                {
                    log.Error("Trying to delete anime-titles.xml.gz result in error.", ex);
                }
            }
        }

        private void tbx_TitleKeyword_KeyUp(object sender, KeyEventArgs e)
        {
            if (e != null)
                if (e.Key == Key.Escape)
                {
                    tbx_TitleKeyword.Text = string.Empty;
                    return;
                }

            AnidbResult ar = SearchMatchFromBothSources(tbx_TitleKeyword.Text);
            if (ar != null)
            {
                if (ar.aid == "xxx")
                {
                    Group presetgroup = groups.Where(x => x.Presenter == ar.presenter).FirstOrDefault();
                    if (presetgroup != null)
                    {
                        lbl_GroupsMatch.Visibility = Visibility.Visible;
                        lView_Groups.SelectedItem = presetgroup;
                        lView_Groups.Items.Refresh();
                        lView_Groups.ScrollIntoView(presetgroup);
                    }
                }
                else
                {
                    lbl_GroupsMatch.Visibility = Visibility.Hidden;
                    tbx_AnidbID.Text = ar.aid;
                }
                tbx_GroupMembers.Text = ar.keywords;
                tbx_Presenter.Text = ar.presenter;
            }
            else
            {
                lbl_GroupsMatch.Visibility = Visibility.Hidden;
                tbx_Presenter.Text = "";
            }
        }

        private void btn_SetAlterKey_Click(object sender, RoutedEventArgs e)
        {
            //if (!string.IsNullOrEmpty(tbx_TitleKeyword.Text) && lView_TargetList.SelectedItems.Count == 1)
            //{
            //    ((ItemToRename)lView_TargetList.SelectedItem).AlterKey = tbx_TitleKeyword.Text.Trim(); //.Replace(":", "：")
            //    btn_SetAlterKey.IsEnabled = false;
            //    lView_TargetList.Items.Refresh();
            //}

            if (!string.IsNullOrEmpty(tbx_TitleKeyword.Text) && lView_TargetList.SelectedItems.Count > 0)
            {
                foreach (ItemToRename item in lView_TargetList.SelectedItems)
                {
                    item.AlterKey = tbx_TitleKeyword.Text.Trim(); //.Replace(":", "：")
                }
                btn_SetAlterKey.IsEnabled = false;
                lView_TargetList.Items.Refresh();
            }
        }

        private void tbx_TitleKeyword_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbx_TitleKeyword.Text))
            {
                btn_SetAlterKey.IsEnabled = true;
                //cbox_AddKeywordToGroup.IsChecked = true;
                tbx_TitleKeyword_KeyUp(null, null);
            }
            else
            {
                btn_SetAlterKey.IsEnabled = false;
                cbox_AddKeywordToGroup.IsChecked = false;
            }
        }

        private void tbx_AnidbID_KeyUp(object sender, KeyEventArgs e)
        {
            AnidbResult ar = GroupHandler.SearchAniDBByID(anititles, tbx_AnidbID.Text.Trim());
            if (ar != null)
            {
                tbx_GroupMembers.Text = ar.keywords;
                tbx_Presenter.Text = ar.presenter;
                btn_SetKeywordKey.IsEnabled = true;
            }
            else
            {
                tbx_Presenter.Text = "";
                btn_SetKeywordKey.IsEnabled = false;
            }
        }

        private void lView_Groups_KeyUp(object sender, KeyEventArgs e)
        {
            //string CurrentKeypress = "[" + e.Key.ToString().ToLowerInvariant();
            //bool CurrentSelectedPattern = ((Group)lView_Groups.SelectedItem).Presenter.ToLowerInvariant().StartsWith(CurrentKeypress);
            //var item = groups.Where(x => x.Presenter.ToLowerInvariant().StartsWith(CurrentKeypress)).FirstOrDefault();
            //if (CurrentSelectedPattern)
            //{
            //    if (!((Group)lView_Groups.Items[lView_Groups.SelectedIndex + 1]).Presenter.ToLowerInvariant().StartsWith(CurrentKeypress))
            //    {
            //        lView_Groups.SelectedItem = item;
            //        lView_Groups.ScrollIntoView(item);
            //    }
            //    else
            //    {
            //        lView_Groups.SelectedIndex = lView_Groups.SelectedIndex + 1;
            //        lView_Groups.ScrollIntoView(lView_Groups.Items[lView_Groups.SelectedIndex + 1]);
            //    }
            //}
            //else
            //{
            //    if (item != null)
            //    {
            //        lView_Groups.SelectedItem = item;
            //        //int firstindex = lView_Groups.SelectedIndex;
            //        //if (LastKeyStrokeOnGroups == e.Key)
            //        //{
            //        //    lView_Groups.ScrollIntoView(lView_Groups.Items[firstindex + 1]);
            //        //}
            //        //else
            //        //{
            //        lView_Groups.ScrollIntoView(item);
            //        //}
            //    }
            //}
            //LastKeyStrokeOnGroups = e.Key;
        }

        private void lView_Groups_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F)
            {
                e.Handled = true;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HitTestResult r = VisualTreeHelper.HitTest(this, e.GetPosition(this));
            if (r.VisualHit.GetType() != typeof(ListBoxItem))
                lView_Groups.UnselectAll();
        }

        private void btn_GetOriginalAsKeyword_Click(object sender, RoutedEventArgs e)
        {
            if (lView_TargetList.SelectedIndex != -1)
            {
                tbx_TitleKeyword.Text = ((ItemToRename)lView_TargetList.SelectedItem).Before;
            }
        }
        private AnidbResult SearchMatchFromBothSources(string keyword)
        {
            AnidbResult ar = new AnidbResult();
            if (!string.IsNullOrEmpty(keyword))
            {
                if (keyword.Trim().ToLowerInvariant().StartsWith("anidb-") && keyword.Trim().Length > 6)
                {
                    ar = GroupHandler.SearchGroups(groups, keyword.Trim(), true);
                }
                else
                {
                    ar = GroupHandler.SearchGroups(groups, keyword.Trim(), false);
                }
                if (ar != null)
                {
                    return ar;
                }
                else
                {
                    if (keyword.Trim().ToLowerInvariant().StartsWith("anidb-") && keyword.Trim().Length > 6)
                    {
                        ar = GroupHandler.SearchAniDB(anititles, keyword.Trim(), true);
                    }
                    else
                    {
                        ar = GroupHandler.SearchAniDB(anititles, keyword.Trim(), false);
                    }
                    if (ar != null)
                    {
                        return ar;
                    }
                }
            }
            return null;
        }

        private void btn_SetKeyWordKey_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbx_AnidbID.Text.Trim()))
            {
                if (int.TryParse(tbx_AnidbID.Text.Trim(), out int tryit))
                {
                    tbx_TitleKeyword.Text = "anidb-" + tbx_AnidbID.Text.Trim();
                }
            }
        }

        private void tbx_AnidbID_TextChanged(object sender, TextChangedEventArgs e)
        {
            btn_SetKeywordKey.IsEnabled = true;
        }

        private void tbx_SearchGroups_TextChanged(object sender, TextChangedEventArgs e)
        {
            string CurrentKeypress = "[" + tbx_SearchGroups.Text.ToLowerInvariant();
            bool CurrentSelectedPattern = false;

            string prepped_keyword = Utilities.Prepare_Keyword(tbx_SearchGroups.Text.ToLowerInvariant());

            Group item = null;
            item = groups.Where(x => x.Presenter.ToLowerInvariant().StartsWith(prepped_keyword)).FirstOrDefault();
            if (item == null)
                item = groups.Where(x => x.Presenter.ToLowerInvariant().StartsWith(CurrentKeypress)).FirstOrDefault();
            if (CurrentSelectedPattern)
            {
                if (!((Group)lView_Groups.Items[lView_Groups.SelectedIndex + 1]).Presenter.ToLowerInvariant().StartsWith(CurrentKeypress))
                {
                    lView_Groups.SelectedItem = item;
                    lView_Groups.ScrollIntoView(item);
                }
                else
                {
                    lView_Groups.SelectedIndex = lView_Groups.SelectedIndex + 1;
                    lView_Groups.ScrollIntoView(lView_Groups.Items[lView_Groups.SelectedIndex + 1]);
                }
            }
            else
            {
                if (item != null)
                {
                    lView_Groups.SelectedItem = item;
                    //int firstindex = lView_Groups.SelectedIndex;
                    //if (LastKeyStrokeOnGroups == e.Key)
                    //{
                    //    lView_Groups.ScrollIntoView(lView_Groups.Items[firstindex + 1]);
                    //}
                    //else
                    //{
                    lView_Groups.ScrollIntoView(item);
                    //}
                }
            }
        }

        private void btn_ResetSearch_Click(object sender, RoutedEventArgs e)
        {
            tbx_SearchGroups.Text = null;
        }

        private void tbx_Presenter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbx_Presenter.Text))
            {
                lbl_PresenterLength.Content = System.Text.Encoding.UTF8.GetBytes(tbx_Presenter.Text).Length.ToString() + " chars";
            }
            else
            {
                if (lbl_PresenterLength != null)
                    lbl_PresenterLength.Content = "";
            }
        }

        private void tbx_SearchGroups_KeyUp(object sender, KeyEventArgs e)
        {
            if (e != null)
                if (e.Key == Key.Escape)
                {
                    tbx_SearchGroups.Text = string.Empty;
                    return;
                }
        }
    }
}
