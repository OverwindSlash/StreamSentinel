using System.Text.Json;
using StreamSentinel.Entities.AnalysisDefinitions;
using StreamSentinel.Entities.Geometric;

namespace StreamSentinel.Core.Tests
{
    public class ImageAnalysisDefinitionTests
    {
        [Test]
        public void TestCreateDefinitionFile()
        {
            int w = 2560;
            int h = 1440;

            ImageAnalysisDefinition definition = new ImageAnalysisDefinition();
            definition.Name = "苏南运河苏州长浒大桥";

            definition.IsObjectAnalyzableRetain = false;
            definition.IsDoubleLineCounting = false;

            {
                // tracking area
                AnalysisArea analysisArea = new AnalysisArea();
                analysisArea.AddPoint(new NormalizedPoint(w, h, 401, 1438));
                analysisArea.AddPoint(new NormalizedPoint(w, h, 2, 449));
                analysisArea.AddPoint(new NormalizedPoint(w, h, 27, 366));
                analysisArea.AddPoint(new NormalizedPoint(w, h, 432, 327));
                analysisArea.AddPoint(new NormalizedPoint(w, h, 2556, 473));
                analysisArea.AddPoint(new NormalizedPoint(w, h, 2558, 1436));

                analysisArea.Name = "water region";

                definition.AddAnalysisArea(analysisArea);
            }

            {
                // excluded area
                ExcludedArea excludedArea = new ExcludedArea();
                excludedArea.AddPoint(new NormalizedPoint(w, h, 2, 449));
                excludedArea.AddPoint(new NormalizedPoint(w, h, 792, 354));
                excludedArea.AddPoint(new NormalizedPoint(w, h, 435, 327));
                excludedArea.AddPoint(new NormalizedPoint(w, h, 28, 366));

                excludedArea.Name = "too far region";

                definition.AddExcludedArea(excludedArea);
            }

            {
                // lane 1
                Lane lane = new Lane();
                lane.AddPoint(new NormalizedPoint(w, h, 229, 345));
                lane.AddPoint(new NormalizedPoint(w, h, 2559, 712));
                lane.AddPoint(new NormalizedPoint(w, h, 2559, 1439));
                lane.AddPoint(new NormalizedPoint(w, h, 402, 1439));
                lane.AddPoint(new NormalizedPoint(w, h, 4, 448));
                lane.AddPoint(new NormalizedPoint(w, h, 27, 367));

                lane.Name = "下行航道";
                lane.Type = "航道";
                lane.Index = 1;

                definition.Lanes.Add(lane);
            }

            {
                // lane 2
                Lane lane = new Lane();
                lane.AddPoint(new NormalizedPoint(w, h, 230, 344));
                lane.AddPoint(new NormalizedPoint(w, h, 2558, 710));
                lane.AddPoint(new NormalizedPoint(w, h, 2555, 467));
                lane.AddPoint(new NormalizedPoint(w, h, 432, 325));

                lane.Name = "上行航道";
                lane.Type = "航道";
                lane.Index = 2;

                definition.Lanes.Add(lane);
            }

            {
                // count line (upward)
                EnterLine enterLine = new EnterLine();
                enterLine.Start = new NormalizedPoint(w, h, 133, 821);
                enterLine.Stop = new NormalizedPoint(w, h, 1794, 417);
                enterLine.Name = "count line";

                LeaveLine leaveLine = new LeaveLine();
                leaveLine.Start = new NormalizedPoint(w, h, 133, 821);
                leaveLine.Stop = new NormalizedPoint(w, h, 1794, 417);
                leaveLine.Name = "count line";

                definition.CountLines.Add(new Tuple<EnterLine, LeaveLine>(enterLine, leaveLine));
            }

            definition.SetImageSize(w, h);

            string jsonString = JsonSerializer.Serialize(definition);

            ImageAnalysisDefinition.SaveToJson("changhudaqiao.json", definition);
        }
    }
}
