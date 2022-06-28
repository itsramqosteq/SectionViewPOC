using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public partial class RadioButtonUserControl : UserControl
    {

        public event RoutedEventHandler Checked;
        public event RoutedEventHandler UnChecked;
        public delegate void RoutedEventHandler(object sender);
        #region label 

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string),
              typeof(RadioButtonUserControl), new PropertyMetadata(string.Empty, OnPropertyChanged));
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RadioButtonUserControl control = (RadioButtonUserControl)d;
            control.lbl.Text = (string)e.NewValue;
            control.lbl.Visibility = string.IsNullOrEmpty((string)e.NewValue) ? Visibility.Collapsed : Visibility.Visible;
            control.border.Margin = new Thickness(0, 27, 0, 0);
            if (string.IsNullOrEmpty((string)e.NewValue))
            {
                control.lbl.Visibility = Visibility.Collapsed;
                control.border.Margin = new Thickness(0, 0, 0, 0);
            }
            else
            {
                control.lbl.Visibility = Visibility.Visible;
                control.border.Margin = new Thickness(0, 27, 0, 0);
            }
        }
        #endregion

        public string Value
        {
            get { return (string)GetValue(valueProperty); }
            set { SetValue(valueProperty, value); }
        }


        public static readonly DependencyProperty valueProperty =
            DependencyProperty.Register("Value", typeof(string),
              typeof(RadioButtonUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedForValue));
        private static void OnPropertyChangedForValue(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RadioButtonUserControl control = (RadioButtonUserControl)d;
            string value = (string)e.NewValue;
            if (!string.IsNullOrEmpty(control.ItemsSource))
            {
                List<RadioButtonClass> radios = new List<RadioButtonClass>();
                List<string> list = control.ItemsSource.Split(',').ToList();
                string randomString = Utility.RandomString(3);
                int i = 0;
                foreach (string item in list)
                {
                    RadioButtonClass rd = new RadioButtonClass
                    {
                        Id = i,
                        Name = item,
                        GroupName = "groupName_" + randomString,
                        IsSelected = string.IsNullOrEmpty(value) ? i == 0 : value.ToLower() == item.ToLower()
                    };
                    double[] margin = new double[] { 0, i == 0 ? 0 : 8, 0, 0 };
                    rd.Margin = margin;
                    radios.Add(rd);
                    i++;
                }
                control.rdList.ItemsSource = radios;
            }
        }

        public string ItemsSource
        {
            get { return (string)GetValue(MSProperty); }
            set { SetValue(MSProperty, value); }
        }


        public static readonly DependencyProperty MSProperty =
            DependencyProperty.Register("ItemsSource", typeof(string),
              typeof(RadioButtonUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedForitemsSource));
        private static void OnPropertyChangedForitemsSource(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RadioButtonUserControl control = (RadioButtonUserControl)d;
            string value = (string)e.NewValue;
            if (!string.IsNullOrEmpty(value))
            {
                List<RadioButtonClass> radios = new List<RadioButtonClass>();
                List<string> list = value.Split(',').ToList();
                string randomString = Utility.RandomString(3);
                int i = 0;
                foreach (string item in list)
                {
                    RadioButtonClass rd = new RadioButtonClass
                    {
                        Id = i,
                        Name = item,
                        GroupName = "groupName_" + randomString,
                        IsSelected = string.IsNullOrEmpty(control.Value) ? i == 0 : control.Value.ToLower() == item.ToLower()
                    };
                    double[] margin = new double[] { 0, i == 0 ? 0 : 8, 0, 0 };
                    rd.Margin = margin;
                    radios.Add(rd);
                    i++;
                }
                control.rdList.ItemsSource = radios;
            }
        }
        public RadioButtonUserControl()
        {
            InitializeComponent();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            Value = ((ContentControl)sender).Content.ToString();
            Checked?.Invoke(this);
        }

        private void RadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            UnChecked?.Invoke(this);
        }
    }
}
