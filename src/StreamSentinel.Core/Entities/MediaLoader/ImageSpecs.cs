namespace StreamSentinel.Entities.MediaLoader;

public class ImageSpecs
{
    public string Uri { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    

    public ImageSpecs(string uri, int width, int height)
    {
        Uri = uri;
        Width = width;
        Height = height;
    }
}