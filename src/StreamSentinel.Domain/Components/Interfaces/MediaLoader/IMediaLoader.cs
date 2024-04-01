using StreamSentinel.Entities.AnalysisEngine;

namespace StreamSentinel.Components.Interfaces.MediaLoader;

public interface IMediaLoader
{
    public string DeviceId { get; }
    public int MediaWidth { get; }
    public int MediaHeight { get; }
    public bool IsOpened { get; }
    public bool IsInPlaying { get; }

    public int BufferedFrameCount { get; }

    void Open(string uri);
    void Play(int stride = 1, bool debugMode = false, int debugFrameCount = 0);
    void Stop();
    void Close();

    Frame RetrieveFrame();
    Task<Frame> RetrieveFrameAsync();
}