using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Windows.UI;
using Windows.UI.Xaml.Media;
using ConnectSdk.Demo.Annotations;

namespace ConnectSdk.Demo.Demo
{
    public class TvDefinition : INotifyPropertyChanged
    {
        private string name;
        private string ip;
        private string url;
        private string modelName;
        private string usn;
        private string key;
        private bool available;
        private string availableText;

        public string Name
        {
            get { return name; }
            set
            {
                if (value == name) return;
                name = value;
                OnPropertyChanged();
            }
        }

        public string Ip
        {
            get { return ip; }
            set
            {
                if (value == ip) return;
                ip = value;
                OnPropertyChanged();
            }
        }

        public string Url
        {
            get { return url; }
            set
            {
                if (value == url) return;
                url = value;
                OnPropertyChanged();
            }
        }

        public string ModelName
        {
            get { return modelName; }
            set
            {
                if (value == modelName) return;
                modelName = value;
                OnPropertyChanged();
            }
        }

        public string Usn
        {
            get { return usn; }
            set
            {
                if (value == usn) return;
                usn = value;
                OnPropertyChanged();
            }
        }

        public string Key
        {
            get { return key; }
            set
            {
                if (value == key) return;
                key = value;
                OnPropertyChanged();
            }
        }

        public string BaseUrl
        {
            get { return Url.Substring(0, Url.IndexOf("/", 10, System.StringComparison.Ordinal)); }
        }

        public bool Available
        {
            get { return available; }
            set
            {
                if (value.Equals(available)) return;
                available = value;
                OnPropertyChanged();

            }
        }

        public string AvailableText
        {
            get { return availableText; }
            set
            {
                if (value == availableText) return;
                availableText = value;
                OnPropertyChanged();
                OnPropertyChanged("AvailableColor");
            }
        }

        [XmlIgnore]
        public SolidColorBrush AvailableColor
        {
            get
            {
                if (availableText.Equals("Available"))
                    return new SolidColorBrush(Color.FromArgb(255, 144, 238, 144));
                return new SolidColorBrush(Colors.Red);
            }
        }

        public string RawDescription { get; set; }

        public string RawAdditionaInfo { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}