using System.Text.Json;
using StreamSentinel.Entities.Geometric;

namespace StreamSentinel.Core.Tests
{
    [TestFixture]
    public class NormalizedLineTests
    {
        private const int ImageWidth = 1920;
        private const int ImageHeight = 1080;
        
        private string lineJson = "{\"Start\":{\"NormalizedX\":0.24635416666666668,\"NormalizedY\":0.10092592592592593}," +
                                  "\"Stop\":{\"NormalizedX\":0.09739583333333333,\"NormalizedY\":0.26296296296296295}}";
        
        [Test]
        public void Test_NormalizedLine_SaveMode_WithSameScale()
        {
            NormalizedPoint p1 = new NormalizedPoint(ImageWidth, ImageHeight, 473, 109);
            NormalizedPoint p2 = new NormalizedPoint(ImageWidth, ImageHeight, 187, 284);

            NormalizedLine l1 = new NormalizedLine(p1, p2);
            
            Assert.AreEqual(p1, l1.Start);
            Assert.AreEqual(p2, l1.Stop);
        }
        
        [Test]
        public void Test_NormalizedLine_SaveMode_WithDifferentScale()
        {
            NormalizedPoint p1 = new NormalizedPoint(ImageWidth, ImageHeight, 473, 109);
            NormalizedPoint p2 = new NormalizedPoint(ImageWidth / 2, ImageHeight / 2, 187, 284);

            Assert.Catch<ArgumentException>(() =>
            {
                NormalizedLine l1 = new NormalizedLine(p1, p2);
            });
        }

        [Test]
        public void Test_NormalizedLine_LoadMode_WithSetImageSize()
        {
            NormalizedLine l1 = JsonSerializer.Deserialize<NormalizedLine>(lineJson);
            Assert.NotNull(l1);
            
            l1.SetImageSize(ImageWidth, ImageHeight);
            
            Assert.AreEqual(473, l1.Start.OriginalX);
            Assert.AreEqual(109, l1.Start.OriginalY);
            Assert.AreEqual(187, l1.Stop.OriginalX);
            Assert.AreEqual(284, l1.Stop.OriginalY);
        }
        
        [Test]
        public void Test_NormalizedLine_LoadMode_WithoutSetImageSize()
        {
            NormalizedLine l1 = JsonSerializer.Deserialize<NormalizedLine>(lineJson);
            Assert.NotNull(l1);

            Assert.Catch<ArgumentException>(() =>
            {
                int originalX = l1.Start.OriginalX;
            });
        }
        
        [Test]
        public void Test_NormalizedLine_IsCrossLine()
        {
            NormalizedPoint p1 = new NormalizedPoint(ImageWidth, ImageHeight, 473, 109);
            NormalizedPoint p2 = new NormalizedPoint(ImageWidth, ImageHeight, 187, 284);

            NormalizedLine l1 = new NormalizedLine(p1, p2);

            NormalizedPoint tp1 = new NormalizedPoint(ImageWidth, ImageHeight, 300, 100);
            NormalizedPoint tp2 = new NormalizedPoint(ImageWidth, ImageHeight, 400, 300);
            NormalizedPoint tp3 = new NormalizedPoint(ImageWidth, ImageHeight, 100, 200);

            Assert.True(l1.IsCrossedLine(tp1, tp2));
            Assert.False(l1.IsCrossedLine(tp1, tp3));
        }
    }
}