using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
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

    public class SampleHandler_FilledRegion : IExternalEventHandler
    {
        DateTime startDate = DateTime.UtcNow;
        UIDocument _uiDoc = null;
        Document _doc = null;
        public void Execute(UIApplication uiApp)
        {

            _uiDoc = uiApp.ActiveUIDocument;
            _doc = _uiDoc.Document;
            int.TryParse(uiApp.Application.VersionNumber, out int RevitVersion);
            string offsetVariable = RevitVersion < 2020 ? "Offset" : "Middle Elevation";
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
                XYZ viewPoint4 = new XYZ(corners[1].X, corners[0].Y, 0);

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
                    FilteredElementCollector fillRegionTypes = new FilteredElementCollector(_doc)
                                       .OfClass(typeof(FilledRegionType));

                    ElementId myPatternId = fillRegionTypes.Cast<FilledRegionType>().FirstOrDefault(x => x.Name == "Diagonal Crosshatch").Id;

                    FilledRegion filledRegion = null;
                    List<CurveLoop> profileloops = new List<CurveLoop>();
                    CurveLoop profileloop = new CurveLoop();

                    XYZ[] points = new XYZ[5];
                    points[0] = viewPoint1;
                    points[1] = viewPoint2;
                    points[2] = viewPoint3;
                    points[3] = viewPoint4;
                    points[4] = viewPoint1;
                    for (int i = 0; i < 4; i++)
                    {
                        Line line = Line.CreateBound(points[i],
                          points[i + 1]);
                        profileloop.Append(line);
                    }
                    profileloops.Add(profileloop);
                    int loopCount = 0;
                    foreach (KeyValuePair<int, List<ConduitGrid>> kvp in conduitGridDictionary)
                    {

                        if (kvp.Value.Any())
                        {
                            loopCount++;
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
                            XYZ point3 = new XYZ(point1.X, point2.Y, 0);
                            XYZ point4 = new XYZ(point2.X, point1.Y, 0);

                            //  Conduit conduit = Utility.CreateConduit(_doc, elements[0] as Conduit, viewPoint1, viewPoint3);

                            points = new XYZ[5];
                            points[0] = point3;
                            points[1] = point1;
                            points[2] = point4;
                            points[3] = point2;
                            points[4] = point3;
                            profileloop = new CurveLoop();

                            for (int i = 0; i < 4; i++)
                            {
                                Line line = Line.CreateBound(points[i],
                                  points[i + 1]);

                                profileloop.Append(line);
                            }
                            profileloops.Add(profileloop);
                            if (conduitGridDictionary.Count() == loopCount)
                            {
                                ElementId activeViewId = _doc.ActiveView.Id;
                                filledRegion = FilledRegion.Create(
                                 _doc, myPatternId, activeViewId, profileloops);
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

        public string GetName()
        {
            return "Revit Addin";
        }
    }
}