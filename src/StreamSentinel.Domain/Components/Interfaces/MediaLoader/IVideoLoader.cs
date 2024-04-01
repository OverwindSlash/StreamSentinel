using StreamSentinel.Entities.MediaLoader;

namespace StreamSentinel.Components.Interfaces.MediaLoader;

public interface IVideoLoader : IMediaLoader
{
    public VideoSpecs Specs { get; }
    
    public int BufferedMaxOccupied { get; }
}