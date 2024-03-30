using StreamSentinel.Entities.AnalysisEngine;
using StreamSentinel.Entities.Events;

namespace StreamSentinel.Components.Interfaces.AnalysisEngine
{
    public interface IAnalysisHandler : IObserver<ObjectExpiredEvent>, IObserver<FrameExpiredEvent>
    {
        AnalysisResult Analyze(Frame frame);
    }
}
