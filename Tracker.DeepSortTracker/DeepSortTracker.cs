using Microsoft.Extensions.Options;
using MOT.CORE.Matchers.Deep;
using MOT.CORE.ReID.Models.Fast_Reid;
using MOT.CORE.ReID.Models.OSNet;
using MOT.CORE.ReID;
using OpenCvSharp;
using StreamSentinel.Components.Interfaces.ObjectTracker;
using StreamSentinel.Entities.AnalysisEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using MOT.CORE.Matchers.Abstract;
using OpenCvSharp.Extensions;
using System.Diagnostics;

namespace Tracker.DeepSortTracker
{
    internal class DeepSortTracker : IObjectTracker
    {
        private readonly Matcher matcher;
        private readonly MatcherOption options;
        public DeepSortTracker()
        {
            options = new MatcherOption();
            matcher = new DeepSortMatcher(
                    ConstructAppearanceExtractorFromOptions(options),
                    options.AppearanceWeight ?? 0.775f,
                    options.Threshold ?? 0.5f,
                    options.MaxMisses ?? 50,
                    options.FramesToAppearanceSmooth ?? 40,
                    options.SmoothAppearanceWeight ?? 0.875f,
                    options.MinStreak ?? 8);
        }
        public void Track(Mat scene, List<DetectedObject> detectedObjects)
        {
            IReadOnlyList<ITrack> tracks = matcher.Run(scene.ToBitmap(), detectedObjects.ToArray<IPrediction>());
            Trace.TraceInformation($"Tracks count: {tracks.Count}; DetectedObject TrackId: {detectedObjects[0].TrackingId}") ;
        }

        private static IAppearanceExtractor ConstructAppearanceExtractorFromOptions(MatcherOption options)
        {
            if (string.IsNullOrEmpty(options.AppearanceExtractorFilePath))
                throw new ArgumentNullException($"{nameof(options.AppearanceExtractorFilePath)} was undefined.");

            if (options.AppearanceExtractorVersion == null)
                throw new ArgumentNullException($"{nameof(options.AppearanceExtractorVersion)} was undefined.");

            const int DefaultExtractorsCount = 4;

            IAppearanceExtractor appearanceExtractor = options.AppearanceExtractorVersion switch
            {
                AppearanceExtractorVersion.OSNet => new ReidScorer<OSNet_x1_0>(File.ReadAllBytes(options.AppearanceExtractorFilePath),
                    options.ExtractorsInMemoryCount ?? DefaultExtractorsCount, SessionOptions.MakeSessionOptionWithCudaProvider()),
                AppearanceExtractorVersion.FastReid => new ReidScorer<Fast_Reid_mobilenetv2>(File.ReadAllBytes(options.AppearanceExtractorFilePath),
                    options.ExtractorsInMemoryCount ?? DefaultExtractorsCount, SessionOptions.MakeSessionOptionWithCudaProvider()),
                _ => throw new Exception("Appearance extractor cannot be constructed.")
            };

            return appearanceExtractor;
        }
    }
}
