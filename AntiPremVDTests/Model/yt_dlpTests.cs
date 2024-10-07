using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AntiPremVD.Model.Tests
{
    [TestClass()]
    public class yt_dlpTests
    {
        [TestMethod()]
        public async Task GetQualitiesTest()
        {
            // Arrange
            string url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
            List<string> expectedQualities = new List<string> { "144p", "240p", "360p", "480p", "720p", "1080p" };

            // Act
            Video video = await yt_dlp.GetVideoInfo(url);

            // Assert
            CollectionAssert.AreEqual(expectedQualities, video.DownloadInfo.Qualities.Keys);
        }
    }
}