using System.Collections.ObjectModel;

namespace AntiPremVD.Model
{
    public class VideoListService
    {
        public ObservableCollection<Video> VideoList { get; private set; }

        public VideoListService()
        {
            VideoList = new ObservableCollection<Video>();
        }

        public void AddVideo(Video video)
        {
            VideoList.Add(video);
        }

        public void RemoveVideo(Video video)
        {
            VideoList.Remove(video);
        }

        public void ClearVideoList()
        {
            VideoList.Clear();
        }
    }
}