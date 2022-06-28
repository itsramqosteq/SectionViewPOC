using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POC
{
    public class ElementsFilter : ISelectionFilter
    {
        static string CategoryName = "";
        public ElementsFilter(string name)
        {
            CategoryName = name;
        }
        public bool AllowElement(Element e)
        {
            if (e != null && e.Category != null && e.Category.Name == CategoryName)
                return true;
            return false;
        }
        public bool AllowReference(Reference r, XYZ p)
        {
            return false;
        }
    }
}
