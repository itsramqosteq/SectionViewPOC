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



                using (Transaction transaction = new Transaction(_doc))
                {
                    transaction.Start("SampleHandler");
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

                    transaction.Commit();
                }
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show("Some error has occured. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }

        private void RecursiveLoopForBoundingBox(ElementGroupByOrder elementGroupByOrder, List<Element> GridCollection, Dictionary<Element, Line> strutCollection)
        {
            if (elementGroupByOrder.ChildGroup != null)
            {

                int i = 0;
                bool isNoStrut = false;

                foreach (ElementGroupByOrder child in elementGroupByOrder.ChildGroup)
                {

                    GetShortestGridLine(child.PreviousElement, GridCollection, ref strutCollection, ref isNoStrut);
                    if (isNoStrut)
                    {
                        List<Element> newElement = CreateDumpConduitsBackward(elementGroupByOrder.ChildGroup[i - 1], child);
                        if (newElement.Count > 0)
                            GetShortestGridLine(newElement, GridCollection, ref strutCollection, ref isNoStrut, true, true);
                    }
                    if (child.ChildGroup != null && child.ChildGroup.Count > 0)
                    {
                        RecursiveLoopForBoundingBox(child, GridCollection, strutCollection);

                        //if (child.ChildGroup != null && child.ChildGroup.Count == 1 && child.ChildGroup[0].ChildGroup != null
                        //     && child.PreviousElement.Count < child.PreviousCurrentGroupElement.Count)
                        //{
                        //    GetShortestGridLine(child.PreviousCurrentGroupElement, GridCollection, ref strutCollection, ref isNoStrut, false);

                        //}
                        //else 
                        if (child.ChildGroup != null && child.ChildGroup.Count == 1 && child.ChildGroup[0].ChildGroup == null
                             && child.PreviousElement.Count < child.PreviousCurrentGroupElement.Count && Utility.IsDifferentElevation(child.CurrentElement[0])
                             )
                        {
                            List<Element> newElement = CreateDumpConduitsForward(child);
                            if (newElement.Count > 0)
                                GetShortestGridLine(newElement, GridCollection, ref strutCollection, ref isNoStrut, false);
                        }
                    }
                    else
                    {
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
                                if (newElement.Count > 0)
                                    GetShortestGridLine(newElement, GridCollection, ref strutCollection, ref isNoStrut, false, true);
                            }
                        }
                    }
                    i++;
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
            //  Utility.CreateConduit(_doc, parent.PreviousElement[0], FinalLine1.GetEndPoint(0), FinalLine1.GetEndPoint(1));

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
                    if (intersect1 == null)
                    {
                        using (SubTransaction transaction = new SubTransaction(_doc))
                        {
                            transaction.Start();
                            newElement.Add(Utility.CreateConduit(_doc, item, line1) as Element);
                            transaction.Commit();
                        }
                    }
                    XYZ intersect2 = Utility.GetIntersection(keyLine1, line2);
                    if (intersect2 == null)
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
                double w = 3;
                double offset = 0.125;
                XYZ maxPt = new XYZ(w, h / 2, 0);
                XYZ minPt = new XYZ(-w, -h / 2, -offset * 3);
                XYZ pt1 = new XYZ(point2.X, point2.Y, maxlvlelev);
                XYZ pt2 = new XYZ(point1.X, point1.Y, lvlelev);
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
                ViewFamilyType viewFamilyType = new FilteredElementCollector(_doc)
                              .OfClass(typeof(ViewFamilyType))
                              .Cast<ViewFamilyType>()
                              .FirstOrDefault<ViewFamilyType>(x => ViewFamily.Section == x.ViewFamily);
                ViewSection sectionview = null;
                using (SubTransaction transaction = new SubTransaction(_doc))
                {
                    transaction.Start();
                    sectionview = ViewSection.CreateSection(_doc, viewFamilyType.Id, box);
                    transaction.Commit();
                }
                _doc.Regenerate();
                if (isNeedToDelete)
                    foreach (Element e in elements)
                    {
                        _doc.Delete(e.Id);
                    }
                Utility.SetAlertColor(sectionview.Id, _uiDoc);
            }

        }

        private void CreateSectionView(Element strut, Line line)
        {

            double strutLength = strut.LookupParameter("STRUT LENGTH").AsDouble();
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
            ViewFamilyType viewFamilyType = new FilteredElementCollector(_doc)
                          .OfClass(typeof(ViewFamilyType))
                          .Cast<ViewFamilyType>()
                          .FirstOrDefault<ViewFamilyType>(x => ViewFamily.Section == x.ViewFamily);
            ViewSection sectionview = ViewSection.CreateSection(_doc, viewFamilyType.Id, box);
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

        private ViewSection CreateViewSection(List<Element> elements, ViewFamilyType viewFamilyType, XYZ newPoint = null, XYZ orgin = null)
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
            double minZ = minList.OrderBy(p => p.Z).First().Z;
            double maxZ = maxList.OrderByDescending(p => p.Z).First().Z;
            if (newPoint != null)
            {
                point2 = orgin.DistanceTo(point1) > orgin.DistanceTo(point2) ? point1 : point2;
                point1 = newPoint;
            }
            double dis = point1.DistanceTo(point2) / 2;
            KeyValuePair<XYZ, XYZ> crossPoints3 = Utility.CrossProduct(elements[0], (point1 + point2) / 2, 5);
            //if (newPoint != null)
            //{
            //    Utility.CreateConduit(_doc, elements[0], point1, point2);
            //Utility.CreateConduit(_doc, elements[0], crossPoints3.Key, crossPoints3.Value);
            //}
            XYZ p = crossPoints3.Key;
            XYZ q = crossPoints3.Value;
            XYZ v = q - p;


            double h = _maxLevel - _minLevel + 1; ;
            double w = v.GetLength();
            double offset = dis - 1;
            XYZ min = new XYZ(-w, minZ - offset, -offset);
            XYZ max = new XYZ(w, maxZ + offset, -offset + 1);
            //double offset = 0.125;

            //XYZ max = new XYZ(w, h / 2, 0);
            //XYZ min = new XYZ(-w, -h / 2, -offset * 3);


            XYZ midpoint = p + 0.5 * v;
            XYZ walldir = v.Normalize();
            XYZ up = XYZ.BasisZ;
            XYZ viewdir = walldir.CrossProduct(up);

            Transform t = Transform.Identity;
            t.Origin = midpoint;
            t.BasisX = walldir;
            t.BasisY = up;
            t.BasisZ = viewdir;

            BoundingBoxXYZ sectionBox = new BoundingBoxXYZ();
            sectionBox.Transform = t;
            sectionBox.Min = min;
            sectionBox.Max = max;
            ViewSection viewSection = null;

            using (SubTransaction transaction = new SubTransaction(_doc))
            {
                transaction.Start();
                viewSection = ViewSection.CreateSection(_doc, viewFamilyType.Id, sectionBox);
                if (newPoint != null)
                {
                    transaction.Commit();
                    return viewSection;
                }
                KeyValuePair<XYZ, XYZ> crossPoints4 = Utility.CrossProduct(elements[0], new XYZ(viewSection.Origin.X, viewSection.Origin.Y, 0), 25, true);
                //Utility.CreateConduit(_doc, elements[0] as Conduit, crossPoints4.Key, crossPoints4.Value);
                Line createLine = Line.CreateBound(crossPoints4.Key, crossPoints4.Value);
                XYZ previousXYZ = null;
                int i = 0;
                XYZ finalInterSectionPoint = null;
                foreach (Element item in elements)
                {
                    Line line1 = Utility.GetLineFromConduit(item, true);
                    XYZ intesectionPoint = Utility.GetIntersection(line1, createLine);
                    if (i > 0)
                        if (intesectionPoint == null && previousXYZ != null)
                        {
                            double lowDis = 0;
                            int j = 0;
                            foreach (Element item1 in elements)
                            {
                                double takeDis = 0;
                                Line line = Utility.GetLineFromConduit(item, true);
                                XYZ xYZ = null;

                                if (previousXYZ.DistanceTo(line.GetEndPoint(0)) < previousXYZ.DistanceTo(line.GetEndPoint(1)))
                                {
                                    takeDis = previousXYZ.DistanceTo(line.GetEndPoint(0));
                                    xYZ = line.GetEndPoint(0);
                                }
                                else
                                {
                                    takeDis = previousXYZ.DistanceTo(line.GetEndPoint(1));
                                    xYZ = line.GetEndPoint(1);
                                }

                                if (j == 0 || lowDis < takeDis)
                                {
                                    lowDis = takeDis;
                                    finalInterSectionPoint = xYZ;
                                }
                                j++;
                            }
                            _doc.Delete(viewSection.Id);

                            offset = dis - (lowDis + 2);// 0.1 * w;

                            min = new XYZ(-w, minZ - offset, -offset);
                            max = new XYZ(w, maxZ + offset, -offset + 1);


                            midpoint = p + 0.5 * v;
                            walldir = v.Normalize();
                            up = XYZ.BasisZ;
                            viewdir = walldir.CrossProduct(up);

                            t = Transform.Identity;
                            t.Origin = midpoint;
                            t.BasisX = walldir;
                            t.BasisY = up;
                            t.BasisZ = viewdir;

                            sectionBox = new BoundingBoxXYZ();
                            sectionBox.Transform = t;
                            sectionBox.Min = min;
                            sectionBox.Max = max;
                            viewSection = ViewSection.CreateSection(_doc, viewFamilyType.Id, sectionBox);
                            break;
                        }
                        else if (intesectionPoint != null && previousXYZ == null)
                        {
                            double lowDis = 0;
                            int j = 0;
                            foreach (Element item1 in elements)
                            {
                                double takeDis = 0;
                                Line line = Utility.GetLineFromConduit(item, true);
                                XYZ xYZ = null;

                                if (intesectionPoint.DistanceTo(line.GetEndPoint(0)) < intesectionPoint.DistanceTo(line.GetEndPoint(1)))
                                {
                                    takeDis = intesectionPoint.DistanceTo(line.GetEndPoint(0));
                                    xYZ = line.GetEndPoint(0);
                                }
                                else
                                {
                                    takeDis = intesectionPoint.DistanceTo(line.GetEndPoint(1));
                                    xYZ = line.GetEndPoint(1);

                                }

                                if (j == 0 || lowDis < takeDis)
                                {
                                    lowDis = takeDis;
                                    finalInterSectionPoint = xYZ;
                                }
                                j++;
                            }
                            _doc.Delete(viewSection.Id);
                            offset = dis - (lowDis + 2);// 0.1 * w;

                            min = new XYZ(-w, minZ - offset, -offset);
                            max = new XYZ(w, maxZ + offset, -offset + 1);


                            midpoint = p + 0.5 * v;
                            walldir = v.Normalize();
                            up = XYZ.BasisZ;
                            viewdir = walldir.CrossProduct(up);

                            t = Transform.Identity;
                            t.Origin = midpoint;
                            t.BasisX = walldir;
                            t.BasisY = up;
                            t.BasisZ = viewdir;

                            sectionBox = new BoundingBoxXYZ();
                            sectionBox.Transform = t;
                            sectionBox.Min = min;
                            sectionBox.Max = max;
                            viewSection = ViewSection.CreateSection(_doc, viewFamilyType.Id, sectionBox);
                            break;

                        }
                    previousXYZ = intesectionPoint;
                    i++;
                }
                transaction.Commit();

            }
            _doc.Regenerate();
            return viewSection;
        }
        public static void SubTransactionForTag(Document _doc, List<Element> elements, out List<XYZ> minAxis, out List<XYZ> maxAxis, out double maxHeight, out double maxWidth)
        {
            minAxis = new List<XYZ>();
            maxAxis = new List<XYZ>();
            maxHeight = 0;
            maxWidth = 0;
            FilteredElementCollector _collector = new FilteredElementCollector(_doc);
            List<Element> tags = _collector.OfCategory(BuiltInCategory.OST_ConduitTags).ToElements().ToList().Where(x => x.Name.ToLower().Contains("snv")).ToList();
            if (tags.Count > 0)
                using (SubTransaction subTransaction = new SubTransaction(_doc))
                {
                    subTransaction.Start();
                    int viewScale = _doc.ActiveView.Scale;
                    foreach (Element e in elements)
                    {
                        var refcon = Reference.ParseFromStableRepresentation(_doc, e.UniqueId);
                        BoundingBoxXYZ bx = e.get_BoundingBox(_doc.ActiveView);
                        minAxis.Add(bx.Min);
                        maxAxis.Add(bx.Max);
                        XYZ midpoint = Utility.GetConduitMidPoint(e);
                        IndependentTag newtag = IndependentTag.Create(_doc, tags[0].Id, _doc.ActiveView.Id, refcon, true, TagOrientation.Horizontal, midpoint);
                        newtag.LeaderEndCondition = LeaderEndCondition.Free;
                        BoundingBoxXYZ boxXYZ = newtag.get_BoundingBox(_doc.ActiveView);

                        XYZ one = Utility.GetXYvalue(boxXYZ.Min);
                        XYZ three = Utility.GetXYvalue(boxXYZ.Max);
                        XYZ two = new XYZ(one.X, three.Y, 0);
                        XYZ four = new XYZ(three.X, one.Y, 0);
                        //Utility.CreateConduit(_doc, e as Conduit, one, two);
                        //Utility.CreateConduit(_doc, e as Conduit, two, three);
                        //Utility.CreateConduit(_doc, e as Conduit, three, four);
                        double width = one.DistanceTo(two);
                        if (width > maxWidth)
                        {
                            maxWidth = width;
                        }
                        double height = two.DistanceTo(three);
                        if (height > maxHeight)
                        {
                            maxHeight = height;
                        }

                    }
                    subTransaction.Commit();
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

