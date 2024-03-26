using StreamSentinel.Entities.AnalysisEngine;
using StreamSentinel.Entities.MediaLoader;

namespace StreamSentinel.Components.Interfaces;

public interface IStreamLoader : IMediaLoader
{
    public bool IsInPlaying { get; }
    public MediaSpecs Specs { get; }
    public int BufferedFrameCount { get; }
    public int BufferedMaxOccupied { get; }

    void Play(int stride = 1, bool debugMode = false, int debugFrameCount = 0);
    void Stop();

    Frame RetrieveFrame();
    Task<Frame> RetrieveFrameAsync();
}