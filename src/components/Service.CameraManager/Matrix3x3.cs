using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Service.CameraManager
{
    internal class Matrix3x3
    {
        public float M11 { get; set; }
        public float M12 { get; set; }
        public float M13 { get; set; }
        public float M21 { get; set; }
        public float M22 { get; set; }
        public float M23 { get; set; }
        public float M31 { get; set; }
        public float M32 { get; set; }
        public float M33 { get; set; }

        public static Matrix3x3 operator *(Matrix3x3 a, Matrix3x3 b)
        {
            return new Matrix3x3
            {
                M11 = a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31,
                M12 = a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32,
                M13 = a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33,
                M21 = a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31,
                M22 = a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32,
                M23 = a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33,
                M31 = a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31,
                M32 = a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32,
                M33 = a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33
            };
        }

        public static Vector3 operator *(Matrix3x3 matrix, Vector3 point)
        {
            return new Vector3(
                matrix.M11 * point.X + matrix.M12 * point.Y + matrix.M13 * point.Z,
                matrix.M21 * point.X + matrix.M22 * point.Y + matrix.M23 * point.Z,
                matrix.M31 * point.X + matrix.M32 * point.Y + matrix.M33 * point.Z
            );
        }

        // 计算相机的旋转矩阵
        public static Matrix3x3 CalculateCameraRotationMatrix(float pitch, float yaw, float roll)
        {
            // 计算相机的旋转矩阵
            var cameraRotationMatrix = new Matrix3x3
            {
                M11 = MathF.Cos(yaw) * MathF.Cos(roll),
                M12 = MathF.Cos(yaw) * MathF.Sin(roll) * MathF.Sin(pitch) - MathF.Sin(yaw) * MathF.Cos(pitch),
                M13 = MathF.Cos(yaw) * MathF.Sin(roll) * MathF.Cos(pitch) + MathF.Sin(yaw) * MathF.Sin(pitch),
                M21 = MathF.Sin(yaw) * MathF.Cos(roll),
                M22 = MathF.Sin(yaw) * MathF.Sin(roll) * MathF.Sin(pitch) + MathF.Cos(yaw) * MathF.Cos(pitch),
                M23 = MathF.Sin(yaw) * MathF.Sin(roll) * MathF.Cos(pitch) - MathF.Cos(yaw) * MathF.Sin(pitch),
                M31 = -MathF.Sin(roll),
                M32 = MathF.Cos(roll) * MathF.Sin(pitch),
                M33 = MathF.Cos(roll) * MathF.Cos(pitch)
            };

            return cameraRotationMatrix;
        }
    }
}
