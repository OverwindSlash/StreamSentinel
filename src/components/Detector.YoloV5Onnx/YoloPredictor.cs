using Detector.YoloV5Onnx.Models;
using Detector.YoloV5Onnx.Utils;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;

namespace Detector.YoloV5Onnx
{
    public class YoloPredictor<TYoloModel> : IYoloPredictor where TYoloModel : IYoloModel
    {
        private readonly TYoloModel _yoloModel;
        private readonly InferenceSession _inferenceSession;

        public YoloPredictor(byte[] model, SessionOptions sessionOptions = null)
        {
            _yoloModel = Activator.CreateInstance<TYoloModel>();
            _inferenceSession = new InferenceSession(model, sessionOptions ?? new SessionOptions());
        }

        public IReadOnlyList<YoloPrediction> Predict(Mat image, float targetConfidence, params DetectionObjectType[] targetDetectionTypes)
        {
            Bitmap original = image.ToBitmap();

            Bitmap resized = original;

            var stopwatch = Stopwatch.StartNew();
            if (original.Width != _yoloModel.Width || original.Height != _yoloModel.Height)
            {
                resized = ResizeBitmap(original);
            }
            stopwatch.Stop();
            Console.WriteLine($"Resize time:{stopwatch.ElapsedMilliseconds}");

            List<NamedOnnxValue> inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor(_yoloModel.Input, ExtractPixels(resized))
            };

            IDisposableReadOnlyCollection<DisposableNamedOnnxValue> onnxOutput = _inferenceSession.Run(inputs, _yoloModel.Outputs);
            List<YoloPrediction> predictions = Suppress(ParseOutput(onnxOutput.First().Value as DenseTensor<float>,
                targetConfidence, original, targetDetectionTypes));

            onnxOutput.Dispose();

            return predictions;
        }

        private Bitmap ResizeBitmap(Bitmap image)
        {
            PixelFormat format = image.PixelFormat;

            Bitmap output = new Bitmap(_yoloModel.Width, _yoloModel.Height, format);

            (float xRatio, float yRatio) = (_yoloModel.Width / (float)image.Width, _yoloModel.Height / (float)image.Height);
            float ratio = float.Min(xRatio, yRatio);
            (int targetWidth, int targetHeight) = ((int)(image.Width * ratio), (int)(image.Height * ratio));
            (int x, int y) = ((_yoloModel.Width / 2) - (targetWidth / 2), (_yoloModel.Height / 2) - (targetHeight / 2));

            Rectangle roi = new Rectangle(x, y, targetWidth, targetHeight);

            using (Graphics graphics = Graphics.FromImage(output))
            {
                graphics.Clear(Color.FromArgb(0, 0, 0, 0));

                graphics.SmoothingMode = SmoothingMode.None;
                graphics.InterpolationMode = InterpolationMode.Default;
                graphics.PixelOffsetMode = PixelOffsetMode.Half;

                graphics.DrawImage(image, roi);
            }

            return output;
        }

        private Tensor<float> ExtractPixels(Bitmap image)
        {
            Rectangle rectangle = new Rectangle(0, 0, image.Width, image.Height);
            BitmapData bitmapData = image.LockBits(rectangle, ImageLockMode.ReadOnly, image.PixelFormat);
            int width = bitmapData.Width;
            int height = bitmapData.Height;
            int bytesPerPixel = Image.GetPixelFormatSize(image.PixelFormat) / 8;

            DenseTensor<float> tensor = new DenseTensor<float>(new[] { _yoloModel.BatchSize, _yoloModel.Channels, _yoloModel.Height, _yoloModel.Width });

            unsafe
            {
                Span<float> rTensorSpan = tensor.Buffer.Span.Slice(0, height * width);
                Span<float> gTensorSpan = tensor.Buffer.Span.Slice(height * width, height * width);
                Span<float> bTensorSpan = tensor.Buffer.Span.Slice(height * width * 2, height * width);

                byte* scan0 = (byte*)bitmapData.Scan0;
                int stride = bitmapData.Stride;

                for (int y = 0; y < height; y++)
                {
                    byte* row = scan0 + (y * stride);
                    int rowOffset = y * width;

                    for (int x = 0; x < width; x++)
                    {
                        int bIndex = x * bytesPerPixel;
                        int point = rowOffset + x;

                        rTensorSpan[point] = row[bIndex + 2] / 255.0f; //R
                        gTensorSpan[point] = row[bIndex + 1] / 255.0f; //G
                        bTensorSpan[point] = row[bIndex] / 255.0f; //B
                    }
                }

                image.UnlockBits(bitmapData);
            }

            return tensor;
        }

        private YoloPrediction[] ParseOutput(DenseTensor<float> output, float targetConfidence, Image image, params DetectionObjectType[] targetDetectionTypes)
        {
            unsafe
            {
                List<YoloPrediction> result = new List<YoloPrediction>();

                (int width, int height) = (image.Width, image.Height);
                (float xGain, float yGain) = (_yoloModel.Width / (float)width, _yoloModel.Height / (float)height);
                float gain = Math.Min(xGain, yGain);
                (float xPad, float yPad) = ((_yoloModel.Width - width * gain) / 2, (_yoloModel.Height - height * gain) / 2);
                var spanOutput = output.Buffer.Span;

                for (int i = 0; i < (int)output.Length / _yoloModel.Dimensions; i++)
                {
                    int iOffset = i * _yoloModel.Dimensions;

                    if (spanOutput[iOffset + 4] <= _yoloModel.Confidence)
                        continue;

                    for (int j = 5; j < _yoloModel.Dimensions; j++)
                    {
                        spanOutput[i * _yoloModel.Dimensions + j] *= spanOutput[i * _yoloModel.Dimensions + 4];
                        DetectionObjectType objectType = (DetectionObjectType)(j);

                        if ((targetDetectionTypes.Length != 0
                            && !targetDetectionTypes.Any(p => p == objectType))
                            || spanOutput[iOffset + j] < targetConfidence)
                            continue;

                        if (spanOutput[i * _yoloModel.Dimensions + j] <= _yoloModel.MulConfidence)
                            continue;

                        float xMin = ((spanOutput[iOffset + 0] - spanOutput[iOffset + 2] / 2) - xPad) / gain; // Unpad bbox top-left-x to original
                        float yMin = ((spanOutput[iOffset + 1] - spanOutput[iOffset + 3] / 2) - yPad) / gain; // Unpad bbox top-left-y to original
                        float xMax = ((spanOutput[iOffset + 0] + spanOutput[iOffset + 2] / 2) - xPad) / gain; // Unpad bbox bottom-right-x to original
                        float yMax = ((spanOutput[iOffset + 1] + spanOutput[iOffset + 3] / 2) - yPad) / gain; // Unpad bbox bottom-right-y to original

                        xMin = Clamp(xMin, 0, width); // Clip bbox top-left-x to boundaries
                        yMin = Clamp(yMin, 0, height); // Clip bbox top-left-y to boundaries
                        xMax = Clamp(xMax, 0, width - 1); // Clip bbox bottom-right-x to boundaries
                        yMax = Clamp(yMax, 0, height - 1); // Clip bbox bottom-right-y to boundaries

                        YoloPrediction prediction = new YoloPrediction(objectType, spanOutput[iOffset + j], new Rectangle((int)xMin, (int)yMin, (int)(xMax - xMin), (int)(yMax - yMin)));

                        result.Add(prediction);
                    }
                }

                return result.ToArray();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float Clamp(float value, float min, float max) => (value < min) ? min : (value > max) ? max : value;

        private List<YoloPrediction> Suppress(YoloPrediction[] predictions)
        {
            List<YoloPrediction> result = new List<YoloPrediction>(predictions);

            foreach (YoloPrediction prediction in predictions)
            {
                foreach (YoloPrediction current in result.ToArray())
                {
                    if (current == prediction)
                        continue;

                    if (Metrics.IntersectionOverUnion(prediction.CurrentBoundingBox, current.CurrentBoundingBox) >= _yoloModel.Overlap)
                    {
                        if (prediction.Confidence >= current.Confidence)
                        {
                            result.Remove(current);
                        }
                    }
                }
            }

            return result;
        }
    }
}
