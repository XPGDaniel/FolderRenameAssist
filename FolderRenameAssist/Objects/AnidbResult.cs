using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FolderRenameAssist.Objects
{
    public class AnidbResult : ObservableCollection<AnidbResult>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler ItemPropertyChanged;
        string m_aid;
        string m_presenter;
        string m_keywords;
        public string aid
        {
            get { return m_aid; }
            set
            {
                if (m_aid != value)
                {
                    m_aid = value;
                    OnPropertyChanged("aid");
                }
            }
        }
        public string presenter
        {
            get { return m_presenter; }
            set
            {
                if (m_presenter != value)
                {
                    m_presenter = value;
                    OnPropertyChanged("presenter");
                }
            }
        }
        public string keywords
        {
            get { return m_keywords; }
            set
            {
                if (m_keywords != value)
                {
                    m_keywords = value;
                    OnPropertyChanged("keywords");
                }
            }
        }
        
        void OnPropertyChanged(string propertyName)
        {
            ItemPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
