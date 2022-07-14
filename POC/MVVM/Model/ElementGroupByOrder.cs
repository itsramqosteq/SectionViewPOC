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
        public List<Element> PreviousGroupElement { get; set; }
        public List<Element> PreviousCurrentGroupElement { get; set; }
        public List<Element> SecondPreviousCurrentGroupElement { get; set; }
        public List<Element> PreviousElement { get; set; }

        public Dictionary<double, List<Element>> GroupByElementByElevation { get; set; }
        public ViewSection ViewSection { get; set; }

        public List< ElementGroupByOrder> ChildGroup { get; set; }

        public List<List<Element>> RunElements { get; set; }
        public List<List<Element>> FullRunElements { get; set; }


    }



}
