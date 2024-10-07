using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AntiPremVD.Model
{
    public static class VideoManager
    {
        private static YouTubeExplode youTubeExplode = new YouTubeExplode();
        public static async Task<Video> GetVideo(string url)
        {
            if (IsValidYouTubeUrl(url))
            {
                return await youTubeExplode.GetVideoInfo(url);
            }
            else
            {
                return await yt_dlp.GetVideoInfo(url);
            }
        }
        private static bool IsValidYouTubeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            // Checking for a YouTube domain
            if (!url.Contains("youtube.com") && !url.Contains("youtu.be"))
            {
                return false;
            }

            // Regular expression to check URL format with playlists also
            var youtubeRegex = new Regex(@"^(https?://)?(www\.)?(youtube\.com/watch\?v=|youtu\.be/)([a-zA-Z0-9_-]{11})(\&list=[a-zA-Z0-9_-]+)?(\&index=\d+)?$", RegexOptions.IgnoreCase);
            return youtubeRegex.IsMatch(url);
        }
    }
}
