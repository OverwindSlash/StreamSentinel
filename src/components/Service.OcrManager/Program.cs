using System.Diagnostics;
using System;
using OpenCvSharp;
using Sdcb.PaddleOCR.Models;
using Sdcb.PaddleOCR;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR.Models.Local;
using Sdcb.PaddleOCR.Models.Online;

namespace OcrManager
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            //RecognizationModel recModel = await LocalDictOnlineRecognizationModel.ChineseV3.DownloadAsync();
            //using (Mat src = Cv2.ImRead(@"./samples/5ghz.jpg"))
            //using (PaddleOcrRecognizer r = new PaddleOcrRecognizer(recModel, PaddleDevice.Gpu()))
            //{
            //    Stopwatch sw = Stopwatch.StartNew();
            //    PaddleOcrRecognizerResult result = r.Run(src);
            //    Console.WriteLine($"elapsed={sw.ElapsedMilliseconds}ms");
            //    Console.WriteLine(result.ToString());
            //}


            FullOcrModel model = LocalFullModels.ChineseV4;
            //FullOcrModel model = OnlineFullModels.ChineseV4.DownloadAsync().Result;
            byte[] sampleImageData = File.ReadAllBytes(@"./5ghz.jpg");

            using (PaddleOcrAll all = new(model, PaddleDevice.Gpu())
            {
                AllowRotateDetection = true,
                Enable180Classification = false,
            })
            {
                //for (global::System.Int32 i = 0; i < 100; i++)
                //{
                    // Load local file by following code:
                    // using (Mat src2 = Cv2.ImRead(@"C:\test.jpg"))
                    using (Mat src = Cv2.ImDecode(sampleImageData, ImreadModes.Color))
                    {
                        PaddleOcrResult result = all.Run(src);
                        Console.WriteLine("Detected all texts: \n" + result.Text);
                        foreach (PaddleOcrResultRegion region in result.Regions)
                        {
                            Console.WriteLine($"Text: {region.Text}, Score: {region.Score}, RectCenter: {region.Rect.Center}, RectSize:    {region.Rect.Size}, Angle: {region.Rect.Angle}");
                        }
                    }
                    
                //}
            }

            PaddleOcrManager paddleOcrManager = new PaddleOcrManager();
            var mat = Cv2.ImRead(@"./5ghz.jpg");
            //for (int i = 0; i < 100; i++)
            //{
                paddleOcrManager.Ocr(mat);
            //}
        }
    }
}
