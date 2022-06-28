using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POC
{
    public class ConduitGrid
    {
        public Element Conduit { get; set; }
        public Line ConduitLine { get; set; }
        public XYZ StartPoint { get; set; }
        public XYZ EndPoint { get; set; }
        public XYZ MidPoint { get; set; }
        public Element RefConduit { get; set; }
        public Line RefLine { get; set; }
        public double Distance { get; set; }
        public ConduitGrid(Element a_Conduit, Element a_RefConduit, double a_Distance)
        {
            Conduit = a_Conduit;
            RefConduit = a_RefConduit;
            Distance = a_Distance;
            ConduitLine = (Conduit.Location as LocationCurve).Curve as Line;
            RefLine = (a_RefConduit.Location as LocationCurve).Curve as Line;
            StartPoint = ConduitLine.GetEndPoint(0);
            StartPoint = new XYZ(StartPoint.X, StartPoint.Y, 0);
            EndPoint = ConduitLine.GetEndPoint(1);
            EndPoint = new XYZ(EndPoint.X, EndPoint.Y, 0);
            MidPoint = (StartPoint + EndPoint) / 2;
        }
    }
}
