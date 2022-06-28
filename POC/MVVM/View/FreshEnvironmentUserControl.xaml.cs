using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Input;
using System.Diagnostics;
using System.Threading.Tasks;
using FreshEnvironment;
using System.Data;
using System.Collections.ObjectModel;
using FreshEnvironment.Internal;

namespace FreshEnvironment
{
    /// <summary>
    /// UI Events
    /// </summary>
    public partial class FreshEnvironmentUserControl : UserControl
    {
        public static FreshEnvironmentUserControl Instance;
        public System.Windows.Window _window = new System.Windows.Window();
        Document _doc = null;
        UIDocument _uidoc = null;
        string _offsetVariable = string.Empty;
        List<MultiSelect> multiSelectComboBoxes = new List<MultiSelect>();
        public FreshEnvironmentUserControl(List<ExternalEvent> externalEvents, CustomUIApplication application, Window window)
        {
            _uidoc = application.UIApplication.ActiveUIDocument;
            _doc = _uidoc.Document;
            _offsetVariable = application.OffsetVariable;
            InitializeComponent();
            Instance = this;
            MultiSelect obj = new MultiSelect();
            obj.Id = 1;
            obj.Name = "India";
            obj.IsChecked = true;
            multiSelectComboBoxes.Add(obj);
            obj = new MultiSelect();
            obj.Id = 2;
            obj.Name = "china";
            multiSelectComboBoxes.Add(obj);
            //RadioButtonClass radio = new RadioButtonClass();
            //radio.id = 1;
            //radio.name =" Active";
            //radio.isSelected = false;
            //radio.groupName = "one";
            //list.Add(radio);
            //radio = new RadioButtonClass();
            //radio.id = 2;
            //radio.name = "Sha";
            //radio.isSelected = false;
            //radio.groupName = "one";
            //list.Add(radio);
            this.DataContext = new FieldViewModel();
            try
            {
                _window = window;
                //rbList.itemList = list;
                ucMultiSelect.Label = "Multi Select";
                ucMultiSelect.ItemsSource = multiSelectComboBoxes;
                ucMultiSelect.IsAllOptionEnable = true;
                ucMultiSelect.IsAllowToAddItem = true;
                ucMultiSelect.AddSuffix = "(-_-)";
               // clrPicker.value = new Autodesk.Revit.DB.Color(225, 75, 0);
            }
            catch (Exception exception)
            {

                System.Windows.MessageBox.Show("Some error has occured. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var excel = btnExcel.DataTableOutput;
           // var ss = clrPicker.value;
        
            //MainWindow.Instance.SnackbarSeven.MessageQueue?.Enqueue(
            //                    $"Placed Sucessfully",
            //                    "OK",
            //                    param => MainWindow.Instance.SnackbarSeven.MessageQueue?.Clear(),
            //                    null,
            //                    false,
            //                    true,
            //                    TimeSpan.FromSeconds(100));
            ((System.Windows.Controls.ContentControl)sender).Content = ((FieldViewModel)this.DataContext).ColorPickerVM.ColorRGB;// ucMultiSelect.SelectedItems.Count();
            DataTable dt = ((FieldViewModel)this.DataContext).ImportExcelVM.dataTable;
        }

    

        private void TextBoxUserControl_TextBox_Changed(object sender)
        {

        }

        private void CheckBox_MouseEnter(object sender, MouseEventArgs e)
        {
            //popup_uc.PlacementTarget = chk;
            //popup_uc.Placement = PlacementMode.Right;
            //popup_uc.IsOpen = true;
            //Header.PopupText.Text = "Home";
            //popup_uc1.PlacementTarget = btn;
            //popup_uc1.IsOpen = true;
            //popup_uc1.Placement = PlacementMode.Right;
        }

        private void CheckBox_MouseLeave(object sender, MouseEventArgs e)
        {
            //popup_uc.Visibility = System.Windows.Visibility.Collapsed;
            //popup_uc.IsOpen = false;
            //popup_uc1.IsOpen = false;
        }

        private async void ButtonUserControl_btnClick(object sender)
        {
          
          var result = await new CustomAlert().Show(CustomAlertType.Warning,
              "Please Select all requirement fields and thanks for your messgae");
          
         
        }
    

        private  void rbList_Checked(object sender)
        {
          
        }

        private  void rbList_UnChecked(object sender)
        {
           
        }

        private async void Cancel_btnClick(object sender)
        {
            var result = await new CustomAlert().Show(CustomAlertType.Successful,
            "Please Select all requirement fields and thanks for your messgae");
        }

        private async void Reset_btnClick(object sender)
        {
            var result = await new CustomAlert().Show(CustomAlertType.Confirm,
             "Please Select all requirement fields and thanks for your messgae");
        }

        private void btnExport_btnClick(object sender)
        {
            btnExport.ExcelName = "Sample";
            //btnExport.IgonreColumns  = new List<string> { "From" };
            btnExport.DataTable = btnExcel.DataTableOutput;
        }
    }
}

