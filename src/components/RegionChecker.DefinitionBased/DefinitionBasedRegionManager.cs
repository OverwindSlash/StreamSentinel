using StreamSentinel.Components.Interfaces.RegionManager;
using StreamSentinel.Entities.AnalysisDefinitions;
using StreamSentinel.Entities.AnalysisEngine;
using StreamSentinel.Entities.Events.Pipeline;
using StreamSentinel.Entities.Geometric;
using System.Collections.Concurrent;

namespace RegionManager.DefinitionBased
{
    public class DefinitionBasedRegionManager : IRegionManager, IObserver<ObjectExpiredEvent>
    {
        public const int NoNeedToCalculateLane = -1;
        public const int NotInAnyLaneIndex = 0;

        private ImageAnalysisDefinition _definition;
        private bool _isObjectAnalyzableRetain;
        private List<AnalysisArea> _analysisAreas;
        private List<ExcludedArea> _excludedAreas;
        private List<Lane> _lanes;

        // Id(type:trackingId) -> trakingId
        private readonly ConcurrentDictionary<string, long> _allTrackingIdsUnderAnalysis;

        public DefinitionBasedRegionManager()
        {
            _allTrackingIdsUnderAnalysis = new ConcurrentDictionary<string, long>();
        }

        public ImageAnalysisDefinition AnalysisDefinition => _definition;

        public void LoadAnalysisDefinition(string jsonFile, int imageWidth, int imageHeight)
        {
            _allTrackingIdsUnderAnalysis.Clear();

            _definition = ImageAnalysisDefinition.LoadFromJson(jsonFile, imageWidth, imageHeight);
            _isObjectAnalyzableRetain = _definition.IsObjectAnalyzableRetain;
            _analysisAreas = _definition.AnalysisAreas;
            _excludedAreas = _definition.ExcludedAreas;
            _lanes = _definition.Lanes;
        }

        public void CalcRegionProperties(List<DetectedObject> detectedObjects)
        {
            foreach (DetectedObject detectedObject in detectedObjects)
            {
                DetermineAnalyzableObject(detectedObject);
                CalculateLane(detectedObject);
            }
        }

        private void DetermineAnalyzableObject(DetectedObject detectedObject)
        {
            if (_isObjectAnalyzableRetain && _allTrackingIdsUnderAnalysis.ContainsKey(detectedObject.Id))
            {
                detectedObject.IsUnderAnalysis = true;
                return;
            }

            NormalizedPoint point = new NormalizedPoint(_definition.ImageWidth, _definition.ImageHeight,
                detectedObject.CenterX, detectedObject.CenterY);

            foreach (AnalysisArea analysisArea in _analysisAreas)
            {
                if (analysisArea.IsPointInPolygon(point))
                {
                    detectedObject.IsUnderAnalysis = true;
                    break;
                }
            }

            foreach (ExcludedArea excludedArea in _excludedAreas)
            {
                if (excludedArea.IsPointInPolygon(point))
                {
                    detectedObject.IsUnderAnalysis = false;
                    break;
                }
            }

            if (detectedObject.IsUnderAnalysis)
            {
                _allTrackingIdsUnderAnalysis.TryAdd(detectedObject.Id, detectedObject.TrackingId);
            }
        }

        private void CalculateLane(DetectedObject detectedObject)
        {
            if (!detectedObject.IsUnderAnalysis)
            {
                return;
            }

            NormalizedPoint point = new NormalizedPoint(_definition.ImageWidth, _definition.ImageHeight,
                detectedObject.CenterX, detectedObject.CenterY);

            foreach (Lane lane in _lanes)
            {
                if (lane.IsPointInPolygon(point))
                {
                    detectedObject.LaneIndex = lane.Index;
                }
            }
        }

        public int GetAnalyzableObjectCount()
        {
            return _allTrackingIdsUnderAnalysis.Count;
        }

        public void OnCompleted()
        {
            // Do nothing
        }

        public void OnError(Exception error)
        {
            // Do nothing
        }

        public void OnNext(ObjectExpiredEvent value)
        {
            Task.Run(() =>
            {
                ReleaseAnalyzableObjectById(value.Id);
            });
        }

        private void ReleaseAnalyzableObjectById(string id)
        {
            if (_allTrackingIdsUnderAnalysis.ContainsKey(id))
            {
                _allTrackingIdsUnderAnalysis.TryRemove(id, out var value);
            }
        }
    }
}
