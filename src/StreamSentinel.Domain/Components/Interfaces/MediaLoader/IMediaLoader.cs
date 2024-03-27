namespace StreamSentinel.Components.Interfaces.MediaLoader;

public interface IMediaLoader
{
    public string DeviceId { get; }
    public bool IsOpened { get; }

    void Open(string uri);
    void Close();
}