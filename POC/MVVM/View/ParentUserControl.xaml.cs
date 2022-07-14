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
using POC;
using System.Data;
using System.Collections.ObjectModel;
using POC.Internal;

namespace POC
{
    /// <summary>
    /// UI Events
    /// </summary>
    public partial class ParentUserControl : UserControl
    {
        public static ParentUserControl Instance;
        public System.Windows.Window _window = new System.Windows.Window();
        Document _doc = null;
        UIDocument _uidoc = null;
        string _offsetVariable = string.Empty;
        List<MultiSelect> multiSelectComboBoxes = new List<MultiSelect>();
        List<ExternalEvent> _externalEvents = new List<ExternalEvent>();
        public ParentUserControl(List<ExternalEvent> externalEvents, CustomUIApplication application, Window window)
        {
            _uidoc = application.UIApplication.ActiveUIDocument;
            _doc = _uidoc.Document;
            _offsetVariable = application.OffsetVariable;
            InitializeComponent();
            Instance = this;
          
            try
            {
                _window = window;
                _externalEvents = externalEvents;
            }
            catch (Exception exception)
            {

                System.Windows.MessageBox.Show("Some error has occured. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _externalEvents[0].Raise();
        }

        private void btnStrut_Click(object sender, RoutedEventArgs e)
        {
            _externalEvents[1].Raise();
        }
    }
}

