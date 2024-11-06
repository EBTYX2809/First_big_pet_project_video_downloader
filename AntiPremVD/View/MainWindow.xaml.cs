using AntiPremVD.Model;
using AntiPremVD.ViewModel;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace AntiPremVD.View
{
    /// <summary>
    /// Logic MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            VideoListService videoListService = new VideoListService();
            JsonParser jsonParser = new JsonParser();

            // Create every needed ViewModels and dependency injections
            var videoItemsViewModel = new VideoItemsViewModel(videoListService, jsonParser);
            var videoInfoViewModel = new VideoInfoViewModel(videoItemsViewModel, videoListService, jsonParser);
            var mainViewModel = new MainViewModel(videoInfoViewModel, videoListService, jsonParser);

            DataContext = mainViewModel;
        }
        // Window parameters
        #region Window
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_MAXIMIZEBOX & ~WS_THICKFRAME);
        }

        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000;
        private const int WS_THICKFRAME = 0x40000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        #endregion
    }
}
