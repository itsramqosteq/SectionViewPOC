using POC.Internal;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace POC
{
    /// <summary>
    /// Interaction logic for FooterPanel.xaml
    /// </summary>
    public partial class ExportExcelUserControl : UserControl
    {
        public event RoutedEventHandler BtnClick;
        public delegate void RoutedEventHandler(object sender);
        private readonly string filePath = "C:\\Users\\" + Environment.UserName + "\\Documents";
        public string ExcelName
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("ExcelName", typeof(string),
              typeof(ExportExcelUserControl), new PropertyMetadata(string.Empty));
        public string Identifier
        {
            get { return (string)GetValue(IdentifierProperty); }
            set { SetValue(IdentifierProperty, value); }
        }
        public static readonly DependencyProperty IdentifierProperty =
            DependencyProperty.Register("Identifier", typeof(string),
              typeof(ExportExcelUserControl), new PropertyMetadata(string.Empty));
        public List<string> IgonreColumns
        {
            get { return (List<string>)GetValue(IgnoreColumnsProperty); }
            set { SetValue(IgnoreColumnsProperty, value); }
        }
        public DataTable DataTable
        {
            get { return (DataTable)GetValue(dataTableProperty); }
            set { SetValue(dataTableProperty, value); }
        }


        public static readonly DependencyProperty IgnoreColumnsProperty =
            DependencyProperty.Register("IgonreColumns", typeof(List<string>), typeof(ExportExcelUserControl), new PropertyMetadata(null));
        public static readonly DependencyProperty dataTableProperty =
           DependencyProperty.Register("DataTable", typeof(DataTable), typeof(ExportExcelUserControl), new PropertyMetadata(null));
       
      
        #region ToolTip
        #region toolTip
        public new string ToolTip
        {
            get { return (string)GetValue(toolTipProperty); }
            set { SetValue(toolTipProperty, value); }
        }
        public static readonly DependencyProperty toolTipProperty =
            DependencyProperty.Register("ToolTip", typeof(string),
              typeof(ExportExcelUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedtoolTip));
        private static void OnPropertyChangedtoolTip(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ExportExcelUserControl control = (ExportExcelUserControl)d;
            control.btnExport.ToolTip = (string)e.NewValue;

        }
        #endregion
        #region toolTipPlacement
        public PlacementMode ToolTipPlacement
        {
            get { return (PlacementMode)GetValue(placementProperty); }
            set { SetValue(placementProperty, value); }
        }
        public static readonly DependencyProperty placementProperty =
            DependencyProperty.Register("ToolTipPlacement", typeof(PlacementMode),
              typeof(ExportExcelUserControl), new PropertyMetadata(PlacementMode.Left, OnPropertyChangedplacement));
        private static void OnPropertyChangedplacement(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ExportExcelUserControl control = (ExportExcelUserControl)d;
            control.btnExport.ToolTipPlacement = (PlacementMode)e.NewValue;
        }
        #endregion
        #region background
        public string ToolTipBackground
        {
            get { return (string)GetValue(TBProperty); }
            set { SetValue(TBProperty, value); }
        }
        public static readonly DependencyProperty TBProperty =
            DependencyProperty.Register("ToolTipBackground", typeof(string),
              typeof(ExportExcelUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedTB));
        private static void OnPropertyChangedTB(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ExportExcelUserControl control = (ExportExcelUserControl)d;
            control.btnExport.ToolTipBackground = (string)e.NewValue;
        }
        #endregion
        #region ForeColor
        public string ToolTipforeColor
        {
            get { return (string)GetValue(tFProperty); }
            set { SetValue(tFProperty, value); }
        }
        public static readonly DependencyProperty tFProperty =
            DependencyProperty.Register("ToolTipforeColor", typeof(string),
              typeof(ExportExcelUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedTF));

        private static void OnPropertyChangedTF(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ExportExcelUserControl control = (ExportExcelUserControl)d;
            control.btnExport.ToolTipforeColor = (string)e.NewValue;

        }
        #endregion
        #region verticalOffset
        public double ToolTipVO
        {
            get { return (double)GetValue(VFProperty); }
            set { SetValue(VFProperty, value); }
        }
        public static readonly DependencyProperty VFProperty =
            DependencyProperty.Register("ToolTipVO", typeof(double),
              typeof(ExportExcelUserControl), new PropertyMetadata(0D, OnPropertyChangedVf));

        private static void OnPropertyChangedVf(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ExportExcelUserControl control = (ExportExcelUserControl)d;
            control.btnExport.ToolTipVO = (double)e.NewValue;

        }
        #endregion
        #region horizontalOffset
        public double ToolTipHO
        {
            get { return (double)GetValue(HSProperty); }
            set { SetValue(HSProperty, value); }
        }
        public static readonly DependencyProperty HSProperty =
            DependencyProperty.Register("ToolTipHO", typeof(double),
              typeof(ExportExcelUserControl), new PropertyMetadata(0D, OnPropertyChangedHO));

        private static void OnPropertyChangedHO(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ExportExcelUserControl control = (ExportExcelUserControl)d;
            control.btnExport.ToolTipHO = (double)e.NewValue;

        }
        #endregion
        #endregion

        public ExportExcelUserControl()
        {
            InitializeComponent();

        }

        private async void BtnExport_Click(object sender)
        {

            BtnClick?.Invoke(this);
            if (!string.IsNullOrEmpty(ExcelName) && DataTable!=null && DataTable.Rows.Count>0)
            {
                bool isTrue = Utility.WriteDataTableToExcel(DataTable, IgonreColumns, ExcelName, filePath, "Details");
                if (isTrue)
                {
                    await new CustomAlert().Show(CustomAlertType.Successful, "Exported successfully.", Identifier);
                }
                else
                {
                    await new CustomAlert().Show(CustomAlertType.Warning, "Exported Failed.", Identifier);
                }
            }
            
        }



    }
}
