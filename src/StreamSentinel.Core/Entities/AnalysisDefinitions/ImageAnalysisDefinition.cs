using StreamSentinel.Entities.Geometric;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text.Json;

namespace StreamSentinel.Entities.AnalysisDefinitions
{
    public class ImageAnalysisDefinition : ImageBasedGeometric
    {
        private List<AnalysisArea> _analysisAreas;
        private List<ExcludedArea> _excludedAreas;
        private List<Lane> _lanes;
        private List<Tuple<EnterLine, LeaveLine>> _countLines;

        public string Name { get; set; }

        // whether IsAnalyzable flag remain true when out of AnalysisArea
        public bool IsObjectAnalyzableRetain { get; set; }

        // whether cross both enter and leave line will count number
        public bool IsDoubleLineCounting { get; set; }

        public List<AnalysisArea> AnalysisAreas
        {
            get
            {
                CheckImageSizeInitialized();
                return _analysisAreas;
            }
            set => _analysisAreas = value;
        }

        public List<ExcludedArea> ExcludedAreas
        {
            get
            {
                CheckImageSizeInitialized();
                return _excludedAreas;
            }
            set => _excludedAreas = value;
        }

        public List<Lane> Lanes
        {
            get
            {
                CheckImageSizeInitialized();
                return _lanes;
            }
            set => _lanes = value;
        }

        public List<Tuple<EnterLine, LeaveLine>> CountLines
        {
            get
            {
                CheckImageSizeInitialized();
                return _countLines;
            }
            set => _countLines = value;
        }

        public ImageAnalysisDefinition()
        {
            AnalysisAreas = new List<AnalysisArea>();
            ExcludedAreas = new List<ExcludedArea>();
            Lanes = new List<Lane>();
            CountLines = new List<Tuple<EnterLine, LeaveLine>>();

            IsObjectAnalyzableRetain = false;
            IsDoubleLineCounting = false;
        }

        public override void SetImageSize(int width, int height)
        {
            base.SetImageSize(width, height);

            // change image size of each contained definition objects.
            foreach (var area in AnalysisAreas)
            {
                area.SetImageSize(width, height);
            }

            foreach (var area in ExcludedAreas)
            {
                area.SetImageSize(width, height);
            }

            foreach (var lane in Lanes)
            {
                lane.SetImageSize(width, height);
            }

            foreach (var countLine in CountLines)
            {
                countLine.Item1.SetImageSize(width, height);
                countLine.Item2.SetImageSize(width, height);
            }
        }

        public void AddAnalysisArea(AnalysisArea analysisArea)
        {
            ValidateImageWidthAndHeight(analysisArea);
            AnalysisAreas.Add(analysisArea);
        }

        public void AddExcludedArea(ExcludedArea excludedArea)
        {
            ValidateImageWidthAndHeight(excludedArea);
            ExcludedAreas.Add(excludedArea);
        }

        public void AddLane(Lane lane)
        {
            ValidateImageWidthAndHeight(lane);
            Lanes.Add(lane);
        }

        public void AddCountLinePair(EnterLine enterLine, LeaveLine leaveLine)
        {
            ValidateImageWidthAndHeight(enterLine);
            ValidateImageWidthAndHeight(leaveLine);
            CountLines.Add(new Tuple<EnterLine, LeaveLine>(enterLine, leaveLine));
        }

        private void ValidateImageWidthAndHeight(ImageBasedGeometric poiGeometric)
        {
            if (!IsInitialized())
            {
                base.SetImageSize(poiGeometric.ImageWidth, poiGeometric.ImageHeight);
            }

            if ((poiGeometric.ImageWidth != ImageWidth) || (poiGeometric.ImageHeight != ImageHeight))
            {
                throw new ArgumentException($"poi geometric does not match scale of others.");
            }
        }

        public static void SaveToJson(string filename, ImageAnalysisDefinition definition)
        {
            string jsonString = JsonSerializer.Serialize(definition);
            File.WriteAllText(filename, jsonString);
        }

        public static ImageAnalysisDefinition LoadFromJson(string filename, int imageWidth, int imageHeight)
        {
            string jsonString = File.ReadAllText(filename);
            var definition = JsonSerializer.Deserialize<ImageAnalysisDefinition>(jsonString);
            definition.SetImageSize(imageWidth, imageHeight);

            return definition;
        }
    }
}
