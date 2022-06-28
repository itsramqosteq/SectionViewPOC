using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POC
{
    public partial class Utility
    {
        public static XYZ FindIntersection(Element element, Line Line)
        {
            Line ConduitLine = (element.Location as LocationCurve).Curve as Line;
            XYZ StartPoint = ConduitLine.GetEndPoint(0);
            XYZ EndPoint = ConduitLine.GetEndPoint(1);
            StartPoint = new XYZ(StartPoint.X, StartPoint.Y, 0);
            EndPoint = new XYZ(EndPoint.X, EndPoint.Y, 0);
            Line newLine = Line.CreateBound(StartPoint, EndPoint);
            XYZ stpt = Line.GetEndPoint(0);
            XYZ edpt = Line.GetEndPoint(1);
            Line = Line.CreateBound(new XYZ(stpt.X, stpt.Y, 0), new XYZ(edpt.X, edpt.Y, 0));
            return FindIntersectionPoint(newLine, Line);
        }
        public static XYZ FindIntersectionPoint(Line lineOne, Line lineTwo)
        {
            return FindIntersectionPoint(lineOne.GetEndPoint(0), lineOne.GetEndPoint(1), lineTwo.GetEndPoint(0), lineTwo.GetEndPoint(1));
        }
        public static XYZ FindIntersectionPoint(XYZ s1, XYZ e1, XYZ s2, XYZ e2)
        {
            s1 = XYZroundOf(s1, 5);
            e1 = XYZroundOf(e1, 5);
            s2 = XYZroundOf(s2, 5);
            e2 = XYZroundOf(e2, 5);
            double a1 = e1.Y - s1.Y;
            double b1 = s1.X - e1.X;
            double c1 = a1 * s1.X + b1 * s1.Y;

            double a2 = e2.Y - s2.Y;
            double b2 = s2.X - e2.X;
            double c2 = a2 * s2.X + b2 * s2.Y;

            double delta = a1 * b2 - a2 * b1;

            //If lines are parallel, the result will be (NaN, NaN).
            return delta == 0 || Convert.ToString(delta).Contains("E") == true ? null
                : new XYZ((b2 * c1 - b1 * c2) / delta, (a1 * c2 - a2 * c1) / delta, 0);
        }

        public static XYZ GetIntersection(Element element, ConduitGrid conGrid, XYZ Point,double maximumSpacing = 0.5)
        {
            try
            {
                double outerDiaOne = element.GetType() == typeof(Conduit) ? element.LookupParameter("Outside Diameter").AsDouble() : element.LookupParameter("Width").AsDouble();
                double outerDiaTwo = conGrid.Conduit.GetType() == typeof(Conduit) ? conGrid.Conduit.LookupParameter("Outside Diameter").AsDouble() : conGrid.Conduit.LookupParameter("Width").AsDouble();
                double radOne = outerDiaOne / 2;
                double radTwo = outerDiaTwo / 2;
                double multiplier = radOne + radTwo + maximumSpacing;
                XYZ direction = conGrid.ConduitLine.Direction;
                XYZ cross = direction.CrossProduct(XYZ.BasisZ);
                XYZ newStart = Point + cross.Multiply(multiplier);
                XYZ newEnd = Point - cross.Multiply(multiplier);
                Line verticalLine = Line.CreateBound(newStart, newEnd);
                Line ConduitLine = (element.Location as LocationCurve).Curve as Line;
                XYZ StartPoint = ConduitLine.GetEndPoint(0);
                XYZ EndPoint = ConduitLine.GetEndPoint(1);
                StartPoint = new XYZ(StartPoint.X, StartPoint.Y, 0);
                EndPoint = new XYZ(EndPoint.X, EndPoint.Y, 0);
                Line newLine = Line.CreateBound(StartPoint, EndPoint);
                return GetIntersection(newLine, verticalLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }
      
        public static XYZ GetIntersection(Line line1, Line line2)
        {
            SetComparisonResult result
              = line1.Intersect(line2, out IntersectionResultArray results);

            if (result != SetComparisonResult.Overlap)
                return null;

            if (results == null || results.Size != 1)
                return null;

            IntersectionResult iResult
              = results.get_Item(0);

            return iResult.XYZPoint;
        }
        public static bool GetHorizontalLine(Line line)
        {
            if ((Convert.ToString(line.Direction.Y).Contains("E") || line.Direction.Y == 0) && !((Convert.ToString(line.Direction.X).Contains("E") || line.Direction.Y == 0)))
            {
                return true;
            }
            return false;

        }
    }
}
