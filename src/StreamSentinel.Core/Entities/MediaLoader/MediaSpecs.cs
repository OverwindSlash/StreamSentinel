namespace StreamSentinel.Entities.MediaLoader;

public class MediaSpecs
{
    public string Uri { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public double Fps { get; private set; }
    public int FrameCount { get; private set; }

    public MediaSpecs(string uri, int width, int height, double fps, int frameCount)
    {
        Uri = uri;
        Width = width;
        Height = height;
        Fps = fps;
        FrameCount = frameCount;
    }
}