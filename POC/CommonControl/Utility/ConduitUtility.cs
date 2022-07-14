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
        public static double GetConduitLength(Element e)
        {
            return e.LookupParameter("Length").AsDouble();
        }
        public static Line GetLineFromConduit(Conduit conduit, bool setZvalueZero = false)
        {
            if (!setZvalueZero)
                return (conduit.Location as LocationCurve).Curve as Line;
            else
            {
                Line line = (conduit.Location as LocationCurve).Curve as Line;
                return Line.CreateBound(GetXYvalue(line.GetEndPoint(0)), GetXYvalue(line.GetEndPoint(1)));
            }
        }
        public static Line GetLineFromConduit(Element conduit, bool setZvalueZero = false)
        {
            if (!setZvalueZero)
                return ((conduit as Conduit).Location as LocationCurve).Curve as Line;
            else
            {
                Line line = ((conduit as Conduit).Location as LocationCurve).Curve as Line;
                return Line.CreateBound(GetXYvalue(line.GetEndPoint(0)), GetXYvalue(line.GetEndPoint(1)));
            }
        }
        public static Conduit GetMinLengthConduit(List<Conduit> conduits)
        {
            return conduits.OrderBy(x => x.LookupParameter("Length").AsDouble()).FirstOrDefault();
        }
        public static Element GetMinLengthConduit(List<Element> conduits)
        {
            return conduits.OrderBy(x => x.LookupParameter("Length").AsDouble()).FirstOrDefault();
        }
        public static Conduit GetMaxLengthConduit(List<Conduit> conduits)
        {
            return conduits.OrderByDescending(x => x.LookupParameter("Length").AsDouble()).FirstOrDefault();
        }
        public static Element GetMaxLengthConduit(List<Element> conduits)
        {
            return conduits.OrderByDescending(x => x.LookupParameter("Length").AsDouble()).FirstOrDefault();
        }
        public static XYZ GetConduitMidPoint(Element element)
        {
            Line line = GetLineFromConduit(element as Conduit);
            return GetMidPoint(line.GetEndPoint(0), line.GetEndPoint(1));

        }

        public static void GetStartEndPointForConduit(Conduit conduit, out XYZ startPoint, out XYZ endPoint)
        {

            Line conduitLine = (conduit.Location as LocationCurve).Curve as Line;
            startPoint = conduitLine.GetEndPoint(0);
            endPoint = conduitLine.GetEndPoint(1);
        }
        public static Conduit CreateConduit(Document doc, Conduit refConduit, Line line)
        {
            ElementId condtypeId = refConduit.GetTypeId();
            ElementId referenceLevel = refConduit.LookupParameter("Reference Level").AsElementId();
            double diameter = refConduit.LookupParameter("Diameter(Trade Size)").AsDouble();
            Conduit newConduit = Conduit.Create(doc, condtypeId, line.GetEndPoint(0), line.GetEndPoint(1), referenceLevel);
            Parameter diap = newConduit.LookupParameter("Diameter(Trade Size)");
            diap.Set(diameter);
            return newConduit;
        }
        public static Conduit CreateConduit(Document doc, Element refConduit, Line line)
        {
            if (refConduit is Conduit conduit)
            {
                ElementId condtypeId = conduit.GetTypeId();
                ElementId referenceLevel = conduit.LookupParameter("Reference Level").AsElementId();
                double diameter = conduit.LookupParameter("Diameter(Trade Size)").AsDouble();
                Conduit newConduit = Conduit.Create(doc, condtypeId, line.GetEndPoint(0), line.GetEndPoint(1), referenceLevel);
                Parameter diap = newConduit.LookupParameter("Diameter(Trade Size)");
                diap.Set(diameter);
                return newConduit;
            }
            else
            {
                return null;
            }
        }
        public static Conduit CreateConduit(Document doc, Conduit refConduit, XYZ startPoint, XYZ endPoint)
        {
            ElementId condtypeId = refConduit.GetTypeId();
            ElementId referenceLevel = refConduit.LookupParameter("Reference Level").AsElementId();
            double diameter = refConduit.LookupParameter("Diameter(Trade Size)").AsDouble();
            Conduit newConduit = Conduit.Create(doc, condtypeId, startPoint, endPoint, referenceLevel);
            Parameter diap = newConduit.LookupParameter("Diameter(Trade Size)");
            diap.Set(diameter);
            return newConduit;
        }
        public static Conduit CreateConduit(Document doc, Element refConduit, XYZ startPoint, XYZ endPoint)
        {
            if (refConduit is Conduit conduit)
            {
                ElementId condtypeId = conduit.GetTypeId();
                ElementId referenceLevel = conduit.LookupParameter("Reference Level").AsElementId();
                double diameter = conduit.LookupParameter("Diameter(Trade Size)").AsDouble();
                Conduit newConduit = Conduit.Create(doc, condtypeId, startPoint, endPoint, referenceLevel);
                Parameter diap = newConduit.LookupParameter("Diameter(Trade Size)");
                diap.Set(diameter);
                return newConduit;
            }
            else
            {
                return null;
            }
        }
        public static XYZ GetMidPoint(Element element, bool setZvalueZero = false)
        {
            Line line = GetLineFromConduit(element, setZvalueZero);
            return (line.GetEndPoint(0) + line.GetEndPoint(1)) / 2;
        }
        public static XYZ GetMidPoint(Conduit conduit, bool setZvalueZero = false)
        {
            Line line = GetLineFromConduit(conduit, setZvalueZero);
            return (line.GetEndPoint(0) + line.GetEndPoint(1)) / 2;
        }
        public static XYZ GetMidPoint(Line line, bool setZvalueZero = false)
        {
            XYZ midPoint = (line.GetEndPoint(0) + line.GetEndPoint(1)) / 2;
            return setZvalueZero ? GetXYvalue(midPoint) : midPoint;
        }
        public static List<Element> OrderTheConduit(List<Element> conduitList)
        {
            XYZ midPoint = GetMidPoint(conduitList[0], true);
            KeyValuePair<XYZ, XYZ> crossProduct = CrossProduct(conduitList[0], midPoint, 100, true);
            Line line = Line.CreateBound(crossProduct.Key, crossProduct.Value);
            SortedDictionary<double, Tuple<XYZ, Element>> orderPoints = new SortedDictionary<double, Tuple<XYZ, Element>>();
            orderPoints.Add(0, new Tuple<XYZ, Element>(midPoint, conduitList[0]));
            foreach (Element e in conduitList.Skip(1))
            {
                Line cline = GetLineFromConduit(e, true);
                XYZ point = Utility.GetIntersection(cline, line);
                if (point != null && !orderPoints.Any(x => x.Key == midPoint.DistanceTo(point)))
                {
                    orderPoints.Add(midPoint.DistanceTo(point), new Tuple<XYZ, Element>(point, e));
                }
            }
            midPoint = orderPoints.LastOrDefault().Value.Item1;
            SortedDictionary<double, Tuple<XYZ, Element>> finalList = new SortedDictionary<double, Tuple<XYZ, Element>>();
            finalList.Add(0, orderPoints.LastOrDefault().Value);
            foreach (KeyValuePair<double, Tuple<XYZ, Element>> keyValue in orderPoints.Reverse().Skip(1))
            {
                Line cline = GetLineFromConduit(keyValue.Value.Item2, true);
                XYZ point = Utility.GetIntersection(cline, line);
                if (point != null && !finalList.Any(x => x.Key == midPoint.DistanceTo(point)))
                {
                    finalList.Add(midPoint.DistanceTo(point), new Tuple<XYZ, Element>(point, keyValue.Value.Item2));
                }
            }

            return finalList.Select(x => x.Value.Item2).ToList();
        }
        public static Conduit FindOuterConduit(List<Element> conduitlist)
        {
            Conduit outerconduit = null;
            List<double> distancecollection = new List<double>();
            double max_distance;
            XYZ point1 = (conduitlist[0].Location as LocationCurve).Curve.GetEndPoint(0);
            XYZ point2 = (conduitlist[0].Location as LocationCurve).Curve.GetEndPoint(1);
            if (Math.Round(point1.X, 4) == Math.Round(point2.X, 4) && Math.Round(point1.Y, 4) == Math.Round(point2.Y, 4))
            {
                foreach (Conduit cond in conduitlist)
                {
                    if (conduitlist[0].Id != cond.Id)
                    {
                        //findintersection
                        XYZ con1_firstconduitpoint = (conduitlist[0].Location as LocationCurve).Curve.GetEndPoint(0);
                        XYZ con2_firstconduitpoint = (cond.Location as LocationCurve).Curve.GetEndPoint(0);
                        double distance = Math.Pow(con1_firstconduitpoint.X - con2_firstconduitpoint.X, 2) + Math.Pow(con1_firstconduitpoint.Y - con2_firstconduitpoint.Y, 2);
                        distance = Math.Sqrt(distance);
                        distancecollection.Add(distance);

                    }
                }
                max_distance = distancecollection.Max<double>();
                foreach (Conduit cond in conduitlist)
                {
                    if (conduitlist[0].Id != cond.Id)
                    {
                        //findintersection
                        XYZ con1_firstconduitpoint = (conduitlist[0].Location as LocationCurve).Curve.GetEndPoint(0);
                        XYZ con2_firstconduitpoint = (cond.Location as LocationCurve).Curve.GetEndPoint(0);
                        XYZ con2_secondconduitpoint = (cond.Location as LocationCurve).Curve.GetEndPoint(1);
                        double distance = Math.Pow(con1_firstconduitpoint.X - con2_firstconduitpoint.X, 2) + Math.Pow(con1_firstconduitpoint.Y - con2_firstconduitpoint.Y, 2);
                        distance = Math.Sqrt(distance);

                        if (distance == max_distance)
                        {
                            outerconduit = cond;
                        }

                    }
                }
                return outerconduit;

            }
            else
            {
                foreach (Conduit cond in conduitlist.Skip(1))
                {

                    //findintersection
                    Line conduitline_dir = (conduitlist[0].Location as LocationCurve).Curve as Line;
                    XYZ cond1_crossproduct = conduitline_dir.Direction.CrossProduct(XYZ.BasisZ);
                    XYZ con1_firstconduitpoint = (conduitlist[0].Location as LocationCurve).Curve.GetEndPoint(0);
                    XYZ con1_secondconduitpoint = con1_firstconduitpoint + cond1_crossproduct.Multiply(5);

                    XYZ con2_firstconduitpoint = (cond.Location as LocationCurve).Curve.GetEndPoint(0);
                    XYZ con2_secondconduitpoint = (cond.Location as LocationCurve).Curve.GetEndPoint(1);

                    XYZ intersectionpoint = FindIntersectionPoint(con1_firstconduitpoint, con1_secondconduitpoint, con2_firstconduitpoint, con2_secondconduitpoint);
                    double distance = 0;
                    if (intersectionpoint != null)
                        distance = Math.Pow(con1_firstconduitpoint.X - intersectionpoint.X, 2) + Math.Pow(con1_firstconduitpoint.Y - intersectionpoint.Y, 2);

                    distance = Math.Sqrt(distance);
                    distancecollection.Add(distance);


                }
                max_distance = distancecollection.Max<double>();
                foreach (Conduit cond in conduitlist.Skip(1))
                {

                    //findintersection
                    Line conduitline_dir = (conduitlist[0].Location as LocationCurve).Curve as Line;
                    XYZ cond1_crossproduct = conduitline_dir.Direction.CrossProduct(XYZ.BasisZ);
                    XYZ con1_firstconduitpoint = (conduitlist[0].Location as LocationCurve).Curve.GetEndPoint(0);
                    XYZ con1_secondconduitpoint = con1_firstconduitpoint + cond1_crossproduct.Multiply(5);

                    XYZ con2_firstconduitpoint = (cond.Location as LocationCurve).Curve.GetEndPoint(0);
                    XYZ con2_secondconduitpoint = (cond.Location as LocationCurve).Curve.GetEndPoint(1);

                    XYZ intersectionpoint = FindIntersectionPoint(con1_firstconduitpoint, con1_secondconduitpoint, con2_firstconduitpoint, con2_secondconduitpoint);
                    double distance = 0;
                    if (intersectionpoint != null)
                        distance = Math.Pow(con1_firstconduitpoint.X - intersectionpoint.X, 2) + Math.Pow(con1_firstconduitpoint.Y - intersectionpoint.Y, 2);
                    distance = Math.Sqrt(distance);

                    if (distance == max_distance)
                    {
                        outerconduit = cond;
                    }


                }
                return outerconduit;
            }
        }
        public static List<Element> ConduitInOrder(List<Element> conduits)
        {
            Conduit outerconduit = FindOuterConduit(conduits);
            List<Element> orederedconduits = new List<Element>();
            List<double> dis_collection = new List<double>();
            XYZ point1 = (conduits[0].Location as LocationCurve).Curve.GetEndPoint(0);
            XYZ point2 = (conduits[0].Location as LocationCurve).Curve.GetEndPoint(1);
            if (Math.Round(point1.X, 4) == Math.Round(point2.X, 4) && Math.Round(point1.Y, 4) == Math.Round(point2.Y, 4))
            {

                foreach (Conduit cond in conduits)
                {
                    if (outerconduit.Id != cond.Id)
                    {
                        //findintersection
                        XYZ con1_firstconduitpoint = (outerconduit.Location as LocationCurve).Curve.GetEndPoint(0);
                        XYZ con2_firstconduitpoint = (cond.Location as LocationCurve).Curve.GetEndPoint(0);
                        XYZ con2_secondconduitpoint = (cond.Location as LocationCurve).Curve.GetEndPoint(1);

                        double distance = Math.Pow(con1_firstconduitpoint.X - con2_firstconduitpoint.X, 2) + Math.Pow(con1_firstconduitpoint.Y - con2_firstconduitpoint.Y, 2);
                        distance = Math.Sqrt(distance);
                        dis_collection.Add(distance);

                    }
                }
                dis_collection = dis_collection.OrderBy(o => o).ToList();
                orederedconduits.Add(outerconduit);
                foreach (double dou in dis_collection)
                {
                    foreach (Conduit cond in conduits)
                    {
                        if (outerconduit.Id != cond.Id)
                        {
                            //findintersection
                            XYZ con1_firstconduitpoint = (outerconduit.Location as LocationCurve).Curve.GetEndPoint(0);
                            XYZ con2_firstconduitpoint = (cond.Location as LocationCurve).Curve.GetEndPoint(0);
                            XYZ con2_secondconduitpoint = (cond.Location as LocationCurve).Curve.GetEndPoint(1);

                            double distance = Math.Pow(con1_firstconduitpoint.X - con2_firstconduitpoint.X, 2) + Math.Pow(con1_firstconduitpoint.Y - con2_firstconduitpoint.Y, 2);
                            distance = Math.Sqrt(distance);

                            if (dou == distance)
                            {
                                orederedconduits.Add(cond);
                            }

                        }
                    }
                }
                return orederedconduits;
            }
            else
            {
                foreach (Conduit cond in conduits)
                {
                    if (outerconduit.Id != cond.Id)
                    {
                        //findintersection
                        Line conduitline_dir = (outerconduit.Location as LocationCurve).Curve as Line;
                        XYZ cond1_crossproduct = conduitline_dir.Direction.CrossProduct(XYZ.BasisZ);
                        XYZ con1_firstconduitpoint = (outerconduit.Location as LocationCurve).Curve.GetEndPoint(0);
                        XYZ con1_secondconduitpoint = con1_firstconduitpoint + cond1_crossproduct.Multiply(5);

                        XYZ con2_firstconduitpoint = (cond.Location as LocationCurve).Curve.GetEndPoint(0);
                        XYZ con2_secondconduitpoint = (cond.Location as LocationCurve).Curve.GetEndPoint(1);

                        XYZ intersectionpoint = FindIntersectionPoint(con1_firstconduitpoint, con1_secondconduitpoint, con2_firstconduitpoint, con2_secondconduitpoint);
                        double distance = 0;
                        if (intersectionpoint != null)
                            distance = Math.Pow(con1_firstconduitpoint.X - intersectionpoint.X, 2) + Math.Pow(con1_firstconduitpoint.Y - intersectionpoint.Y, 2);
                        distance = Math.Sqrt(distance);
                        dis_collection.Add(distance);

                    }
                }
                dis_collection = dis_collection.OrderBy(o => o).ToList();
                orederedconduits.Add(outerconduit);
                foreach (double dou in dis_collection)
                {
                    foreach (Conduit cond in conduits)
                    {
                        if (outerconduit.Id != cond.Id)
                        {
                            //findintersection
                            Line conduitline_dir = (outerconduit.Location as LocationCurve).Curve as Line;
                            XYZ cond1_crossproduct = conduitline_dir.Direction.CrossProduct(XYZ.BasisZ);
                            XYZ con1_firstconduitpoint = (outerconduit.Location as LocationCurve).Curve.GetEndPoint(0);
                            XYZ con1_secondconduitpoint = con1_firstconduitpoint + cond1_crossproduct.Multiply(5);

                            XYZ con2_firstconduitpoint = (cond.Location as LocationCurve).Curve.GetEndPoint(0);
                            XYZ con2_secondconduitpoint = (cond.Location as LocationCurve).Curve.GetEndPoint(1);

                            XYZ intersectionpoint = FindIntersectionPoint(con1_firstconduitpoint, con1_secondconduitpoint, con2_firstconduitpoint, con2_secondconduitpoint);
                            double distance = 0;
                            if (intersectionpoint != null)
                                distance = Math.Pow(con1_firstconduitpoint.X - intersectionpoint.X, 2) + Math.Pow(con1_firstconduitpoint.Y - intersectionpoint.Y, 2);
                            distance = Math.Sqrt(distance);

                            if (dou == distance)
                            {
                                orederedconduits.Add(cond);
                            }

                        }
                    }
                }
                return orederedconduits;
            }

        }
        public static Dictionary<Conduit, List<XYZ>> GetClosestSideAxisForConduit(List<Conduit> conduits)
        {
            Dictionary<Conduit, List<XYZ>> keyValuePairsFirst = new Dictionary<Conduit, List<XYZ>>();
            Dictionary<Conduit, List<XYZ>> keyValuePairsSec = new Dictionary<Conduit, List<XYZ>>();

            GetStartEndPointForConduit(conduits[0], out XYZ startKey, out XYZ endKey);
            keyValuePairsFirst.Add(conduits[0], new List<XYZ>() { startKey, endKey });
            keyValuePairsSec.Add(conduits[0], new List<XYZ>() { endKey, startKey });
            double startCount = 0;
            double endCount = 0;
            foreach (Conduit con in conduits.Skip(1))
            {

                GetStartEndPointForConduit(con, out XYZ start, out XYZ end);
                if (startKey.DistanceTo(start) < startKey.DistanceTo(end))
                {
                    keyValuePairsFirst.Add(con, new List<XYZ>() { start, end });
                    startCount += startKey.DistanceTo(start);
                }
                else
                {
                    keyValuePairsFirst.Add(con, new List<XYZ>() { end, start });
                    startCount += startKey.DistanceTo(end);
                }

                if (endKey.DistanceTo(end) < endKey.DistanceTo(start))
                {
                    keyValuePairsSec.Add(con, new List<XYZ>() { end, start });
                    endCount += endKey.DistanceTo(end);
                }
                else
                {
                    keyValuePairsSec.Add(con, new List<XYZ>() { start, end });
                    endCount += endKey.DistanceTo(start);
                }
            }

            return startCount < endCount ? keyValuePairsFirst : keyValuePairsSec;
        }
        public static List<Element> GetConduitsByReference(IList<Reference> References, Document doc)
        {
            List<Element> conduits = new List<Element>();
            foreach (Reference r in References)
            {
                Element e = doc.GetElement(r);
                if (e.GetType() == typeof(Conduit))
                {
                    conduits.Add(e);
                }
            }
            return conduits;
        }
    }
}
