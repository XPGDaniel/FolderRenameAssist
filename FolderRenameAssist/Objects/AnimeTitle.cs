using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FolderRenameAssist.Objects
{
    public class AnimeTitle : ObservableCollection<AnimeTitle>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler GroupPropertyChanged;
        int m_aid;
        string m_type;
        string m_lang;
        string m_title;
        public int aid
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
        public string type
        {
            get { return m_type; }
            set
            {
                if (m_type != value)
                {
                    m_type = value;
                    OnPropertyChanged("type");
                }
            }
        }
        public string lang
        {
            get { return m_lang; }
            set
            {
                if (m_lang != value)
                {
                    m_lang = value;
                    OnPropertyChanged("lang");
                }
            }
        }
        public string title
        {
            get { return m_title; }
            set
            {
                if (m_title != value)
                {
                    m_title = value;
                    OnPropertyChanged("title");
                }
            }
        }

        void OnPropertyChanged(string propertyName)
        {
            GroupPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
