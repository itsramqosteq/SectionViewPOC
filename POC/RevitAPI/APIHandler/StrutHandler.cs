using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using POC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace POC
{
    [Transaction(TransactionMode.Manual)]

    public class StrutHandler : IExternalEventHandler
    {
        DateTime startDate = DateTime.UtcNow;
        UIDocument _uiDoc = null;
        Document _doc = null;
        public UIApplication _uiApp = null;
        static string _fileName = "SNV STRUT-2020.rfa";
        static string _familyName = "SNV STRUT-2020";
        static readonly string _familyFolder = Path.GetDirectoryName(typeof(Command).Assembly.Location);
        static string __familyPath = null;
        static string _familyPath
        {
            get
            {
                if (__familyPath == null)
                {

                    __familyPath = Path.Combine(_familyFolder, _fileName);
                }
                return __familyPath;
            }
        }
        string _offsetVariable = string.Empty;
        public void Execute(UIApplication uiApp)
        {
            _uiApp = uiApp;
            _uiDoc = uiApp.ActiveUIDocument;
            _doc = _uiDoc.Document;
            int.TryParse(uiApp.Application.VersionNumber, out int RevitVersion);
            _offsetVariable = RevitVersion < 2020 ? "Offset" : "Middle Elevation";
            _fileName = RevitVersion < 2020 ? "SNV STRUT-2019.rfa" : "SNV STRUT-2020.rfa";
            _familyName = RevitVersion < 2020 ? "SNV STRUT-2019" : "SNV STRUT-2020";
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
                    conduitGridDictionary = GroupElementsByFilter.GroupByElements(elementsCollector, _offsetVariable);

                }
                else
                {

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
                    conduitGridDictionary = GroupElementsByFilter.GroupByElements(elementsMerged, _offsetVariable);
                    elementGroupByOrder.CurrentElement = elementsCollector.OrderBy(x => x.LookupParameter("Length").AsDouble()).ToList();
                    elementGroupByOrder.GroupByElementByElevation = GroupElementsByFilter.GroupByElementsWithElevation(elementGroupByOrder.CurrentElement, _offsetVariable);
                    elementGroupByOrder.RunElements = elementsMapped;
                    elementsMappedForReference.Add(elementsCollector);
                    conduitGridDictionary.Remove(conduitGridDictionary.FirstOrDefault().Key);
                    elementGroupByOrder = RecursiveLoopForFindTheBranchInOrder(conduitGridDictionary, elementGroupByOrder);
                }




                using (Transaction transaction = new Transaction(_doc))
                {
                    transaction.Start("SampleHandler");
                    startDate = DateTime.UtcNow;
                    LoadDefaultStrut();


                    RecursiveLoopForStrutPlacement(elementGroupByOrder.ChildGroup);



                    transaction.Commit();
                }
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show("Some error has occured. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }

        private void RecursiveLoopForStrutPlacement(List<ElementGroupByOrder> childGroup)
        {
            if (childGroup != null)
            {
                int i = 0;
                Line crossLineEndOfFitting = null;
                foreach (ElementGroupByOrder child in childGroup)
                {
                    double maxDistance = 0;

                    XYZ midPoint = null;
                    GetMaxRackSizeAndMidPoint( child.GroupByElementByElevation, ref maxDistance, ref midPoint);
                    Element minConduit = Utility.GetMinLengthConduit(child.PreviousElement);
                    Line minLengthConduit = Utility.GetLineFromConduit(minConduit, true);
                    XYZ minMidPoint = Utility.GetMidPoint(minLengthConduit);
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
                        // Utility.CreateConduit(_doc, child.PreviousElement[0], firstCross.Key, firstCross.Value);
                        var lastCross = Utility.CrossProduct(lastElement, lastMidPoint, 20, true);
                        Line lastLine = Line.CreateBound(lastCross.Key, lastCross.Value);
                        lastCross = Utility.CrossProduct(lastLine, lastMidPoint, Utility.GetConduitLength(lastElement) * 2, true);
                        lastLine = Line.CreateBound(lastCross.Key, lastCross.Value);
                        //  Utility.CreateConduit(_doc, child.PreviousElement[0], lastCross.Key, lastCross.Value);
                        var checkCross = Utility.CrossProduct(child.PreviousElement[0], minMidPoint, 20, true);
                        Line checkLine = Line.CreateBound(checkCross.Key, checkCross.Value);
                        checkCross = Utility.CrossProduct(checkLine, minMidPoint, 200, true);
                        Line mainLine = Line.CreateBound(checkCross.Key, checkCross.Value);
                        // Utility.CreateConduit(_doc, child.PreviousElement[0], checkCross.Key, checkCross.Value);

                        XYZ firstPoint = Utility.GetIntersection(mainLine, firstLine);
                        XYZ lastPoint = Utility.GetIntersection(mainLine, lastLine);
                        FinalLine = minMidPoint.DistanceTo(firstPoint) > minMidPoint.DistanceTo(lastPoint) ? firstLine : lastLine;
                        // Utility.CreateConduit(_doc, child.PreviousElement[0], FinalLine.GetEndPoint(0), FinalLine.GetEndPoint(1));
                    }
                    else
                    {
                        XYZ firstMidPoint = Utility.GetMidPoint(child.CurrentElement[0], true);
                        var firstCross = Utility.CrossProduct(child.CurrentElement[0], firstMidPoint, 20, true);
                        Line firstLine = Line.CreateBound(firstCross.Key, firstCross.Value);
                        firstCross = Utility.CrossProduct(firstLine, firstMidPoint, Utility.GetConduitLength(child.CurrentElement[0]) * 2, true);
                        FinalLine = Line.CreateBound(firstCross.Key, firstCross.Value);
                    }
                    if (i == 0)
                    {
                        KeyValuePair<XYZ, XYZ> valuePair2 = GetMidLineOfGroup(minLengthConduit, minMidPoint, midPoint);
                        Conduit conduit = Utility.CreateConduit(_doc, child.PreviousElement[0], valuePair2.Key, valuePair2.Value);
                        Utility.SetAlertColor(conduit.Id, _uiDoc);
                        crossLineEndOfFitting = FinalLine;
                    }
                    else
                    {
                        XYZ point = Utility.GetIntersection(minLengthConduit, crossLineEndOfFitting);
                        if (point != null)
                        {
                            List<Element> elements = child.FullRunElements
                                           .FirstOrDefault(x => x.Any(y => y.Id == minConduit.Id));


                            Element family = child.FullRunElements
                                             .FirstOrDefault(x => x.Any(y => y.Id == minConduit.Id))
                                             .FirstOrDefault(n => n is FamilyInstance && Utility.GetFamilyInstancePartType(n) == "elbow");
                            XYZ familyPoint = (family.Location as LocationPoint).Point;
                            XYZ conduitEndPoint = familyPoint.DistanceTo(minLengthConduit.GetEndPoint(0)) < familyPoint.DistanceTo(minLengthConduit.GetEndPoint(1)) ?
                                  minLengthConduit.GetEndPoint(0) : minLengthConduit.GetEndPoint(1);
                            minLengthConduit = Line.CreateBound(point, Utility.GetXYvalue(conduitEndPoint));
                            KeyValuePair<XYZ, XYZ> valuePair2 = GetMidLineOfGroup(minLengthConduit, Utility.GetMidPoint(minLengthConduit), midPoint);
                            Conduit conduit = Utility.CreateConduit(_doc, child.PreviousElement[0], valuePair2.Key, valuePair2.Value);
                            Utility.SetAlertColor(conduit.Id, _uiDoc);
                            crossLineEndOfFitting = FinalLine;
                        }
                        else
                        {

                        }
                    }
                    if (child.ChildGroup != null)
                    {
                        RecursiveLoopForStrutPlacement(child.ChildGroup);
                    }
                    else
                    {
                        if (child.CurrentElement.Count > 1)
                        {
                            Dictionary<double, List<Element>> groupByElementByElevation = GroupElementsByFilter.GroupByElementsWithElevation(child.CurrentElement, _offsetVariable);
                            maxDistance = 0;
                            GetMaxRackSizeAndMidPoint(groupByElementByElevation, ref maxDistance, ref midPoint);
                            minConduit = Utility.GetMinLengthConduit(child.CurrentElement);
                            minLengthConduit = Utility.GetLineFromConduit(minConduit, true);
                            minMidPoint = Utility.GetMidPoint(minLengthConduit);
                            KeyValuePair<XYZ, XYZ> valuePair2 = GetMidLineOfGroup(minLengthConduit, minMidPoint, midPoint);
                            Conduit conduit = Utility.CreateConduit(_doc, child.PreviousElement[0], valuePair2.Key, valuePair2.Value);
                            Utility.SetAlertColor(conduit.Id, _uiDoc);
                        }
                    }
                    i++;
                }
            }
        }
        private void GetMaxRackSizeAndMidPoint(Dictionary<double, List<Element>> groupByElementByElevation, ref double maxDistance, ref XYZ midPoint)
        {
            foreach (var item in groupByElementByElevation)
            {

                List<Element> elements = Utility.OrderTheConduit(item.Value);
                Element firstElement = elements.FirstOrDefault();
                Element lastElement = elements.LastOrDefault();
                Line firstLine = Utility.GetLineFromConduit(firstElement);
                Line lastLine = Utility.GetLineFromConduit(lastElement);
                XYZ firstDimension = (firstLine.GetEndPoint(0) + firstLine.GetEndPoint(1)) / 2;
                XYZ conlinedir = firstLine.Direction;
                XYZ conlinedir_cross = conlinedir.CrossProduct(XYZ.BasisZ);
                XYZ secDimension = firstDimension + conlinedir_cross.Multiply(2);
                XYZ findintersection = Utility.FindIntersectionPoint(lastLine.GetEndPoint(0), lastLine.GetEndPoint(1), firstDimension, secDimension);
                if (findintersection != null)
                {
                    Line newLine = Line.CreateBound(Utility.GetXYvalue(firstDimension), findintersection);
                    Reference firstRef = Reference.ParseFromStableRepresentation(_doc, firstElement.UniqueId);
                    Reference secRef = Reference.ParseFromStableRepresentation(_doc, lastElement.UniqueId);
                    ReferenceArray refarry = new ReferenceArray();
                    refarry.Append(firstRef);
                    refarry.Append(secRef);
                    Dimension con_dim = _doc.Create.NewDimension(_doc.ActiveView, newLine, refarry);
                    double distance = Math.Round(Convert.ToDouble(con_dim.Value), 3);
                    distance = distance + firstElement.LookupParameter("Outside Diameter").AsDouble() / 2;
                    distance = distance + lastElement.LookupParameter("Outside Diameter").AsDouble() / 2;
                    _doc.Delete(con_dim.Id);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        midPoint = Utility.GetMidPoint(newLine);
                    }
                }

            }
        }
        private KeyValuePair<XYZ, XYZ> GetMidLineOfGroup(Line minLengthConduit, XYZ minMidPoint, XYZ midPoint)
        {
            KeyValuePair<XYZ, XYZ> valuePair = Utility.CrossProduct(minLengthConduit, minMidPoint, 50, true);
            Line crosLine = Line.CreateBound(valuePair.Key, valuePair.Value);
            KeyValuePair<XYZ, XYZ> valuePair1 = Utility.CrossProduct(crosLine, midPoint, 50, true);

            Line crosLine1 = Line.CreateBound(valuePair1.Key, valuePair1.Value);

            XYZ intersection = Utility.FindIntersectionPoint(crosLine, crosLine1);
            return Utility.CrossProduct(crosLine, intersection, minLengthConduit.Length / 2);

        }
        private ElementGroupByOrder RecursiveLoopForFindTheBranchInOrder(Dictionary<int, List<ConduitGrid>> conduitGridDictionary, ElementGroupByOrder elementGroupByOrder)
        {

            for (int k = 0; k < elementGroupByOrder.RunElements.Count; k++)
            {

                int orderIndex = elementGroupByOrder.RunElements.FindIndex(x => x.Any(y => y.Id == elementGroupByOrder.CurrentElement[k].Id));
                List<Element> e = elementGroupByOrder.RunElements[orderIndex];
                int index = e.FindIndex(n => n is FamilyInstance && Utility.GetFamilyInstancePartType(n) == "elbow");
                if (index == -1)
                    continue;
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

                    objGroup.GroupByElementByElevation = GroupElementsByFilter.GroupByElementsWithElevation(objGroup.PreviousCurrentGroupElement, _offsetVariable);
                    if (elementGroupByOrder.ChildGroup == null)
                        elementGroupByOrder.ChildGroup = new List<ElementGroupByOrder>();
                    elementGroupByOrder.ChildGroup.Add(objGroup);
                    conduitGridDictionary.Remove(obj.Key);
                    if (elements1.Count == 1)
                        break;
                }
                if (k == elementGroupByOrder.RunElements.Count - 1 && elementGroupByOrder.ChildGroup != null)
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
        private void LoadDefaultStrut()
        {
            FilteredElementCollector SupportCollector = new FilteredElementCollector(_doc);
            SupportCollector.OfCategory(BuiltInCategory.OST_ElectricalFixtures);
            FamilySymbol Symbol = SupportCollector.FirstOrDefault(r => r.Name == _familyName) as FamilySymbol;
            Family family = null;
            using (SubTransaction subTransaction = new SubTransaction(_doc))
            {
                subTransaction.Start();
                if (Symbol == null)
                {
                    // It is not present, so check for
                    // the file to load it from:
                    if (!File.Exists(_familyPath))
                    {
                        TaskDialog.Show(
                          "Please ensure that the sample table "
                          + "family file '{0}' exists in '{1}'.",
                          _familyPath + _familyFolder);
                    }
                    // Load family from file:

                    if (_doc.LoadFamily(_familyPath, new FamilyOption(), out family))
                    {
                        ISet<ElementId> familySymbolIds = family.GetFamilySymbolIds();
                        foreach (ElementId id in familySymbolIds)
                        {
                            Symbol = _doc.GetElement(id) as FamilySymbol;
                            if (!Symbol.IsActive)
                            {
                                Symbol.Activate();
                                subTransaction.Commit();
                                _doc.Regenerate();
                            }
                        }
                    }
                }
                if (Symbol == null)
                    subTransaction.Dispose();
            }


        }

        private void Get()
        {

        }
        public string GetName()
        {
            return "Revit Addin";
        }
    }
}

