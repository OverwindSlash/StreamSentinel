using StreamSentinel.Components.Interfaces.AnalysisEngine;
using StreamSentinel.DataStructures;

namespace StreamSentinel.Pipeline
{
    public class AnalysisPipeline
    {
        private const int DefaultFrameLifeTime = 125;
        private readonly List<IAnalysisHandler> _analysisHandlers;
        private readonly ObservableSlideWindow _slideWindow;
    }
}
