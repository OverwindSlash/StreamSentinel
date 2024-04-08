using Service.CameraManager;
using Service.CameraManager.Service;
using System.Numerics;

namespace CameraManager
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            // 将 JSON 配置写入临时文件
            string configFile = "config.json";

            CameraManagementService cameraManagementService = new CameraManagementService(configFile);

            var target = new TargetBase
            {
                BBox = new System.Drawing.RectangleF(480, 810, 100, 100),
                CommandSource = CommandSourceEnum.FixedCamera,
                Direction = DirectionEnum.Coming
            };

            cameraManagementService.LookTo(target);

            var relativeTarget = new TargetBase
            {
                BBox = new System.Drawing.RectangleF(1440, 810, 100, 100),
                CommandSource = CommandSourceEnum.PtzCamera,
                Direction = DirectionEnum.Coming
            };
            cameraManagementService.LookTo(relativeTarget);
        }
    }
}
