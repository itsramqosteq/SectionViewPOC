using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
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
    public partial class CopyClipBoardUserControl : UserControl
    {
        public event RoutedEventHandler Click;
        public delegate void RoutedEventHandler(object sender);
        public string Value
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Value", typeof(string),
              typeof(CopyClipBoardUserControl), new PropertyMetadata(string.Empty));
        
        public Snackbar Identifier
        {
            get { return (Snackbar)GetValue(IdentifierProperty); }
            set { SetValue(IdentifierProperty, value); }
        }
        public static readonly DependencyProperty IdentifierProperty =
            DependencyProperty.Register("Identifier", typeof(Snackbar),
              typeof(CopyClipBoardUserControl), new PropertyMetadata(null, OnPropertyChangedIdentifier));
        private static void OnPropertyChangedIdentifier(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CopyClipBoardUserControl control = (CopyClipBoardUserControl)d;

            //control.Value = (string)e.NewValue;


        }
        #region ToolTip
        #region toolTip
        public new string ToolTip
        {
            get { return (string)GetValue(toolTipProperty); }
            set { SetValue(toolTipProperty, value); }
        }
        public static readonly DependencyProperty toolTipProperty =
            DependencyProperty.Register("ToolTip", typeof(string),
              typeof(CopyClipBoardUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedtoolTip));
        private static void OnPropertyChangedtoolTip(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CopyClipBoardUserControl control = (CopyClipBoardUserControl)d;
            control.BtnCopy.ToolTip = (string)e.NewValue;

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
              typeof(CopyClipBoardUserControl), new PropertyMetadata(PlacementMode.Left, OnPropertyChangedplacement));
        private static void OnPropertyChangedplacement(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CopyClipBoardUserControl control = (CopyClipBoardUserControl)d;
            control.BtnCopy.ToolTipPlacement = (PlacementMode)e.NewValue;
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
              typeof(CopyClipBoardUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedTB));
        private static void OnPropertyChangedTB(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CopyClipBoardUserControl control = (CopyClipBoardUserControl)d;
            control.BtnCopy.ToolTipBackground = (string)e.NewValue;
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
              typeof(CopyClipBoardUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedTF));

        private static void OnPropertyChangedTF(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CopyClipBoardUserControl control = (CopyClipBoardUserControl)d;
            control.BtnCopy.ToolTipforeColor = (string)e.NewValue;

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
              typeof(CopyClipBoardUserControl), new PropertyMetadata(0D, OnPropertyChangedVf));

        private static void OnPropertyChangedVf(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CopyClipBoardUserControl control = (CopyClipBoardUserControl)d;
            control.BtnCopy.ToolTipVO = (double)e.NewValue;

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
              typeof(CopyClipBoardUserControl), new PropertyMetadata(0D, OnPropertyChangedHO));

        private static void OnPropertyChangedHO(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CopyClipBoardUserControl control = (CopyClipBoardUserControl)d;
            control.BtnCopy.ToolTipHO = (double)e.NewValue;

        }
        #endregion
        #endregion
        public CopyClipBoardUserControl()
        {
            InitializeComponent();
        }


        private void BtnCopy_Click(object sender)
        {
            Click?.Invoke(this);
            Clipboard.SetText(this.Value);

            Utility.AlertMessage(string.IsNullOrEmpty(this.Value)? "Value is empty": "Copied successfully!", !string.IsNullOrEmpty(this.Value),Identifier);
            
        }
    }
}
