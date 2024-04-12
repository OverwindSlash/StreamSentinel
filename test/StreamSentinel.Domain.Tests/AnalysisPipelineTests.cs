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
            Console.ReadLine();
        }

        [Test]
        public void TestTwoPipeline() {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("settings.json", true, true)
                .Build();

            var pipeline = new AnalysisPipeline(config);

            pipeline.Run();
            IConfiguration config2 = new ConfigurationBuilder()
                .AddJsonFile("settings2.json", true, true)
                .Build();
            var pipeline2 = new AnalysisPipeline(config2);
            pipeline2.Run();
            Console.ReadLine();

        }

        [Test]
        public void TestPipeline2()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("settings2.json", true, true)
                .Build();

            var pipeline = new AnalysisPipeline(config);

            pipeline.Run();
            Console.ReadLine();
        }
    }
}
