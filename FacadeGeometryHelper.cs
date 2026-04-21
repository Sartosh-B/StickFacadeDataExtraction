using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;

namespace StickFacadeDataExtraction
{
    public static class FacadeGeometryHelper
    {
        public static bool IsPointOnLine(Point2d pt, Point2d lineStart, Point2d lineEnd, double tolerance = 1e-6)
        {
            
            // Is point within the bounding box of the line segment (AABB check)
            if (pt.X < System.Math.Min(lineStart.X, lineEnd.X) - tolerance || pt.X > System.Math.Max(lineStart.X, lineEnd.X) + tolerance)
                return false;
            if (pt.Y < System.Math.Min(lineStart.Y, lineEnd.Y) - tolerance || pt.Y > System.Math.Max(lineStart.Y, lineEnd.Y) + tolerance)
                return false;

            // Check if the three points are collinear (calculate the area of the triangle)
            double area = 0.5 * System.Math.Abs(
                (lineEnd.X - lineStart.X) * (pt.Y - lineStart.Y) -
                (pt.X - lineStart.X) * (lineEnd.Y - lineStart.Y)
            );

            return area < tolerance;
        }
    }
}
