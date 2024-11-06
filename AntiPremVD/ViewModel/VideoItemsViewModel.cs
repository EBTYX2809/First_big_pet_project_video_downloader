using AntiPremVD.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace AntiPremVD.ViewModel
{
    public class VideoItemsViewModel : INotifyPropertyChanged
    {
        // Services ////////////////////////////////////////////////////////////////////////////////
        private readonly VideoListService _videoListService;
        public ObservableCollection<Video> VideoList => _videoListService.VideoList;
        private readonly JsonParser _jsonParser;
        public JsonParser JsonParser => _jsonParser;

        // Properties //////////////////////////////////////////////////////////////////////////////
        private string DownloadPath = null;
        private bool _isActualState;
        public bool IsActualState
        {
            get => _isActualState;
            set
            {
                if (_isActualState != value)
                {
                    _isActualState = value;
                    OnPropertyChanged();

                    ((RelayCommand)CloseVideoCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteVideoCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)StartPauseCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)SettingsVideoCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)FolderVideoCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DownloadAllCommand).RaiseCanExecuteChanged();

                    if (!IsActualState)
                    {
                        ItemOpacity = 0.3;
                    }
                    else
                    {
                        ItemOpacity = 1;
                    }
                }
            }
        }
        private double _itemOpacity;
        public double ItemOpacity
        {
            get => _itemOpacity;
            set
            {
                if (_itemOpacity != value)
                {
                    _itemOpacity = value;
                    OnPropertyChanged();
                }
            }
        }
        public Action<Video> SettingsVideoAction;

        // Commands ////////////////////////////////////////////////////////////////////////////////
        public ICommand CloseVideoCommand { get; }
        public ICommand DeleteVideoCommand { get; }
        public ICommand StartPauseCommand { get; }
        public ICommand SettingsVideoCommand { get; }
        public ICommand FolderVideoCommand { get; }
        public ICommand DownloadAllCommand { get; }

        // Contructors /////////////////////////////////////////////////////////////////////////////
        public VideoItemsViewModel() { }
        public VideoItemsViewModel(VideoListService videoListService, JsonParser jsonParser)
        {
            _videoListService = videoListService;
            _jsonParser = jsonParser;

            CloseVideoCommand = new RelayCommand(CloseVideo, CanButtonsExecute);
            DeleteVideoCommand = new RelayCommand(DeleteVideo, CanButtonsExecute);
            StartPauseCommand = new RelayCommand(StartPause, CanButtonsExecute);
            SettingsVideoCommand = new RelayCommand(SettingsVideo, CanSettingsExecute);
            FolderVideoCommand = new RelayCommand(OpenVideoFolder, CanButtonsExecute);
            DownloadAllCommand = new RelayCommand(DownloadAll, CanButtonsExecute);
        }

        // Methods /////////////////////////////////////////////////////////////////////////////////
        private void CloseVideo(object parameter)
        {
            if (parameter is Video video)
            {
                _videoListService.RemoveVideo(video);
                DownloadManager.CancelDownload(video);
            }
        }
        private void DeleteVideo(object parameter)
        {
            if (parameter is Video video)
            {
                string sanitizedTitle = string.Concat(video.Title.Split(Path.GetInvalidFileNameChars()));
                string searchPattern = $"{sanitizedTitle}.*";

                string filePath = Directory.GetFiles(DownloadPath, searchPattern).FirstOrDefault();

                if (!string.IsNullOrEmpty(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                        CloseVideo(video);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to delete file:  {ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show("File not found.");
                }
            }
        }
        private void OpenVideoFolder(object parameter)
        {
            if (parameter is Video video)
            {
                string sanitizedTitle = string.Concat(video.Title.Split(Path.GetInvalidFileNameChars()));

                var folder = Directory.GetFiles(DownloadPath);

                string fullPath = folder.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f) == sanitizedTitle);

                if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
                {
                    try
                    {
                        Process.Start("explorer.exe", $"/select,\"{fullPath}\"");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to open file:  {ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show("File not found.");
                }
            }
        }
        private async void StartPause(object parameter)
        {
            if (parameter is Video video)
            {
                // Check if Qualities didn't fill yet
                if (video.DownloadInfo.Qualities.Count == 0)
                {
                    await video.DownloadInfo.QualitiesLoaded.Task;
                }

                await DownloadManager.StartPauseDownload(video, DownloadPath);
            }
        }
        private async void DownloadAll(object parameter)
        {
            await DownloadManager.DownloadAll(VideoList.ToList(), DownloadPath);
        }
        private void SettingsVideo(object parameter)
        {
            if (parameter is Video video)
            {
                SettingsVideoAction?.Invoke(video);
            }
        }
        public void LoadDownloadPath(string path)
        {
            DownloadPath = path;
        }
        public void ChangeState(bool state)
        {
            IsActualState = !state;
        }
        private bool CanButtonsExecute(object parameter)
        {
            return IsActualState;
        }
        private bool CanSettingsExecute(object parameter)
        {
            if (CanButtonsExecute(parameter))
            {
                if (parameter is Video video)
                {
                    return !DownloadManager.IsDownloadStarted(video);
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        // PropertyChanged ///////////////////////////////////////////////////////////////////////
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}