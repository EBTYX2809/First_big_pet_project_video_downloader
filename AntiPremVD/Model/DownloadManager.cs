using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AntiPremVD.Model
{
    public static class DownloadManager
    {
        private static Dictionary<Video, DownloadService> DownloadList = new Dictionary<Video, DownloadService>();
        private static Dictionary<Video, bool> IsPaused = new Dictionary<Video, bool>();
        private static List<Task> DownloadListTask = new List<Task>();

        public static async Task StartPauseDownload(Video video, string output)
        {
            // If downloading didn't started
            if (!DownloadList.ContainsKey(video))
            {
                string sanitizedTitle = string.Concat(video.Title.Split(Path.GetInvalidFileNameChars()));
                string filePath = Path.Combine(output, $"{sanitizedTitle}.%(ext)s"); // {video.DownloadInfo.Format}
                var downloadService = new DownloadService(video, filePath);
                DownloadList[video] = downloadService;
                IsPaused[video] = false;
                _ = downloadService.StartDownload();
            }
            else
            {
                // If downloading already started
                if (IsPaused[video])
                {
                    IsPaused[video] = false;
                    _ = DownloadList[video].ResumeDownload();
                }
                else
                {
                    IsPaused[video] = true;
                    DownloadList[video].PauseDownload();
                }
            }
        }

        public static async Task DownloadAll(List<Video> videos, string output)
        {
            foreach (var video in videos)
            {
                if (video.DownloadInfo.Qualities.Count == 0)
                {
                    // Waiting for loading Qualities
                    await video.DownloadInfo.QualitiesLoaded.Task;
                }
                DownloadListTask.Add(StartPauseDownload(video, output));
            }

            await Task.WhenAll(DownloadListTask);
        }

        public static void CancelDownload(Video video)
        {
            if (DownloadList.ContainsKey(video))
            {
                DownloadList[video].CancelDownload();
                DownloadList.Remove(video);
                IsPaused.Remove(video);
            }
            else
            {
                return;
            }
        }

        public static bool IsDownloadStarted(Video video)
        {
            return DownloadList.ContainsKey(video);
        }
    }
}
