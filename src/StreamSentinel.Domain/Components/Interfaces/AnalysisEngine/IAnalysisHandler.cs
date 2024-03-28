using StreamSentinel.Entities.AnalysisEngine;

namespace StreamSentinel.Components.Interfaces.AnalysisEngine
{
    public interface IAnalysisHandler
    {
        AnalysisResult Analyze(Frame frame);
    }
}
