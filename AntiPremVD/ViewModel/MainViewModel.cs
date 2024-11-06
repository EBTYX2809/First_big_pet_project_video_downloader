using AntiPremVD.Model;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Input;
using System.Threading.Tasks;

namespace AntiPremVD.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // ViewModels //////////////////////////////////////////////////////////////////////////////
        private readonly VideoInfoViewModel _videoInfoViewModel;
        public VideoInfoViewModel VideoInfoViewModel => _videoInfoViewModel;

        // Services ////////////////////////////////////////////////////////////////////////////////
        private readonly VideoListService _videoListService;
        private readonly JsonParser _jsonParser;
        public JsonParser JsonParser => _jsonParser;

        // Properties //////////////////////////////////////////////////////////////////////////////
        private string _downloadPath;
        public string DownloadPath
        {
            get => _downloadPath;
            set
            {
                if (_downloadPath != value)
                {
                    _downloadPath = value;
                    OnPropertyChanged();
                    _videoInfoViewModel.GiveDownloadPathToVideoItemsViewModel(DownloadPath);
                }
            }
        }
        private string _videoUrl;
        public string VideoUrl
        {
            get => _videoUrl;
            set
            {
                if (_videoUrl != value)
                {
                    _videoUrl = value;
                    OnPropertyChanged();
                    IsPlaceholderVisible = string.IsNullOrEmpty(_videoUrl);

                    if (_videoUrl.Contains("http"))
                    {
                        GiveUrlToVideoInfoViewModel();                        
                    }
                }
            }
        }
        private bool _isPlaceholderVisible;
        public bool IsPlaceholderVisible
        {
            get => _isPlaceholderVisible;
            set
            {
                if (_isPlaceholderVisible != value)
                {
                    _isPlaceholderVisible = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool _folderToggleButton;
        public bool FolderToggleButton
        {
            get => _folderToggleButton;
            set
            {
                if (_folderToggleButton != value)
                {
                    _folderToggleButton = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool _settingsToggleButton;
        public bool SettingsToggleButton
        {
            get => _settingsToggleButton;
            set
            {
                if (_settingsToggleButton != value)
                {
                    _settingsToggleButton = value;
                    OnPropertyChanged();
                }
            }
        }

        // Commands ////////////////////////////////////////////////////////////////////////////////
        public ICommand ClearVideosCommand { get; }
        public ICommand ChangeDownloadPathCommand { get; }
        public ICommand SettingsCommand { get; }

        // Constructors ////////////////////////////////////////////////////////////////////////////
        public MainViewModel() { }
        public MainViewModel(VideoInfoViewModel videoInfoViewModel, VideoListService videoListService, JsonParser jsonParser)
        {
            _videoInfoViewModel = videoInfoViewModel;
            _videoListService = videoListService;
            _jsonParser = jsonParser;

            ClearVideosCommand = new RelayCommand(ClearVideoItems);
            ChangeDownloadPathCommand = new RelayCommand(SelectFolder);
            SettingsCommand = new RelayCommand(Settings);

            _videoInfoViewModel.IsVideoInContentControlChanged += _videoInfoViewModel_IsVideoInContentControlChanged;

            DownloadPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            IsPlaceholderVisible = true;
        }

        // Methods /////////////////////////////////////////////////////////////////////////////////
        private void _videoInfoViewModel_IsVideoInContentControlChanged(object sender, EventArgs e)
        {
            VideoUrl = "";
        }
        public async void GiveUrlToVideoInfoViewModel()
        {
            string url = VideoUrl;            
            LoadingUrl();
            await _videoInfoViewModel.LoadVideoInfo(url);
            VideoUrl = "Select parameters";
        }
        private void ClearVideoItems(object parameter)
        {
            _videoListService.ClearVideoList();
            _videoInfoViewModel.CloseVideo(parameter);
        }
        private void SelectFolder(object parameter)
        {
            if (FolderToggleButton)
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    dialog.Description = "Select folder for downloads.";
                    dialog.ShowNewFolderButton = true;

                    var result = dialog.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        DownloadPath = dialog.SelectedPath;
                    }

                    FolderToggleButton = !FolderToggleButton;
                }
            }

        }
        private void Settings(object parameter)
        {
            if (!SettingsToggleButton) JsonParser.SetLanguage("RU");
            else JsonParser.SetLanguage("EN");
            SettingsToggleButton = !SettingsToggleButton;
        }
        private async void LoadingUrl()
        {
            VideoUrl = "Loading";
            while (!_videoInfoViewModel.IsVideoInContentControl)
            {
                if(VideoUrl.Contains("..."))
                {
                    VideoUrl = "Loading";
                }
                else
                {
                    VideoUrl += '.';
                }
                await Task.Delay(500);
            }
        }

        // PropertyChanged /////////////////////////////////////////////////////////////////////////
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}