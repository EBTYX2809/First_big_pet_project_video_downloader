using AntiPremVD.Model;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AntiPremVD.ViewModel
{
    public class VideoInfoViewModel : INotifyPropertyChanged
    {
        // ViewModels //////////////////////////////////////////////////////////////////////////////
        private readonly VideoItemsViewModel _videoItemsViewModel;
        public VideoItemsViewModel VideoItemsViewModel => _videoItemsViewModel;

        // Services ////////////////////////////////////////////////////////////////////////////////
        private readonly VideoListService _videoListService;

        // Propetries //////////////////////////////////////////////////////////////////////////////
        private Video _currentVideo;
        public Video CurrentVideo
        {
            get => _currentVideo;
            set
            {
                if (_currentVideo != value)
                {
                    _currentVideo = value;
                    OnPropertyChanged();
                    // Костыль
                    if (CurrentVideo != null) QualityPanelListRows = CurrentVideo.DownloadInfo.QualityPanelList.Count; // Сделать проверку на 0
                }
            }
        }
        private bool _isVideoInContentControl;
        public bool IsVideoInContentControl
        {
            get => _isVideoInContentControl;
            set
            {
                if (_isVideoInContentControl != value)
                {
                    _isVideoInContentControl = value;
                    OnPropertyChanged();
                    _videoItemsViewModel.ChangeState(IsVideoInContentControl);
                    ((RelayCommand)SwitchFormatCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)OpenQualityPanelCommand).RaiseCanExecuteChanged();
                    if(!IsVideoInContentControl)
                    {
                        OnIsVideoInContentControlChanged();
                    }
                }
            }
        }
        private bool _format;
        public bool Format
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
        private bool _isQualityPanelOpen;
        public bool IsQualityPanelOpen
        {
            get => _isQualityPanelOpen;
            set
            {
                if (_isQualityPanelOpen != value)
                {
                    _isQualityPanelOpen = value;
                    OnPropertyChanged();
                }
            }
        }
        // Костыль
        private int _qualityPanelListRows;
        public int QualityPanelListRows
        {
            get => _qualityPanelListRows;
            set
            {
                if (_qualityPanelListRows != value)
                {
                    _qualityPanelListRows = value;
                    OnPropertyChanged();
                }
            }
        }

        // Commands ////////////////////////////////////////////////////////////////////////////////
        public ICommand AddVideoCommand { get; }
        public ICommand CloseVideoCommand { get; }
        public ICommand SwitchFormatCommand { get; }
        public ICommand OpenQualityPanelCommand { get; }
        public ICommand SelectQualityCommand { get; }

        // Constructors //////////////////////////////////////////////////////////////////////////
        public VideoInfoViewModel() { }
        public VideoInfoViewModel(VideoItemsViewModel videoItemsViewModel, VideoListService videoListService)
        {
            _videoItemsViewModel = videoItemsViewModel;
            _videoListService = videoListService;

            AddVideoCommand = new RelayCommand(AddVideo);
            CloseVideoCommand = new RelayCommand(CloseVideo);
            SwitchFormatCommand = new RelayCommand(SwitchFormat, CanButtonsExecute);
            OpenQualityPanelCommand = new RelayCommand(OpenQualityPanel, CanButtonsExecute);
            SelectQualityCommand = new RelayCommand(SelectQuality);

            _videoItemsViewModel.SettingsVideoAction += SettingsVideo;
        }

        // Methods /////////////////////////////////////////////////////////////////////////////////
        public async Task LoadVideoInfo(string url)
        {
            CurrentVideo = await VideoManager.GetVideo(url);
            IsVideoInContentControl = true;
        }
        private void AddVideo(object parameter)
        {
            _videoListService.AddVideo(CurrentVideo);
            CloseVideo(parameter);
        }
        public void CloseVideo(object parameter)
        {
            CurrentVideo = null;
            IsVideoInContentControl = false;
            if (Format) Format = false; // Switch to default video type
        }
        private void SwitchFormat(object parameter)
        {
            if (!Format)
            {
                CurrentVideo.DownloadInfo.Type = "video";
                QualityPanelListRows = CurrentVideo.DownloadInfo.QualityPanelList.Count;
            }
            else
            {
                CurrentVideo.DownloadInfo.Type = "audio";
                QualityPanelListRows = CurrentVideo.DownloadInfo.QualityPanelList.Count;
            }
        }
        private void OpenQualityPanel(object parameter)
        {
            IsQualityPanelOpen = !IsQualityPanelOpen;
        }
        private void SelectQuality(object parameter)
        {
            if (parameter is string quality)
            {
                CurrentVideo.DownloadInfo.SelectedQuality = quality;
            }
            IsQualityPanelOpen = false;
        }
        public void GiveDownloadPathToVideoItemsViewModel(string path)
        {
            _videoItemsViewModel.LoadDownloadPath(path);
        }
        private bool CanButtonsExecute(object parameter)
        {
            return IsVideoInContentControl;
        }
        private void SettingsVideo(Video video)
        {
            CurrentVideo = video;
            IsVideoInContentControl = true;
            _videoListService.RemoveVideo(video);
        }

        public event EventHandler IsVideoInContentControlChanged;
        protected virtual void OnIsVideoInContentControlChanged()
        {
            IsVideoInContentControlChanged?.Invoke(this, EventArgs.Empty);
        }

        // PropertyChanged /////////////////////////////////////////////////////////////////////////
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}