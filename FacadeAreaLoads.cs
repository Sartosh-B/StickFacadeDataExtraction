using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;

namespace StickFacadeDataExtraction
{
    public class FacadeAreaLoad
    {
        public string Layer { get; set; }
        public List<Point2d> Vertices { get; set; } = new List<Point2d>();

      
    }
    public class PressureAreaLoad : FacadeAreaLoad
    {
        public string Direction { get; set; }
        public string Zone { get; set; }
        public double Value { get; set; }
    }

    public class SuctionAreaLoad : FacadeAreaLoad
    {
        public string Direction { get; set; }
        public string Zone { get; set; }
        public double Value { get; set; }
    }
}

