using System.Text.Json.Serialization;
using StreamSentinel.Entities.Geometric;

namespace StreamSentinel.Entities.AnalysisDefinitions
{
    public class EnterLine : NormalizedLine
    {
        public string Name { get; set; }

        [JsonIgnore]
        public LeaveLine LeaveLine { get; set; }
    }
}
