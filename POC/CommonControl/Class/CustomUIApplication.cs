using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POC
{
   public class CustomUIApplication
    {
        public ExternalCommandData CommandData { get; set; }
        public UIApplication UIApplication {
            get {
                return CommandData.Application;
            } }
        public string OffsetVariable { 
            get {
                int.TryParse(UIApplication.Application.VersionNumber, out int RevitVersion);
                return RevitVersion < 2020 ? "Offset" : "Middle Elevation";
            } 
        }
    }
}
