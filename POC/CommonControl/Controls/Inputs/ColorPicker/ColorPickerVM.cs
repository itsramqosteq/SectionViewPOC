using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POC
{
    public class ColorPickerVM : ViewModelBase
    {
        private Autodesk.Revit.DB.Color _colorRGB;
        public Autodesk.Revit.DB.Color ColorRGB
        {
            get => _colorRGB;
            set => SetProperty(ref _colorRGB, value);
        }
      
    }
}
