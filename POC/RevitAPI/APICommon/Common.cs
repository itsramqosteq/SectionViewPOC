using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using POC;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace POC
{
    public class APICommon
    {
        public static void AlertMessage(string msg, bool isSuccess, Snackbar SnackbarSeven)
        {
            if (isSuccess)
                SnackbarSeven.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#005D9A");
            else if (!isSuccess)
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
