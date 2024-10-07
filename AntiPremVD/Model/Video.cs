using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AntiPremVD.Model
{
    public class Video : INotifyPropertyChanged
    {
        public Video(string url = "",
                     string title = "",
                     string duration = "",
                     string previewImg = "",
                     string views = "",
                     DownloadItem downloadInfo = null)
        {
            URL = url;
            Title = title;
            Duration = duration;
            PreviewImg = previewImg;
            Views = views;
            DownloadInfo = downloadInfo ?? new DownloadItem();
        }
        private string _url;
        private string _title;
        private string _duration;
        private string _previewImg;
        private string _views;
        public DownloadItem DownloadInfo { get; set; }

        public string URL
        {
            get => _url;
            set
            {
                if (_url != value)
                {
                    _url = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Duration
        {
            get => _duration;
            set
            {
                if (_duration != value)
                {
                    _duration = value;
                    OnPropertyChanged();
                }
            }
        }

        public string PreviewImg
        {
            get => _previewImg;
            set
            {
                if (_previewImg != value)
                {
                    _previewImg = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Views
        {
            get => _views;
            set
            {
                if (_views != value)
                {
                    _views = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}