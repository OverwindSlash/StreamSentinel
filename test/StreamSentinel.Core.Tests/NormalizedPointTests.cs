using System.Text.Json;
using StreamSentinel.Entities.Geometric;

namespace StreamSentinel.Core.Tests
{
    [TestFixture]
    public class NormalizedPointTests
    {
        private const int ImageWidth = 1920;
        private const int ImageHeight = 1080;
        string pointJson = "{\"NormalizedX\":0.24635416666666668,\"NormalizedY\":0.10092592592592593}";
        
        [Test]
        public void Test_NormalizedPoint_SaveMode()
        {
            NormalizedPoint p1 = new NormalizedPoint(ImageWidth, ImageHeight, 473, 109);
            
            Assert.AreEqual(473, p1.OriginalX);
            Assert.AreEqual(109, p1.OriginalY);
            Assert.AreEqual((double)473 / 1920, p1.NormalizedX);
            Assert.AreEqual((double)109 / 1080, p1.NormalizedY);
        }
        
        [Test]
        public void Test_NormalizedPoint_LoadMode_WithoutSetImageSize()
        {
            NormalizedPoint p1 = JsonSerializer.Deserialize<NormalizedPoint>(pointJson);
            Assert.NotNull(p1);
            
            Assert.Catch<ArgumentException>(() =>
            {
                int x = p1.OriginalX;
            });
        }
        
        [Test]
        public void Test_NormalizedPoint_LoadMode_WithSetImageSize()
        {
            NormalizedPoint p1 = JsonSerializer.Deserialize<NormalizedPoint>(pointJson);
            Assert.NotNull(p1);
            
            p1.SetImageSize(ImageWidth, ImageHeight);

            Assert.AreEqual(473, p1.OriginalX);
            Assert.AreEqual(109, p1.OriginalY);
            Assert.AreEqual((double)473 / 1920, p1.NormalizedX);
            Assert.AreEqual((double)109 / 1080, p1.NormalizedY);
        }
    }
}