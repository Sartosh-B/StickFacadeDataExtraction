using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.ApplicationServices;

namespace StickFacadeDataExtraction
{
    public class FacadeElement
    {
        public string Layer { get; set; }
        public Point2d Start { get; set; }
        public Point2d End { get; set; }


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
        public string Position { get; set; } // "MID" or "SID"
        public double? WindSuctionValue { get; set; }
        public double? WindPressureValue { get; set; }
        public double? DistanceToNeighbour1 { get; set; }
        public double? DistanceToNeighbour2 { get; set; }

        public string Tag { get; set; }

        public List<Transom> ConnectedTransoms { get; set; } = new List<Transom>();

        public void AssignWindLoadsFromConnectedTransoms()
        {
            var editor = Application.DocumentManager.MdiActiveDocument.Editor;

            editor.WriteMessage($"\n[DEBUG] Mullion: Start={Start}, End={End}, ConnectedTransoms={ConnectedTransoms.Count}");

            var valuesPressure = new List<double>();
            var valuesSuction = new List<double>();

            var pointsToCheck = new List<Point2d> { Start, End };

            foreach (var transom in ConnectedTransoms)
            {
                editor.WriteMessage($"\n[DEBUG]   Transom: Start={transom.Start}, End={transom.End}, Pressure={transom.WindPressureValue}, Suction={transom.WindSuctionValue}");

                pointsToCheck.Add(transom.Start);
                pointsToCheck.Add(transom.End);
                pointsToCheck.Add(transom.GetCenter());

                if (transom.WindPressureValue.HasValue)
                    valuesPressure.Add(transom.WindPressureValue.Value);
                if (transom.WindSuctionValue.HasValue)
                    valuesSuction.Add(transom.WindSuctionValue.Value);
            }

            WindPressureValue = valuesPressure.Count > 0 ? valuesPressure.Max() : 0.0;
            WindSuctionValue = valuesSuction.Count > 0 ? valuesSuction.Min() : 0.0;

            editor.WriteMessage($"\n[DEBUG] => Assigned to Mullion: Pressure={WindPressureValue}, Suction={WindSuctionValue}");
        }
    }



    public class Transom : FacadeElement
    {
        public string Position { get; set; } // "BOT", "MID", "TOP"
        public double? WindSuctionValue { get; set; }
        public double? WindPressureValue { get; set; }
        public double? DistanceToNeighbour1 { get; set; }
        public double? DistanceToNeighbour2 { get; set; }
        public Transom UpperNeighbour { get; set; }
        public Transom LowerNeighbour { get; set; }

        public string Tag { get; set; }

        public void AssignWindLoads(List<PressureAreaLoad> pressureLoads, List<SuctionAreaLoad> suctionLoads)
        {
            var pointsToCheck = new[] { Start, End, GetCenter() };

            var pressures = new List<double>();
            var suctions = new List<double>();

            foreach (var pt in pointsToCheck)
            {
                foreach (var pl in pressureLoads)
                {
                    if (PointInPolygon(pt, pl.Vertices))
                        pressures.Add(pl.Value);
                }

                foreach (var sl in suctionLoads)
                {
                    if (PointInPolygon(pt, sl.Vertices))
                        suctions.Add(sl.Value);
                }
            }

            WindPressureValue = pressures.Count > 0 ? pressures.Max() : 0.0;
            WindSuctionValue = suctions.Count > 0 ? suctions.Min() : 0.0;
        }
    }
}
