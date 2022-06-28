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
    public partial class IconButtonUserControl : UserControl
    {
        public event RoutedEventHandler BtnClick;
        public delegate void RoutedEventHandler(object sender);

        #region Button

        public PackIconKind Icon
        {
            get { return (PackIconKind)GetValue(iconProperty); }
            set { SetValue(iconProperty, value); }
        }
        public static readonly DependencyProperty iconProperty =
            DependencyProperty.Register("Icon", typeof(PackIconKind),
              typeof(IconButtonUserControl), new PropertyMetadata(PackIconKind.Tick, OnPropertyChangedIcon));

        private static void OnPropertyChangedIcon(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IconButtonUserControl control = (IconButtonUserControl)d;
           
                control.btnIcon.Kind = (PackIconKind)e.NewValue;
            

        }
     
        public double IconWidth
        {
            get { return (double)GetValue(IwidthProperty); }
            set { SetValue(IwidthProperty, value); }
        }
        public static readonly DependencyProperty IwidthProperty =
            DependencyProperty.Register("IconWidth", typeof(double),
              typeof(IconButtonUserControl), new PropertyMetadata(0D, OnPropertyChangedForIWidth));
        private static void OnPropertyChangedForIWidth(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IconButtonUserControl control = (IconButtonUserControl)d;
            control.btnIcon.Width = (double)e.NewValue;
        }
        public new double Width
        {
            get { return (double)GetValue(widthProperty); }
            set { SetValue(widthProperty, value); }
        }
        public static readonly DependencyProperty widthProperty =
            DependencyProperty.Register("Width", typeof(double),
              typeof(IconButtonUserControl), new PropertyMetadata(0D, OnPropertyChangedForWidth));
        private static void OnPropertyChangedForWidth(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IconButtonUserControl control = (IconButtonUserControl)d;
            control.btn.Width = (double)e.NewValue;
        }
        public double IconHeight
        {
            get { return (double)GetValue(IheightProperty); }
            set { SetValue(IheightProperty, value); }
        }
        public static readonly DependencyProperty IheightProperty =
            DependencyProperty.Register("IconHeight", typeof(double),
              typeof(IconButtonUserControl), new PropertyMetadata(0D, OnPropertyChangedForIHeight));
        private static void OnPropertyChangedForIHeight(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IconButtonUserControl control = (IconButtonUserControl)d;
            control.btnIcon.Height = (double)e.NewValue;
        }

        public new double Height
        {
            get { return (double)GetValue(heightProperty); }
            set { SetValue(heightProperty, value); }
        }
        public static readonly DependencyProperty heightProperty =
            DependencyProperty.Register("Height", typeof(double),
              typeof(IconButtonUserControl), new PropertyMetadata(0D, OnPropertyChangedForHeight));
        private static void OnPropertyChangedForHeight(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IconButtonUserControl control = (IconButtonUserControl)d;
            control.btn.Height = (double)e.NewValue;
        }
        public new string Background
        {
            get { return (string)GetValue(BGProperty); }
            set { SetValue(BGProperty, value); }
        }
        public static readonly DependencyProperty BGProperty =
            DependencyProperty.Register("Background", typeof(string),
              typeof(IconButtonUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedBG));

        private static void OnPropertyChangedBG(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IconButtonUserControl control = (IconButtonUserControl)d;
            control.btn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom((string)e.NewValue);
            control.btn.BorderBrush = control.btn.Background;

        }
        public string ForeColor
        {
            get { return (string)GetValue(FCProperty); }
            set { SetValue(FCProperty, value); }
        }
        public static readonly DependencyProperty FCProperty =
            DependencyProperty.Register("ForeColor", typeof(string),
              typeof(IconButtonUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedFC));
        private static void OnPropertyChangedFC(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IconButtonUserControl control = (IconButtonUserControl)d;
            control.btnIcon.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom((string)e.NewValue);

        }
        public string HoverForeColor
        {
            get { return (string)GetValue(HFCProperty); }
            set { SetValue(HFCProperty, value); }
        }
        public static readonly DependencyProperty HFCProperty =
            DependencyProperty.Register("HoverForeColor", typeof(string),
              typeof(IconButtonUserControl), new PropertyMetadata(string.Empty));
        public string HoverBackground
        {
            get { return (string)GetValue(typeProperty); }
            set { SetValue(typeProperty, value); }
        }
        public static readonly DependencyProperty typeProperty =
            DependencyProperty.Register("HoverBackground", typeof(string),
              typeof(IconButtonUserControl), new PropertyMetadata(string.Empty));
        public bool Disabled
        {
            get { return (bool)GetValue(DisabledProperty); }
            set { SetValue(DisabledProperty, value); }
        }
       

       

        public static readonly DependencyProperty DisabledProperty =
            DependencyProperty.Register("Disabled", typeof(bool),
              typeof(IconButtonUserControl), new PropertyMetadata(false, OnPropertyChangedForDisabled));
        private static void OnPropertyChangedForDisabled(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IconButtonUserControl control = (IconButtonUserControl)d;
            control.btn.IsEnabled = !(bool)e.NewValue;
            if ((bool)e.NewValue)
            {
                    control.btn.Opacity = 0.4;
                    control.btnIcon.Opacity = 0.7;
            }
            else
            {
                control.btn.Opacity = 1;
                control.btnIcon.Opacity = 1;
            }
        }

        #endregion
        #region ToolTip
        #region toolTip
        public new string ToolTip
        {
            get { return (string)GetValue(toolTipProperty); }
            set { SetValue(toolTipProperty, value); }
        }
        public static readonly DependencyProperty toolTipProperty =
            DependencyProperty.Register("ToolTip", typeof(string),
              typeof(IconButtonUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedtoolTip));
        private static void OnPropertyChangedtoolTip(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IconButtonUserControl control = (IconButtonUserControl)d;
            control.btnTooltip.Content = (string)e.NewValue;
            control.btnTooltip.PlacementTarget = control.btn;
            if (e.NewValue.ToString() == "Right" || e.NewValue.ToString() == "Left")
            {
                control.ToolTipVO = -(control.Height / 2);
            }
            else if (e.NewValue.ToString() == "Top" || e.NewValue.ToString() == "Bottom")
            {
                control.ToolTipVO = 0;
                control.ToolTipHO = -((control.Width / 2) - 5);
            }
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
              typeof(IconButtonUserControl), new PropertyMetadata(PlacementMode.Left, OnPropertyChangedplacement));
        private static void OnPropertyChangedplacement(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IconButtonUserControl control = (IconButtonUserControl)d;
            control.btnTooltip.Placement = (PlacementMode)e.NewValue;
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
              typeof(IconButtonUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedTB));
        private static void OnPropertyChangedTB(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IconButtonUserControl control = (IconButtonUserControl)d;
            control.btnTooltip.Background = (string)e.NewValue;
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
              typeof(IconButtonUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedTF));

        private static void OnPropertyChangedTF(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IconButtonUserControl control = (IconButtonUserControl)d;
            control.btnTooltip.ForeColor = (string)e.NewValue;

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
              typeof(IconButtonUserControl), new PropertyMetadata(0D, OnPropertyChangedVf));

        private static void OnPropertyChangedVf(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IconButtonUserControl control = (IconButtonUserControl)d;
            control.btnTooltip.VerticalOffset = (double)e.NewValue;

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
              typeof(IconButtonUserControl), new PropertyMetadata(0D, OnPropertyChangedHO));

        private static void OnPropertyChangedHO(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IconButtonUserControl control = (IconButtonUserControl)d;
            control.btnTooltip.HorizontalOffset = (double)e.NewValue;

        }
        #endregion
        #endregion
        public IconButtonUserControl()
        {
            InitializeComponent();
            btn.Width = 24;
            Width = 24;
            btn.Width = 24;
            Height = 24;
            btnIcon.Width = 16;
            btnIcon.Height = 16;
            ForeColor = "#005D9A";
            Background = "#FFFFFF";
        }

        private void Btn_MouseEnter(object sender, MouseEventArgs e)
        {
            btnTooltip.IsOpen = !string.IsNullOrEmpty(btnTooltip.Content);
            btnIcon.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom(HoverForeColor);
            btn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(HoverBackground);
            btn.BorderBrush = btn.Background;


        }

        private void Btn_MouseLeave(object sender, MouseEventArgs e)
        {
            if (btnTooltip.IsOpen)
            {
                btnTooltip.IsOpen = false;
                btnTooltip.Visibility = System.Windows.Visibility.Collapsed;
            }
            btnIcon.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom(ForeColor);
            btn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(Background);
            btn.BorderBrush = btn.Background;
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            BtnClick?.Invoke(this);
        }
    }
}
