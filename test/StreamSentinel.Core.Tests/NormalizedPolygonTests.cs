using System.Text.Json;
using StreamSentinel.Entities.Geometric;

namespace StreamSentinel.Core.Tests
{
    [TestFixture]
    public class NormalizedPolygonTests
    {
        private const int ImageWidth = 1920;
        private const int ImageHeight = 1080;

        private string polygonJson =
            "{\"Points\":[" +
            "{\"NormalizedX\":0.24635416666666668,\"NormalizedY\":0}," +
            "{\"NormalizedX\":0.09739583333333333,\"NormalizedY\":0.26296296296296295}," +
            "{\"NormalizedX\":0.24479166666666666,\"NormalizedY\":0.2601851851851852}," +
            "{\"NormalizedX\":0.3328125,\"NormalizedY\":0}]}";

        [Test]
        public void Test_NormalizedPolygon_SaveMode_UseList_WithSameScale()
        {
            NormalizedPoint p1 = new NormalizedPoint(ImageWidth, ImageHeight, 473, 0);
            NormalizedPoint p2 = new NormalizedPoint(ImageWidth, ImageHeight, 187, 284);
            NormalizedPoint p3 = new NormalizedPoint(ImageWidth, ImageHeight, 470, 281);
            NormalizedPoint p4 = new NormalizedPoint(ImageWidth, ImageHeight, 639, 0);

            List<NormalizedPoint> points = new List<NormalizedPoint>();
            points.Add(p1);
            points.Add(p2);
            points.Add(p3);
            points.Add(p4);

            NormalizedPolygon polygon1 = new NormalizedPolygon(points);

            Assert.AreEqual(473, polygon1.Points[0].OriginalX);
            Assert.AreEqual(284, polygon1.Points[1].OriginalY);
            Assert.AreEqual(470, polygon1.Points[2].OriginalX);
            Assert.AreEqual(0, polygon1.Points[3].OriginalY);
        }
        
        [Test]
        public void Test_NormalizedPolygon_SaveMode_UseList_WithoutSameScale()
        {
            NormalizedPoint p1 = new NormalizedPoint(ImageWidth, ImageHeight, 473, 0);
            NormalizedPoint p2 = new NormalizedPoint(ImageWidth, ImageHeight, 187, 284);
            NormalizedPoint p3 = new NormalizedPoint(ImageWidth, ImageHeight, 470, 281);
            NormalizedPoint p4 = new NormalizedPoint(ImageWidth / 2, ImageHeight / 2, 639, 0);

            List<NormalizedPoint> points = new List<NormalizedPoint>();
            points.Add(p1);
            points.Add(p2);
            points.Add(p3);
            points.Add(p4);

            Assert.Catch<ArgumentException>(() =>
            {
                NormalizedPolygon polygon1 = new NormalizedPolygon(points);
            });
        }
        
        [Test]
        public void Test_NormalizedPolygon_SaveMode_UseAdd_WithSameScale()
        {
            // Lane1 points
            NormalizedPoint p1 = new NormalizedPoint(ImageWidth, ImageHeight, 473, 0);
            NormalizedPoint p2 = new NormalizedPoint(ImageWidth, ImageHeight, 187, 284);
            NormalizedPoint p3 = new NormalizedPoint(ImageWidth, ImageHeight, 470, 281);
            NormalizedPoint p4 = new NormalizedPoint(ImageWidth, ImageHeight, 639, 0);

            NormalizedPolygon polygon1 = new NormalizedPolygon();
            polygon1.AddPoint(p1);
            polygon1.AddPoint(p2);
            polygon1.AddPoint(p3);
            polygon1.AddPoint(p4);

            Assert.AreEqual(473, polygon1.Points[0].OriginalX);
            Assert.AreEqual(284, polygon1.Points[1].OriginalY);
            Assert.AreEqual(470, polygon1.Points[2].OriginalX);
            Assert.AreEqual(0, polygon1.Points[3].OriginalY);
        }
        
        [Test]
        public void Test_NormalizedPolygon_SaveMode_UseAdd_WithoutSameScale()
        {
            // Lane1 points
            NormalizedPoint p1 = new NormalizedPoint(ImageWidth, ImageHeight, 473, 0);
            NormalizedPoint p2 = new NormalizedPoint(ImageWidth, ImageHeight, 187, 284);
            NormalizedPoint p3 = new NormalizedPoint(ImageWidth, ImageHeight, 470, 281);
            NormalizedPoint p4 = new NormalizedPoint(ImageWidth / 2, ImageHeight / 2, 639, 0);

            NormalizedPolygon polygon1 = new NormalizedPolygon();
            polygon1.AddPoint(p1);
            polygon1.AddPoint(p2);
            polygon1.AddPoint(p3);

            Assert.Catch<ArgumentException>(() =>
            {
                polygon1.AddPoint(p4);
            });
        }

        [Test]
        public void Test_NormalizedPolygon_LoadMode_WithSetImageSize()
        {
            NormalizedPolygon polygon1 = JsonSerializer.Deserialize<NormalizedPolygon>(polygonJson);
            Assert.NotNull(polygon1);
            
            polygon1.SetImageSize(ImageWidth, ImageHeight);
            
            Assert.AreEqual(473, polygon1.Points[0].OriginalX);
            Assert.AreEqual(284, polygon1.Points[1].OriginalY);
            Assert.AreEqual(470, polygon1.Points[2].OriginalX);
            Assert.AreEqual(0, polygon1.Points[3].OriginalY);
        }
        
        [Test]
        public void Test_NormalizedPolygon_LoadMode_WithoutSetImageSize()
        {
            NormalizedPolygon polygon1 = JsonSerializer.Deserialize<NormalizedPolygon>(polygonJson);
            Assert.NotNull(polygon1);

            Assert.Catch<ArgumentException>(() =>
            {
                int originalX = polygon1.Points[0].OriginalX;
            });
        }
        
        [Test]
        public void Test_NormalizedPolygon_SaveMode_UseAdd_IsPointInPolygon()
        {
            // Lane1 points
            NormalizedPoint p1 = new NormalizedPoint(ImageWidth, ImageHeight, 473, 0);
            NormalizedPoint p2 = new NormalizedPoint(ImageWidth, ImageHeight, 187, 284);
            NormalizedPoint p3 = new NormalizedPoint(ImageWidth, ImageHeight, 470, 281);
            NormalizedPoint p4 = new NormalizedPoint(ImageWidth, ImageHeight, 639, 0);

            NormalizedPolygon polygon1 = new NormalizedPolygon();
            polygon1.AddPoint(p1);
            polygon1.AddPoint(p2);
            polygon1.AddPoint(p3);
            polygon1.AddPoint(p4);

            // points to check
            NormalizedPoint inPoint = new NormalizedPoint(ImageWidth, ImageHeight, 433, 142);
            bool inResult = polygon1.IsPointInPolygon(inPoint);
            Assert.True(inResult);

            NormalizedPoint outPoint = new NormalizedPoint(ImageWidth, ImageHeight, 618, 194);
            bool outResult = polygon1.IsPointInPolygon(outPoint);
            Assert.False(outResult);
        }
    }
}