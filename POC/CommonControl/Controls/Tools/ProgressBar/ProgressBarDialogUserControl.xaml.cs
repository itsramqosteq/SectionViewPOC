using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace POC
{
    /// <summary>
    /// Interaction logic for SampleProgressDialog.xaml
    /// </summary>
    public partial class ProgressBarDialogUserControl : UserControl
    {
      
             
              public  double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }


        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double),
              typeof(ProgressBarDialogUserControl), new PropertyMetadata(0D, OnPropertyChangedForValue));
        private static void OnPropertyChangedForValue(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProgressBarDialogUserControl control = (ProgressBarDialogUserControl)d;
            control.pb.Value = (double)e.NewValue;
            control.lblPercentage.Text = Convert.ToString(e.NewValue);
            control.lbl.Text = (double)e.NewValue >= control.pb.Maximum  ? "COMPLETED!" : "LOADING...";
        }
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }


        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double),
              typeof(ProgressBarDialogUserControl), new PropertyMetadata(0D, OnPropertyChangedFoMinimum));
        private static void OnPropertyChangedFoMinimum(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProgressBarDialogUserControl control = (ProgressBarDialogUserControl)d;
            control.pb.Minimum = (double)e.NewValue;
        }

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }


        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double),
              typeof(ProgressBarDialogUserControl), new PropertyMetadata(0D, OnPropertyChangedForMaximum));
        private static void OnPropertyChangedForMaximum(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProgressBarDialogUserControl control = (ProgressBarDialogUserControl)d;
            control.pb.Maximum = (double)e.NewValue;
            
        }
        public bool IsIndeterminate
        {
            get { return (bool)GetValue(IsIndeterminateProperty); }
            set { SetValue(IsIndeterminateProperty, value); }
        }


        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register("IsIndeterminate", typeof(bool),
              typeof(ProgressBarDialogUserControl), new PropertyMetadata(false, OnPropertyChangedForIsIndeterminate));
        private static void OnPropertyChangedForIsIndeterminate(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProgressBarDialogUserControl control = (ProgressBarDialogUserControl)d;
            control.pb.IsIndeterminate = (bool)e.NewValue;
        }


        public ProgressBarDialogUserControl()
        {
            InitializeComponent();
            Minimum = 0;
            Maximum=100;
        }
       
    }
}
