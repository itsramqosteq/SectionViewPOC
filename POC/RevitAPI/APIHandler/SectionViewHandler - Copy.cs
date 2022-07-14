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

    public class SectionViewHandlerCopy : IExternalEventHandler
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
                    RecursiveLoopForBoundingBox(elementGroupByOrder.ChildGroup, GridCollection);



                    transaction.Commit();
                }
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show("Some error has occured. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }

        private void RecursiveLoopForBoundingBox(List<ElementGroupByOrder> childGroup, List<Element> GridCollection, int parentCout = 0)
        {
            if (childGroup != null)
            {
                XYZ startPoint = null;
                int i = 0;


                foreach (ElementGroupByOrder child in childGroup)
                {
                    if (child.CurrentElement.Count <= 2)
                        continue;

                    //if(parentCout == child.CurrentElement.Count)
                    //{
                    //    RecursiveLoopForBoundingBox(child.ChildGroup, GridCollection, parentCout > 0 ? 0 : child.CurrentElement.Count);
                    //    continue;
                    //}
                    XYZ midPoint = null;
                    if (i == 0 || (i > 0 && startPoint != null))
                    {


                        StrutFilterUsingBoundingBox(GridCollection, child.PreviousElement, startPoint, out midPoint);

                        //Conduit conduit = Utility.CreateConduit(_doc, child.PreviousElement[0], Utility.SetZvalue(point1, null, minZ), Utility.SetZvalue(point2, null, maxZ));
                        //Utility.SetAlertColor(conduit.Id, _uiDoc);
                    }
                    if (child.ChildGroup != null && child.ChildGroup.Count > 0)
                    {
                        RecursiveLoopForBoundingBox(child.ChildGroup, GridCollection, parentCout > 0 ? 0 : child.CurrentElement.Count);
                        continue;
                    }
                    else
                    {

                        StrutFilterUsingBoundingBox(GridCollection, child.CurrentElement, null, out XYZ newMidPoint);
                        //Conduit conduit = Utility.CreateConduit(_doc, child.CurrentElement[0], Utility.SetZvalue(point1, null, minZ), Utility.SetZvalue(point2, null, maxZ));
                        //Utility.SetAlertColor(conduit.Id, _uiDoc);

                        if (Utility.IsDifferentElevation(Utility.GetLineFromConduit(child.PreviousElement[0])))
                            StrutFilterUsingBoundingBox(GridCollection, child.SecondPreviousCurrentGroupElement, null, out midPoint, true);
                        else if (midPoint == null)
                            StrutFilterUsingBoundingBox(GridCollection, child.PreviousElement, null, out midPoint, true);
                        else
                            continue;
                    }
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
                        firstCross = Utility.CrossProduct(firstLine, firstMidPoint, Utility.GetConduitLength(firstElement) * 2, true);
                        firstLine = Line.CreateBound(firstCross.Key, firstCross.Value);
                        //   Utility.CreateConduit(_doc, child.PreviousElement[0], firstCross.Key, firstCross.Value);
                        var lastCross = Utility.CrossProduct(lastElement, lastMidPoint, 20, true);
                        Line lastLine = Line.CreateBound(lastCross.Key, lastCross.Value);
                        lastCross = Utility.CrossProduct(lastLine, lastMidPoint, Utility.GetConduitLength(lastElement) * 2, true);
                        lastLine = Line.CreateBound(lastCross.Key, lastCross.Value);
                        //  Utility.CreateConduit(_doc, child.PreviousElement[0], lastCross.Key, lastCross.Value);
                        Line mainLine = null;
                        if (Utility.IsDifferentElevation(Utility.GetLineFromConduit(child.PreviousElement[0])))
                        {
                            //Utility.CreateConduit(_doc, child.PreviousElement[0], firstCross.Key, firstCross.Value);
                            //Utility.CreateConduit(_doc, child.PreviousElement[0], lastCross.Key, lastCross.Value);
                            var checkCross = Utility.CrossProduct(child.SecondPreviousCurrentGroupElement[0], midPoint, 20, true);
                            Line checkLine = Line.CreateBound(checkCross.Key, checkCross.Value);
                            checkCross = Utility.CrossProduct(checkLine, midPoint, 200, true);
                            mainLine = Line.CreateBound(checkCross.Key, checkCross.Value);
                            //Utility.CreateConduit(_doc, child.PreviousElement[0], checkCross.Key, checkCross.Value);
                        }
                        else
                        {
                            var checkCross = Utility.CrossProduct(child.PreviousElement[0], midPoint, 20, true);
                            Line checkLine = Line.CreateBound(checkCross.Key, checkCross.Value);
                            checkCross = Utility.CrossProduct(checkLine, midPoint, 200, true);
                            mainLine = Line.CreateBound(checkCross.Key, checkCross.Value);
                            // Utility.CreateConduit(_doc, child.PreviousElement[0], checkCross.Key, checkCross.Value);
                        }
                        XYZ firstPoint = Utility.GetIntersection(mainLine, firstLine);
                        XYZ lastPoint = Utility.GetIntersection(mainLine, lastLine);
                        if (firstPoint != null && lastPoint != null)
                        {
                            FinalLine = midPoint.DistanceTo(firstPoint) > midPoint.DistanceTo(lastPoint) ? firstLine : lastLine;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        XYZ firstMidPoint = Utility.GetMidPoint(child.CurrentElement[0], true);
                        var firstCross = Utility.CrossProduct(child.CurrentElement[0], firstMidPoint, 20, true);
                        Line firstLine = Line.CreateBound(firstCross.Key, firstCross.Value);
                        firstCross = Utility.CrossProduct(firstLine, firstMidPoint, Utility.GetConduitLength(child.CurrentElement[0]) * 2, true);
                        FinalLine = Line.CreateBound(firstCross.Key, firstCross.Value);
                        //Utility.CreateConduit(_doc, child.PreviousElement[0], FinalLine.GetEndPoint(0), FinalLine.GetEndPoint(1));
                    }

                    midPoint = (FinalLine.GetEndPoint(0) + FinalLine.GetEndPoint(1)) / 2;
                    if (i != childGroup.Count - 1)
                    {
                        double distance = 100000000;
                        foreach (Element e in childGroup[i + 1].PreviousElement)
                        {
                            Line cline = Utility.GetLineFromConduit(e, true);
                            XYZ point = Utility.GetIntersection(cline, FinalLine);

                            if (point != null && midPoint.DistanceTo(point) < distance)
                            {
                                distance = midPoint.DistanceTo(point);
                                startPoint = point;
                            }
                        }
                    }
                    else
                    {
                        List<Element> endElements = new List<Element>();
                        if (Utility.IsDifferentElevation(Utility.GetLineFromConduit(child.PreviousElement[0])))
                            endElements = child.SecondPreviousCurrentGroupElement.Where(x => !child.PreviousElement.Any(y => y.Id == x.Id)).ToList();
                        else
                            endElements = child.PreviousCurrentGroupElement.Where(x => !child.PreviousElement.Any(y => y.Id == x.Id)).ToList();


                        //Utility.CreateConduit(_doc, child.PreviousElement[0], FinalLine.GetEndPoint(0), FinalLine.GetEndPoint(1));
                        if (endElements != null && endElements.Count > 0)
                        {
                            double distance = 100000000;
                            foreach (Element item in endElements)
                            {
                                Line cline = Utility.GetLineFromConduit(item, true);
                                XYZ point = Utility.GetIntersection(cline, FinalLine);

                                if (point != null && midPoint.DistanceTo(point) < distance)
                                {
                                    distance = midPoint.DistanceTo(point);
                                    startPoint = point;
                                }
                            }
                            if (startPoint != null)
                            {

                                if (!StrutFilterUsingBoundingBox(GridCollection, endElements, startPoint, out midPoint))
                                {
                                    i++;
                                    continue;
                                }
                                //Conduit conduit = Utility.CreateConduit(_doc, child.PreviousElement[0], Utility.SetZvalue(point1, null, minZ), Utility.SetZvalue(point2, null, maxZ));
                                //Utility.SetAlertColor(conduit.Id, _uiDoc);
                            }
                        }
                    }
                    i++;
                }
            }
        }
        private bool StrutFilterUsingBoundingBox(List<Element> GridCollection, List<Element> elements, XYZ startPoint, out XYZ midPoint, bool uptoMidpoint = false)
        {

            List<XYZ> minList = new List<XYZ>();
            List<XYZ> maxList = new List<XYZ>();
            foreach (Element ele in elements)
            {
                var refcon = Reference.ParseFromStableRepresentation(_doc, ele.UniqueId);
                BoundingBoxXYZ bx = ele.get_BoundingBox(_doc.ActiveView);
                minList.Add(bx.Min);
                maxList.Add(bx.Max);
            }
            minList.OrderByDescending(p => p.X).ThenBy(t => t.Y).ToList();
            maxList.OrderBy(p => p.X).ThenBy(t => t.Y).ToList();
            XYZ point1 = startPoint == null ? new XYZ(minList.OrderBy(p => p.X).First().X, minList.OrderBy(p => p.Y).First().Y, 0) : startPoint;
            XYZ point2 = new XYZ(maxList.OrderByDescending(p => p.X).First().X, maxList.OrderByDescending(p => p.Y).First().Y, 0);
            midPoint = (point1 + point2) / 2;
            if (Utility.IsDifferentElevation(Utility.GetLineFromConduit(elements[0])))
                return true;
            double minZ = minList.OrderBy(p => p.Z).First().Z;
            double maxZ = maxList.OrderByDescending(p => p.Z).First().Z;
            if (uptoMidpoint == true)
                return true;
            try
            {
                BoundingBoxXYZ box = new BoundingBoxXYZ();
                box.Max = Utility.SetZvalue(point1, null, minZ);
                box.Min = Utility.SetZvalue(point2, null, maxZ);
                Outline myOutLn = new Outline(Utility.SetZvalue(point1, null, minZ), Utility.SetZvalue(point2, null, maxZ));
                BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(myOutLn);
                FilteredElementCollector collector = new FilteredElementCollector(_doc, _doc.ActiveView.Id);
                List<Element> elementsCollector = collector.OfClass(typeof(FamilyInstance)).WherePasses(filter).ToElements().ToList();
                var s = elementsCollector.Where(x => Utility.GetFamilyInstanceName(x as FamilyInstance).ToLower().Contains("strut")).ToList();
                if (s.Count > 0)
                {
                    double val = (double)s.Count() / (double)2;
                    int index = Convert.ToInt32(Math.Ceiling(val));
                    GetShortestGridLine(GridCollection, s[index - 1], elements[0]);
                    return true;
                }

            }
            catch (Exception)
            {

                return false;
            }
            return false;
        }
        private void GetShortestGridLine(List<Element> GridCollection, Element strut, Element elements)
        {
            double distance = 10000;
            Line line = null;

            foreach (Element grid in GridCollection)
            {
                Line gridLine = (grid as Autodesk.Revit.DB.Grid).Curve as Line;
                XYZ gridDirection = gridLine.Direction;

                Options options = new Options
                {
                    ComputeReferences = true,
                    View = _doc.ActiveView,
                    IncludeNonVisibleObjects = true
                };
                GeometryElement GeoEle = strut.get_Geometry(options);

                foreach (GeometryObject obj in GeoEle)
                {
                    if (obj.GetType() == typeof(Line))
                    {
                        Line strutLine = obj as Line;
                        XYZ strutDirection = strutLine.Direction;
                        XYZ strutLocationPoint = (strut.Location as LocationPoint).Point;
                        XYZ cross = strutDirection.CrossProduct(XYZ.BasisZ);
                        Line perpendicularLine = Line.CreateBound(strutLocationPoint, strutLocationPoint + cross.Multiply(10));
                        //Utility.CreateConduit(_doc, elements, perpendicularLine.GetEndPoint(0), perpendicularLine.GetEndPoint(1));
                        if (Utility.IsSameDirection(gridDirection, perpendicularLine.Direction))
                        {
                            XYZ interSectionPoint = Utility.FindIntersectionPoint(strutLine, gridLine);
                            if (interSectionPoint != null && strutLocationPoint.DistanceTo(interSectionPoint) < distance)
                            {
                                distance = strutLocationPoint.DistanceTo(interSectionPoint);
                                line = Line.CreateBound(strutLocationPoint, interSectionPoint);

                            }
                        }
                        break;
                    }

                }
            }
            if (strut != null)
                CreateSectionView(strut, line);
            else
            {
                CreateSectionView(strut, null);

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

            double w = strutLength / 2 + (line ==null ? (strutLength/3) : line.Length);
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
                    foreach (List<Element> runEle in elementGroupByOrder.RunElements.Where(x => x.Any(y => elements1.Any(z => z.Id == y.Id))).ToList())
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

