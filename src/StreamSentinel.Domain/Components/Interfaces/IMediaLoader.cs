namespace StreamSentinel.Components.Interfaces;

public interface IMediaLoader
{
    public string DeviceId { get; }
    public bool IsOpened { get; }

    void Open(string uri);
    void Close();
}