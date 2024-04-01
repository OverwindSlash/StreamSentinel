using System.Collections.Generic;
using StreamSentinel.Entities.Geometric;

namespace StreamSentinel.Entities.AnalysisDefinitions
{
    public class Lane : NormalizedPolygon
    {
        private string _type;

        public string Name { get; set; }
        public int Index { get; set; }

        public string Type
        {
            get => _type;
            set
            {
                _type = value;
            }
        }

        public HashSet<string> ForbiddenTypes { get; set; }

        public Lane()
        {
            ForbiddenTypes = new HashSet<string>();
        }
    }
}
