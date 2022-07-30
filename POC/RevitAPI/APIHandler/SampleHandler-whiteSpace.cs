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

    public class SampleHandlerWhiteSpace : IExternalEventHandler
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

                double cellWidth = 3;
                double cellHeight = 3;
                using (Transaction transaction = new Transaction(_doc))
                {
                    transaction.Start("SampleHandler");
                    startDate = DateTime.UtcNow;
                    foreach (KeyValuePair<int, List<ConduitGrid>> kvp in conduitGridDictionary)
                    {

                        if (kvp.Value.Any())
                        {

                            List<Element> elements = kvp.Value.Select(r => r.Conduit).ToList();
                            if (elements.Count > 1)
                            {
                                elements = Utility.OrderTheConduit(elements);

                                Element firstElement = elements.First();
                                Element lastElement = elements.Last();
                                Line firstLine = Utility.GetLineFromConduit(firstElement);
                                Line lastLine = Utility.GetLineFromConduit(lastElement);
                                FindCellPlacement(firstLine, lastLine,  out Line startCrossLine, out Line endCrossLine);
                                Utility.CreateConduit(_doc, firstElement, startCrossLine);
                                Utility.CreateConduit(_doc, firstElement, endCrossLine);
                                FindCellPlacement(lastLine, firstLine, out  startCrossLine, out  endCrossLine);
                                Utility.CreateConduit(_doc, firstElement, startCrossLine);
                                Utility.CreateConduit(_doc, firstElement, endCrossLine);



                            }
                            else
                            {

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


        private bool FindCellPlacement(Line firstLine,Line lastLine, out Line startCrossLine, out Line endCrossLine)
        {
           
            startCrossLine = Utility.CrossProductForwardLine(firstLine, firstLine.GetEndPoint(0), 10);
            endCrossLine = null;
            bool isForwardDirction = Utility.GetIntersection(startCrossLine, lastLine) == null;
            if (isForwardDirction)
            {
                endCrossLine = Utility.CrossProductForwardLine(firstLine, firstLine.GetEndPoint(1), 10);
                return true;
            }
            else
            {
                startCrossLine = Utility.CrossProductBackwardLine(firstLine, firstLine.GetEndPoint(0), 10);
                endCrossLine = Utility.CrossProductBackwardLine(firstLine, firstLine.GetEndPoint(1), 10);
                return false;
            }

            
        }

        public string GetName()
        {
            return "Revit Addin";
        }
    }
}

