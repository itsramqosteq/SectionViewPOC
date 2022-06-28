using Autodesk.Revit.UI;
using POC.Internal;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace POC
{
   public  class CustomAlert
    {
       
        public  async Task<MessageBoxResult> Show(CustomAlertType alertType, string content = null,string host = "POCRootDialog", PackIconKind? Icon = null)
        {
            var view = new AlertBoxContentUserControl
            {
                Content = content,
                AlertType = alertType,
                DataContext = new AlertVM()
            };
            object identifier = host;
            var result=  await DialogHost.Show(view, identifier, new DialogOpenedEventHandler((object sender, DialogOpenedEventArgs args) =>
           {
               //((MaterialDesignThemes.Wpf.DialogHost)sender).CloseOnClickAway = true;
           }));
            return ((AlertVM)view.DataContext).MessageBoxResult;
        }
       
        
    }

    public enum CustomAlertType
    {
        [Display(Name = "Warning!")]
        Warning =0,
        [Display(Name = "Alert!")]
        Alert =1,
        [Display(Name = "Failed!")]
        Failed =2,
        [Display(Name = "Successful!")]
        Successful =3,
        [Display(Name = "Confirm?")]
        Confirm =4,
        [Display(Name = "Information!")]
        Information = 5,
        [Display(Name = "Confirm Reset?")]
        Reset = 6,
    }
}
