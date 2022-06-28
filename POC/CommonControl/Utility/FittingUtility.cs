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
        public static List<XYZ> GetFittingStartAndEndPoint(FamilyInstance fittingFamilyInstance)
        {

            List<XYZ> xyzList = new List<XYZ>();
            Options options = new Options();
            GeometryElement geometryElement = (fittingFamilyInstance.get_Geometry(options) as GeometryElement);
            foreach (GeometryInstance item in geometryElement)
            {
                if (item != null)
                {

                    List<XYZ> _tessellate = (item.GetInstanceGeometry().FirstOrDefault(r => r.GetType() == typeof(Arc)) as Arc).Tessellate().ToList();
                    if (_tessellate.Count > 0)
                    {
                        xyzList.Add(_tessellate.FirstOrDefault());
                        xyzList.Add(_tessellate.LastOrDefault());
                    }

                }
            }

            return xyzList;

        }
        public static XYZ GetFittingStartAndEndPoint(Document doc, Conduit conduit, FamilyInstance currentFitting, out XYZ oppositeOrgin)
        {
            XYZ xYZ = null;
            oppositeOrgin = null;
            Dictionary<FamilyInstance, XYZ> nearestFitting = new Dictionary<FamilyInstance, XYZ>();
            if (conduit != null)
            {
                List<Conduit> conduits = GetTwoSideConduitsByFamily(doc, currentFitting);
                Conduit flipConduit = conduits.FirstOrDefault(x => x.Id != conduit.Id);
                bool isTrue = false;
                foreach (Connector con in flipConduit.ConnectorManager.Connectors)
                {
                    if (con.IsConnected)
                    {
                        foreach (Connector allRefs in con.AllRefs)
                        {
                            if (doc.GetElement(allRefs.Owner.Id) is FamilyInstance _familyInstance)
                            {
                                string partType = _familyInstance.Symbol.Family.get_Parameter(BuiltInParameter.FAMILY_CONTENT_PART_TYPE).AsValueString().ToLower();
                                if (allRefs.IsConnected && currentFitting.Id == allRefs.Owner.Id && partType == "elbow")
                                {
                                    oppositeOrgin = con.Origin;
                                    isTrue = true;
                                    break;
                                }
                            }
                        }
                        if (isTrue)
                            break;
                    }
                }
                isTrue = false;
                foreach (Connector con in conduit.ConnectorManager.Connectors)
                {
                    if (con.IsConnected)
                    {
                        foreach (Connector allRefs in con.AllRefs)
                        {
                            if (doc.GetElement(allRefs.Owner.Id) is FamilyInstance _familyInstance)
                            {
                                string partType = _familyInstance.Symbol.Family.get_Parameter(BuiltInParameter.FAMILY_CONTENT_PART_TYPE).AsValueString().ToLower();
                                if (allRefs.IsConnected && currentFitting.Id == allRefs.Owner.Id && partType == "elbow")
                                {
                                    return con.Origin;
                                }
                            }
                        }
                    }
                }


            }
            return xYZ;
        }
        public static XYZ GetFittingConduitOrgin(FamilyInstance fittingFamilyInstance, Conduit conduit, out XYZ oppositeOrgin)
        {
            oppositeOrgin = null;
            Line conduitLine = (conduit.Location as LocationCurve).Curve as Line;
            List<XYZ> xyzConduitList = conduitLine.Tessellate().ToList();
            List<XYZ> xyzFamilyList = GetFittingStartAndEndPoint(fittingFamilyInstance);
            XYZ connectedOrgin = xyzConduitList.FirstOrDefault(x => xyzFamilyList.Any(y => IsXYZTrue(y, x)));
            oppositeOrgin = xyzFamilyList.FirstOrDefault(x => IsXYZTrue(x, connectedOrgin) == false);
            return connectedOrgin;
        }
        public static FamilyInstance GetFittingByConduit(Document doc, Conduit conduit, FamilyInstance currentFitting = null, string type = "elbow")
        {
            if (conduit != null)
                foreach (Connector con in conduit.ConnectorManager.Connectors)
                {
                    if (con.IsConnected)
                    {
                        foreach (Connector allRefs in con.AllRefs)
                        {
                            if (doc.GetElement(allRefs.Owner.Id) is FamilyInstance _familyInstance)
                            {
                                string partType = _familyInstance.Symbol.Family.get_Parameter(BuiltInParameter.FAMILY_CONTENT_PART_TYPE).AsValueString().ToLower();
                                if (allRefs.IsConnected && (currentFitting == null || currentFitting.Id != allRefs.Owner.Id) && partType == type)
                                {
                                    return _familyInstance;
                                }
                            }

                        }
                    }

                }
            return null;
        }
        public static List<FamilyInstance> GetTwoSideFittingsByConduit(Document doc, Conduit conduit, string type = null)
        {
            List<FamilyInstance> nearestFitting = new List<FamilyInstance>();
            if (conduit != null)
            {
                foreach (Connector con in conduit.ConnectorManager.Connectors)
                {
                    if (con.IsConnected)
                    {
                        foreach (Connector allRefs in con.AllRefs)
                        {
                            if (doc.GetElement(allRefs.Owner.Id) is FamilyInstance _familyInstance)
                            {
                                string partType = _familyInstance.Symbol.Family.get_Parameter(BuiltInParameter.FAMILY_CONTENT_PART_TYPE).AsValueString().ToLower();
                                if (allRefs.IsConnected && (partType == type || partType == null))
                                {
                                    nearestFitting.Add(_familyInstance);
                                }
                            }

                        }
                    }

                }
            }
            return nearestFitting;
        }
        public static Dictionary<XYZ, FamilyInstance> GetTwoSideFittingsWithOrginByConduit(Document doc, Conduit conduit, string type = "elbow")
        {
            Dictionary<XYZ, FamilyInstance> nearestFitting = new Dictionary<XYZ, FamilyInstance>();
            if (conduit != null)
                foreach (Connector con in conduit.ConnectorManager.Connectors)
                {
                    if (con.IsConnected)
                    {
                        foreach (Connector allRefs in con.AllRefs)
                        {
                            if (doc.GetElement(allRefs.Owner.Id) is FamilyInstance _familyInstance)
                            {
                                string partType = _familyInstance.Symbol.Family.get_Parameter(BuiltInParameter.FAMILY_CONTENT_PART_TYPE).AsValueString().ToLower();
                                if (allRefs.IsConnected && (partType == type || partType == null))
                                {
                                    nearestFitting.Add(allRefs.Origin, _familyInstance);
                                }
                            }

                        }
                    }

                }
            return nearestFitting;
        }
        public static Double CalculateNonCureFittingLength(FamilyInstance fittingFamilyInstance)
        {
            double family_angle = fittingFamilyInstance.LookupParameter("Angle").AsDouble();
            double family_conduitLength = fittingFamilyInstance.LookupParameter("Conduit Length").AsDouble();
            double family_bendRadius = fittingFamilyInstance.LookupParameter("Bend Radius").AsDouble();
            double lengthOfFillting = (family_angle * family_bendRadius) + 2 * family_conduitLength;
            return lengthOfFillting;
        }
        public static Double CalculateFittingAngle(FamilyInstance fittingFamilyInstance)
        {
            if (fittingFamilyInstance != null)
            {
                double family_angle = fittingFamilyInstance.LookupParameter("Angle").AsDouble();
                return (Math.Round(family_angle / (Math.PI / 180), 2));
            }
            return 0.0;
        }
        public static Double CalculateCurveFittingLength(FamilyInstance fittingFamilyInstance)
        {
            if (fittingFamilyInstance != null)
            {
                double family_conduitLength = 0.0;
                double family_bendRadius;
                double family_angle;

                Parameter p = fittingFamilyInstance.LookupParameter("Conduit Length");
                if (p == null || p.ToString() == string.Empty)
                {
                    family_angle = fittingFamilyInstance.LookupParameter("Angle").AsDouble();
                    family_bendRadius = fittingFamilyInstance.LookupParameter("Bend Radius").AsDouble();
                }
                else
                {
                    family_conduitLength = fittingFamilyInstance.LookupParameter("Conduit Length").AsDouble();
                    family_angle = fittingFamilyInstance.LookupParameter("Angle").AsDouble();
                    family_bendRadius = fittingFamilyInstance.LookupParameter("Bend Radius").AsDouble();
                }


                return ((family_angle * family_bendRadius) + (family_conduitLength * 2));
            }
            return 0.0;

        }
    }
}
