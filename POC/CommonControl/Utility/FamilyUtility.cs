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
        public static List<Line> GetFamilyLines(FamilyInstance fittingFamilyInstance, Options options,bool takeGeometryElementLine=true)
        {
            List<Line> lines = new List<Line>();

            GeometryElement geometryElement = fittingFamilyInstance.get_Geometry(options);
            if (takeGeometryElementLine)
            {
                foreach (GeometryObject item in geometryElement)
                {
                    if(item is Line line)
                    {
                        lines.Add(line);
                    }
                }
                return lines;
            }
            foreach (GeometryInstance item in geometryElement)
            {

                return item.GetInstanceGeometry().OfType<Line>().ToList();


            }
            return lines;
        }
        public static Conduit GetOneSideConduitByFamily(Document doc, FamilyInstance fittingFamilyInstance, XYZ orgin = null, ElementId id = null)
        {

            if (fittingFamilyInstance != null)
            {
                ConnectorSet fittingConnectorList = fittingFamilyInstance.MEPModel.ConnectorManager.Connectors;
                foreach (Connector fittingCon in fittingConnectorList)
                {
                    if (fittingCon.IsConnected)
                    {
                        foreach (Connector fittingConAllRefs in fittingCon.AllRefs)
                        {
                            Conduit connectedConduit = doc.GetElement(fittingConAllRefs.Owner.Id) as Conduit;
                            if (orgin != null)
                            {
                                Line conduitLine = (connectedConduit.Location as LocationCurve).Curve as Line;
                                XYZ xyz = conduitLine.Tessellate().ToList().FirstOrDefault(x => IsXYZTrue(x, orgin));
                                if (xyz != null)
                                    return connectedConduit;
                            }
                            else
                            {
                                if (connectedConduit.Id != id)
                                {
                                    return connectedConduit;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
        public static List<Conduit> GetTwoSideConduitsByFamily(Document doc, FamilyInstance fittingFamilyInstance)
        {
            List<Conduit> conduits = new List<Conduit>();
            if (fittingFamilyInstance != null)
            {
                ConnectorSet fittingConnectorList = fittingFamilyInstance.MEPModel.ConnectorManager.Connectors;
                foreach (Connector fittingCon in fittingConnectorList)
                {
                    if (fittingCon.IsConnected)
                    {
                        foreach (Connector fittingConAllRefs in fittingCon.AllRefs)
                        {
                            if (doc.GetElement(fittingConAllRefs.Owner.Id) is Conduit connectedConduit && connectedConduit.Id != null)
                                conduits.Add(connectedConduit);
                        }
                    }
                }
            }
            return conduits;
        }
        public static string GetFamilyInstanceName(FamilyInstance familyInstance)
        {
            return familyInstance.LookupParameter("Family").AsValueString().ToLower();
        }
        public static string GetFamilyInstancePartType(Object familyInstance)
        {
            FamilySymbol familySymbol = familyInstance is FamilyInstance ? (familyInstance as FamilyInstance).Symbol : (familyInstance as FamilySymbol);
            return familySymbol.Family.get_Parameter(BuiltInParameter.FAMILY_CONTENT_PART_TYPE).AsValueString().ToLower();
        }
        public static List<XYZ> GetCouplingStartAndEndPoint(FamilyInstance fittingFamilyInstance)
        {

            List<XYZ> xyzList = new List<XYZ>();
            if (fittingFamilyInstance != null)
            {
                ConnectorSet fittingConnectorList = fittingFamilyInstance.MEPModel.ConnectorManager.Connectors;
                foreach (Connector fittingCon in fittingConnectorList)
                {
                    if (fittingCon.IsConnected)
                    {
                        foreach (Connector fittingConAllRefs in fittingCon.AllRefs)
                        {
                            xyzList.Add(fittingConAllRefs.Origin);
                        }
                    }
                }
            }

            return xyzList;

        }
        public static FamilyInstance CreateNewUnionFitting(Document doc, Conduit refConduit, Conduit newConduit, XYZ coupoint)
        {
            ConnectorSet primaryConnectorSet = refConduit.ConnectorManager.Connectors;
            ConnectorSet secondaryConnectorSet = newConduit.ConnectorManager.Connectors;
            Connector c1 = GetConnectorClosestTo(primaryConnectorSet, coupoint);
            Connector c2 = GetConnectorClosestTo(secondaryConnectorSet, coupoint);
            FamilyInstance unionFamily = doc.Create.NewUnionFitting(c1, c2);
            return unionFamily;
        }
    }
}
