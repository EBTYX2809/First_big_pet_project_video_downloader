using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace AntiPremVD.Model.Tests
{
    [TestClass()]
    public class YouTubeExplodeTests
    {
        YouTubeExplode youTubeExplode = new YouTubeExplode();

        [TestMethod()]
        public async Task GetVideoInfoTest()
        {
            // Arrange
            string url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
            string expectedTitle = "Rick Astley - Never Gonna Give You Up (Official Music Video)";

            // Act
            Video video = await youTubeExplode.GetVideoInfo(url);

            // Assert
            Assert.AreEqual(expectedTitle, video.Title);
        }
    }
}