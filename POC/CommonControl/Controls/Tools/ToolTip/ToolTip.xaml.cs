using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for PopupUserControl.xaml
    /// </summary>
    public partial class ToolTipUserControl : UserControl
    {

        #region placementTarget
        public UIElement PlacementTarget
        {
            get { return (UIElement)GetValue(UIElementProperty); }
            set { SetValue(UIElementProperty, value); }
        }
        public static readonly DependencyProperty UIElementProperty =
            DependencyProperty.Register("PlacementTarget", typeof(UIElement),
              typeof(ToolTipUserControl), new PropertyMetadata(null, OnPropertyChangedUIElement));
        private static void OnPropertyChangedUIElement(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToolTipUserControl control = (ToolTipUserControl)d;
            control.PopUpContainer.PlacementTarget = e.NewValue as UIElement;
        }
        #endregion
        #region isOpen
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool),
              typeof(ToolTipUserControl), new PropertyMetadata(false, OnPropertyChangedIsOpen));
        private static void OnPropertyChangedIsOpen(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToolTipUserControl control = (ToolTipUserControl)d;
            control.PopUpContainer.IsOpen = (bool)e.NewValue;
        }
        #endregion
        #region placement
        public PlacementMode Placement
        {
            get { return (PlacementMode)GetValue(placementProperty); }
            set { SetValue(placementProperty, value); }
        }
        public static readonly DependencyProperty placementProperty =
            DependencyProperty.Register("Placement", typeof(PlacementMode),
              typeof(ToolTipUserControl), new PropertyMetadata(PlacementMode.Left, OnPropertyChangedplacement));
        private static void OnPropertyChangedplacement(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToolTipUserControl control = (ToolTipUserControl)d;
            control.PopUpContainer.Placement = (PlacementMode)e.NewValue;
            control.LeftPath.LayoutTransform = new RotateTransform(0);
            control.RightPath.LayoutTransform = new RotateTransform(0);
            switch ((PlacementMode)e.NewValue)
            {
                case PlacementMode.Left:
                    {

                        control.StackContainer.Orientation = Orientation.Horizontal;
                        control.LeftPath.Visibility = Visibility.Collapsed;
                        control.RightPath.Visibility = Visibility.Visible;
                        control.RightPath.LayoutTransform = new RotateTransform(-90);
                        control.RightPath.HorizontalAlignment = HorizontalAlignment.Right;
                        control.RightPath.VerticalAlignment = VerticalAlignment.Center;
                        control.BorderContainer.Padding = new Thickness(0, 0, 5, 0);
                        break;
                    }
                case PlacementMode.Top:
                    {
                        control.StackContainer.Orientation = Orientation.Vertical;
                        control.LeftPath.Visibility = Visibility.Collapsed;
                        control.RightPath.Visibility = Visibility.Visible;

                        control.LeftPath.LayoutTransform = new RotateTransform(-90);
                        control.RightPath.HorizontalAlignment = HorizontalAlignment.Center;
                        control.BorderContainer.Padding = new Thickness(0, 0, 0, 5);
                        break;
                    }
                case PlacementMode.Bottom:
                    {

                        control.StackContainer.Orientation = Orientation.Vertical;
                        control.LeftPath.Visibility = Visibility.Visible;
                        control.RightPath.Visibility = Visibility.Collapsed;
                        control.LeftPath.LayoutTransform = new RotateTransform(180);
                        control.LeftPath.HorizontalAlignment = HorizontalAlignment.Center;
                        control.BorderContainer.Padding = new Thickness(0, 5, 0, 0);
                        break;
                    }
                case PlacementMode.Right:
                    {

                        control.StackContainer.Orientation = Orientation.Horizontal;
                        control.LeftPath.Visibility = Visibility.Visible;
                        control.RightPath.Visibility = Visibility.Collapsed;
                        control.LeftPath.LayoutTransform = new RotateTransform(90);
                        control.LeftPath.HorizontalAlignment = HorizontalAlignment.Left;
                        control.LeftPath.VerticalAlignment = VerticalAlignment.Center;
                        control.BorderContainer.Padding = new Thickness(5, 0, 0, 0);
                        break;
                    }
                default:
                    break;
            }

        }
        #endregion
        #region content
        public new string Content
        {
            get { return (string)GetValue(contentProperty); }
            set { SetValue(contentProperty, value); }
        }
        public static readonly DependencyProperty contentProperty =
            DependencyProperty.Register("Content", typeof(string),
              typeof(ToolTipUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedcontent));
        private static void OnPropertyChangedcontent(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToolTipUserControl control = (ToolTipUserControl)d;
            control.PopupText.Text = (string)e.NewValue;
            Size size = Utility.MeasureString((string)e.NewValue, control.PopupText);
            control.PathContainer.Width = size.Width + 40;
            if (!string.IsNullOrEmpty((string)e.NewValue) && string.IsNullOrEmpty(control.Placement.ToString()))
            {
                control.Placement = PlacementMode.Left;
            }


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
              typeof(ToolTipUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedBG));

        private static void OnPropertyChangedBG(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToolTipUserControl control = (ToolTipUserControl)d;
            if (string.IsNullOrEmpty((string)e.NewValue))
            {
                control.PathContainer.Background = (SolidColorBrush)new BrushConverter().ConvertFrom((string)e.NewValue);
                control.RightPath.Fill = control.RightPath.Stroke = control.LeftPath.Fill = control.LeftPath.Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom((string)e.NewValue);
            }
            else
            {
                control.PathContainer.Background = control.RightPath.Fill = control.RightPath.Stroke = control.LeftPath.Fill = control.LeftPath.Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom("#b9cddf");

            }
        }
        #endregion
        #region verticalOffset
        public double VerticalOffset
        {
            get { return (double)GetValue(VFProperty); }
            set { SetValue(VFProperty, value); }
        }
        public static readonly DependencyProperty VFProperty =
            DependencyProperty.Register("VerticalOffset", typeof(double),
              typeof(ToolTipUserControl), new PropertyMetadata(0D, OnPropertyChangedVf));

        private static void OnPropertyChangedVf(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToolTipUserControl control = (ToolTipUserControl)d;
            control.PopUpContainer.VerticalOffset = (double)e.NewValue;

        }
        #endregion
        #region horizontalOffset
        public double HorizontalOffset
        {
            get { return (double)GetValue(HSProperty); }
            set { SetValue(HSProperty, value); }
        }
        public static readonly DependencyProperty HSProperty =
            DependencyProperty.Register("HorizontalOffset", typeof(double),
              typeof(ToolTipUserControl), new PropertyMetadata(0D, OnPropertyChangedHO));

        private static void OnPropertyChangedHO(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToolTipUserControl control = (ToolTipUserControl)d;
            control.PopUpContainer.HorizontalOffset = (double)e.NewValue;

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
              typeof(ToolTipUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedFC));

        private static void OnPropertyChangedFC(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToolTipUserControl control = (ToolTipUserControl)d;
            control.PopupText.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom((string)e.NewValue);

        }
        #endregion

        public ToolTipUserControl()
        {
            InitializeComponent();
            PopUpContainer.Placement = PlacementMode.Left;
            StackContainer.Orientation = Orientation.Horizontal;
            LeftPath.Visibility = Visibility.Collapsed;
            RightPath.Visibility = Visibility.Visible;
            RightPath.LayoutTransform = new RotateTransform(-90);
            RightPath.HorizontalAlignment = HorizontalAlignment.Right;
            RightPath.VerticalAlignment = VerticalAlignment.Center;
            BorderContainer.Padding = new Thickness(0, 0, 5, 0);
            Background = "#b9cddf";
            //this.Loaded += new RoutedEventHandler(View1_Loaded);

        }
        void View1_Loaded(object sender, RoutedEventArgs e)
        {
            if (PlacementTarget != null)
            {
                Window w = Window.GetWindow(PlacementTarget);
                // w should not be Null now!
                if (null != w)
                {
                    w.LocationChanged += delegate (object sender2, EventArgs args)
                    {
                        var offset = PopUpContainer.HorizontalOffset;
                        // "bump" the offset to cause the popup to reposition itself
                        //   on its own
                        PopUpContainer.HorizontalOffset = offset + 1;
                        PopUpContainer.HorizontalOffset = offset;
                    };
                    // Also handle the window being resized (so the popup's position stays
                    //  relative to its target element if the target element moves upon 
                    //  window resize)
                    w.SizeChanged += delegate (object sender3, SizeChangedEventArgs e2)
                    {
                        var offset = PopUpContainer.HorizontalOffset;
                        PopUpContainer.HorizontalOffset = offset + 1;
                        PopUpContainer.HorizontalOffset = offset;
                    };
                }
            }
        }
    }
}
