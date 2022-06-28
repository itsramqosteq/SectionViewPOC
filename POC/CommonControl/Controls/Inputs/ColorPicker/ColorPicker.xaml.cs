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
    public partial class ColorPickerUserControl : UserControl
    {
        #region label 

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string),
              typeof(ColorPickerUserControl), new PropertyMetadata(string.Empty, OnPropertyChanged));
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorPickerUserControl control = (ColorPickerUserControl)d;
            control.lbl.Text = (string)e.NewValue;
            if (string.IsNullOrEmpty((string)e.NewValue))
            {
                control.lbl.Visibility = Visibility.Collapsed;
                control.cmb_colorPicker.Margin = new Thickness(0, 0, 0, 0);
            }
            else
            {
                control.lbl.Visibility = Visibility.Visible;
                control.cmb_colorPicker.Margin = new Thickness(0, 8, 0, 0);
            }
        }
        #endregion
        public new double Width
        {
            get { return (double)GetValue(widthProperty); }
            set { SetValue(widthProperty, value); }
        }


        public static readonly DependencyProperty widthProperty =
            DependencyProperty.Register("Width", typeof(double),
              typeof(ColorPickerUserControl), new PropertyMetadata(0D, OnPropertyChangedForWidth));
        private static void OnPropertyChangedForWidth(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorPickerUserControl control = (ColorPickerUserControl)d;
            control.cmb_colorPicker.Width = (double)e.NewValue;
            control.grdContainer.Width = (double)e.NewValue;
            control.popupContainer.Width = (double)e.NewValue - 20;
        }
        public Autodesk.Revit.DB.Color Value
        {
            get { return (Autodesk.Revit.DB.Color)GetValue(ColorRGBProperty); }
            set { SetValue(ColorRGBProperty, value); }
        }


        public static readonly DependencyProperty ColorRGBProperty =
            DependencyProperty.Register("Value", typeof(Autodesk.Revit.DB.Color),
              typeof(ColorPickerUserControl), new PropertyMetadata(null, OnPropertyChangedForRBGValue));
        private static void OnPropertyChangedForRBGValue(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorPickerUserControl control = (ColorPickerUserControl)d;
            if (e.NewValue != null && e.NewValue is Autodesk.Revit.DB.Color)
            {
                Autodesk.Revit.DB.Color obj = (e.NewValue as Autodesk.Revit.DB.Color);
                List<CustomColorPicker> customColors = CustomColors.customColors;
                int index = CustomColors.customColors.FindIndex(x => Convert.ToByte(x.ColorR) == obj.Red && Convert.ToByte(x.ColorG) == obj.Green && Convert.ToByte(x.ColorB) == obj.Blue);

                if (index < 0)
                {


                    control.PopupBox_Opened(obj, null);

                }
                else
                {
                    control.cmb_colorPicker.SelectedIndex = index;
                }
            }
        }
        public ColorPickerUserControl()
        {
            InitializeComponent();
            List<CustomColorPicker> customColors = CustomColors.customColors;
            cmb_colorPicker.ItemsSource = customColors;
            cmb_colorPicker.SelectedIndex = 0;
        }
        private void PopupBox_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is Autodesk.Revit.DB.Color obj)
            {
                slColorR.Value = Convert.ToDouble(obj.Red);
                slColorG.Value = Convert.ToDouble(obj.Green);
                slColorB.Value = Convert.ToDouble(obj.Blue);
                PopupBox_Closed(null, null);
            }
        }
        private void PopupBox_Closed(object sender, RoutedEventArgs e)
        {


            List<CustomColorPicker> customColors = cmb_colorPicker.ItemsSource.Cast<CustomColorPicker>().ToList();
            CustomColorPicker custom = customColors.FirstOrDefault(x =>
                                        x.SolidColorBrush.Color.R == (byte)slColorR.Value &&
                                        x.SolidColorBrush.Color.G == (byte)slColorG.Value &&
                                        x.SolidColorBrush.Color.B == (byte)slColorB.Value
                                        );
            if (custom != null)
            {
                cmb_colorPicker.SelectedIndex = customColors.IndexOf(custom);
            }
            else if (!customColors.Any(x => x.ColorR == Convert.ToString(slColorR.Value) &&
                      x.ColorG == Convert.ToString(slColorG.Value) &&
                      x.ColorB == Convert.ToString(slColorB.Value)))
            {
                int count = customColors.Count(x => x.ColorName.Contains("Custom Color")) + 1;
                CustomColorPicker obj = new CustomColorPicker
                {
                    ColorName = "Custom Color " + (count == 1 ? "" : count.ToString()),
                    ColorR = slColorR.Value.ToString(),
                    ColorG = slColorG.Value.ToString(),
                    ColorB = slColorB.Value.ToString()
                };
                customColors.Add(obj);
                cmb_colorPicker.ItemsSource = customColors;
                cmb_colorPicker.SelectedIndex = customColors.Count - 1;
                CustomColorPicker selectedItem = (cmb_colorPicker.SelectedItem as CustomColorPicker);
                Value = selectedItem.RGBvalue;
            }
        }
        private void ColorSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            if (slColorR != null && slColorG != null && slColorB != null)
            {
                slColorR.Value = TrickyValue(slColorR.Value);
                slColorG.Value = TrickyValue(slColorG.Value);
                slColorB.Value = TrickyValue(slColorB.Value);

                Color color = Color.FromRgb((byte)slColorR.Value, (byte)slColorG.Value, (byte)slColorB.Value);
                colorFill.Background = new SolidColorBrush(color);
               

            }

        }

        private void Cmb_colorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmb_colorPicker.SelectedItem != null)
            {
                CustomColorPicker selectedItem = (cmb_colorPicker.SelectedItem as CustomColorPicker);
                colorFill.Background = selectedItem.SolidColorBrush;
                slColorR.Value = Convert.ToByte(selectedItem.ColorR);
                slColorG.Value = Convert.ToByte(selectedItem.ColorG);
                slColorB.Value = Convert.ToByte(selectedItem.ColorB);
            }
        }
        private double TrickyValue(double value)
        {
            string[] str = value.ToString().Split('.');
            value = Convert.ToDouble(str[0]) + (str.Count() > 1 ? 1 : 0);
            return value;
        }
        private void Cmb_colorPicker_DropDownClosed(object sender, EventArgs e)
        {
            CustomColorPicker selectedItem = cmb_colorPicker.SelectedItem as CustomColorPicker;
            Value = selectedItem.RGBvalue;
        }


        private void PackIcon_MouseEnter(object sender, MouseEventArgs e)
        {
            btnTooltip.IsOpen = true;
            btnTooltip.PlacementTarget = btnMoreColors;
            btnTooltip.Placement = PlacementMode.Top;
            btnTooltip.Content = "More Color";
            btnTooltip.HorizontalOffset = -50;
        }

        private void PackIcon_MouseLeave(object sender, MouseEventArgs e)
        {
            btnTooltip.IsOpen = false;
            btnTooltip.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
