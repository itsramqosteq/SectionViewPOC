using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using POC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace POC
{
    [Transaction(TransactionMode.Manual)]

    public class SampleHandlerBACKUP : IExternalEventHandler
    {
        DateTime startDate = DateTime.UtcNow;
        UIDocument _uiDoc = null;
        Document _doc = null;
        public UIApplication _uiApp = null;
        public Element _element = null;
        public void Execute(UIApplication uiApp)
        {
            _uiApp = uiApp;
            _uiDoc = uiApp.ActiveUIDocument;
            _doc = _uiDoc.Document;
            int.TryParse(uiApp.Application.VersionNumber, out int RevitVersion);
            string offsetVariable = RevitVersion < 2020 ? "Offset" : "Middle Elevation";

            //List<Element> elements1 = Utility.GetPickedElements(_uiDoc, "trail", typeof(Conduit), true);

            //FilteredElementCollector _collector = new FilteredElementCollector(_doc);
            //List<Element> tags = _collector.OfCategory(BuiltInCategory.OST_ConduitTags).ToElements().ToList().Where(x => x.Name.ToLower().Contains("snv")).ToList();
            //if (tags.Count > 0)
            //    using (Transaction transaction = new Transaction(_doc))
            //    {
            //        transaction.Start("SampleHandler");
            //        int viewScale = _doc.ActiveView.Scale;
            //        foreach (Element e in elements1)
            //        {
            //            var refcon = Reference.ParseFromStableRepresentation(_doc, e.UniqueId);
            //            BoundingBoxXYZ bx = e.get_BoundingBox(_doc.ActiveView);
            //            XYZ midpoint = Utility.GetConduitMidPoint(e);
            //            IndependentTag newtag = IndependentTag.Create(_doc, tags[0].Id, _doc.ActiveView.Id, refcon, true, TagOrientation.Horizontal, midpoint);
            //            newtag.LeaderEndCondition = LeaderEndCondition.Free;
            //            BoundingBoxXYZ boxXYZ = newtag.get_BoundingBox(_doc.ActiveView);

            //            XYZ one =Utility.GetXYvalue(boxXYZ.Min) ;
            //            XYZ three = Utility.GetXYvalue(boxXYZ.Max);
            //            XYZ two = new XYZ(one.X, three.Y, 0);
            //            XYZ four = new XYZ(three.X, one.Y, 0);
            //            //Utility.CreateConduit(_doc, elements1[0] as Conduit, one, two);
            //            //Utility.CreateConduit(_doc, elements1[0] as Conduit, two, three);
            //            //Utility.CreateConduit(_doc, elements1[0] as Conduit, three, four);
            //            double height = one.DistanceTo(two);
            //          double width = two.DistanceTo(three);
            //        }
            //        transaction.Commit();
            //    }

            //return;
            try
            {


                View view = _doc.ActiveView;
                UIView uiview = null;
                IList<UIView> uiviews = _uiDoc.GetOpenUIViews();

                foreach (UIView uv in uiviews)
                {
                    if (uv.ViewId.Equals(view.Id))
                    {
                        uiview = uv;
                        break;
                    }
                }



                Rectangle rect = uiview.GetWindowRectangle();
                IList<XYZ> corners = uiview.GetZoomCorners();
                XYZ viewPoint1 = corners[0];
                XYZ viewPoint2 = new XYZ(corners[0].X, corners[1].Y, 0);
                XYZ viewPoint3 = corners[1];
                XYZ iewPoint4 = new XYZ(corners[1].X, corners[0].Y, 0);

                FilteredElementCollector collectors = new FilteredElementCollector(_doc, _doc.ActiveView.Id);
                ICollection<Element> collections = collectors.OfClass(typeof(Conduit)).ToElements();
                double maxElevation = 0;
                double minElevation = 1000000;
                foreach (Element item in collections)
                {
                    double ele = item.LookupParameter("Top Elevation").AsDouble();
                    if (ele > maxElevation)
                    {
                        maxElevation = ele;
                    }
                    else if (ele < minElevation)
                    {
                        minElevation = ele;
                    }
                }



                Outline myOutLn = new Outline(new XYZ(viewPoint1.X, viewPoint1.Y, minElevation), new XYZ(viewPoint3.X, viewPoint3.Y, maxElevation));


                BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(myOutLn);


                FilteredElementCollector collector = new FilteredElementCollector(_doc, _doc.ActiveView.Id);
                List<Element> elementsCollector = collector.OfClass(typeof(Conduit)).WherePasses(filter).ToElements().ToList();

                Dictionary<int, List<ConduitGrid>> conduitGridDictionary = GroupElementsByFilter.GroupByElements(elementsCollector, offsetVariable);
                using (Transaction transaction = new Transaction(_doc))
                {
                    transaction.Start("SampleHandler");
                    startDate = DateTime.UtcNow;
                    foreach (KeyValuePair<int, List<ConduitGrid>> kvp in conduitGridDictionary)
                    {

                        if (kvp.Value.Any())
                        {
                            List<Element> elements = kvp.Value.Select(r => r.Conduit).ToList();
                            List<XYZ> minList = new List<XYZ>();
                            List<XYZ> maxList = new List<XYZ>();
                            foreach (Element item in elements)
                            {
                                BoundingBoxXYZ boundingBox = item.get_BoundingBox(_doc.ActiveView);
                                minList.Add(boundingBox.Min);
                                maxList.Add(boundingBox.Max);
                            }
                            minList.OrderByDescending(p => p.X).ThenBy(t => t.Y).ToList();
                            maxList.OrderBy(p => p.X).ThenBy(t => t.Y).ToList();
                            XYZ point1 = new XYZ(minList.OrderBy(p => p.X).First().X, minList.OrderBy(p => p.Y).First().Y, 0);
                            XYZ point2 = new XYZ(maxList.OrderByDescending(p => p.X).First().X, maxList.OrderByDescending(p => p.Y).First().Y, 0);
                            XYZ midPoint = (point1 + point2) / 2;
                            double length = (point1.DistanceTo(point2)) / 2;
                            KeyValuePair<XYZ, XYZ> crossPoints3 = Utility.CrossProduct(elements[0] , midPoint, length);
                            Line line = Line.CreateBound(Utility.GetXYvalue(crossPoints3.Key), Utility.GetXYvalue(crossPoints3.Value));
                            crossPoints3 = Utility.CrossProduct(line, midPoint, length);
                            KeyValuePair<XYZ, XYZ> crossPoints = Utility.CrossProduct(elements[0], crossPoints3.Key, 10);
                            KeyValuePair<XYZ, XYZ> crossPoints1 = Utility.CrossProduct(elements[0], crossPoints3.Value, 10);


                            Line sideLineV = Line.CreateBound(crossPoints.Key, crossPoints.Value);
                            Line anotherSideLineV = Line.CreateBound(crossPoints1.Key, crossPoints1.Value);

                            Line sideLineH = null;
                            Line anotherSideLineH = null;

                            //Utility.CreateConduit(_doc, (elements[0] as Conduit), crossPoints.Key, crossPoints.Value);
                            //Utility.CreateConduit(_doc, (elements[0] as Conduit), crossPoints1.Key, crossPoints1.Value);
                            if (crossPoints.Key.DistanceTo(crossPoints1.Key) < crossPoints.Key.DistanceTo(crossPoints1.Value))
                            {
                                //Utility.CreateConduit(_doc, (elements[0] as Conduit), crossPoints.Key, crossPoints1.Key);
                                //Utility.CreateConduit(_doc, (elements[0] as Conduit), crossPoints.Value, crossPoints1.Value);
                                sideLineH = Line.CreateBound(crossPoints.Key, crossPoints1.Key);
                                anotherSideLineH = Line.CreateBound(crossPoints.Value, crossPoints1.Value);
                            }
                            else
                            {
                                //Utility.CreateConduit(_doc, (elements[0] as Conduit), crossPoints.Key, crossPoints1.Value);
                                //Utility.CreateConduit(_doc, (elements[0] as Conduit), crossPoints.Value, crossPoints1.Key);
                                sideLineH = Line.CreateBound(crossPoints.Key, crossPoints1.Value);
                                anotherSideLineH = Line.CreateBound(crossPoints.Value, crossPoints1.Key);
                            }
                            List<LineCollection> horizontalLineList = new List<LineCollection>();
                            List<LineCollection> verticalLineList = new List<LineCollection>();
                            LineCollection obj = new LineCollection();
                            obj.line = anotherSideLineV;
                            verticalLineList.Add(obj);
                            obj = new LineCollection();
                            obj.line = anotherSideLineH;
                            horizontalLineList.Add(obj);

                            double round = Math.Floor((length * 2) / 3);

                            List<Line> lineList = new List<Line>();
                            for (int i = 0; i < round; i++)
                            {
                                double value = (length * 2) - (3 * (i + 1));
                                XYZ endpoint = crossPoints.Key + sideLineH.Direction.Multiply(value);
                                KeyValuePair<XYZ, XYZ> crossPoints4 = Utility.CrossProduct(elements[0], endpoint, 30);
                                XYZ intersection = Utility.FindIntersectionPoint(crossPoints3.Key, crossPoints3.Value, crossPoints4.Key, crossPoints4.Value);
                                crossPoints4 = Utility.CrossProduct(elements[0] , intersection, 10);

                                obj = new LineCollection();
                                obj.line = Line.CreateBound(crossPoints4.Key, crossPoints4.Value);
                                verticalLineList.Add(obj);
                            }
                            if (((length * 2) / 3) <= round)
                            {
                                obj = new LineCollection();
                                obj.line = sideLineV;
                                verticalLineList.Add(obj);
                            }

                            round = Math.Floor((sideLineV.Length) / 3);
                            for (int i = 0; i < round; i++)
                            {
                                double value = sideLineV.Length - (3 * (i + 1));
                                XYZ endpoint = crossPoints.Key + sideLineV.Direction.Multiply(value);
                                KeyValuePair<XYZ, XYZ> crossPoints4 = Utility.CrossProduct(sideLineV, endpoint, sideLineH.Length * 3);
                                XYZ intersection = Utility.FindIntersectionPoint(crossPoints4.Key, crossPoints4.Value, crossPoints1.Key, crossPoints1.Value);
                                obj = new LineCollection();
                                obj.line = Line.CreateBound(endpoint, intersection);
                                horizontalLineList.Add(obj);
                            }
                            if ((sideLineV.Length) / 3 <= round)
                            {
                                obj = new LineCollection();
                                obj.line = sideLineH;
                                horizontalLineList.Add(obj);
                            }
                            //foreach (LineCollection item in verticalLineList)
                            //{
                            //    Utility.CreateConduit(_doc, (elements[0] as Conduit), item.start, item.end);
                            //}
                            //foreach (LineCollection item in horizontalLineList)
                            //{
                            //    Utility.CreateConduit(_doc, (elements[0] as Conduit), item.start, item.end);
                            //}
                            List<List<XYZ>> gridCells = new List<List<XYZ>>();
                            for (int k = 0; k < horizontalLineList.Count - 1; k++)
                            {


                                for (int i = 0; i < verticalLineList.Count - 1; i++)
                                {

                                    List<XYZ> xYZs = new List<XYZ>();
                                    LineCollection first_V = verticalLineList[i];
                                    LineCollection sec_V = verticalLineList[i + 1];
                                    LineCollection first_H = horizontalLineList[k];
                                    LineCollection sec_H = horizontalLineList[k + 1];
                                    XYZ intersection1 = Utility.FindIntersectionPoint(first_V.start, first_V.end, first_H.start, first_H.end);
                                    XYZ intersection11 = Utility.FindIntersectionPoint(sec_V.start, sec_V.end, first_H.start, first_H.end);
                                    XYZ intersection2 = Utility.FindIntersectionPoint(first_V.start, first_V.end, sec_H.start, sec_H.end);
                                    XYZ intersection22 = Utility.FindIntersectionPoint(sec_V.start, sec_V.end, sec_H.start, sec_H.end);
                                    //Utility.CreateConduit(_doc, (elements[0] as Conduit), new XYZ(intersection1.X, intersection1.Y, maxElevation), new XYZ(intersection11.X, intersection11.Y, maxElevation));
                                    //Utility.CreateConduit(_doc, (elements[0] as Conduit), new XYZ(intersection11.X, intersection11.Y, maxElevation), new XYZ(intersection22.X, intersection22.Y, maxElevation));
                                    //Utility.CreateConduit(_doc, (elements[0] as Conduit), new XYZ(intersection22.X, intersection22.Y, maxElevation), new XYZ(intersection2.X, intersection2.Y, maxElevation));
                                    //Utility.CreateConduit(_doc, (elements[0] as Conduit), new XYZ(intersection2.X, intersection2.Y, maxElevation), new XYZ(intersection1.X, intersection1.Y, maxElevation));

                                    //Utility.CreateConduit(_doc, (elements[0] as Conduit), new XYZ(intersection1.X, intersection1.Y, minElevation), new XYZ(intersection11.X, intersection11.Y, minElevation));
                                    //Utility.CreateConduit(_doc, (elements[0] as Conduit), new XYZ(intersection11.X, intersection11.Y, minElevation), new XYZ(intersection22.X, intersection22.Y, minElevation));
                                    //Utility.CreateConduit(_doc, (elements[0] as Conduit), new XYZ(intersection22.X, intersection22.Y, minElevation), new XYZ(intersection2.X, intersection2.Y, minElevation));
                                    //Utility.CreateConduit(_doc, (elements[0] as Conduit), new XYZ(intersection2.X, intersection2.Y, minElevation), new XYZ(intersection1.X, intersection1.Y, minElevation));

                                    //Utility.CreateConduit(_doc, (elements[0] as Conduit), new XYZ(intersection1.X, intersection1.Y, maxElevation), new XYZ(intersection1.X, intersection1.Y, minElevation));
                                    //Utility.CreateConduit(_doc, (elements[0] as Conduit), new XYZ(intersection11.X, intersection11.Y, maxElevation), new XYZ(intersection11.X, intersection11.Y, minElevation));
                                    //Utility.CreateConduit(_doc, (elements[0] as Conduit), new XYZ(intersection22.X, intersection22.Y, maxElevation), new XYZ(intersection22.X, intersection22.Y, minElevation));
                                    //Utility.CreateConduit(_doc, (elements[0] as Conduit), new XYZ(intersection2.X, intersection2.Y, maxElevation), new XYZ(intersection2.X, intersection2.Y, minElevation));





                                    List<Curve> profile = new List<Curve>();

                                    //profile.Add(Line.CreateBound(new XYZ(intersection1.X, intersection1.Y, maxElevation), new XYZ(intersection11.X, intersection11.Y, maxElevation)));
                                    //profile.Add(Line.CreateBound(new XYZ(intersection11.X, intersection11.Y, maxElevation), new XYZ(intersection22.X, intersection22.Y, maxElevation)));
                                    //profile.Add(Line.CreateBound(new XYZ(intersection22.X, intersection22.Y, maxElevation), new XYZ(intersection2.X, intersection2.Y, maxElevation)));
                                    //profile.Add(Line.CreateBound(new XYZ(intersection2.X, intersection2.Y, maxElevation), new XYZ(intersection1.X, intersection1.Y, maxElevation)));

                                    profile.Add(Line.CreateBound(new XYZ(intersection1.X, intersection1.Y, minElevation), new XYZ(intersection11.X, intersection11.Y, minElevation)));
                                    profile.Add(Line.CreateBound(new XYZ(intersection11.X, intersection11.Y, minElevation), new XYZ(intersection22.X, intersection22.Y, minElevation)));
                                    profile.Add(Line.CreateBound(new XYZ(intersection22.X, intersection22.Y, minElevation), new XYZ(intersection2.X, intersection2.Y, minElevation)));
                                    profile.Add(Line.CreateBound(new XYZ(intersection2.X, intersection2.Y, minElevation), new XYZ(intersection1.X, intersection1.Y, minElevation)));

                                    //profile.Add(Line.CreateBound(new XYZ(intersection1.X, intersection1.Y, maxElevation), new XYZ(intersection1.X, intersection1.Y, minElevation)));
                                    //profile.Add(Line.CreateBound(new XYZ(intersection11.X, intersection11.Y, maxElevation), new XYZ(intersection11.X, intersection11.Y, minElevation)));
                                    //profile.Add(Line.CreateBound(new XYZ(intersection22.X, intersection22.Y, maxElevation), new XYZ(intersection22.X, intersection22.Y, minElevation)));
                                    //profile.Add(Line.CreateBound(new XYZ(intersection2.X, intersection2.Y, maxElevation), new XYZ(intersection2.X, intersection2.Y, minElevation)));

                                    CurveLoop curveLoop = CurveLoop.Create(profile);


                                    SolidOptions options = new SolidOptions(
                                    ElementId.InvalidElementId,
                                    ElementId.InvalidElementId);

                                    Solid newSolid = GeometryCreationUtilities
                                                         .CreateExtrusionGeometry(
                                                         new CurveLoop[] { curveLoop },
                                                         XYZ.BasisZ, maxElevation, options);

                                    var ds = DirectShape.CreateElement(_doc, new ElementId(BuiltInCategory.OST_GenericModel));
                                    ds.SetName("Cube");
                                    ds.SetShape(new List<GeometryObject>() { newSolid });
                                    //Solid newSolid = SolidBoundingBox(cube1, maxElevation);

                                    ElementIntersectsSolidFilter solidfilter
                                                               = new ElementIntersectsSolidFilter(newSolid);

                                    List<Element> fixtures = new FilteredElementCollector(_doc, _doc.ActiveView.Id).WhereElementIsNotElementType().WherePasses(solidfilter).ToList();
                                    if (fixtures.Count == 0)
                                    {
                                        Conduit conduit1 = Utility.CreateConduit(_doc, (elements[0] as Conduit), intersection1, intersection11);
                                        Conduit conduit2 = Utility.CreateConduit(_doc, (elements[0] as Conduit), intersection11, intersection22);
                                        Conduit conduit3 = Utility.CreateConduit(_doc, (elements[0] as Conduit), intersection22, intersection2);
                                        Conduit conduit4 = Utility.CreateConduit(_doc, (elements[0] as Conduit), intersection2, intersection1);
                                        //Conduit conduit5= Utility.CreateConduit(_doc, (elements[0] as Conduit), intersection11, intersection2);
                                        Utility.SetAlertColor(conduit1.Id, _uiDoc);
                                        Utility.SetAlertColor(conduit2.Id, _uiDoc);
                                        Utility.SetAlertColor(conduit3.Id, _uiDoc);
                                        Utility.SetAlertColor(conduit4.Id, _uiDoc);
                                        //Utility.SetAlertColor(conduit5.Id, _uiDoc);
                                        //  PaintSolid(_uiApp, newSolid, 3);
                                    }
                                    else
                                    {
                                        _doc.Delete(ds.Id);
                                    }

                                    ////Utility.CreateConduit(_doc, (elements[0] as Conduit), intersection11, intersection2);
                                    //// MessageBox.Show("");
                                    //Line line1 = Line.CreateBound(new XYZ(intersection11.X, intersection11.Y, maxElevation), new XYZ(intersection2.X, intersection2.Y, minElevation));
                                    //XYZ midpoint = (line1.GetEndPoint(0) + line1.GetEndPoint(1)) / 2;
                                    //double len = (line1.Length / 2) - 0.1;
                                    //XYZ a = midpoint + line1.Direction.Multiply(len);
                                    //XYZ b = midpoint - line1.Direction.Multiply(len);



                                    //try
                                    //{
                                    //    BoundingBoxIntersectsFilter filter1 = new BoundingBoxIntersectsFilter(myOutLn1);
                                    //    FilteredElementCollector collector1 = new FilteredElementCollector(_doc, _doc.ActiveView.Id);
                                    //    List<Element> elementsCollector1 = collector1.OfClass(typeof(Conduit)).WherePasses(filter1).ToElements().ToList();
                                    //    if (elementsCollector1.Count == 0)
                                    //    {

                                    //        Conduit conduit1 = Utility.CreateConduit(_doc, (elements[0] as Conduit), intersection1, intersection11);
                                    //        Conduit conduit2 = Utility.CreateConduit(_doc, (elements[0] as Conduit), intersection11, intersection22);
                                    //        Conduit conduit3 = Utility.CreateConduit(_doc, (elements[0] as Conduit), intersection22, intersection2);
                                    //        Conduit conduit4 = Utility.CreateConduit(_doc, (elements[0] as Conduit), intersection2, intersection1);
                                    //        //Conduit conduit5= Utility.CreateConduit(_doc, (elements[0] as Conduit), intersection11, intersection2);
                                    //        Utility.SetAlertColor(conduit1.Id, _uiDoc);
                                    //        Utility.SetAlertColor(conduit2.Id, _uiDoc);
                                    //        Utility.SetAlertColor(conduit3.Id, _uiDoc);
                                    //        Utility.SetAlertColor(conduit4.Id, _uiDoc);
                                    //        //Utility.SetAlertColor(conduit5.Id, _uiDoc);
                                    //    }
                                    //    else
                                    //    {
                                    //        Conduit conduit5 = Utility.CreateConduit(_doc, (elements[0] as Conduit), myOutLn1.MinimumPoint, myOutLn1.MaximumPoint);

                                    //    }
                                    //}
                                    //catch (Exception)
                                    //{
                                    //    Conduit con = null;
                                    //    using (SubTransaction sub = new SubTransaction(_doc))
                                    //    {
                                    //        sub.Start();
                                    //        con = Utility.CreateConduit(_doc, (elements[0] as Conduit), a, b);

                                    //        sub.Commit();
                                    //        _doc.Regenerate();
                                    //    }

                                    //    BoundingBoxXYZ boxXYZs = con.get_BoundingBox(_doc.ActiveView);
                                    //    myOutLn1 = new Outline(boxXYZs.Min, boxXYZs.Max);
                                    //    using (SubTransaction sub = new SubTransaction(_doc))
                                    //    {
                                    //        sub.Start();
                                    //        _doc.Delete(con.Id);
                                    //        sub.Commit();
                                    //        _doc.Regenerate();
                                    //    }
                                    //    BoundingBoxIntersectsFilter filter1 = new BoundingBoxIntersectsFilter(myOutLn1);
                                    //    FilteredElementCollector collector1 = new FilteredElementCollector(_doc, _doc.ActiveView.Id);
                                    //    List<Element> elementsCollector1 = collector1.OfClass(typeof(Conduit)).WherePasses(filter1).ToElements().ToList();
                                    //    if (elementsCollector1.Count == 0)
                                    //    {

                                    //        Conduit conduit1 = Utility.CreateConduit(_doc, (elements[0] as Conduit), intersection1, intersection11);
                                    //        Conduit conduit2 = Utility.CreateConduit(_doc, (elements[0] as Conduit), intersection11, intersection22);
                                    //        Conduit conduit3 = Utility.CreateConduit(_doc, (elements[0] as Conduit), intersection22, intersection2);
                                    //        Conduit conduit4 = Utility.CreateConduit(_doc, (elements[0] as Conduit), intersection2, intersection1);
                                    //        //Conduit conduit5= Utility.CreateConduit(_doc, (elements[0] as Conduit), intersection11, intersection2);
                                    //        Utility.SetAlertColor(conduit1.Id, _uiDoc);
                                    //        Utility.SetAlertColor(conduit2.Id, _uiDoc);
                                    //        Utility.SetAlertColor(conduit3.Id, _uiDoc);
                                    //        Utility.SetAlertColor(conduit4.Id, _uiDoc);
                                    //        //Conduit conduit5 = Utility.CreateConduit(_doc, (elements[0] as Conduit), myOutLn1.MinimumPoint, myOutLn1.MaximumPoint);

                                    //        //Utility.SetAlertColor(conduit5.Id, _uiDoc);
                                    //    }
                                    //    else
                                    //    {
                                    //        Conduit conduit5 = Utility.CreateConduit(_doc, (elements[0] as Conduit), myOutLn1.MinimumPoint, myOutLn1.MaximumPoint);

                                    //    }
                                    //    //bool isTrue = true;
                                    //    //Line linez = Line.CreateBound(intersection11, intersection2);
                                    //    //foreach (Conduit e in elementsCollector)
                                    //    //{
                                    //    //    Line line4 = Utility.GetLineFromConduit(e, true);
                                    //    //    XYZ xYZ = Utility.GetIntersection(linez, line4);
                                    //    //    if (xYZ != null)
                                    //    //    {
                                    //    //        isTrue = false;
                                    //    //        break;
                                    //    //    }
                                    //    //}
                                    //    //if (isTrue)
                                    //    //{
                                    //    //    Conduit conduit1 = Utility.CreateConduit(_doc, (elements[0] as Conduit), intersection1, intersection11);
                                    //    //    Conduit conduit2 = Utility.CreateConduit(_doc, (elements[0] as Conduit), intersection11, intersection22);
                                    //    //    Conduit conduit3 = Utility.CreateConduit(_doc, (elements[0] as Conduit), intersection22, intersection2);
                                    //    //    Conduit conduit4 = Utility.CreateConduit(_doc, (elements[0] as Conduit), intersection2, intersection1);
                                    //    //    //Conduit conduit5= Utility.CreateConduit(_doc, (elements[0] as Conduit), intersection11, intersection2);
                                    //    //    Utility.SetAlertColor(conduit1.Id, _uiDoc);
                                    //    //    Utility.SetAlertColor(conduit2.Id, _uiDoc);
                                    //    //    Utility.SetAlertColor(conduit3.Id, _uiDoc);
                                    //    //    Utility.SetAlertColor(conduit4.Id, _uiDoc);
                                    //    //}
                                    //}


                                }

                            }
                        }

                    }
                    transaction.Commit();
                }
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show("Some error has occured. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //Task task = Utility.UserActivityLog(uiApp, Util.ApplicationWindowTitle, startDate, "Failed", "SampleHandler");
            }


        }

        public static Solid SolidBoundingBox(Solid inputSolid, double maxElevation)
        {
            try
            {
                List<Face> faces = new List<Face>();
                foreach (Face l_face in inputSolid.Faces)
                {
                    if (l_face is PlanarFace)
                        faces.Add(l_face);
                }
                if (faces.Any(r => (r as PlanarFace).FaceNormal.Z == 1))
                {
                    Face face = faces.FirstOrDefault(r => (r as PlanarFace).FaceNormal.Z == 1);

                    if (maxElevation > 0)
                    {
                        List<CurveLoop> loopList = face.GetEdgesAsCurveLoops().ToList();
                        Solid preTransformBox = GeometryCreationUtilities.CreateExtrusionGeometry(loopList, XYZ.BasisZ, maxElevation);
                        BoundingBoxXYZ bb = preTransformBox.GetBoundingBox();
                        return preTransformBox;
                    }
                    else
                        return null;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
        static Solid CreateCube(double d, Conduit reference, Document doc)
        {
            return CreateRectangularPrism(
                 XYZ.Zero, d, d, d, reference, doc);
        }
        static Solid CreateRectangularPrism(
                 XYZ center,
                 double d1,
                 double d2,
                 double d3, Conduit reference, Document doc)
        {
            List<Curve> profile = new List<Curve>();
            XYZ profile00 = new XYZ(-d1 / 2, -d2 / 2, -d3 / 2);
            XYZ profile01 = new XYZ(-d1 / 2, d2 / 2, -d3 / 2);
            XYZ profile11 = new XYZ(d1 / 2, d2 / 2, -d3 / 2);
            XYZ profile10 = new XYZ(d1 / 2, -d2 / 2, -d3 / 2);
            Utility.CreateConduit(doc, reference, profile00, profile01);
            Utility.CreateConduit(doc, reference, profile01, profile11);
            Utility.CreateConduit(doc, reference, profile11, profile10);
            Utility.CreateConduit(doc, reference, profile10, profile00);
            profile.Add(Line.CreateBound(profile00, profile01));
            profile.Add(Line.CreateBound(profile01, profile11));
            profile.Add(Line.CreateBound(profile11, profile10));
            profile.Add(Line.CreateBound(profile10, profile00));

            CurveLoop curveLoop = CurveLoop.Create(profile);

            SolidOptions options = new SolidOptions(
              ElementId.InvalidElementId,
              ElementId.InvalidElementId);

            return GeometryCreationUtilities
              .CreateExtrusionGeometry(
                new CurveLoop[] { curveLoop },
                XYZ.BasisZ, d3, options);
        }
        static public Solid CreateSphereAt(
    XYZ centre,
    double radius)
        {
            // Use the standard global coordinate system 
            // as a frame, translated to the sphere centre.

            Frame frame = new Frame(centre, XYZ.BasisX,
              XYZ.BasisY, XYZ.BasisZ);

            // Create a vertical half-circle loop 
            // that must be in the frame location.

            Arc arc = Arc.Create(
              centre - radius * XYZ.BasisZ,
              centre + radius * XYZ.BasisZ,
              centre + radius * XYZ.BasisX);

            Line line = Line.CreateBound(
              arc.GetEndPoint(1),
              arc.GetEndPoint(0));

            CurveLoop halfCircle = new CurveLoop();
            halfCircle.Append(arc);
            halfCircle.Append(line);

            List<CurveLoop> loops = new List<CurveLoop>(1);
            loops.Add(halfCircle);

            return GeometryCreationUtilities
              .CreateRevolvedGeometry(frame, loops,
                0, 2 * Math.PI);
        }
        private static int schemaId = -1;

        static void PaintSolid(
         UIApplication uiApp,
         Solid s,
         double value)
        {
            Autodesk.Revit.ApplicationServices.Application app = uiApp.Application;
            View view = uiApp.ActiveUIDocument.Document.ActiveView;

            if (view.AnalysisDisplayStyleId
              == ElementId.InvalidElementId)
            {
                CreateAVFDisplayStyle(uiApp.ActiveUIDocument.Document, view);
            }

            SpatialFieldManager sfm = SpatialFieldManager
              .GetSpatialFieldManager(view);

            if (null == sfm)
            {
                sfm = SpatialFieldManager
                  .CreateSpatialFieldManager(view, 1);
            }

            if (-1 != schemaId)
            {
                IList<int> results = sfm.GetRegisteredResults();
                if (!results.Contains(schemaId))
                {
                    schemaId = -1;
                }
            }

            if (-1 == schemaId)
            {
                AnalysisResultSchema resultSchema1
                  = new AnalysisResultSchema("PaintedSolid",
                    "Description");

                schemaId = sfm.RegisterResult(resultSchema1);
            }

            FaceArray faces = s.Faces;
            Transform trf = Transform.Identity;
            foreach (Face face in faces)
            {
                int idx = sfm.AddSpatialFieldPrimitive(face, trf);
                IList<UV> uvPts = new List<UV>();
                List<double> doubleList = new List<double>();
                IList<ValueAtPoint> valList = new List<ValueAtPoint>();
                BoundingBoxUV bb = face.GetBoundingBox();
                uvPts.Add(bb.Min);
                doubleList.Add(value);
                valList.Add(new ValueAtPoint(doubleList));

                FieldDomainPointsByUV pnts
                  = new FieldDomainPointsByUV(uvPts);

                FieldValues vals = new FieldValues(valList);
                sfm.UpdateSpatialFieldPrimitive(idx, pnts,
                  vals, schemaId);
            }
        }

        static void CreateAVFDisplayStyle(
          Document doc,
          View view)
        {
            using (SubTransaction t = new SubTransaction(doc))
            {
                t.Start();

                AnalysisDisplayColoredSurfaceSettings
                  coloredSurfaceSettings = new
                    AnalysisDisplayColoredSurfaceSettings();

                coloredSurfaceSettings.ShowGridLines = true;

                AnalysisDisplayColorSettings colorSettings
                  = new AnalysisDisplayColorSettings();

                AnalysisDisplayLegendSettings legendSettings
                  = new AnalysisDisplayLegendSettings();

                legendSettings.ShowLegend = false;

                AnalysisDisplayStyle analysisDisplayStyle
                  = AnalysisDisplayStyle.CreateAnalysisDisplayStyle(
                    doc, "Paint Solid", coloredSurfaceSettings,
                    colorSettings, legendSettings);

                view.AnalysisDisplayStyleId = analysisDisplayStyle.Id;

                t.Commit();
            }
        }
        public string GetName()
        {
            return "Revit Addin";
        }
    }
}

