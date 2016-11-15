using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using DotSpatial.Projections;
using DotSpatial.Topology;

namespace CSM.WaterUsage.Geography
{
    public class ProjectionConverter
    {
        private static readonly ProjectionInfo startProjection = KnownCoordinateSystems.Projected.StatePlaneNad1983Feet.NAD1983StatePlaneCaliforniaVFIPS0405Feet;
        private static readonly ProjectionInfo endProjection = KnownCoordinateSystems.Geographic.World.WGS1984;
        private static readonly int endSRID = 4326;

        /// <summary>
        /// Reproject a point or linestring from 2229 --> 4326
        /// </summary>
        public static DbGeometry Convert(DbGeometry geometry)
        {
            double[] xy = getPointArray(geometry).ToArray();
            bool reprojected = reprojectPoints(xy);

            if (reprojected)
            {
                var coordinates = getCoordinates(xy);

                if (geometry.SpatialTypeName.Equals("Point") && coordinates.Count() == 1)
                {
                    var point = new Point(coordinates.Single());
                    return DbGeometry.PointFromText(point.ToText(), endSRID);
                }
                else if (geometry.SpatialTypeName.Equals("LineString"))
                {
                    var linestring = new LineString(coordinates);
                    return DbGeometry.LineFromText(linestring.ToText(), endSRID);
                }
            }

            return null;
        }

        /// <summary>
        /// Returns an x-y interleaved point array
        /// [ x1, y1, x2, y2, .... ]
        /// </summary>
        private static IEnumerable<double> getPointArray(DbGeometry geometry)
        {
            //points indexing starts at 1
            for (int i = 1; i <= geometry.PointCount; i++)
            {
                var point = geometry.PointAt(i);
                if (validGeometry(point))
                {
                    yield return point.XCoordinate.Value;
                    yield return point.YCoordinate.Value;
                }
            }
        }

        /// <summary>
        /// Reprojects the point array *in-place*
        /// </summary>
        private static bool reprojectPoints(double[] points)
        {
            if (points != null && points.Any())
            {
                //hardcode a Z-value of 1
                var z = points.Take(points.Length / 2).Select(coord => 1D).ToArray();
                //2229 --> 4326
                Reproject.ReprojectPoints(points, z, startProjection, endProjection, 0, z.Length);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Create a coordinate collection from the points
        /// </summary>
        private static IEnumerable<Coordinate> getCoordinates(double[] points)
        {
            for (int i = 1; i < points.Length; i += 2)
            {
                yield return new Coordinate(points[i - 1], points[i]);
            }
        }

        private static bool validGeometry(DbGeometry geometry)
        {
            return geometry.XCoordinate.HasValue
                && geometry.YCoordinate.HasValue;
        }
    }
}
