#region Namespaces
#endregion 

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace POC
{

    public partial class Utility
    {

        #region Axis
        public static XYZ GetXYvalue(XYZ xy)
        {

            return xy != null ? new XYZ(xy.X, xy.Y, 0) : null;

        }
      
        public static XYZ SetZvalue(XYZ xy, XYZ xYZ = null, double Z = 0)
        {

            return xYZ != null ? new XYZ(xy.X, xy.Y, xYZ.Z) : new XYZ(xy.X, xy.Y, Z);

        }
        public static XYZ XYZroundOf(XYZ xyz, int digit)
        {
            return new XYZ(Math.Round(xyz.X, digit), Math.Round(xyz.Y, digit), Math.Round(xyz.Z, digit));
        }
        public static string FindDifferentAxis(XYZ value1, XYZ value2)
        {
            string axis = string.Empty;
            if (value1 != null && value2 != null)
            {
                if (Math.Round(value1.X, 4) != Math.Round(value2.X, 4))
                {
                    axis = "X";
                }
                if (Math.Round(value1.Y, 4) != Math.Round(value2.Y, 4))
                {
                    axis += "Y";
                }
                if (Math.Round(value1.Z, 4) != Math.Round(value2.Z, 4))
                {
                    axis += "Z";
                }
            }

            return axis;

        }
        public static XYZ FindClonePoint(XYZ value1, XYZ value2, XYZ cloneCoupling)
        {
            string axis = FindDifferentAxis(value1, value2);

            if (axis == "X")
            {
                return new XYZ(cloneCoupling.X, value1.Y, value1.Z);
            }
            else if (axis == "Y")
            {
                return new XYZ(value1.X, cloneCoupling.Y, value1.Z);
            }
            else if (axis == "Z")
            {
                return new XYZ(value1.X, value1.Y, cloneCoupling.Z);
            }
            else if (axis == "XY")
            {
                return new XYZ(cloneCoupling.X, cloneCoupling.Y, value1.Z);
            }
            else if (axis == "XZ")
            {
                return new XYZ(cloneCoupling.X, value1.Y, cloneCoupling.Z);
            }
            else if (axis == "YZ")
            {
                return new XYZ(value1.X, cloneCoupling.Y, cloneCoupling.Z);
            }
            else
            {
                return new XYZ(cloneCoupling.X, cloneCoupling.Y, cloneCoupling.Z); ;
            }

        }
        public static double GetMinimumDistance(XYZ first, XYZ second, XYZ key)
        {
            double f = first.DistanceTo(key);
            double s = second.DistanceTo(key);
            return f < s ? f : s;
        }
        public static double GetMaximumDistance(XYZ first, XYZ second, XYZ key)
        {
            double f = first.DistanceTo(key);
            double s = second.DistanceTo(key);
            return f > s ? f : s;

        }
        public static XYZ GetMinimumXYZ(XYZ first, XYZ second, XYZ key)
        {
            double f = first.DistanceTo(key);
            double s = second.DistanceTo(key);
            return f < s ? first : second;
        }
        public static XYZ GetMaximumXYZ(XYZ first, XYZ second, XYZ key)
        {
            double f = first.DistanceTo(key);
            double s = second.DistanceTo(key);
            return f > s ? first : second;

        }

        public static XYZ GetMidPoint(XYZ first, XYZ last)
        {
            return (first + last) / 2;
        }
        #endregion
        #region boolean
        public static bool IsDifferentElevation(Line line)
        {
            double zValue = Math.Round(line.Direction.Z, 5);
            return !(zValue == 1 || zValue == -1 || zValue == 0);
        }
        public static bool IsDifferentElevation(Element element)
        {
            Line line = GetLineFromConduit(element);
            double zValue = Math.Round(line.Direction.Z, 5);
            return !(zValue == 1 || zValue == -1 || zValue == 0);
        }
        public static bool IsXYZTrue(XYZ value1, XYZ value2)
        {

            if (value1 != null && value2 != null)
            {
                return (Math.Round(value1.X, 5) == Math.Round(value2.X, 5) || Math.Round(value1.X, 4) == Math.Round(value2.X, 4)) &&
                                    (Math.Round(value1.Y, 5) == Math.Round(value2.Y, 5) || Math.Round(value1.Y, 4) == Math.Round(value2.Y, 4)) &&
                                    (Math.Round(value1.Z, 5) == Math.Round(value2.Z, 5) || Math.Round(value1.Z, 4) == Math.Round(value2.Z, 4));
            }
            return false;
        }
        public static bool IsXYZTrueRoundOfTwoThree(XYZ value1, XYZ value2)
        {

            if (value1 != null && value2 != null)
            {
                return (Math.Round(value1.X, 3) == Math.Round(value2.X, 2) || Math.Round(value1.X, 3) == Math.Round(value2.X, 3) || Math.Round(value1.X, 5) == Math.Round(value2.X, 5) || Math.Round(value1.X, 4) == Math.Round(value2.X, 4)) &&
                                    (Math.Round(value1.Y, 2) == Math.Round(value2.Y, 2) || Math.Round(value1.Y, 3) == Math.Round(value2.Y, 3) || Math.Round(value1.Y, 5) == Math.Round(value2.Y, 5) || Math.Round(value1.Y, 4) == Math.Round(value2.Y, 4)) &&
                                    (Math.Round(value1.Z, 2) == Math.Round(value2.Z, 2) || Math.Round(value1.Z, 3) == Math.Round(value2.Z, 3) || Math.Round(value1.Z, 5) == Math.Round(value2.Z, 5) || Math.Round(value1.Z, 4) == Math.Round(value2.Z, 4));
            }
            return false;
        }
        public static bool IsSameDirection(XYZ value1, XYZ value2)
        {

            if (value1 != null && value2 != null)
            {
                return Math.Round(Math.Abs(value1.X), 3) == Math.Round(Math.Abs(value2.X), 3) &&
                                    Math.Round(Math.Abs(value1.Y), 3) == Math.Round(Math.Abs(value2.Y), 3);
            }
            return false;
        }
        #endregion
        #region CrossProduct
        public static Line CrossProductLine(Line line, XYZ point, Double multiply, bool setZvalueZero = false)
        {
            XYZ directionForward = line.Direction;
            XYZ directionBackward = (-1) * directionForward;
            directionForward = point + directionForward.CrossProduct(XYZ.BasisZ).Multiply(multiply);
            directionBackward = point + directionBackward.CrossProduct(XYZ.BasisZ).Multiply(multiply);
            if (setZvalueZero)
                return Line.CreateBound(GetXYvalue(directionForward), GetXYvalue(directionBackward));
            else
                return Line.CreateBound(directionForward, directionBackward);

        }
        public static Line CrossProductLine(Element element, XYZ point, Double multiply, bool setZvalueZero = false)
        {

            XYZ directionForward = GetLineFromConduit(element, setZvalueZero).Direction;
            XYZ directionBackward = (-1) * directionForward;
            directionForward = point + directionForward.CrossProduct(XYZ.BasisZ).Multiply(multiply);
            directionBackward = point + directionBackward.CrossProduct(XYZ.BasisZ).Multiply(multiply);
            if (setZvalueZero)
                return Line.CreateBound(GetXYvalue(directionForward), GetXYvalue(directionBackward));
            else
                return Line.CreateBound(directionForward, directionBackward);

        }

        public static Line CrossProductLine(Conduit conduit, XYZ point, Double multiply, bool setZvalueZero = false)
        {

            XYZ directionForward = GetLineFromConduit(conduit, setZvalueZero).Direction;
            XYZ directionBackward = (-1) * directionForward;
            directionForward = point + directionForward.CrossProduct(XYZ.BasisZ).Multiply(multiply);
            directionBackward = point + directionBackward.CrossProduct(XYZ.BasisZ).Multiply(multiply);
            if (setZvalueZero)
                return Line.CreateBound(GetXYvalue(directionForward), GetXYvalue(directionBackward));
            else
                return  Line.CreateBound(directionForward, directionBackward);

        }

        public static KeyValuePair<XYZ, XYZ> CrossProduct(Line line, XYZ point, Double multiply, bool setZvalueZero = false)
        {
            XYZ directionForward = line.Direction;
            XYZ directionBackward = (-1) * directionForward;
            directionForward = point + directionForward.CrossProduct(XYZ.BasisZ).Multiply(multiply);
            directionBackward = point + directionBackward.CrossProduct(XYZ.BasisZ).Multiply(multiply);
            if (setZvalueZero)
                return new KeyValuePair<XYZ, XYZ>(GetXYvalue(directionForward), GetXYvalue(directionBackward));
            else
                return new KeyValuePair<XYZ, XYZ>(directionForward, directionBackward);

        }
        public static KeyValuePair<XYZ, XYZ> CrossProduct(Element element, XYZ point, Double multiply, bool setZvalueZero = false)
        {
           
                XYZ directionForward = GetLineFromConduit(element, setZvalueZero).Direction;
                XYZ directionBackward = (-1) * directionForward;
                directionForward = point + directionForward.CrossProduct(XYZ.BasisZ).Multiply(multiply);
                directionBackward = point + directionBackward.CrossProduct(XYZ.BasisZ).Multiply(multiply);
                if (setZvalueZero)
                    return new KeyValuePair<XYZ, XYZ>(GetXYvalue(directionForward), GetXYvalue(directionBackward));
                else
                    return new KeyValuePair<XYZ, XYZ>(directionForward, directionBackward);

        }

        public static KeyValuePair<XYZ, XYZ> CrossProduct(Conduit conduit, XYZ point, Double multiply, bool setZvalueZero = false)
        {

            XYZ directionForward = GetLineFromConduit(conduit, setZvalueZero).Direction;
            XYZ directionBackward = (-1) * directionForward;
            directionForward = point + directionForward.CrossProduct(XYZ.BasisZ).Multiply(multiply);
            directionBackward = point + directionBackward.CrossProduct(XYZ.BasisZ).Multiply(multiply);
            if (setZvalueZero)
                return new KeyValuePair<XYZ, XYZ>(GetXYvalue(directionForward), GetXYvalue(directionBackward));
            else
                return new KeyValuePair<XYZ, XYZ>(directionForward, directionBackward);

        }
        public static KeyValuePair<XYZ, XYZ> CrossProductForward(Line line, XYZ point, Double multiply, bool setZvalueZero = false)
        {
            XYZ directionForward = line.Direction;
            directionForward = point + directionForward.CrossProduct(XYZ.BasisZ).Multiply(multiply);
            if (setZvalueZero)
                return new KeyValuePair<XYZ, XYZ>(GetXYvalue(directionForward), GetXYvalue(point));
            else
                return new KeyValuePair<XYZ, XYZ>(directionForward, point);

        }
        public static KeyValuePair<XYZ, XYZ> CrossProductBackward(Line line, XYZ point, Double multiply, bool setZvalueZero = false)
        {
            XYZ directionForward = line.Direction;
            XYZ directionBackward = (-1) * directionForward;
            directionBackward = point + directionBackward.CrossProduct(XYZ.BasisZ).Multiply(multiply);
            if (setZvalueZero)
                return new KeyValuePair<XYZ, XYZ>(GetXYvalue(point), GetXYvalue(directionBackward));
            else
                return new KeyValuePair<XYZ, XYZ>(point, directionBackward);

        }
        #endregion
        #region Color
        public static void SetAlertColor(ElementId id, UIDocument _uidoc, Autodesk.Revit.DB.Color color = null)
        {

            if (color == null)
                color = new Autodesk.Revit.DB.Color(255, 0, 0);
            var patternCollector = new FilteredElementCollector(_uidoc.Document);
            patternCollector.OfClass(typeof(FillPatternElement));
            FillPatternElement fpe = patternCollector.ToElements().Cast<FillPatternElement>().First(x => x.GetFillPattern().Name == "<Solid fill>");
            Autodesk.Revit.DB.OverrideGraphicSettings ogs = SetOverrideGraphicSettings(fpe, color);
            _uidoc.ActiveView.SetElementOverrides(id, ogs);
        }
        public static Autodesk.Revit.DB.OverrideGraphicSettings SetOverrideGraphicSettings(FillPatternElement fpe, Autodesk.Revit.DB.Color color)
        {
            Autodesk.Revit.DB.OverrideGraphicSettings ogs = new Autodesk.Revit.DB.OverrideGraphicSettings();
            ogs.SetProjectionLineColor(color);
            ogs.SetCutBackgroundPatternColor(color);
            ogs.SetCutForegroundPatternColor(color);
            ogs.SetCutLineColor(color);
            ogs.SetSurfaceBackgroundPatternColor(color);
            ogs.SetSurfaceForegroundPatternColor(color);
            ogs.SetSurfaceBackgroundPatternId(fpe.Id);
            ogs.SetSurfaceBackgroundPatternVisible(true);
            ogs.SetSurfaceForegroundPatternId(fpe.Id);
            ogs.SetSurfaceForegroundPatternVisible(true);
            return ogs;
        }
        #endregion
        #region Connectors
        public static void GetClosestConnectors(ConnectorSet PrimaryConnectors, ConnectorSet SecondaryConnectors, out Connector ConnectorOne, out Connector ConnectorTwo)
        {
            ConnectorOne = null;
            ConnectorTwo = null;
            double MinDist = double.MaxValue;
            foreach (Connector Con in PrimaryConnectors)
            {
                foreach (Connector con2 in SecondaryConnectors)
                {
                    double dist = Con.Origin.DistanceTo(con2.Origin);
                    if (dist < MinDist)
                    {
                        ConnectorOne = Con;
                        ConnectorTwo = con2;
                        MinDist = dist;
                    }
                }
            }
        }
        //Author - Common 
        public static void Connect(XYZ p, Element a, Connector cb)
        {
            ConnectorManager cm = GetConnectorManager(a);
            if (null == cm)
            {
                throw new ArgumentException(
                  "Element a has no connectors.");
            }
            Connector ca = GetConnectorClosestTo(
              cm.Connectors, p);
            ca.ConnectTo(cb);
        }
        //Author - Common 
        public static Connector GetConnectorClosestTo(ConnectorSet connectors, XYZ p)
        {
            Connector targetConnector = null;
            double minDist = double.MaxValue;

            foreach (Connector c in connectors)
            {
                double d = c.Origin.DistanceTo(p);

                if (d < minDist)
                {
                    targetConnector = c;
                    minDist = d;
                }
            }
            return targetConnector;
        }
        //Author - Common 
        public static void Connect(XYZ p, Element a, Element b)
        {
            ConnectorManager cm = GetConnectorManager(a);

            if (null == cm)
            {
                throw new ArgumentException(
                  "Element a has no connectors.");
            }

            Connector ca = GetConnectorClosestTo(
              cm.Connectors, p);

            cm = GetConnectorManager(b);

            if (null == cm)
            {
                throw new ArgumentException(
                  "Element b has no connectors.");
            }

            Connector cb = GetConnectorClosestTo(
              cm.Connectors, p);

            ca.ConnectTo(cb);
            //cb.ConnectTo( ca );
        }
        //Author - Common 
        static ConnectorManager GetConnectorManager(Element e)
        {
            MEPCurve mc = e as MEPCurve;
            FamilyInstance fi = e as FamilyInstance;

            if (null == mc && null == fi)
            {
                throw new ArgumentException(
                  "Element is neither an MEP curve nor a fitting.");
            }

            return null == mc
              ? fi.MEPModel.ConnectorManager
              : mc.ConnectorManager;
        }
        //Author - Common 
        public static ConnectorSet GetConnectors(Element e)
        {
            ConnectorSet connectors = null;

            if (e is FamilyInstance instance)
            {
                MEPModel m = instance.MEPModel;

                if (null != m
                  && null != m.ConnectorManager)
                {
                    connectors = m.ConnectorManager.Connectors;
                }
            }
            else if (e is Wire wire)
            {
                connectors = wire
                  .ConnectorManager.Connectors;
            }
            else
            {
                Debug.Assert(
                  e.GetType().IsSubclassOf(typeof(MEPCurve)),
                  "expected all candidate connector provider "
                  + "elements to be either family instances or "
                  + "derived from MEPCurve");

                if (e is MEPCurve curve)
                {
                    connectors = curve
                      .ConnectorManager.Connectors;
                }
            }
            return connectors;
        }
        public static ConnectorSet GetUnusedConnectors(Element e)
        {
            ConnectorSet unConnectors = null;

            if (e is FamilyInstance instance)
            {
                MEPModel m = instance.MEPModel;

                if (null != m
                  && null != m.ConnectorManager)
                {
                    unConnectors = m.ConnectorManager.UnusedConnectors;
                }
            }
            else if (e is Wire wire)
            {
                unConnectors = wire
                  .ConnectorManager.UnusedConnectors;
            }
            else if (e is Conduit conduit)
            {
                unConnectors = conduit
                  .ConnectorManager.UnusedConnectors;
            }
            else
            {
                Debug.Assert(
                  e.GetType().IsSubclassOf(typeof(MEPCurve)),
                  "expected all candidate connector provider "
                  + "elements to be either family instances or "
                  + "derived from MEPCurve");

                if (e is MEPCurve curve)
                {
                    unConnectors = curve
                      .ConnectorManager.UnusedConnectors;
                }
            }
            return unConnectors;
        }
        #endregion
        #region Parameter

        public static string GetParameterValue(Parameter Param)
        {
            return Param.StorageType switch
            {
                StorageType.Double => Param.AsValueString(),
                StorageType.ElementId => Param.AsElementId().IntegerValue.ToString(),
                StorageType.Integer => Param.AsValueString(),
                StorageType.None => Param.AsValueString(),
                StorageType.String => Param.AsString(),
                _ => string.Empty,
            };
        }
        public static void SetParameterValue(Parameter OldParam, Parameter NewParam)
        {
            switch (OldParam.StorageType)
            {
                case StorageType.Double:
                    OldParam.Set(NewParam.AsDouble());
                    break;
                case StorageType.ElementId:
                    OldParam.Set(NewParam.AsElementId().IntegerValue);
                    break;
                case StorageType.Integer:
                    OldParam.Set(NewParam.AsInteger());
                    break;
                case StorageType.None:
                    OldParam.Set(NewParam.AsValueString());
                    break;
                case StorageType.String:
                    OldParam.Set(NewParam.AsString());
                    break;
                default:
                    OldParam.Set(NewParam.AsValueString());
                    break;
            }
        }
        public static GlobalParameter GlobalParameterWatch(Document document, object obj, string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            GlobalParameter globalParameter;
            ElementId elementId = GlobalParametersManager.FindByName(document, name);
            if (elementId != ElementId.InvalidElementId)
                globalParameter = document.GetElement(elementId) as GlobalParameter;
            else
                globalParameter = GlobalParameter.Create(document, name, ParameterType.Text);
            if (globalParameter != null && globalParameter.GetValue() is StringParameterValue stringParameterValue)
            {
                stringParameterValue.Value = JsonConvert.SerializeObject(obj);
                globalParameter.SetValue(stringParameterValue);
            }
            return globalParameter;


        }
        #endregion

        public static double CalculateSplitLengthByCloneValue(Conduit con, Line _crossLine, XYZ _cloneCoupling, XYZ primaryConnectedOrgin, double minus = 0)
        {
            XYZ clonePoint;
            GetStartEndPointForConduit(con, out XYZ startOrgin, out XYZ endOrgin);
            if (_crossLine != null)
            {
                startOrgin = new XYZ(startOrgin.X, startOrgin.Y, _crossLine.GetEndPoint(0).Z);
                endOrgin = new XYZ(endOrgin.X, endOrgin.Y, _crossLine.GetEndPoint(0).Z);
                Line line = Line.CreateBound(startOrgin, endOrgin);
                clonePoint = GetIntersection(_crossLine, line);
            }
            else
            {
                clonePoint = FindClonePoint(startOrgin, endOrgin, _cloneCoupling);

            }
            return clonePoint == null ? 0.0 : clonePoint.DistanceTo(primaryConnectedOrgin) - minus;
        }
        public static string GetStringFromNumber(Document doc, double number, UnitType unitType)
        {
            return UnitFormatUtils.Format(doc.GetUnits(), unitType, number, false, false);
        }

        public static void GetProjectUnits(Document doc, string value, out double asDouble, out string asString, bool hideMessageBox = false)
        {
            asString = string.Empty;
            string message = string.Empty;
            UnitFormatUtils.TryParse(doc.GetUnits(), UnitType.UT_Length, value, out asDouble, out message);

            if (!string.IsNullOrEmpty(message) && !hideMessageBox)
            {
                MessageBox.Show(message);
                return;
            }
            asString = UnitFormatUtils.Format(doc.GetUnits(), UnitType.UT_Length, asDouble, true, false);

        }

        public static string GetGlobalParametersManager(UIApplication uiApp, string name)
        {
            name = name + "_" + uiApp.Application.LoginUserId;
            Document doc = uiApp.ActiveUIDocument.Document;
            ElementId gpNWC = Autodesk.Revit.DB.GlobalParametersManager.FindByName(doc, name);
            if (gpNWC != ElementId.InvalidElementId)
            {
                if (doc.GetElement(gpNWC) is GlobalParameter gp)
                {
                    ParameterValue value = gp.GetValue();
                    if (value != null)
                    {
                        StringParameterValue svp = value as StringParameterValue;
                        return svp.Value;

                    }
                }
            }
            return null;
        }
        public static void SetGlobalParametersManager(UIApplication uiApp, string name, object item)
        {
            name = name + "_" + uiApp.Application.LoginUserId;
            Document doc = uiApp.ActiveUIDocument.Document;
            string json = JsonConvert.SerializeObject(item);
            GlobalParameter gp = null;
            ElementId gpNWC = GlobalParametersManager.FindByName(doc, name);
            if (gpNWC != ElementId.InvalidElementId)
            {
                doc.Delete(gpNWC);
            }
            gp = GlobalParameter.Create(doc, name, ParameterType.Text);
            if (gp.GetValue() is StringParameterValue stringParameterValue)
            {
                stringParameterValue.Value = json;
                gp.SetValue(stringParameterValue);
            }
        }

        public static void AlertMessage(string msg, bool? isSuccess, Snackbar SnackbarSeven)
        {
            if (isSuccess == true)
                SnackbarSeven.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#005D9A");
            else if (isSuccess == false)
                SnackbarSeven.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#fbb511");
            else
                SnackbarSeven.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF0000");
            SnackbarSeven.MessageQueue?.Enqueue(
                                 msg,
                                 "OK",
                                 param => SnackbarSeven.MessageQueue?.Clear(),
                                 null,
                                 false,
                                 true,
                                 TimeSpan.FromSeconds(15));

        }
    }
}
