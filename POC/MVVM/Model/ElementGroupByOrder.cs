using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POC
{

    public class ElementGroupByOrder

    {

        public List<Element> CurrentElement { get; set; }
        public List<Element> PreviousElement { get; set; }
        public ViewSection ViewSection { get; set; }

        public List< ElementGroupByOrder> ChildGroup { get; set; }

        public List<List<Element>> RunElements { get; set; }  

    }



}
