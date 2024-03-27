namespace StreamSentinel.Entities.MediaLoader
{
    public class VideoSpecs : ImageSpecs
    {
        public double Fps { get; private set; }
        public int FrameCount { get; private set; }

        public VideoSpecs(string uri, int width, int height, double fps, int frameCount) 
            : base(uri, width, height)
        {
            Fps = fps;
            FrameCount = frameCount;
        }
    }
}
