using Microsoft.Extensions.Configuration;
using StreamSentinel.Pipeline;

namespace StreamSentinel.Domain.Tests
{
    public class AnalysisPipelineTests
    {
        [Test]
        public void TestCreatePipeline()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("settings.json", true, true)
                .Build();

            var pipeline = new AnalysisPipeline(config);

            pipeline.Run();
        }
    }
}
