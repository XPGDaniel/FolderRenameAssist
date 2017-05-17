using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FolderRenameAssist.Objects
{
    public class Group : ObservableCollection<Group>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler GroupPropertyChanged;
        bool m_Enable;
        string m_Presenter;
        string m_Members;
        public bool Enable
        {
            get { return m_Enable; }
            set
            {
                if (m_Enable != value)
                {
                    m_Enable = value;
                    OnPropertyChanged("Enable");
                }
            }
        }
        public string Presenter
        {
            get { return m_Presenter; }
            set
            {
                if (m_Presenter != value)
                {
                    m_Presenter = value;
                    OnPropertyChanged("Presenter");
                }
            }
        }
        public string Members
        {
            get { return m_Members; }
            set
            {
                if (m_Members != value)
                {
                    m_Members = value;
                    OnPropertyChanged("Members");
                }
            }
        }

        void OnPropertyChanged(string propertyName)
        {
            GroupPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
