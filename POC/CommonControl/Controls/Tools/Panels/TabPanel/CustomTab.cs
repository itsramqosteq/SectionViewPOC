using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace POC
{
    public class CustomTab : BaseClass
    {
        private double _iconWidth = 20;
        private double _iconHeight = 20;
     
        public bool IsTittleOnly
        {
            get
            {
                return Icon == null && !string.IsNullOrEmpty(Name);
            }

        }
        public bool IsIconOnly
        {
            get
            {
                return Icon !=null && string.IsNullOrEmpty(Name);
            }
          
        }
        public double IconWidth {
            get
            {
                return _iconWidth;
            }
            set
            {
                _iconWidth = value;
            }
        }
        public double IconHeight {
            get
            {
                return _iconHeight;
            }
            set
            {
                _iconHeight = value;
            }
        }
        public double ItemWidth { get; set; }
     
        public PackIconKind? Icon { get; set; }
       
        
    }

}
