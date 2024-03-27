using StreamSentinel.Entities.AnalysisEngine;

namespace StreamSentinel.Entities.Events
{
    public class ObjectExpiredEvent : EventBase
    {
        private readonly string _id;
        private readonly int _labelId;
        private readonly string _label;
        private readonly long _trackingId;

        public string Id => _id;
        public int LabelId => _labelId;
        public string Label => _label;
        public long TrackingId => _trackingId;

        public ObjectExpiredEvent(string id, int labelId, string label, long trackingId)
        {
            _id = id;
            _labelId = labelId;
            _label = label;
            _trackingId = trackingId;
        }

        public ObjectExpiredEvent(DetectedObject detectedObject)
        {
            _id = detectedObject.Id;
            _labelId = detectedObject.LabelId;
            _label = detectedObject.Label;
            _trackingId = detectedObject.TrackingId;
        }
    }
}
