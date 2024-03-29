using System.Drawing;

namespace Tracker.SortTracker
{
    public interface ITracker
    {
        IEnumerable<Track> Track(IEnumerable<RectangleF> boxes);
    }
}