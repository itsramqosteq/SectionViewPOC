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
    public partial class ButtonUserControl : UserControl
    {
        public event RoutedEventHandler BtnClick;
        public delegate void RoutedEventHandler(object sender);

        #region Button
        #region Icon
        public PackIconKind Icon
        {
            get { return (PackIconKind)GetValue(iconProperty); }
            set { SetValue(iconProperty, value); }
        }
        public static readonly DependencyProperty iconProperty =
            DependencyProperty.Register("Icon", typeof(PackIconKind),
              typeof(ButtonUserControl), new PropertyMetadata(PackIconKind.Airport, OnPropertyChangedIcon));

        private static void OnPropertyChangedIcon(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ButtonUserControl control = (ButtonUserControl)d;
            control.btnIcon.Visibility = Visibility.Visible;
            control.btn.Padding = new Thickness(0, 0, 0, 0);
            control.btnIcon.Kind = (PackIconKind)e.NewValue;
            control.btnText.Margin = new Thickness(2, 0, 0, 0);

        }
        #endregion
        #region Label
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string),
              typeof(ButtonUserControl), new PropertyMetadata(string.Empty, OnPropertyChanged));
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ButtonUserControl control = (ButtonUserControl)d;
            control.btnText.Text = (string)e.NewValue;

        }
        #endregion
        #region Width
        public new double Width
        {
            get { return (double)GetValue(widthProperty); }
            set { SetValue(widthProperty, value); }
        }
        public static readonly DependencyProperty widthProperty =
            DependencyProperty.Register("Width", typeof(double),
              typeof(ButtonUserControl), new PropertyMetadata(0D, OnPropertyChangedForWidth));
        private static void OnPropertyChangedForWidth(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ButtonUserControl control = (ButtonUserControl)d;
            control.btn.Width = (double)e.NewValue;
        }
        #endregion
        #region Height
        public new double Height
        {
            get { return (double)GetValue(heightProperty); }
            set { SetValue(heightProperty, value); }
        }
        public static readonly DependencyProperty heightProperty =
            DependencyProperty.Register("Height", typeof(double),
              typeof(ButtonUserControl), new PropertyMetadata(0D, OnPropertyChangedForHeight));
        private static void OnPropertyChangedForHeight(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ButtonUserControl control = (ButtonUserControl)d;
            control.btn.Height = (double)e.NewValue;
        }
        #endregion
        #region Background
        public new string Background
        {
            get { return (string)GetValue(BGProperty); }
            set { SetValue(BGProperty, value); }
        }
        public static readonly DependencyProperty BGProperty =
            DependencyProperty.Register("Background", typeof(string),
              typeof(ButtonUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedBG));

        private static void OnPropertyChangedBG(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ButtonUserControl control = (ButtonUserControl)d;
            control.btn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom((string)e.NewValue);
            control.btn.BorderBrush = control.btn.Background;

        }
        #endregion
        #region ForeColor
        public string ForeColor
        {
            get { return (string)GetValue(FCProperty); }
            set { SetValue(FCProperty, value); }
        }
        public static readonly DependencyProperty FCProperty =
            DependencyProperty.Register("ForeColor", typeof(string),
              typeof(ButtonUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedFC));

        private static void OnPropertyChangedFC(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ButtonUserControl control = (ButtonUserControl)d;
            control.btnIcon.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom((string)e.NewValue);
            control.btnText.Foreground = control.btnIcon.Foreground;

        }
        #endregion
        #region Disabled
        public bool Disabled
        {
            get { return (bool)GetValue(DisabledProperty); }
            set { SetValue(DisabledProperty, value); }
        }

        public static readonly DependencyProperty DisabledProperty =
            DependencyProperty.Register("Disabled", typeof(bool),
              typeof(ButtonUserControl), new PropertyMetadata(false, OnPropertyChangedForDisabled));
        private static void OnPropertyChangedForDisabled(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ButtonUserControl control = (ButtonUserControl)d;
            control.btn.IsEnabled = !(bool)e.NewValue;
            if ((bool)e.NewValue)
            {
                if (control.BtnType.ToLower() == "cancel")
                {
                    control.btn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#D85922");
                    control.btn.BorderBrush = control.btn.Background;
                    control.btn.Opacity = 1;
                    control.btnIcon.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFFF");
                    control.btnText.Foreground = control.btnIcon.Foreground;
                    control.btn.Opacity = 0.4;
                    control.btn.Cursor = Cursors.No;
                    control.btnText.Opacity = 0.7;
                    control.btnIcon.Opacity = 0.7;
                }
                else if (control.BtnType.ToLower() == "reset")
                {
                    control.btn.Opacity = 0.4;
                    control.btn.Cursor = Cursors.No;
                    control.btnText.Opacity = 0.7;
                    control.btnIcon.Opacity = 0.7;
                }
                else
                {
                    control.btn.Opacity = 0.4;
                    control.btn.Cursor = Cursors.No;
                    control.btnText.Opacity = 0.7;
                    control.btnIcon.Opacity = 0.7;
                }
            }
            else
            {
                control.btn.Opacity = 1;
                control.btn.Cursor = Cursors.Hand;
                control.btnText.Opacity = 1;
                control.btnIcon.Opacity = 1;
            }
        }
        #endregion
        #region btnType
        public string BtnType
        {
            get { return (string)GetValue(typeProperty); }
            set { SetValue(typeProperty, value); }
        }
        public static readonly DependencyProperty typeProperty =
            DependencyProperty.Register("BtnType", typeof(string),
              typeof(ButtonUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedType));

        private static void OnPropertyChangedType(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ButtonUserControl control = (ButtonUserControl)d;

            if ((string)e.NewValue.ToString().ToLower() == "cancel")
            {
                if (control.Disabled)
                {
                    control.btn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#D85922");
                    control.btn.BorderBrush = control.btn.Background;
                    control.btn.Opacity = 1;
                    control.btnIcon.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFFF");
                    control.btnText.Foreground = control.btnIcon.Foreground;
                    control.btn.Opacity = 0.4;
                    control.btn.Cursor = Cursors.No;
                    control.btnText.Opacity = 0.7;
                    control.btnIcon.Opacity = 0.7;
                }
                else
                {
                    control.btn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFFF");
                    control.btn.BorderBrush = control.btn.Background;
                    control.btn.Opacity = 0.8;
                    control.btnIcon.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#000000");
                    control.btnText.Foreground = control.btnIcon.Foreground;
                }
            }
            else if ((string)e.NewValue.ToString().ToLower() == "reset")
            {
                control.btn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FBB511");
                control.btn.BorderBrush = control.btn.Background;
                control.btn.Opacity = 1;
                control.btnIcon.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFFF");
                control.btnText.Foreground = control.btnIcon.Foreground;

            }
            else
            {
                control.btn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#005D9A");
                control.btn.BorderBrush = control.btn.Background;
                control.btn.Opacity = 1;
                control.btnIcon.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFFF");
                control.btnText.Foreground = control.btnIcon.Foreground;
            }


        }
        #endregion
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
              typeof(ButtonUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedtoolTip));
        private static void OnPropertyChangedtoolTip(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ButtonUserControl control = (ButtonUserControl)d;
            control.btnTooltip.Content = (string)e.NewValue;
            control.btnTooltip.PlacementTarget = control.btn;
            if (!string.IsNullOrEmpty((string)e.NewValue) && control.ToolTipVO == 0 && ( control.ToolTipPlacement.ToString()=="Right" || control.ToolTipPlacement.ToString() == "Left"))
            {
                control.ToolTipVO =-( control.Height/2);
            }
            else if (!string.IsNullOrEmpty((string)e.NewValue) && control.ToolTipHO == 0 && (e.NewValue.ToString() == "Top" || e.NewValue.ToString() == "Bottom"))
            {
                control.ToolTipVO = 0;
                control.ToolTipHO = - ((control.Width / 2)-5);
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
              typeof(ButtonUserControl), new PropertyMetadata(PlacementMode.Left, OnPropertyChangedplacement));
        private static void OnPropertyChangedplacement(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ButtonUserControl control = (ButtonUserControl)d;
            if (e.NewValue.ToString() == "Right" || e.NewValue.ToString() == "Left"){
                control.ToolTipVO = -(control.Height / 2);
            }
            else if(e.NewValue.ToString() == "Top" || e.NewValue.ToString() == "Bottom")
            {
                control.ToolTipVO = 0;
                control.ToolTipHO = -((control.Width / 2) - 5);
            }
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
              typeof(ButtonUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedTB));
        private static void OnPropertyChangedTB(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ButtonUserControl control = (ButtonUserControl)d;
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
              typeof(ButtonUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedTF));

        private static void OnPropertyChangedTF(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ButtonUserControl control = (ButtonUserControl)d;
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
              typeof(ButtonUserControl), new PropertyMetadata(0D, OnPropertyChangedVf));

        private static void OnPropertyChangedVf(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ButtonUserControl control = (ButtonUserControl)d;
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
              typeof(ButtonUserControl), new PropertyMetadata(0D, OnPropertyChangedHO));

        private static void OnPropertyChangedHO(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ButtonUserControl control = (ButtonUserControl)d;
            control.btnTooltip.HorizontalOffset = (double)e.NewValue;

        }
        #endregion
        #endregion


        public ButtonUserControl()
        {
            InitializeComponent();
            Width = 70;
            Height = 24;
        }

        private void Btn_MouseEnter(object sender, MouseEventArgs e)
        {


            btnTooltip.IsOpen = !string.IsNullOrEmpty(btnTooltip.Content);
            if (this.BtnType.ToLower() == "cancel")
            {
                btn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#D85922");
                btn.BorderBrush = btn.Background;
                btnIcon.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFFF");
                btnText.Foreground = btnIcon.Foreground;
            }
            else
            {
                btn.Opacity = 0.8;
            }

        }

        private void Btn_MouseLeave(object sender, MouseEventArgs e)
        {
            if (btnTooltip.IsOpen)
            {
                btnTooltip.IsOpen = false;
                btnTooltip.Visibility = System.Windows.Visibility.Collapsed;
            }
            if (this.BtnType.ToLower() == "cancel")
            {
                btn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFFF");
                btn.BorderBrush = btn.Background;
                btn.Opacity = 0.8;
                btnIcon.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#000000");
                btnText.Foreground = btnIcon.Foreground;
            }
            else
            {
                btn.Opacity = 1;
            }
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            BtnClick?.Invoke(this);
        }
    }
}
