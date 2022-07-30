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

    public class SectionViewHandler : IExternalEventHandler
    {
        DateTime startDate = DateTime.UtcNow;
        UIDocument _uiDoc = null;
        Document _doc = null;
        public UIApplication _uiApp = null;
        public Element _element = null;
        double _minLevel = 0;
        double _maxLevel = 0;
        List<ViewSection> _unStrutSections = new List<ViewSection>();
        bool isSkipLoop = false;
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

                FilteredElementCollector collector = new FilteredElementCollector(_doc, _doc.ActiveView.Id);
                ICollection<Element> collections = collector.OfClass(typeof(Conduit)).ToElements();
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



                List<Element> elementsCollector = Utility.GetPickedElements(_uiDoc, "Select the conduit route", typeof(Conduit));
                Dictionary<int, List<ConduitGrid>> conduitGridDictionary = new Dictionary<int, List<ConduitGrid>>();
                ElementGroupByOrder elementGroupByOrder = new ElementGroupByOrder();
                if (elementsCollector.Count == 0)
                {
                    BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(myOutLn);
                    FilteredElementCollector collectors = new FilteredElementCollector(_doc, _doc.ActiveView.Id);
                    elementsCollector = collectors.OfClass(typeof(Conduit)).WherePasses(filter).ToElements().ToList();
                    conduitGridDictionary = GroupElementsByFilter.GroupByElements(elementsCollector, offsetVariable);

                }
                else
                {
                    Level lvl = (_doc.GetElement(_doc.ActiveView.GenLevel.Id)) as Level;
                    _minLevel = lvl.Elevation;
                    List<Element> LvlCollection = new FilteredElementCollector(_doc).OfClass(typeof(Autodesk.Revit.DB.Level)).ToList();
                    if (LvlCollection.OrderByDescending(r => (r as Level).Elevation).ToList().Any(r => (r as Level).Elevation > _minLevel))
                    {
                        if (LvlCollection.OrderByDescending(r => (r as Level).Elevation).ToList().LastOrDefault(r => (r as Level).Elevation > _minLevel) is Level abovelevel)
                        {
                            _minLevel = abovelevel.Elevation;
                        }
                    }
                    List<List<Element>> elementsMapped = new List<List<Element>>();
                    List<List<Element>> elementsMappedForReference = new List<List<Element>>();

                    List<Element> elementsMerged = new List<Element>();
                    elementsMerged.AddRange(elementsCollector);
                    foreach (Conduit conduit in elementsCollector)
                    {
                        List<Element> runElements = new List<Element>();
                        GetRunElementsByFilter.ConduitSelection(_doc, conduit, null, ref runElements, true);
                        elementsMapped.Add(runElements);
                        elementsMerged.AddRange(runElements.Where(x => x is Conduit).ToList().Skip(1));

                    }
                    conduitGridDictionary = GroupElementsByFilter.GroupByElements(elementsMerged, offsetVariable);
                    elementGroupByOrder.CurrentElement = elementsCollector.OrderBy(x => x.LookupParameter("Length").AsDouble()).ToList();
                    elementGroupByOrder.RunElements = elementsMapped;
                    elementsMappedForReference.Add(elementsCollector);
                    conduitGridDictionary.Remove(conduitGridDictionary.FirstOrDefault().Key);
                    elementGroupByOrder = RecursiveLoopForFindTheBranchInOrder(conduitGridDictionary, elementGroupByOrder);
                }


                _unStrutSections.Clear();
                using (Transaction transaction = new Transaction(_doc))
                {
                    transaction.Start("SampleHandler");

                    using (SubTransaction subTransaction = new SubTransaction(_doc))
                    {
                        subTransaction.Start();

                        startDate = DateTime.UtcNow;
                        ViewFamilyType viewFamilyType = new FilteredElementCollector(_doc)
                                                    .OfClass(typeof(ViewFamilyType))
                                                    .Cast<ViewFamilyType>()
                                                    .FirstOrDefault<ViewFamilyType>(x => ViewFamily.Section == x.ViewFamily);

                        List<Element> GridCollection = new FilteredElementCollector(_doc, _doc.ActiveView.Id).OfClass(typeof(Autodesk.Revit.DB.Grid)).ToList();
                        FilteredElementCollector strutCollector = new FilteredElementCollector(_doc, _doc.ActiveView.Id);

                        List<Element> strutElement = strutCollector.OfClass(typeof(FamilyInstance)).ToElements().ToList().Where(x => Utility.GetFamilyInstanceName(x as FamilyInstance).ToLower().Contains("strut")).ToList();


                        Dictionary<Element, Line> strutCollection = new Dictionary<Element, Line>();
                        List<DetailLine> detailLines = new List<DetailLine>();
                        foreach (FamilyInstance str in strutElement)
                        {

                            XYZ locationpoint = (str.Location as LocationPoint).Point;
                            double angle = (str.Location as LocationPoint).Rotation;
                            BoundingBoxXYZ bb = str.get_BoundingBox(_doc.ActiveView);
                            if (bb != null)
                            {
                                XYZ bbMin = bb.Min;
                                XYZ bbMax = bb.Max;
                                Line bbLine = Line.CreateBound(new XYZ(bbMin.X, bbMin.Y, 0), new XYZ(bbMax.X, bbMax.Y, 0));
                                if (bbLine != null)
                                {
                                    double strlength = bbLine.Length;
                                    strlength /= 2;
                                    // locationpoint = (bbLine.GetEndPoint(0) + bbLine.GetEndPoint(1)) / 2;
                                    XYZ pt1 = new XYZ(locationpoint.X, locationpoint.Y, 0);
                                    XYZ pt2 = new XYZ(locationpoint.X + strlength, locationpoint.Y, 0);
                                    XYZ pt3 = new XYZ(locationpoint.X - strlength, locationpoint.Y, 0);
                                    Line line1 = Line.CreateBound(pt2, pt3);
                                    Line axis = Line.CreateBound(pt1, new XYZ(pt1.X, pt1.Y, pt1.Z + 1));

                                    DetailLine curve = _doc.Create.NewDetailCurve(_doc.ActiveView, line1) as DetailLine;
                                    ElementTransformUtils.RotateElement(_doc, curve.Id, axis, angle);
                                    detailLines.Add(curve);
                                    Line supportline = (curve.Location as LocationCurve).Curve as Line;
                                    strutCollection.Add(str, supportline);



                                }
                            }
                        }
                        RecursiveLoopForBoundingBox(elementGroupByOrder, GridCollection, strutCollection);

                        foreach (DetailLine item in detailLines)
                        {
                            _doc.Delete(item.Id);
                        }
                        subTransaction.Commit();
                        _doc.Regenerate();
                    }
                    using (SubTransaction subTransaction1 = new SubTransaction(_doc))
                    {
                        subTransaction1.Start();

                        FilteredElementCollector collectorsdss = new FilteredElementCollector(_doc, _doc.ActiveView.Id);
                        List<Element> elementsCollectorsdd = collectorsdss.OfCategory(BuiltInCategory.OST_Viewers).ToElements().ToList();
                        ViewFamilyType viewFamilyType = (ViewFamilyType)ParentUserControl.Instance.FamilyForViewSheetBox.SelectionBoxItem;
                        foreach (Element item in elementsCollectorsdd)
                        {
                            if (_doc.GetElement(item.GetTypeId()).Name == viewFamilyType.Name && _unStrutSections.Any(x => x.Name == item.Name))
                            {
                                Utility.SetAlertColor(item.Id, _uiDoc);
                            }
                        }


                        subTransaction1.Commit();
                    }
                    transaction.Commit();
                }

            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show("Some error has occured. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }

        private void RecursiveLoopForBoundingBox(ElementGroupByOrder elementGroupByOrder, List<Element> GridCollection, Dictionary<Element, Line> strutCollection, bool isRecursiveLoop = false)
        {
            if (elementGroupByOrder.ChildGroup != null)
            {
                int i = 0;
                bool isNoStrut = false;
                foreach (ElementGroupByOrder child in elementGroupByOrder.ChildGroup)
                {

                    if ((child.PreviousElement.Count > 2 || (child.CurrentElement.Count <= 2 && child.PreviousCurrentGroupElement.Count > 2)))
                    {
                        GetShortestGridLine(child.PreviousElement, GridCollection, ref strutCollection, ref isNoStrut);
                    }
                    if (isNoStrut && i > 0 && elementGroupByOrder.ChildGroup.Count > 1)
                    {
                        List<Element> newElement = CreateDumpConduitsBackward(elementGroupByOrder.ChildGroup[i - 1], child);

                        if (newElement.Count > 0)
                            GetShortestGridLine(newElement, GridCollection, ref strutCollection, ref isNoStrut, true, true);
                    }
                    else if (isNoStrut && (elementGroupByOrder.ChildGroup.Count == 1 || i == 0))
                    {
                        WithoutStrut(child.PreviousElement, false);
                    }
                    if (child.ChildGroup != null && child.ChildGroup.Count > 0)
                    {

                        RecursiveLoopForBoundingBox(child, GridCollection, strutCollection, true);


                        if (child.ChildGroup != null && child.ChildGroup.Count == 1 && child.ChildGroup[0].ChildGroup == null
                             && child.PreviousElement.Count < child.PreviousCurrentGroupElement.Count && Utility.IsDifferentElevation(child.CurrentElement[0])
                             )
                        {
                            List<Element> newElement = CreateDumpConduitsForward(child);
                            if (newElement.Count > 2)
                                GetShortestGridLine(newElement, GridCollection, ref strutCollection, ref isNoStrut, false, true);
                            else
                            {
                                foreach (Element e in newElement)
                                {
                                    _doc.Delete(e.Id);
                                }
                            }

                        }
                    }
                    else
                    {
                        if (child.CurrentElement.Count > 2)
                            GetShortestGridLine(child.CurrentElement, GridCollection, ref strutCollection, ref isNoStrut, true, false, (child.ChildGroup == null || child.ChildGroup.Count == 0));



                        if ((child.ChildGroup == null || child.ChildGroup.Count == 0) && i == (elementGroupByOrder.ChildGroup.Count - 1))
                        {
                            List<Element> CurrentElement = new List<Element>();
                            CurrentElement.AddRange(elementGroupByOrder.CurrentElement);
                            for (int k = 0; k < i; k++)
                            {
                                CurrentElement.RemoveAll(x => elementGroupByOrder.ChildGroup[k].PreviousElement.Any(y => y.Id == x.Id));
                            }
                            if (child.CurrentElement.Count < CurrentElement.Count())
                            {
                                List<Element> newElement = CreateDumpConduitsForward(child);
                                if (newElement.Count > 2)
                                    GetShortestGridLine(newElement, GridCollection, ref strutCollection, ref isNoStrut, false, true);
                                else
                                {
                                    foreach (Element e in newElement)
                                    {
                                        _doc.Delete(e.Id);
                                    }
                                }
                            }
                        }
                    }
                    i++;
                }
            }
            else
            {
                bool isNoStrut = false;
                if (elementGroupByOrder.CurrentElement !=null && elementGroupByOrder.CurrentElement.Count > 2)
                {
                    GetShortestGridLine(elementGroupByOrder.CurrentElement, GridCollection, ref strutCollection, ref isNoStrut);
                }
                if (isNoStrut )
                {
                    WithoutStrut(elementGroupByOrder.CurrentElement, false);
                }
               
            }
        }
        private List<Element> CreateDumpConduitsForSlop(ElementGroupByOrder child)
        {
            Element minLengthConduit = Utility.GetMinLengthConduit(child.PreviousElement);
            XYZ midPoint = Utility.GetMidPoint(minLengthConduit, true);

            return new List<Element>();
        }
        private List<Element> CreateDumpConduitsBackward(ElementGroupByOrder parent, ElementGroupByOrder child)
        {
            Element minLengthConduit1 = Utility.GetMinLengthConduit(parent.PreviousElement);
            XYZ midPoint1 = Utility.GetMidPoint(minLengthConduit1, true);

            Line FinalLine1 = null;
            if (parent.CurrentElement.Count >= 2)
            {

                Element firstElement = null;
                Element lastElement = null;
                if (parent.CurrentElement.Count == 2)
                {
                    firstElement = parent.CurrentElement.FirstOrDefault();
                    lastElement = parent.CurrentElement.LastOrDefault();
                }
                else
                {
                    List<Element> elements = Utility.OrderTheConduit(parent.CurrentElement);
                    firstElement = elements.FirstOrDefault();
                    lastElement = elements.LastOrDefault();
                }
                XYZ firstMidPoint = Utility.GetMidPoint(firstElement, true);
                XYZ lastMidPoint = Utility.GetMidPoint(lastElement, true);
                var firstCross = Utility.CrossProduct(firstElement, firstMidPoint, 20, true);
                Line firstLine = Line.CreateBound(firstCross.Key, firstCross.Value);
                firstCross = Utility.CrossProduct(firstLine, firstMidPoint, Utility.GetConduitLength(firstElement) * 2, true);
                firstLine = Line.CreateBound(firstCross.Key, firstCross.Value);
                var lastCross = Utility.CrossProduct(lastElement, lastMidPoint, 20, true);
                Line lastLine = Line.CreateBound(lastCross.Key, lastCross.Value);
                lastCross = Utility.CrossProduct(lastLine, lastMidPoint, Utility.GetConduitLength(lastElement) * 2, true);
                lastLine = Line.CreateBound(lastCross.Key, lastCross.Value);
                Line mainLine = null;

                var checkCross = Utility.CrossProduct(parent.PreviousElement[0], midPoint1, 20, true);
                Line checkLine = Line.CreateBound(checkCross.Key, checkCross.Value);
                checkCross = Utility.CrossProduct(checkLine, midPoint1, 200, true);
                mainLine = Line.CreateBound(checkCross.Key, checkCross.Value);
                // Utility.CreateConduit(_doc, child.PreviousElement[0], checkCross.Key, checkCross.Value);

                XYZ firstPoint = Utility.GetIntersection(mainLine, firstLine);
                XYZ lastPoint = Utility.GetIntersection(mainLine, lastLine);
                if (firstPoint != null && lastPoint != null)
                {
                    FinalLine1 = midPoint1.DistanceTo(firstPoint) > midPoint1.DistanceTo(lastPoint) ? firstLine : lastLine;
                }
                else
                {

                }
            }
            else
            {
                XYZ firstMidPoint = Utility.GetMidPoint(parent.CurrentElement[0], true);
                var firstCross = Utility.CrossProduct(parent.CurrentElement[0], firstMidPoint, 20, true);
                Line firstLine = Line.CreateBound(firstCross.Key, firstCross.Value);
                firstCross = Utility.CrossProduct(firstLine, firstMidPoint, Utility.GetConduitLength(parent.CurrentElement[0]) * 2, true);
                FinalLine1 = Line.CreateBound(firstCross.Key, firstCross.Value);
            }
            // Utility.CreateConduit(_doc, parent.PreviousElement[0], FinalLine1.GetEndPoint(0), FinalLine1.GetEndPoint(1));

            Element minLengthConduit2 = Utility.GetMinLengthConduit(child.PreviousElement);
            XYZ midPoint2 = Utility.GetMidPoint(minLengthConduit2, true);

            Line FinalLine2 = null;
            if (child.CurrentElement.Count >= 2)
            {

                Element firstElement = null;
                Element lastElement = null;
                if (child.CurrentElement.Count == 2)
                {
                    firstElement = child.CurrentElement.FirstOrDefault();
                    lastElement = child.CurrentElement.LastOrDefault();
                }
                else
                {
                    List<Element> elements = Utility.OrderTheConduit(child.CurrentElement);
                    firstElement = elements.FirstOrDefault();
                    lastElement = elements.LastOrDefault();
                }
                XYZ firstMidPoint = Utility.GetMidPoint(firstElement, true);
                XYZ lastMidPoint = Utility.GetMidPoint(lastElement, true);
                var firstCross = Utility.CrossProduct(firstElement, firstMidPoint, 20, true);
                Line firstLine = Line.CreateBound(firstCross.Key, firstCross.Value);
                firstCross = Utility.CrossProduct(firstLine, firstMidPoint, Utility.GetConduitLength(firstElement) * 2, true);
                firstLine = Line.CreateBound(firstCross.Key, firstCross.Value);
                var lastCross = Utility.CrossProduct(lastElement, lastMidPoint, 20, true);
                Line lastLine = Line.CreateBound(lastCross.Key, lastCross.Value);
                lastCross = Utility.CrossProduct(lastLine, lastMidPoint, Utility.GetConduitLength(lastElement) * 2, true);
                lastLine = Line.CreateBound(lastCross.Key, lastCross.Value);
                Line mainLine = null;

                var checkCross = Utility.CrossProduct(child.PreviousElement[0], midPoint2, 20, true);
                Line checkLine = Line.CreateBound(checkCross.Key, checkCross.Value);
                checkCross = Utility.CrossProduct(checkLine, midPoint2, 200, true);
                mainLine = Line.CreateBound(checkCross.Key, checkCross.Value);
                // Utility.CreateConduit(_doc, child.PreviousElement[0], checkCross.Key, checkCross.Value);

                XYZ firstPoint = Utility.GetIntersection(mainLine, firstLine);
                XYZ lastPoint = Utility.GetIntersection(mainLine, lastLine);
                if (firstPoint != null && lastPoint != null)
                {
                    FinalLine2 = midPoint2.DistanceTo(firstPoint) > midPoint2.DistanceTo(lastPoint) ? firstLine : lastLine;
                }
                else
                {

                }
            }
            else
            {
                XYZ firstMidPoint = Utility.GetMidPoint(child.CurrentElement[0], true);
                var firstCross = Utility.CrossProduct(child.CurrentElement[0], firstMidPoint, 20, true);
                Line firstLine = Line.CreateBound(firstCross.Key, firstCross.Value);
                firstCross = Utility.CrossProduct(firstLine, firstMidPoint, Utility.GetConduitLength(child.CurrentElement[0]) * 2, true);
                FinalLine2 = Line.CreateBound(firstCross.Key, firstCross.Value);
            }
            // Utility.CreateConduit(_doc, child.PreviousElement[0], FinalLine2.GetEndPoint(0), FinalLine2.GetEndPoint(1));

            XYZ intersectPoint = Utility.GetIntersection(FinalLine1, Utility.GetLineFromConduit(minLengthConduit2, true));

            if (intersectPoint != null)
            {
                List<Element> newElement = new List<Element>();
                Line line = Utility.GetLineFromConduit(minLengthConduit2);
                List<Element> runElements = child.FullRunElements.FirstOrDefault(x => x.Any(y => y.Id == minLengthConduit2.Id)).ToList();
                int index = runElements.FindIndex(x => x.Id == minLengthConduit2.Id);
                runElements = runElements.Skip(index + 1).ToList();
                Element family = runElements.FirstOrDefault(x => (x is FamilyInstance) && Utility.GetFamilyInstancePartType(x as FamilyInstance) == "elbow");
                XYZ familyLP = (family.Location as LocationPoint).Point;
                XYZ endPoint = familyLP.DistanceTo(line.GetEndPoint(0)) < familyLP.DistanceTo(line.GetEndPoint(1)) ? line.GetEndPoint(0) : line.GetEndPoint(1);
                line = Line.CreateBound(intersectPoint, Utility.GetXYvalue(endPoint));
                using (SubTransaction transaction = new SubTransaction(_doc))
                {
                    transaction.Start();
                    if (line.Length > 1)
                        newElement.Add(Utility.CreateConduit(_doc, minLengthConduit2, line) as Element);
                    transaction.Commit();
                }
                _doc.Regenerate();
                return newElement;
            }
            return new List<Element>();

        }
        private List<Element> CreateDumpConduitsForward(ElementGroupByOrder child)
        {
            Element minLengthConduit = Utility.GetMinLengthConduit(child.PreviousElement);
            XYZ midPoint = Utility.GetMidPoint(minLengthConduit, true);

            Line FinalLine = null;
            if (child.CurrentElement.Count >= 2)
            {

                Element firstElement = null;
                Element lastElement = null;
                if (child.CurrentElement.Count == 2)
                {
                    firstElement = child.CurrentElement.FirstOrDefault();
                    lastElement = child.CurrentElement.LastOrDefault();
                }
                else
                {
                    List<Element> elements = Utility.OrderTheConduit(child.CurrentElement);
                    firstElement = elements.FirstOrDefault();
                    lastElement = elements.LastOrDefault();
                }
                XYZ firstMidPoint = Utility.GetMidPoint(firstElement, true);
                XYZ lastMidPoint = Utility.GetMidPoint(lastElement, true);
                var firstCross = Utility.CrossProduct(firstElement, firstMidPoint, 20, true);
                Line firstLine = Line.CreateBound(firstCross.Key, firstCross.Value);
                firstCross = Utility.CrossProduct(firstLine, firstMidPoint, Utility.GetConduitLength(firstElement) * 4, true);
                firstLine = Line.CreateBound(firstCross.Key, firstCross.Value);
                var lastCross = Utility.CrossProduct(lastElement, lastMidPoint, 20, true);
                Line lastLine = Line.CreateBound(lastCross.Key, lastCross.Value);
                lastCross = Utility.CrossProduct(lastLine, lastMidPoint, Utility.GetConduitLength(lastElement) * 4, true);
                lastLine = Line.CreateBound(lastCross.Key, lastCross.Value);
                Line mainLine = null;

                var checkCross = Utility.CrossProduct(child.PreviousElement[0], midPoint, 20, true);
                Line checkLine = Line.CreateBound(checkCross.Key, checkCross.Value);
                checkCross = Utility.CrossProduct(checkLine, midPoint, 200, true);
                mainLine = Line.CreateBound(checkCross.Key, checkCross.Value);
                // Utility.CreateConduit(_doc, child.PreviousElement[0], checkCross.Key, checkCross.Value);

                XYZ firstPoint = Utility.GetIntersection(mainLine, firstLine);
                XYZ lastPoint = Utility.GetIntersection(mainLine, lastLine);
                if (firstPoint != null && lastPoint != null)
                {
                    FinalLine = midPoint.DistanceTo(firstPoint) > midPoint.DistanceTo(lastPoint) ? firstLine : lastLine;
                }
                else
                {

                }
            }
            else
            {
                XYZ firstMidPoint = Utility.GetMidPoint(child.CurrentElement[0], true);
                var firstCross = Utility.CrossProduct(child.CurrentElement[0], firstMidPoint, 20, true);
                Line firstLine = Line.CreateBound(firstCross.Key, firstCross.Value);
                firstCross = Utility.CrossProduct(firstLine, firstMidPoint, Utility.GetConduitLength(child.CurrentElement[0]) * 2, true);
                FinalLine = Line.CreateBound(firstCross.Key, firstCross.Value);
            }
            //Utility.CreateConduit(_doc, child.PreviousElement[0], FinalLine.GetEndPoint(0), FinalLine.GetEndPoint(1));
            List<Element> endElements = child.PreviousCurrentGroupElement.Where(x => !child.PreviousElement.Any(y => y.Id == x.Id)).ToList();

            Line keyLine1 = Utility.CrossProductLine(minLengthConduit, Utility.GetMidPoint(minLengthConduit), 10, true);

            List<Element> newElement = new List<Element>();
            foreach (Element item in endElements)
            {

                Line cline = Utility.GetLineFromConduit(item, true);
                XYZ point = Utility.GetIntersection(cline, FinalLine);
                if (point != null)
                {
                    Line line1 = null;
                    Line line2 = null;
                    try
                    {
                        line1 = Line.CreateBound(point, cline.GetEndPoint(0));

                        line2 = Line.CreateBound(point, cline.GetEndPoint(1));
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    XYZ intersect1 = Utility.GetIntersection(keyLine1, line1);
                    if (intersect1 == null && line1.Length >= 1)
                    {
                        using (SubTransaction transaction = new SubTransaction(_doc))
                        {
                            transaction.Start();
                            newElement.Add(Utility.CreateConduit(_doc, item, line1) as Element);
                            transaction.Commit();
                        }
                    }
                    XYZ intersect2 = Utility.GetIntersection(keyLine1, line2);
                    if (intersect2 == null && line2.Length >= 1)
                    {
                        using (SubTransaction transaction = new SubTransaction(_doc))
                        {
                            transaction.Start();

                            newElement.Add(Utility.CreateConduit(_doc, item, line2) as Element);
                            transaction.Commit();
                        }
                    }
                }
            }

            _doc.Regenerate();
            return newElement;
        }
        private void GetShortestGridLine(List<Element> elements, List<Element> GridCollection,
            ref Dictionary<Element, Line> strutCollection,
            ref bool isNoStrut,
            bool isMinLength = true, bool isNeedToDelete = false, bool isEndBranch = false)
        {

            if (Utility.IsDifferentElevation(Utility.GetLineFromConduit(elements[0])))
            {
                return;
            }
            Element element = isMinLength ? Utility.GetMinLengthConduit(elements) : Utility.GetMaxLengthConduit(elements);
            Line line = Utility.GetLineFromConduit(element, true);
            Dictionary<Element, Line> strutDictionary = new Dictionary<Element, Line>();
            foreach (KeyValuePair<Element, Line> item in strutCollection)
            {
                XYZ intersectedPoint = Utility.GetIntersection(item.Value, line);
                if (intersectedPoint != null)
                {
                    strutDictionary.Add(item.Key, item.Value);
                }
            }
            foreach (KeyValuePair<Element, Line> item in strutDictionary)
            {
                strutCollection.Remove(item.Key);
            }
            if (isMinLength && strutDictionary.Count == 0 && !isNeedToDelete && !isEndBranch)
            {
                isNoStrut = true;
                return;
            }
            if (strutDictionary.Count > 0)
            {
                double val = (double)strutDictionary.Count() / (double)2;
                int index = Convert.ToInt32(Math.Ceiling(val));
                double distance = 10000;
                Line intersectLine = null;
                KeyValuePair<Element, Line> selectedStrut = strutDictionary.ElementAt(index - 1);
                KeyValuePair<XYZ, XYZ> crossPoints = Utility.CrossProduct(selectedStrut.Value, Utility.GetMidPoint(selectedStrut.Value), 2);
                Line perpendicularLine = Line.CreateBound(crossPoints.Key, crossPoints.Value);
                foreach (Grid grid in GridCollection)
                {
                    Line gridLine = grid.Curve as Line;
                    XYZ gridDirection = gridLine.Direction;
                    if (Utility.IsSameDirection(gridDirection, perpendicularLine.Direction))
                    {
                        XYZ interSectionPoint = Utility.FindIntersectionPoint(selectedStrut.Value, gridLine);
                        XYZ strutLocationPoint = (selectedStrut.Key.Location as LocationPoint).Point;
                        if (interSectionPoint != null && strutLocationPoint.DistanceTo(interSectionPoint) < distance)
                        {
                            distance = strutLocationPoint.DistanceTo(interSectionPoint);
                            intersectLine = Line.CreateBound(strutLocationPoint, interSectionPoint);

                        }
                    }
                }
                CreateSectionView(selectedStrut.Key, intersectLine);
            }
            else
            {

                WithoutStrut(elements, isNeedToDelete);
            }

        }

        private void WithoutStrut(List<Element> elements, bool isNeedToDelete = false)
        {
            List<XYZ> minList = new List<XYZ>();
            List<XYZ> maxList = new List<XYZ>();
            foreach (Element e in elements)
            {
                var refcon = Reference.ParseFromStableRepresentation(_doc, e.UniqueId);
                BoundingBoxXYZ bx = e.get_BoundingBox(_doc.ActiveView);
                minList.Add(bx.Min);
                maxList.Add(bx.Max);
            }
            minList.OrderByDescending(p => p.X).ThenBy(t => t.Y).ToList();
            maxList.OrderBy(p => p.X).ThenBy(t => t.Y).ToList();
            XYZ point1 = new XYZ(minList.OrderBy(p => p.X).First().X, minList.OrderBy(p => p.Y).First().Y, 0);
            XYZ point2 = new XYZ(maxList.OrderByDescending(p => p.X).First().X, maxList.OrderByDescending(p => p.Y).First().Y, 0);
            Level lvl = (_doc.GetElement(_doc.ActiveView.GenLevel.Id)) as Level;


            double levelofElevation = lvl.Elevation;


            double maxlvlelev = 0;
            List<Element> LvlCollection = new FilteredElementCollector(_doc).OfClass(typeof(Autodesk.Revit.DB.Level)).ToList();
            if (LvlCollection.OrderByDescending(r => (r as Level).Elevation).ToList().Any(r => (r as Level).Elevation > levelofElevation))
            {
                if (LvlCollection.OrderByDescending(r => (r as Level).Elevation).ToList().LastOrDefault(r => (r as Level).Elevation > levelofElevation) is Level abovelevel)
                {
                    maxlvlelev = abovelevel.Elevation;
                }
            }

            double elevationdiff = maxlvlelev - levelofElevation + 1;

            double h = elevationdiff;
            double w = 3;
            double offset = 0.125;
            XYZ maxPt = new XYZ(w, h / 2, 0);
            XYZ minPt = new XYZ(-w, -h / 2, -offset * 3);
            XYZ pt1 = new XYZ(point2.X, point2.Y, maxlvlelev);
            XYZ pt2 = new XYZ(point1.X, point1.Y, levelofElevation);
            XYZ middle = ((pt1 + pt2) / 2);
            KeyValuePair<XYZ, XYZ> crossPoints3 = Utility.CrossProduct(elements[0], (point1 + point2) / 2, 5);

            XYZ p = crossPoints3.Key;
            XYZ q = crossPoints3.Value;
            XYZ v = q - p;


            XYZ edir = v.Normalize();
            XYZ up = XYZ.BasisZ;
            XYZ viewdir = edir.CrossProduct(up);
            Transform transform = Transform.Identity;

            transform.BasisX = edir;
            transform.BasisY = up;
            transform.BasisZ = viewdir;
            transform.Origin = middle;

            // crate bounding box for section view
            BoundingBoxXYZ box = new BoundingBoxXYZ();
            box.Max = maxPt;
            box.Min = minPt;
            box.Transform = transform;
            ViewFamilyType viewFamilyType = (ViewFamilyType)ParentUserControl.Instance.FamilyForViewSheetBox.SelectionBoxItem;
            ViewSection sectionview = null;
            using (SubTransaction transaction = new SubTransaction(_doc))
            {
                transaction.Start();
                sectionview = ViewSection.CreateSection(_doc, viewFamilyType.Id, box);
                if (ParentUserControl.Instance.TemplateForView.SelectionBoxItem.ToString() != string.Empty)
                {
                    var ViewTemp = (View)ParentUserControl.Instance.TemplateForView.SelectionBoxItem;
                    sectionview.ViewTemplateId = ViewTemp.Id;
                }
                transaction.Commit();
            }
            _doc.Regenerate();
            if (isNeedToDelete)
                foreach (Element e in elements)
                {
                    _doc.Delete(e.Id);
                }
            Utility.SetAlertColor(sectionview.Id, _uiDoc);
            _unStrutSections.Add(sectionview);

        }
        private void CreateSectionView(Element strut, Line line)
        {
            string param = (string)ParentUserControl.Instance.strutParamList.SelectedValue;
            if (string.IsNullOrEmpty(param))
            {
                return;
            }
            double strutLength = strut.LookupParameter(param).AsDouble();
            XYZ abshandorienation = new XYZ();
            XYZ handorienation = (strut as FamilyInstance).HandOrientation;
            if ((Math.Sign(handorienation.X) == 1 && Math.Sign(handorienation.Y) == 1) || (Math.Sign(handorienation.X) == -1 && Math.Sign(handorienation.Y) == -1))
            {
                abshandorienation = new XYZ(Math.Abs(handorienation.X), Math.Abs(handorienation.Y), Math.Abs(handorienation.Z));
            }
            else if (Math.Sign(handorienation.X) == -1 && Math.Sign(handorienation.Y) == 1)
            {
                abshandorienation = new XYZ(handorienation.X, Math.Abs(handorienation.Y), Math.Abs(handorienation.Z));
            }
            else
            {
                abshandorienation = new XYZ(-Math.Abs(handorienation.X), Math.Abs(handorienation.Y), Math.Abs(handorienation.Z));
            }



            Level lvl = (_doc.GetElement(_doc.ActiveView.GenLevel.Id)) as Level;
            double lvlelev = lvl.Elevation;

            double maxlvlelev = 0;
            List<Element> LvlCollection = new FilteredElementCollector(_doc).OfClass(typeof(Autodesk.Revit.DB.Level)).ToList();
            if (LvlCollection.OrderByDescending(r => (r as Level).Elevation).ToList().Any(r => (r as Level).Elevation > lvlelev))
            {
                if (LvlCollection.OrderByDescending(r => (r as Level).Elevation).ToList().LastOrDefault(r => (r as Level).Elevation > lvlelev) is Level abovelevel)
                {
                    maxlvlelev = abovelevel.Elevation;
                }
            }

            double elevationdiff = maxlvlelev - lvlelev + 1;

            double h = elevationdiff;

            double w = strutLength / 2 + (line == null ? (strutLength / 3) : line.Length);
            double offset = 0.125;

            BoundingBoxXYZ boxXYZ = strut.get_BoundingBox(null);


            XYZ maxPt = new XYZ(w, h / 2, 0);
            XYZ minPt = new XYZ(-w, -h / 2, -offset * 3);
            XYZ pt1 = new XYZ(boxXYZ.Max.X, boxXYZ.Max.Y, maxlvlelev);
            XYZ pt2 = new XYZ(boxXYZ.Min.X, boxXYZ.Min.Y, lvlelev);
            XYZ middle = ((pt1 + pt2) / 2);

            XYZ edir = abshandorienation.Normalize();
            XYZ up = XYZ.BasisZ;
            XYZ viewdir = edir.CrossProduct(up);

            //create transform
            // tried here
            Transform transform = Transform.Identity;

            transform.BasisX = edir;
            transform.BasisY = up;
            transform.BasisZ = viewdir;
            transform.Origin = middle;

            // crate bounding box for section view
            BoundingBoxXYZ box = new BoundingBoxXYZ();
            box.Max = maxPt;
            box.Min = minPt;
            box.Transform = transform;
            ViewFamilyType viewFamilyType = (ViewFamilyType)ParentUserControl.Instance.FamilyForViewSheetBox.SelectionBoxItem;
            ViewSection sectionview = ViewSection.CreateSection(_doc, viewFamilyType.Id, box);
            if (ParentUserControl.Instance.TemplateForView.SelectionBoxItem.ToString() != string.Empty)
            {
                var ViewTemp = (View)ParentUserControl.Instance.TemplateForView.SelectionBoxItem;
                sectionview.ViewTemplateId = ViewTemp.Id;
            }


        }

        private ElementGroupByOrder RecursiveLoopForFindTheBranchInOrder(Dictionary<int, List<ConduitGrid>> conduitGridDictionary, ElementGroupByOrder elementGroupByOrder)
        {
            bool isHavingChildGroup = false;
            for (int k = 0; k < elementGroupByOrder.RunElements.Count; k++)
            {

                int orderIndex = elementGroupByOrder.RunElements.FindIndex(x => x.Any(y => y.Id == elementGroupByOrder.CurrentElement[k].Id));
                List<Element> e = elementGroupByOrder.RunElements[orderIndex];
                int index = e.FindIndex(n => n is FamilyInstance && Utility.GetFamilyInstancePartType(n) == "elbow");
                if (index == -1)
                {
                    if (k == elementGroupByOrder.RunElements.Count - 1 && isHavingChildGroup && conduitGridDictionary.Count > 0)
                    {

                        foreach (ElementGroupByOrder item in elementGroupByOrder.ChildGroup)
                        {
                            if (item.ChildGroup == null)
                                item.ChildGroup = new List<ElementGroupByOrder>();

                            ElementGroupByOrder updateChild = RecursiveLoopForFindTheBranchInOrder(conduitGridDictionary, item);
                        }
                        return elementGroupByOrder;
                    }
                    else
                        continue;
                }

                Element nextElement = e[index + 1];
                KeyValuePair<int, List<ConduitGrid>> obj = conduitGridDictionary.ToList().FirstOrDefault(x => x.Value.Any(y => y.Conduit.Id == nextElement.Id));
                if (!obj.Equals(new KeyValuePair<int, List<ConduitGrid>>()))
                {
                    List<Element> elements1 = obj.Value.Select(x => x.Conduit as Element).ToList();
                    ElementGroupByOrder objGroup = new ElementGroupByOrder();
                    objGroup.CurrentElement = elements1.OrderBy(x => x.LookupParameter("Length").AsDouble()).ToList();
                    if (objGroup.RunElements == null)
                        objGroup.RunElements = new List<List<Element>>();
                    objGroup.PreviousElement = new List<Element>();
                    objGroup.FullRunElements = elementGroupByOrder.RunElements.Where(x => x.Any(y => elements1.Any(z => z.Id == y.Id))).ToList();
                    foreach (List<Element> runEle in objGroup.FullRunElements)
                    {
                        int ix = runEle.FindIndex(n => n is FamilyInstance && Utility.GetFamilyInstancePartType(n) == "elbow");
                        objGroup.PreviousElement.Add(runEle[ix - 1]);
                        List<Element> ele = runEle.Skip(ix + 1).ToList();
                        if (ele.Count > 0)
                        {
                            objGroup.RunElements.Add(ele);
                        }
                    }
                    objGroup.PreviousGroupElement = new List<Element>();
                    objGroup.PreviousGroupElement.AddRange(elementGroupByOrder.CurrentElement);
                    objGroup.PreviousCurrentGroupElement = new List<Element>();
                    if (elementGroupByOrder.ChildGroup != null && elementGroupByOrder.ChildGroup.Count > 0)
                        objGroup.PreviousCurrentGroupElement.AddRange(elementGroupByOrder.CurrentElement.Where(x => !elementGroupByOrder.ChildGroup[elementGroupByOrder.ChildGroup.Count - 1].PreviousElement.Any(y => y.Id == x.Id)));
                    else
                        objGroup.PreviousCurrentGroupElement.AddRange(elementGroupByOrder.CurrentElement);

                    objGroup.SecondPreviousCurrentGroupElement = elementGroupByOrder.PreviousCurrentGroupElement;
                    if (elementGroupByOrder.ChildGroup == null)
                        elementGroupByOrder.ChildGroup = new List<ElementGroupByOrder>();
                    elementGroupByOrder.ChildGroup.Add(objGroup);
                    isHavingChildGroup = true;
                    conduitGridDictionary.Remove(obj.Key);
                    if (elements1.Count == 1)
                        break;
                }
                if (k == elementGroupByOrder.RunElements.Count - 1 && elementGroupByOrder.ChildGroup != null && conduitGridDictionary.Count > 0)
                {
                    List<ElementGroupByOrder> list = new List<ElementGroupByOrder>();
                    foreach (ElementGroupByOrder item in elementGroupByOrder.ChildGroup)
                    {
                        ElementGroupByOrder updateChild = RecursiveLoopForFindTheBranchInOrder(conduitGridDictionary, item);
                        list.Add(updateChild);
                    }
                    elementGroupByOrder.ChildGroup = list;
                }

            }


            return elementGroupByOrder;
        }


        public string GetName()
        {
            return "Revit Addin";
        }
    }
}

