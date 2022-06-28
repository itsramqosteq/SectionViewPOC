using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace POC
{
    public class CustomColorPicker
    {
        public string ColorName { get; set; }
        public string ColorR { get; set; }
        public string ColorG { get; set; }
        public string ColorB { get; set; }
        public SolidColorBrush SolidColorBrush {
            get
            {
               
                return new SolidColorBrush(Color.FromRgb( Convert.ToByte(ColorR), Convert.ToByte(ColorG), Convert.ToByte(ColorB)));

            }
        }
        public string RGBStringvalue
        {
            get
            {

                return Convert.ToString(ColorR)+","+ Convert.ToString(ColorG)+"," +Convert.ToString(ColorB);
            }
        }
        public Autodesk.Revit.DB.Color RGBvalue
        {
            get
            {

                return new Autodesk.Revit.DB.Color(Convert.ToByte(ColorR), Convert.ToByte(ColorG), Convert.ToByte(ColorB));
            }
        }
     
    }
    public static class CustomColors
    {
        public static  List<CustomColorPicker> customColors= new List<CustomColorPicker> ()
                {
                new CustomColorPicker { ColorName = "Black", ColorR = "0", ColorG = "0", ColorB = "0" },
                new CustomColorPicker { ColorName = "White", ColorR = "255", ColorG = "255", ColorB = "255" },
                new CustomColorPicker { ColorName = "Red", ColorR = "255", ColorG = "0", ColorB = "0" },
                new CustomColorPicker { ColorName = "Lime", ColorR = "0", ColorG = "255", ColorB = "0" },
                new CustomColorPicker { ColorName = "Blue", ColorR = "0", ColorG = "0", ColorB = "255" },
                new CustomColorPicker { ColorName = "Yellow", ColorR = "255", ColorG = "255", ColorB = "0" },
                new CustomColorPicker { ColorName = "Aqua", ColorR = "0", ColorG = "255", ColorB = "255" },
                new CustomColorPicker { ColorName = "Magenta", ColorR = "255", ColorG = "0", ColorB = "255" },
                new CustomColorPicker { ColorName = "Silver", ColorR = "192", ColorG = "192", ColorB = "192" },
                new CustomColorPicker { ColorName = "Gray", ColorR = "128", ColorG = "128", ColorB = "128" },
                new CustomColorPicker { ColorName = "Maroon", ColorR = "128", ColorG = "0", ColorB = "0" },
                new CustomColorPicker { ColorName = "Olive", ColorR = "128", ColorG = "128", ColorB = "0" },
                new CustomColorPicker { ColorName = "Green", ColorR = "0", ColorG = "128", ColorB = "0" },
                new CustomColorPicker { ColorName = "Purple", ColorR = "128", ColorG = "0", ColorB = "128" },
                new CustomColorPicker { ColorName = "Teal", ColorR = "0", ColorG = "128", ColorB = "128" },
                new CustomColorPicker { ColorName = "Navy", ColorR = "0", ColorG = "0", ColorB = "0" }

                };
    }
}
