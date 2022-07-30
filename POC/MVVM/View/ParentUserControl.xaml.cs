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


                IEnumerable<ViewFamilyType> SectionType = new FilteredElementCollector(_doc)
                                         .OfClass(typeof(ViewFamilyType))
                                         .Cast<ViewFamilyType>()
                                         .Where(e => e.ViewFamily == ViewFamily.Section);

                IEnumerable<View> ViewTemplates = new FilteredElementCollector(_doc)
                                       .OfClass(typeof(View))
                                       .Cast<View>().Where(x => x.IsTemplate).Where(r => r.ViewType == ViewType.Section);

                FamilyForViewSheetBox.ItemsSource = SectionType;
                FamilyForViewSheetBox.SelectedIndex = 0;

                TemplateForView.ItemsSource = ViewTemplates;
                TemplateForView.SelectedIndex = 0;

                //  List<Element> DimensionTypes = new FilteredElementCollector(_doc).OfClass(typeof(DimensionType)).
                //Where(r => r.GetType() == typeof(DimensionType) && (r as DimensionType).StyleType == DimensionStyleType.Linear && (r as DimensionType).FamilyName != r.Name).ToList();
                //  if (DimensionTypes.Any())
                //  {
                //      List<BaseClass> typeDetails = DimensionTypes.Select(r => new BaseClass()
                //      { Name = string.Format("{0} : {1}", (r as DimensionType).FamilyName, r.Name), Id = r.Id }).ToList();
                //      ddlFamilyType.ItemsSource = typeDetails;
                //      ddlFamilyType.DisplayMemberPath = "Name";
                //      ddlFamilyType.SelectedValuePath = "Id";
                //      ddlFamilyType.SelectedIndex = 0;
                //  }
                List<Element> supportInstances = new FilteredElementCollector(_doc, _doc.ActiveView.Id).OfClass(typeof(FamilyInstance))
                .Where(x => x.Category.Name == "Electrical Fixtures").ToList();

                List<MultiSelect> list = new List<MultiSelect>();
                foreach (FamilyInstance si in supportInstances)
                {
                    string name = si.Symbol.FamilyName;
                    if (!list.Any(x => x.Name == name))
                    {
                        MultiSelect obj = new MultiSelect();
                        obj.Name = name;
                        obj.Id = si.Id;
                        obj.Item = si;
                        list.Add(obj);
                    }

                }
                strutList.ItemsSource = list;
                strutList.DisplayMemberPath = "Name";
                strutList.SelectedValuePath = "Id";

                strutList.SelectedIndex = 0;
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

        private void strutList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FamilyInstance selectedStrut = (((POC.MultiSelect)((System.Windows.Controls.Primitives.Selector)sender).SelectedItem).Item as FamilyInstance);
            List<string> list = new List<string>();   
            foreach (var item in selectedStrut.Parameters)
            {
                list.Add(((Autodesk.Revit.DB.InternalDefinition)((Autodesk.Revit.DB.Parameter)item).Definition).Name);
                
            }
            list = list.OrderBy(x=>x).ToList();
            strutParamList.ItemsSource = list;
            int index = list.FindIndex(x => x == "STRUT LENGTH");
            if (index >= 0)
            {
                strutParamList.SelectedIndex = index;
            }
        }

        private void btnPlaceTags_Click(object sender, RoutedEventArgs e)
        {
            _externalEvents[2].Raise();
        }
    }
}

