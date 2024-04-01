using System.Text.Json.Serialization;
using StreamSentinel.Entities.Geometric;

namespace StreamSentinel.Entities.AnalysisDefinitions
{
    public class LeaveLine : NormalizedLine
    {
        public string Name { get; set; }

        [JsonIgnore]
        public EnterLine EnterLine { get; set; }
    }
}
