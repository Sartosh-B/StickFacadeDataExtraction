using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;

namespace StickFacadeDataExtraction
{
    public class FacadeElement
    {
        public string Layer { get; set; }
        public Point2d Start { get; set; }
        public Point2d End { get; set; }

        public virtual void AnalyzeMetadata()
        {
            // Tu można później rozszerzyć analizę nazwy warstwy, pozycji itp.
        }
        public Point2d GetCenter()
        {
            return new Point2d((Start.X + End.X) / 2.0, (Start.Y + End.Y) / 2.0);
        }
        public static bool PointInPolygon(Point2d point, List<Point2d> polygon)
        {
            int n = polygon.Count;
            bool inside = false;

            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                Point2d pi = polygon[i];
                Point2d pj = polygon[j];

                bool intersect = ((pi.Y > point.Y) != (pj.Y > point.Y)) &&
                                 (point.X < (pj.X - pi.X) * (point.Y - pi.Y) / (pj.Y - pi.Y + 1e-10) + pi.X);

                if (intersect)
                    inside = !inside;
            }

            return inside;
        }
    }

    public class Mullion : FacadeElement
    {
        public string Position { get; set; } // "MID" lub "SID"
        public double? WindSuctionValue { get; set; }
        public double? WindPressureValue { get; set; }
        public double? DistanceToNeighbour1 { get; set; }
        public double? DistanceToNeighbour2 { get; set; }
        public void AssignWindLoads(List<PressureAreaLoad> pressureLoads, List<SuctionAreaLoad> suctionLoads)
        {
            Point2d center = GetCenter();

            foreach (var load in pressureLoads)
            {
                if (PointInPolygon(center, load.Vertices))
                {
                    WindPressureValue = load.Value;
                    break;
                }
            }

            foreach (var load in suctionLoads)
            {
                if (PointInPolygon(center, load.Vertices))
                {
                    WindSuctionValue = load.Value;
                    break;
                }
            }
        }
    }

    public class Transom : FacadeElement
    {
        public string Position { get; set; } // "BOT", "MID", "TOP"
        public double? WindSuctionValue { get; set; }
        public double? WindPressureValue { get; set; }
        public double? DistanceToNeighbour1 { get; set; }
        public double? DistanceToNeighbour2 { get; set; }

        public void AssignWindLoads(List<PressureAreaLoad> pressureLoads, List<SuctionAreaLoad> suctionLoads)
        {
            Point2d center = GetCenter();

            foreach (var load in pressureLoads)
            {
                if (PointInPolygon(center, load.Vertices))
                {
                    WindPressureValue = load.Value;
                    break;
                }
            }

            foreach (var load in suctionLoads)
            {
                if (PointInPolygon(center, load.Vertices))
                {
                    WindSuctionValue = load.Value;
                    break;
                }
            }
        }
    }
}
