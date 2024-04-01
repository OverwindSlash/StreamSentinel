using StreamSentinel.Entities.AnalysisDefinitions;
using StreamSentinel.Entities.AnalysisEngine;

namespace StreamSentinel.Components.Interfaces.RegionManager
{
    public interface IRegionManager
    {
        public ImageAnalysisDefinition AnalysisDefinition { get; }

        void LoadAnalysisDefinition(string jsonFile, int imageWidth, int imageHeight);
        void DetermineAnalyzableObjects(List<DetectedObject> detectedObjects);
    }
}
