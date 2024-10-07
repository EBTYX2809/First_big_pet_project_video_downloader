using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace AntiPremVD.Model
{
    public class YouTubeExplode
    {
        private readonly YoutubeClient youtubeClient;
        public YouTubeExplode()
        {
            youtubeClient = new YoutubeClient();
        }

        public async Task<Video> GetVideoInfo(string videoUrl)
        {
            try
            {
                var videoTask = youtubeClient.Videos.GetAsync(videoUrl).AsTask();
                var streamsTask = youtubeClient.Videos.Streams.GetManifestAsync(videoUrl).AsTask();

                await Task.WhenAll(videoTask, streamsTask);

                var video = videoTask.Result;
                var streams = streamsTask.Result;

                var videoStreamQualities = new HashSet<string>();
                var qualitySizeMap = new Dictionary<string, string>();

                // Filtering and adding streams in hashsets
                foreach (var stream in streams.Streams)
                {
                    if (stream is IVideoStreamInfo videoStream)
                    {
                        string qualityLabel = videoStream.VideoQuality.Label;
                        videoStreamQualities.Add(qualityLabel);

                        if (videoStream.VideoQuality.Label == "144p")
                        {
                            // There is complex conditional logic to approximate the size of audio files
                            qualitySizeMap["64Kbit/s"] = $"~{SizeConverter.FormatFileSize((long)(videoStream.Size.Bytes * 0.08))}";
                            qualitySizeMap["128Kbit/s"] = $"~{SizeConverter.FormatFileSize((long)(videoStream.Size.Bytes * 0.16))}";
                        }

                        string formattedSize = SizeConverter.FormatFileSize(videoStream.Size.Bytes);
                        qualitySizeMap[qualityLabel] = formattedSize;
                    }
                }

                var previewImage = video.Thumbnails.OrderByDescending(t => t.Resolution.Area).FirstOrDefault()?.Url;

                Video _video = new Video
                (
                    url: videoUrl,
                    title: video.Title,
                    duration: video.Duration.ToString(),
                    views: video.Engagement.ViewCount.ToString("N0", System.Globalization.CultureInfo.InvariantCulture),
                    previewImg: previewImage,
                    downloadInfo: new DownloadItem(
                        type: "video",
                        qualityPanelList: new ObservableCollection<string>(videoStreamQualities.OrderByDescending(f => int.Parse(System.Text.RegularExpressions.Regex.Match(f, @"\d+").Value))),
                        sizes: qualitySizeMap,
                        selectedQuality: videoStreamQualities.Max())
                );

                _ = LoadQualities(_video);

                return _video;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed loading data: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        private async Task LoadQualities(Video video)
        {
            try
            {
                var qualities = await yt_dlp.GetQualities(video.URL);

                video.DownloadInfo.Qualities = qualities;

                video.DownloadInfo.QualitiesLoaded.TrySetResult(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed loading qualities: {ex.Message}");
            }
        }

        private static readonly HttpClient httpClient = new HttpClient();

        public async Task<BitmapImage> GetPreviewImage(string videoUrl)
        {
            try
            {
                var video = await youtubeClient.Videos.GetAsync(videoUrl);

                var previewUrl = video.Thumbnails.OrderByDescending(t => t.Resolution.Area).FirstOrDefault()?.Url;

                if (string.IsNullOrEmpty(previewUrl))
                    throw new Exception("Preview URL is null or empty.");

                var response = await httpClient.GetAsync(previewUrl);
                response.EnsureSuccessStatusCode();

                var thumbnailData = await response.Content.ReadAsByteArrayAsync();
                var bitmap = new BitmapImage();

                using (var stream = new MemoryStream(thumbnailData))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }

                return bitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fail: {ex.Message}");
                return null;
            }
        }
    }
}