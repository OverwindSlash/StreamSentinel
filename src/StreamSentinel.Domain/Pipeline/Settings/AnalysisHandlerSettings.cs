using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamSentinel.Pipeline.Settings
{
    public class AnalysisHandlerSettings
    {
        public string AssemblyFile { get; set; }
        public string FullQualifiedClassName { get; set; }
        public string[] Parameters { get; set; }
    }
}
