using StreamSentinel.Entities.AnalysisEngine;
using StreamSentinel.Entities.MediaLoader;

namespace StreamSentinel.Components.Interfaces.MediaLoader;

public interface IVideoLoader : IMediaLoader
{
    public bool IsInPlaying { get; }
    public VideoSpecs Specs { get; }
    public int BufferedFrameCount { get; }
    public int BufferedMaxOccupied { get; }

    void Play(int stride = 1, bool debugMode = false, int debugFrameCount = 0);
    void Stop();

    Frame RetrieveFrame();
    Task<Frame> RetrieveFrameAsync();
}