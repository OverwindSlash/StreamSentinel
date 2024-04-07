using DotSpatial.Projections;

namespace Service.CameraManager
{
    internal class CoordinateConverter
    {
        private static ProjectionInfo wgs84 = KnownCoordinateSystems.Geographic.World.WGS1984;
        private static ProjectionInfo mercator = KnownCoordinateSystems.Projected.World.Mercatorworld;

        public static double[] ToPlaneCoordinate(double longitude, double latitude)
        {
            double[] xy = new double[2] { longitude, latitude };
            double[] z = new double[1] { 0 };

            Reproject.ReprojectPoints(xy, z, wgs84, mercator, 0, 1);

            return xy;
        }

        public static double[] GetRelativePosition(double longitude1, double latitude1, double longitude2, double latitude2)
        {
            double[] xy1 = ToPlaneCoordinate(longitude1, latitude1);
            double[] xy2 = ToPlaneCoordinate(longitude2, latitude2);

            double[] relativePosition = new double[2];
            relativePosition[0] = xy2[0] - xy1[0];
            relativePosition[1] = xy2[1] - xy1[1];

            return relativePosition;
        }
    }
}
