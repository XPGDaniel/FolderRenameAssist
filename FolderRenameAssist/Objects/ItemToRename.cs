using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FolderRenameAssist.Objects
{
    public class ItemToRename : ObservableCollection<ItemToRename>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler ItemPropertyChanged;
        string m_Path;
        string m_Before;
        string m_AlterKey;
        string m_After;
        string m_Result;
        public string Path
        {
            get { return m_Path; }
            set
            {
                if (m_Path != value)
                {
                    m_Path = value;
                    OnPropertyChanged("Path");
                }
            }
        }
        public string Before
        {
            get { return m_Before; }
            set
            {
                if (m_Before != value)
                {
                    m_Before = value;
                    OnPropertyChanged("Before");
                }
            }
        }
        public string AlterKey
        {
            get { return m_AlterKey; }
            set
            {
                if (m_AlterKey != value)
                {
                    m_AlterKey = value;
                    OnPropertyChanged("AlterKey");
                }
            }
        }
        public string After
        {
            get { return m_After; }
            set
            {
                if (m_After != value)
                {
                    m_After = value;
                    OnPropertyChanged("After");
                }
            }
        }
        public string Result
        {
            get { return m_Result; }
            set
            {
                if (m_Result != value)
                {
                    m_Result = value;
                    OnPropertyChanged("Result");
                }
            }
        }
        void OnPropertyChanged(string propertyName)
        {
            ItemPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
