using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AntiPremVD.Model
{
    public class DownloadItem : INotifyPropertyChanged
    {
        public DownloadItem(string format = null,
                            string type = "video",
                            string selectedQuality = "",
                            bool isDownloaded = false,
                            double progress = 0,
                            Dictionary<string, string> qualities = null,
                            Dictionary<string, string> sizes = null,
                            ObservableCollection<string> qualityPanelList = null)
        {
            Format = format;
            Type = type;
            SelectedQuality = selectedQuality;
            _isDownloaded = isDownloaded;
            _progress = progress;            

            Qualities = qualities ?? new Dictionary<string, string>();
            Sizes = sizes ?? new Dictionary<string, string>();

            QualityPanelList = qualityPanelList ?? new ObservableCollection<string>();
            tempQualityPanelList = QualityPanelList ?? new ObservableCollection<string>();

            if (!string.IsNullOrEmpty(_selectedQuality) && Sizes.ContainsKey(_selectedQuality))
            {
                ActualSize = Sizes[_selectedQuality];
            }
        }
        private string _type;
        private string _format;
        private bool _isDownloaded;
        private double _progress;
        private string _selectedQuality;
        private string _actualSize;

        public TaskCompletionSource<bool> QualitiesLoaded { get; } = new TaskCompletionSource<bool>();
        private ObservableCollection<string> tempQualityPanelList;
        private ObservableCollection<string> _qualityPanelList;
        public ObservableCollection<string> QualityPanelList
        {
            get => _qualityPanelList;
            set
            {
                if (_qualityPanelList != value)
                {
                    _qualityPanelList = value;
                    OnPropertyChanged();
                }
            }
        }
        public Dictionary<string, string> Sizes { get; set; }
        private Dictionary<string, string> _qualities;
        public Dictionary<string, string> Qualities
        {
            get => _qualities;
            set
            {
                if (_qualities != value)
                {
                    _qualities = value;
                }
            }
        }
        public string Type // Need to work this
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;
                    OnPropertyChanged();
                    if (Type == "audio")
                    {
                        QualityPanelList = new ObservableCollection<string> { "64Kbit/s", "128Kbit/s" };
                        Format = "opus"; 
                    }
                    else if (Type == "video")
                    {
                        QualityPanelList = tempQualityPanelList;
                        Format = "webm";
                    }
                }
            }
        }

        public string SelectedQuality
        {
            get => _selectedQuality;
            set
            {
                if (_selectedQuality != value)
                {
                    _selectedQuality = value;
                    OnPropertyChanged();
                    SizeChanger();
                }
            }
        }

        public string Format
        {
            get => _format;
            set
            {
                if (_format != value)
                {
                    _format = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ActualSize
        {
            get => _actualSize;
            set
            {
                if (_actualSize != value)
                {
                    _actualSize = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsDownloaded
        {
            get => _isDownloaded;
            set
            {
                if (_isDownloaded != value)
                {
                    _isDownloaded = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnPropertyChanged();
                    if (_progress == 100)
                    {
                        IsDownloaded = true;
                    }
                }
            }
        }

        private void SizeChanger()
        {
            ActualSize = Sizes?[SelectedQuality];
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
