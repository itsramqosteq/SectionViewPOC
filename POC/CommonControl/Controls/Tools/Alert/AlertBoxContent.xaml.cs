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

namespace POC.Internal
{
    /// <summary>
    /// Interaction logic for FooterPanel.xaml
    /// </summary>
    public partial class AlertBoxContentUserControl : UserControl
    {
        
        #region Content
        public new string Content
        {
            get { return (string)GetValue(contentProperty); }
            set { SetValue(contentProperty, value); }
        }
        public static readonly DependencyProperty contentProperty =
            DependencyProperty.Register("Content", typeof(string),
              typeof(AlertBoxContentUserControl), new PropertyMetadata(string.Empty, OnPropertyChangedContent));
        private static void OnPropertyChangedContent(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AlertBoxContentUserControl control = (AlertBoxContentUserControl)d;
            control.txtContent.Text = (string)e.NewValue;

        }
        #endregion
        #region AlertType
        public CustomAlertType AlertType
        {
            get { return (CustomAlertType)GetValue(AlertTypeProperty); }
            set { SetValue(AlertTypeProperty, value); }
        }
        public static readonly DependencyProperty AlertTypeProperty =
            DependencyProperty.Register("AlertType", typeof(CustomAlertType),
              typeof(AlertBoxContentUserControl), new PropertyMetadata(CustomAlertType.Information, OnPropertyChangedAlertType));
        private static void OnPropertyChangedAlertType(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AlertBoxContentUserControl control = (AlertBoxContentUserControl)d;
            control.txtTittle.Text = Utility.GetEnumDisplayName((CustomAlertType)e.NewValue);
            control.eventRow.Visibility = Visibility.Visible;
            control.confirmEventRow.Visibility = Visibility.Collapsed;
            control.icon.Width = control.IconBorder.Width = control.icon.Height = control.IconBorder.Height = 80;
            switch ((CustomAlertType)e.NewValue)
            {
                case CustomAlertType.Warning:
                    {
                        control.icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Warning;
                        control.icon.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#fbb511");
                        break;
                    }
                case CustomAlertType.Information:
                    {
                        control.icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Information;
                        control.icon.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#005D9A");
                        break;
                    }
                case CustomAlertType.Alert:
                    {
                        control.icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Alert;
                        control.icon.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#d85922");
                        break;
                    }
                case CustomAlertType.Failed:
                    {
                        control.icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Alert;
                        control.icon.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#d85922");
                        break;
                    }
                case CustomAlertType.Successful:
                    {
                        control.icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.CheckCircle;
                        control.icon.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#005D9A");
                        break;
                    }
                case CustomAlertType.Confirm:
                    {
                        control.icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.CheckCircle;
                        control.icon.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#fbb511");
                        control.eventRow.Visibility = Visibility.Collapsed;
                        control.confirmEventRow.Visibility = Visibility.Visible;
                        break;
                    }
                case CustomAlertType.Reset:
                    {
                        control.icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.RotateRight;
                        control.icon.Width = control.IconBorder.Width = control.icon.Height = control.IconBorder.Height = 60;

                        control.icon.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFFF");
                        control.IconBorder.Background= (SolidColorBrush)new BrushConverter().ConvertFrom("#fbb511"); 
                        control.eventRow.Visibility = Visibility.Collapsed;
                        
                        control.confirmEventRow.Visibility = Visibility.Visible;
                        control.txtContent.Text = "Are you sure you want to proceed?";
                        break;
                    }
            }
        }
        #endregion
        public AlertBoxContentUserControl()
        {
            InitializeComponent();
        }

        private void BtnOkay_Click(object sender, RoutedEventArgs e)
        {
            AlertVM alertVM = new AlertVM
            {
                MessageBoxResult = MessageBoxResult.OK
            };
            DataContext = alertVM;
        }

        private void Cnf_btnOkay_Click(object sender, RoutedEventArgs e)
        {
            AlertVM alertVM = new AlertVM
            {
                MessageBoxResult = MessageBoxResult.Yes
            };
            DataContext = alertVM;
        }

        private void Cnf_btnNo_Click(object sender, RoutedEventArgs e)
        {
            AlertVM alertVM = new AlertVM
            {
                MessageBoxResult = MessageBoxResult.No
            };
            DataContext = alertVM;
        }
    }
}
