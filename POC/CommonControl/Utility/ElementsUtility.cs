using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POC
{
    public partial class Utility
    {
        public static List<Element> GetPickedElements(UIDocument uIDocument, string statusPrompt, Type type = null, bool isRequired = false)
        {

            try
            {
                ICollection<ElementId> ecol = uIDocument.Selection.GetElementIds();
                if (ecol.Count == 0)
                {
                    if (!isRequired)
                        return new List<Element>();
                    try
                    {
                        IList<Reference> getSelectionData = uIDocument.Selection.PickObjects(ObjectType.Element, statusPrompt);
                        ecol = Utility.GetElementsIdByReference(getSelectionData, uIDocument.Document);
                    }
                    catch (Exception)
                    {
                        return new List<Element>();
                    }
                    if (ecol.Count == 0)
                        return new List<Element>();
                }
                return Utility.GetElementsByReference(uIDocument.Document, ecol, type);
            }
            catch (Exception)
            {
                return new List<Element>();
            }
        }

        public static Element GetElementByReference(Document doc, Reference References, Type type = null, string familyName = null)
        {


            Element e = doc.GetElement(References);
            if (e.GetType() == type || type == null)
            {
                if (familyName != null && GetFamilyInstanceName(e as FamilyInstance).Contains(familyName))
                    return e;
                else if (familyName == null)
                    return e;
            }
            return e;

        }
        public static List<ElementId> GetElementsIdByReference(IList<Reference> References, Document doc)
        {
            List<ElementId> Elements = new List<ElementId>();
            foreach (Reference r in References)
            {
                Element e = doc.GetElement(r);
                Elements.Add(e.Id);
            }
            return Elements;
        }
        public static List<Element> GetElementsByReference(IList<Reference> References, Document doc)
        {
            List<Element> Elements = new List<Element>();
            foreach (Reference r in References)
            {
                Element e = doc.GetElement(r);
                if (e is Element)
                {
                    Elements.Add(e);
                }
            }
            return Elements;
        }
        public static List<Element> GetElementsByReference(Document doc, IList<Reference> References, Type type = null, string familyName = null)
        {
            List<Element> Elements = new List<Element>();

            foreach (Reference r in References)
            {
                Element e = doc.GetElement(r);
                if (e.GetType() == type || type == null)
                {
                    if (familyName != null && GetFamilyInstanceName(e as FamilyInstance).Contains(familyName))
                        Elements.Add(e);
                    else if (familyName == null)
                        Elements.Add(e);
                }
            }

            return Elements;
        }
        public static List<Element> GetElementsByReference(Document doc, ICollection<ElementId> elementIds = null, Type type = null, string familyName = null)
        {
            List<Element> Elements = new List<Element>();

            foreach (ElementId id in elementIds)
            {
                Element e = doc.GetElement(id);
                if (e.GetType() == type || type == null)
                {
                    if (familyName != null && GetFamilyInstanceName(e as FamilyInstance).Contains(familyName.ToLower()))
                        Elements.Add(e);
                    else if (familyName == null)
                        Elements.Add(e);
                }
            }
            return Elements;
        }
        public static void DeleteElement(Document document, ElementId elementId, Element element = null)
        {
            elementId = element != null ? element.Id : elementId;
            ICollection<Autodesk.Revit.DB.ElementId> deletedIdSet = document.Delete(elementId);

            if (0 == deletedIdSet.Count)
            {
                throw new Exception("Deleting the selected element in Revit failed.");
            }
        }

        public static XYZ GetPointFromSelection(UIDocument uidoc)
        {
            SketchPlane sp = SketchPlane.Create(uidoc.Document, Plane.CreateByNormalAndOrigin(
                           uidoc.Document.ActiveView.ViewDirection,
                           uidoc.Document.ActiveView.Origin));
            uidoc.Document.ActiveView.SketchPlane = sp;
            XYZ point = uidoc.Selection.PickPoint();
            uidoc.Document.Delete(sp.Id);
            return point;
        }
        public static Dictionary<double, List<Element>> GroupByElevation(List<Element> a_Elements, string offSetVar, ref Dictionary<double, List<Element>> groupedElements)
        {
            double dia = a_Elements.FirstOrDefault().LookupParameter("Diameter(Trade Size)").AsDouble();
            if (a_Elements.All(r => r.LookupParameter("Diameter(Trade Size)").AsDouble() == dia))
            {
                groupedElements = a_Elements.GroupBy(r => Math.Round(r.LookupParameter(offSetVar).AsDouble(), 2)).ToDictionary(x => x.Key, x => x.ToList());
            }
            else
            {
                GetGroupedElementsByElevation(a_Elements, offSetVar, ref groupedElements);
            }
            return groupedElements;
        }

        public static void GetGroupedElementsByElevation(List<Element> a_Elements, string offSetVar, ref Dictionary<double, List<Element>> groupedElements)
        {
            Element highElevatedConduit = a_Elements.OrderByDescending(r => Math.Round(r.LookupParameter(offSetVar).AsDouble(), 8)).FirstOrDefault();
            double highElevation = highElevatedConduit.LookupParameter(offSetVar).AsDouble();
            double refElevation = highElevation - (2.5 / 12); // 2.5 inches converted to feet 
            List<Element> TopElements = a_Elements.Where(r => r.LookupParameter(offSetVar).AsDouble() > refElevation).ToList();
            double middleElevation = TopElements.FirstOrDefault().LookupParameter(offSetVar).AsDouble();
            double topElevation = TopElements.FirstOrDefault().LookupParameter("Top Elevation").AsDouble();
            double bottomElevation = TopElements.FirstOrDefault().LookupParameter("Bottom Elevation").AsDouble();

            if (TopElements.All(r => Math.Round(r.LookupParameter(offSetVar).AsDouble(), 2) == Math.Round(middleElevation, 5)))
            {
                groupedElements.Add(highElevation, TopElements);
            }
            else if (TopElements.All(r => Math.Round(r.LookupParameter("Top Elevation").AsDouble(), 2) == Math.Round(topElevation, 5)))
            {
                groupedElements.Add(highElevation, TopElements);
            }
            else if (TopElements.All(r => Math.Round(r.LookupParameter("Bottom Elevation").AsDouble(), 2) == Math.Round(bottomElevation, 5)))
            {
                groupedElements.Add(highElevation, TopElements);
            }
            else
            {
                groupedElements.Add(highElevation, TopElements);
            }
            a_Elements = a_Elements.Except(TopElements).ToList();
            if (a_Elements.Count > 0)
            {
                GetGroupedElementsByElevation(a_Elements, offSetVar, ref groupedElements);
            }
        }
    }
}
