using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handler.SnapshotEventService
{
    internal class BlurDetector
    {
        private double _threshold = 20;
        public BlurDetector(double threshold = 20)
        {
            _threshold = threshold;
        }
        public double VarianceOfLaplacian(Mat image)
        {
            using Mat laplacian = new Mat();
            Cv2.Laplacian(image, laplacian, MatType.CV_64F);

            Scalar mean, stddev;
            Cv2.MeanStdDev(laplacian, out mean, out stddev);

            return stddev.Val0;
        }

        public bool IsImageSharp(Mat image)
        {
            double sharpness = VarianceOfLaplacian(image);
            var isSharp = sharpness > _threshold;
            Trace.TraceInformation($"Image has a sharpness score of {sharpness}, {isSharp}");
            return isSharp;
        }
    }
}
