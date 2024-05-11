using OpenCvSharp;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models;
using Sdcb.PaddleOCR.Models.Local;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcrManager
{
    public class PaddleOcrManager:IDisposable
    {
        private readonly FullOcrModel? model;
        private readonly PaddleOcrAll? ocrInfer;

        public PaddleOcrManager()
        {
            model = LocalFullModels.ChineseV4;
            ocrInfer = new(model, PaddleDevice.Gpu())
            {
                AllowRotateDetection = true,
                Enable180Classification = false,
            };
        }

        public void Dispose()
        {
            ocrInfer?.Dispose();
        }

        public string Ocr(Mat mat)
        {
            try
            {
                PaddleOcrResult result = ocrInfer?.Run(mat);
                Trace.TraceInformation("Detected all texts: \n" + result.Text);
                string plate = string.Empty;
                foreach (PaddleOcrResultRegion region in result.Regions)
                {
                    plate += region.Text;
                    Trace.TraceInformation($"Text: {region.Text}, Score: {region.Score}, RectCenter: {region.Rect.Center}, RectSize:    {region.Rect.Size}, Angle: {region.Rect.Angle}");
                }
                return plate;
            }
            catch (Exception ex)
            {
                Cv2.ImWrite("ErrorImage.jpg", mat);
                //throw;
                return String.Empty;
            }
            
        }

        
    }
}
