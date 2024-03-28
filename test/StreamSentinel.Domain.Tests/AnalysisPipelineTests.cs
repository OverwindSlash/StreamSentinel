using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StreamSentinel.Pipeline;

namespace StreamSentinel.Domain.Tests
{
    public class AnalysisPipelineTests
    {
        [Test]
        public void TestCreatePipeline()
        {
            var pipeline = new AnalysisPipeline();

            pipeline.Run();
        }
    }
}
