using Autodesk.Revit.DB;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
    public partial class TextBoxUserControl : UserControl
    {
        public event TextChangedEventHandler TextBox_Changed;
        public delegate void TextChangedEventHandler(object sender);
        public bool IsRequired
        {
            get { return (bool)GetValue(IsRequiredProperty); }
            set { SetValue(IsRequiredProperty, value); }
        }
        public static readonly DependencyProperty IsRequiredProperty =
            DependencyProperty.Register("IsRequired", typeof(bool),
              typeof(TextBoxUserControl), new PropertyMetadata(false, OnPropertyChangedIsRequired));
        private static void OnPropertyChangedIsRequired(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBoxUserControl control = (TextBoxUserControl)d;
            control.TxtIsRequired.Visibility = (bool)e.NewValue ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }
        public string UnitsLabel
        {
            get { return (string)GetValue(UnitsLabelProperty); }
            set { SetValue(UnitsLabelProperty, value); }
        }
        public static readonly DependencyProperty UnitsLabelProperty =
            DependencyProperty.Register("UnitsLabel", typeof(string),
              typeof(TextBoxUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedUnitsLabel));
        private static void OnPropertyChangedUnitsLabel(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBoxUserControl control = (TextBoxUserControl)d;
            control.TextBoxUnitsLabel.Text = (string)e.NewValue;
            
            control.TextBoxUnitsLabelBorder.Height = control.txtValue.Height - 2;
            control.TextBoxUnitsLabelBorder.Visibility = System.Windows.Visibility.Visible;
            control.TextBoxUnitsLabelBorder.Margin = new Thickness(0, -(control.txtValue.Height), 1, 0);
        }
        #region label 

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string),
              typeof(TextBoxUserControl), new PropertyMetadata(string.Empty, OnPropertyChanged));
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBoxUserControl control = (TextBoxUserControl)d;
            control.lbl.Text = (string)e.NewValue;
            if (string.IsNullOrEmpty((string)e.NewValue))
            {
                control.lbl.Visibility = System.Windows.Visibility.Collapsed;
                control.txtValue.Margin = new Thickness(0, 0, 0, 0);
            }
            else
            {
                control.lbl.Visibility = System.Windows.Visibility.Visible;
                control.txtValue.Margin = new Thickness(0, 8, 0, 0);
            }

        }
        #endregion

        #region validationMessage
        public string ValidationMessage
        {
            get { return (string)GetValue(ErrorContentProperty); }
            set { SetValue(ErrorContentProperty, value); }
        }
        public static readonly DependencyProperty ErrorContentProperty =
            DependencyProperty.Register("ValidationMessage", typeof(string),
              typeof(TextBoxUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedForErrorContent));

        private static void OnPropertyChangedForErrorContent(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBoxUserControl control = (TextBoxUserControl)d;
            control.validation.errorContent = (string)e.NewValue;
            control.validation.ValidatesOnTargetUpdated = !string.IsNullOrEmpty((string)e.NewValue) && !string.IsNullOrWhiteSpace((string)e.NewValue);
        }

        #endregion
        #region textInput
        public string Text
        {
            get { return (string)GetValue(TextInputProperty); }
            set { SetValue(TextInputProperty, value); }
        }


        public static readonly DependencyProperty TextInputProperty =
            DependencyProperty.Register("Text", typeof(string),
              typeof(TextBoxUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedText));

        private static void OnPropertyChangedText(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBoxUserControl control = (TextBoxUserControl)d;
            string value = (string)e.NewValue;
            if (string.IsNullOrEmpty(value))
            {
                control.AsDouble = 0;
                control.AsString = string.Empty;
            }
            else if (control.IsUnit)
            {
                Utility.GetProjectUnits(control.Document, value, out double asDouble, out string asString, true);
                control.AsDouble = asDouble;
                control.AsString = asString;
            }
        }
        #endregion

        public double AsDouble
        {
            get { return (double)GetValue(AsDoubleInputProperty); }
            set { SetValue(AsDoubleInputProperty, value); }
        }


        public static readonly DependencyProperty AsDoubleInputProperty =
            DependencyProperty.Register("AsDouble", typeof(double),
              typeof(TextBoxUserControl), new PropertyMetadata(0D));

        public string AsString
        {
            get { return (string)GetValue(AsStringInputProperty); }
            set { SetValue(AsStringInputProperty, value); }
        }


        public static readonly DependencyProperty AsStringInputProperty =
            DependencyProperty.Register("AsString", typeof(string),
              typeof(TextBoxUserControl), new PropertyMetadata(string.Empty));

        #region regex
        public string Regex
        {
            get { return (string)GetValue(regexProperty); }
            set { SetValue(regexProperty, value); }
        }


        public static readonly DependencyProperty regexProperty =
            DependencyProperty.Register("Regex", typeof(string),
              typeof(TextBoxUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedForRegex));

        private static void OnPropertyChangedForRegex(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //TextBoxUserControl control = (TextBoxUserControl)d;
            //control.lblPlaceHolder.Text = (string)e.NewValue;
        }
        #endregion
        public Document Document
        {
            get { return (Document)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }
        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.Register("Document", typeof(Document),
              typeof(TextBoxUserControl), new PropertyMetadata(null));

        public bool IsUnit
        {
            get { return (bool)GetValue(IsUnitProperty); }
            set { SetValue(IsUnitProperty, value); }
        }
        public static readonly DependencyProperty IsUnitProperty =
            DependencyProperty.Register("IsUnit", typeof(bool),
              typeof(TextBoxUserControl), new PropertyMetadata(false, OnPropertyChangedForIsunit));
        private static void OnPropertyChangedForIsunit(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBoxUserControl control = (TextBoxUserControl)d;
        }
        public new double Width
        {
            get { return (double)GetValue(widthProperty); }
            set { SetValue(widthProperty, value); }
        }


        public static readonly DependencyProperty widthProperty =
            DependencyProperty.Register("Width", typeof(double),
              typeof(TextBoxUserControl), new PropertyMetadata(0D, OnPropertyChangedForWidth));
        private static void OnPropertyChangedForWidth(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBoxUserControl control = (TextBoxUserControl)d;
            control.txtValue.Width = (double)e.NewValue;
        }
        public new double Height
        {
            get { return (double)GetValue(heightProperty); }
            set { SetValue(heightProperty, value); }
        }


        public static readonly DependencyProperty heightProperty =
            DependencyProperty.Register("Height", typeof(double),
              typeof(TextBoxUserControl), new PropertyMetadata(0D, OnPropertyChangedForHeight));
        private static void OnPropertyChangedForHeight(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBoxUserControl control = (TextBoxUserControl)d;
            control.txtValue.Height = (double)e.NewValue;
        }
        public  PackIconKind LeftIcon
        {
            get { return (PackIconKind)GetValue(LeftIconProperty); }
            set { SetValue(LeftIconProperty, value); }
        }


        public static readonly DependencyProperty LeftIconProperty =
            DependencyProperty.Register("LeftIcon", typeof(object),
              typeof(TextBoxUserControl), new PropertyMetadata(null, OnPropertyChangedForLeftIcon));
        private static void OnPropertyChangedForLeftIcon(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBoxUserControl control = (TextBoxUserControl)d;
            control.iconSearch.Visibility = System.Windows.Visibility.Visible;
            control.lblPlaceHolder.Margin = new Thickness(35, -27, 0, 0);
            control.txtValue.Padding = new Thickness(35, 12, 0, 0);
            control.iconSearch.Kind = (PackIconKind)Enum.Parse(typeof(PackIconKind), (string)e.NewValue, true);

        }

        public string HintText
        {
            get { return (string)GetValue(placeHolderProperty); }
            set { SetValue(placeHolderProperty, value); }
        }


        public static readonly DependencyProperty placeHolderProperty =
            DependencyProperty.Register("HintText", typeof(string),
              typeof(TextBoxUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedForplaceHolder));
        private static void OnPropertyChangedForplaceHolder(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBoxUserControl control = (TextBoxUserControl)d;
            control.lblPlaceHolder.Text = (string)e.NewValue;
        }
        public Thickness VerticalAlignPlaceHolder
        {
            get { return (Thickness)GetValue(VerticalAlignPlaceHolderProperty); }
            set { SetValue(VerticalAlignPlaceHolderProperty, value); }
        }


        public static readonly DependencyProperty VerticalAlignPlaceHolderProperty =
            DependencyProperty.Register("VerticalAlignPlaceHolder", typeof(Thickness),
              typeof(TextBoxUserControl), new PropertyMetadata(new Thickness(10, -25, 0, 0), OnPropertyChangedForvo));
        private static void OnPropertyChangedForvo(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBoxUserControl control = (TextBoxUserControl)d;
            control.lblPlaceHolder.Margin = (Thickness)e.NewValue;
        }

        public TextBoxUserControl()
        {
            InitializeComponent();
        }

        private void TxtValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(((System.Windows.Controls.TextBox)sender).Text))
            {
                lblPlaceHolder.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                lblPlaceHolder.Visibility = System.Windows.Visibility.Visible;
            }
            TextBox_Changed?.Invoke(this);
        }

        private void LblPlaceHolder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtValue.Focus();
        }

        private void TxtValue_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!(string.IsNullOrEmpty(Regex) && string.IsNullOrWhiteSpace(Regex)))
            {
                Regex regex = new Regex(Regex);
                e.Handled = regex.IsMatch(e.Text);
            }
            else if (IsUnit)
            {
                string pattern = @"[^0-9/.-]+";
                Regex regex = new Regex(pattern);
                e.Handled = regex.IsMatch(e.Text);
                if (e.Handled)
                    e.Handled = !(e.Text.Contains("\"") || e.Text.Contains("'"));
            }
        }

        private void TxtValue_LostFocus(object sender, RoutedEventArgs e)
        {
            Click_load(this);
            if (!string.IsNullOrEmpty(UnitsLabel))
            {
                lblPlaceHolder.Margin = new Thickness(10, -26, 0, 0);
                TextBoxUnitsLabelBorder.Margin = new Thickness(0, -(txtValue.Height ), 1, 0);
            }
            
        }



        public void Click_load(TextBoxUserControl obj)
        {
            if (!string.IsNullOrEmpty(obj.Text) && obj.IsUnit)
            {

                Utility.GetProjectUnits(Document, obj.Text, out double asDouble, out string asString);
                AsDouble = asDouble;
                AsString = asString;
                IsUnit = obj.IsUnit;
                Text = AsString;
            }

        }

        private void TxtValue_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(UnitsLabel))
            {
                TextBoxUnitsLabelBorder.Margin = new Thickness(0, -(txtValue.Height+1), 1, 0);
                TextBoxUnitsLabelBorder.Height = txtValue.Height - 3;
            }
        }

        private void txtValue_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
