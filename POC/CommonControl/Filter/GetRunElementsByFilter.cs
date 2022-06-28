using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace POC
{
    [Transaction(TransactionMode.Manual)]
    public class GetRunElementsByFilter
    {

        public static void FittingSelection(Document doc, FamilyInstance familyInstance, ElementId selectedFamilyId, ElementId ConduitId,
          ref List<Element> lstElements, bool IsWholeRun, ElementId conduitRunId = null, bool isConduitOnly = false, bool isFamilyOnly = false, string familyPartType = null, string familyName = null)
        {
            try
            {
                if (familyInstance.Category.Name.ToLower() == "conduit fittings")
                {
                    if (!lstElements.Any(x => x.Id == familyInstance.Id))
                    {
                        string partType = familyInstance.Symbol.Family.get_Parameter(BuiltInParameter.FAMILY_CONTENT_PART_TYPE).AsValueString().ToLower();
                        if (familyPartType != null && partType == familyPartType.ToLower())
                        {
                            if (familyName != null && familyInstance.Symbol.FamilyName.ToLower().Contains(familyName.ToLower()))
                                lstElements.Add(familyInstance);
                            else if (familyName == null)
                                lstElements.Add(familyInstance);
                        }
                        else if (familyPartType == null)
                        {
                            lstElements.Add(familyInstance);

                        }
                    }
                    ConnectorSet familyConnectorList = familyInstance.MEPModel.ConnectorManager.Connectors;
                    string partName = familyInstance.Symbol.Family.get_Parameter(BuiltInParameter.FAMILY_CONTENT_PART_TYPE).AsValueString().ToLower();
                    if (!IsWholeRun && familyPartType != null && partName != familyPartType.ToLower())
                    {
                        familyInstance = null;
                    }
                    if (familyInstance != null && familyInstance.Id != selectedFamilyId)
                    {
                        foreach (Connector familyCon in familyConnectorList)
                        {
                            if (familyCon.IsConnected)
                            {
                                foreach (Connector fittingConAllRefs in familyCon.AllRefs)
                                {
                                    Conduit connectedConduit = doc.GetElement(fittingConAllRefs.Owner.Id) as Conduit;
                                    if (ConduitId != connectedConduit.Id)
                                    {
                                        if (isConduitOnly == false && isFamilyOnly)
                                        {
                                            selectedFamilyId = familyInstance.Id;
                                        }
                                        if (conduitRunId != null && connectedConduit.RunId != conduitRunId)
                                        {
                                            break;
                                        }
                                        ConduitSelection(doc, connectedConduit, selectedFamilyId, ref lstElements, IsWholeRun, conduitRunId, isConduitOnly, isFamilyOnly, familyPartType, familyName);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
        public static void ConduitSelection(Document doc, Conduit conduit, ElementId selectedFamilyId,
          ref List<Element> lstElements, bool IsWholeRun, ElementId conduitRunId = null, bool isConduitOnly = false, bool isFamilyOnly = false,
            string familyPartType = null, string familyName = null)
        {
            try
            {
                if (conduit == null)
                    return;
                if (!lstElements.Any(x => x.Id == conduit.Id))
                {
                    lstElements.Add(conduit);
                    ConnectorSet connectorSets = conduit.ConnectorManager.Connectors;
                    foreach (Connector con in connectorSets)
                    {
                        if (con.IsConnected)
                        {
                            foreach (Connector conAllRefs in con.AllRefs)
                            {
                                if (conAllRefs.IsConnected && conAllRefs.Owner.GetType() == typeof(FamilyInstance) && selectedFamilyId != conAllRefs.Owner.Id)
                                {
                                    FamilyInstance fittingFamilyInstance = doc.GetElement(conAllRefs.Owner.Id) as FamilyInstance;

                                    if (conduitRunId != null && Utility.GetTwoSideConduitsByFamily(doc, fittingFamilyInstance).Count(x => x.RunId != conduitRunId) == 1)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        FittingSelection(doc, fittingFamilyInstance, selectedFamilyId, conduit.Id, ref lstElements, IsWholeRun, conduitRunId, isConduitOnly, isFamilyOnly, familyPartType, familyName);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
        public static List<Conduit> CollectOneSideConduits(Document doc, List<Conduit> conduit)
        {
            List<Element> collection = new List<Element>();
            List<Conduit> endConduits = new List<Conduit>();
            List<ForConduit> forConduits = new List<ForConduit>();

            foreach (Conduit item in conduit)
            {
                Utility.GetStartEndPointForConduit(item, out XYZ startOrgin, out XYZ endOrgin);
                ForConduit forConduit1 = new ForConduit
                {
                    Conduit = item
                };
                ForConduit forConduit2 = new ForConduit
                {
                    Conduit = item,
                    ConnectedOrgin = endOrgin
                };
                Dictionary<XYZ, FamilyInstance> dicFamily = Utility.GetTwoSideFittingsWithOrginByConduit(doc, item);
                forConduit2.FamilyInstance = dicFamily.FirstOrDefault(x => Utility.IsXYZTrue(x.Key, endOrgin)).Value;
                forConduit2.IsFamily = forConduit2.FamilyInstance != null;
                forConduits.Add(forConduit2);
                forConduit1.ConnectedOrgin = startOrgin;
                forConduit1.FamilyInstance = dicFamily.FirstOrDefault(x => Utility.IsXYZTrue(x.Key, startOrgin)).Value;
                forConduit1.IsFamily = forConduit1.FamilyInstance != null;
                forConduits.Add(forConduit1);

            }
            var tuples = forConduits.Select((x, i) => new Tuple<ForConduit, double>(x, x.ConnectedOrgin.DistanceTo(forConduits[0].ConnectedOrgin))).OrderBy(y => y.Item2).ToList();
            var _tuples = forConduits.Select((x, i) => new Tuple<ForConduit, double>(x, x.ConnectedOrgin.DistanceTo(forConduits[1].ConnectedOrgin))).OrderBy(y => y.Item2).ToList();
            bool isReverse = tuples.Take(conduit.Count).Sum(x => x.Item2) > _tuples.Take(conduit.Count).Sum(x => x.Item2);
            var oneSide = isReverse ? _tuples.Take(conduit.Count).ToList() : tuples.Take(conduit.Count).ToList();
            var anotherSide = isReverse ? _tuples.Skip(conduit.Count).ToList() : tuples.Skip(conduit.Count).ToList();

            int s = 1;
            double value1 = 0.0;
            double value2 = 0.0;
            bool isFirstNull = oneSide.TrueForAll(x => x.Item1.IsFamily);
            bool isSecNull = anotherSide.TrueForAll(x => x.Item1.IsFamily);
            if (isFirstNull == false && isSecNull == false)
            {
                TaskDialog.Show("Warning", "Kindly select at source");
                return new List<Conduit>();
            }

            foreach (var item in oneSide)
            {
                if (s == oneSide.Count)
                    break;
                value1 += item.Item1.ConnectedOrgin.DistanceTo(oneSide[s].Item1.ConnectedOrgin);
                s++;
            }

            s = 1;
            foreach (var item in anotherSide)
            {
                if (s == anotherSide.Count)
                    break;
                value2 += item.Item1.ConnectedOrgin.DistanceTo(anotherSide[s].Item1.ConnectedOrgin);
                s++;
            }

            if (isFirstNull && isSecNull == false)
            {
                foreach (var item in anotherSide)
                {
                    ElementId familyID = item.Item1.IsFamily == false ? null : item.Item1.FamilyInstance.Id;
                    ConduitSelection(doc, item.Item1.Conduit, familyID, ref collection,false,item.Item1.Conduit.RunId, true);
                    if (item.Item1.IsFamily == false)
                    {
                        collection.Remove(collection.First(x => x.Id == item.Item1.Conduit.Id));
                    }
                }
            }
            else
            {

                if (value1 >= value2 || (isFirstNull == false && isSecNull))
                {
                    foreach (var item in oneSide)
                    {
                        ElementId familyID = item.Item1.IsFamily == false ? null : item.Item1.FamilyInstance.Id;
                        ConduitSelection(doc, item.Item1.Conduit, familyID, ref collection,false, item.Item1.Conduit.RunId, true);
                        if (item.Item1.IsFamily == false)
                        {
                            collection.Remove(collection.First(x => x.Id == item.Item1.Conduit.Id));
                        }
                    }
                }
                else
                {
                    foreach (var item in anotherSide)
                    {
                        ElementId familyID = item.Item1.IsFamily == false ? null : item.Item1.FamilyInstance.Id;
                        ConduitSelection(doc, item.Item1.Conduit, item.Item1.FamilyInstance.Id, ref collection,false, item.Item1.Conduit.RunId, true);
                        if (item.Item1.IsFamily == false)
                        {
                            collection.Remove(collection.First(x => x.Id == item.Item1.Conduit.Id));
                        }
                    }
                }
            }
            foreach (Element e in collection)
            {
                ConnectorSet connectorSet = Utility.GetUnusedConnectors(e);
                if (!connectorSet.IsEmpty)
                {
                    foreach (Connector c in connectorSet)
                    {
                        endConduits.Add(e as Conduit);
                    }
                }
                else
                {

                    foreach (Connector con in (e as Conduit).ConnectorManager.Connectors)
                    {
                        if (con.IsConnected)
                        {
                            foreach (Connector allRefs in con.AllRefs)
                            {
                                if (doc.GetElement(allRefs.Owner.Id) is FamilyInstance _familyInstance)
                                {
                                    string partType = _familyInstance.Symbol.Family.get_Parameter(BuiltInParameter.FAMILY_CONTENT_PART_TYPE).AsValueString().ToLower();
                                    if (allRefs.IsConnected && partType == "union" && _familyInstance.Symbol.FamilyName.Contains("Coupling"))
                                    {
                                        endConduits.Add(e as Conduit);
                                    }
                                }

                            }
                        }

                    }

                }
            }


            return endConduits;
        }
        public static List<FamilyInstance> GetStartEndCouplingFromRun(Document doc, Conduit conduit, ElementId conduitRunId)
        {
            List<FamilyInstance> familyInstances = new List<FamilyInstance>();
            List<Element> collection = new List<Element>();
            ConduitSelection(doc, conduit, null, ref collection,false, conduitRunId);

            if (collection.Count > 0)
            {
                if (collection[0].GetType() == typeof(Conduit))
                {
                    FamilyInstance familyInstanceUnion = Utility.GetFittingByConduit(doc, collection[0] as Conduit, null, "union");
                    familyInstances.Add(familyInstanceUnion);
                }
                if (collection[collection.Count - 1].GetType() == typeof(Conduit))
                {
                    FamilyInstance familyInstanceUnion = Utility.GetFittingByConduit(doc, collection[collection.Count - 1] as Conduit, null, "union");
                    familyInstances.Add(familyInstanceUnion);
                }

                return familyInstances;
            }
            else if (collection.Count == 0 && collection[0].GetType() == typeof(Conduit))
            {
                return Utility.GetTwoSideFittingsByConduit(doc, conduit);
            }

            return familyInstances;
        }
        private static XYZ GetXYZForUnRunID(Document doc, Element familyInstance, Conduit conduit)
        {
            FamilyInstance family = null;
            XYZ secondaryConnectedOrgin = null;
            if (familyInstance is FamilyInstance)
            {
                family = familyInstance as FamilyInstance;
                string partType = family.Symbol.Family.get_Parameter(BuiltInParameter.FAMILY_CONTENT_PART_TYPE).AsValueString().ToLower();
                if (partType != "elbow")
                    familyInstance = null;
            }

            if (family != null)
            {
                XYZ primaryConnectedOrgin = Utility.GetFittingConduitOrgin(family, conduit, out secondaryConnectedOrgin);
                Line conduitLine = (conduit.Location as LocationCurve).Curve as Line;
                return conduitLine.Tessellate().FirstOrDefault(x => !Utility.IsXYZTrue(x, primaryConnectedOrgin));
            }
            else
            {
                List<FamilyInstance> fi = Utility.GetTwoSideFittingsByConduit(doc, conduit);
                family = fi.FirstOrDefault(x => x.Id == family.Id);
                XYZ primaryConnectedOrgin = Utility.GetFittingConduitOrgin(family, conduit, out secondaryConnectedOrgin);
                Line conduitLine = (conduit.Location as LocationCurve).Curve as Line;
                return conduitLine.Tessellate().FirstOrDefault(x => !Utility.IsXYZTrue(x, primaryConnectedOrgin));
            }
        }
    }

}
public class ForConduit
{
    public Conduit Conduit { get; set; }
    public XYZ ConnectedOrgin { get; set; }
    public FamilyInstance FamilyInstance { get; set; }
    public bool IsFamily { get; set; }
}