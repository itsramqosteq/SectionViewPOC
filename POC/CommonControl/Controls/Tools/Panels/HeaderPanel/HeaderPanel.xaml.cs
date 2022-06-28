
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    /// Interaction logic for HeaderPanel.xaml
    /// </summary>
    public partial class HeaderPanelUserControl : UserControl
    {
        private Window window;
        public object Instance
        {
            get { return (object)GetValue(WindowProperty); }
            set { SetValue(WindowProperty, value); }
        }

        /// <summary>
        /// Identified the Label dependency property
        /// </summary>
        public static readonly DependencyProperty WindowProperty =
            DependencyProperty.Register("Instance", typeof(object),
              typeof(HeaderPanelUserControl), new PropertyMetadata(null, OnPropertyChangedInstance));
        private static void OnPropertyChangedInstance(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HeaderPanelUserControl control = (HeaderPanelUserControl)d;

            control.window = (Window)e.NewValue;
        }
        public string Tittle
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        /// <summary>
        /// Identified the Label dependency property
        /// </summary>
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Tittle", typeof(string),
              typeof(HeaderPanelUserControl), new PropertyMetadata(string.Empty));
        public ImageSource Logo
        {
            get { return (ImageSource)GetValue(LogoProperty); }
            set { SetValue(LogoProperty, value); }
        }

        /// <summary>
        /// Identified the Label dependency property
        /// </summary>
        public static readonly DependencyProperty LogoProperty =
            DependencyProperty.Register("Logo", typeof(ImageSource),
              typeof(HeaderPanelUserControl), new PropertyMetadata(null, OnPropertyChangedLogo));
        private static void OnPropertyChangedLogo(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HeaderPanelUserControl control = (HeaderPanelUserControl)d;
            if (e.NewValue != null)
                control.Img.Source = (ImageSource)e.NewValue;

        }
        public string DocumentPath
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        /// <summary>
        /// Identified the Label dependency property
        /// </summary>
        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("DocumentPath", typeof(string),
              typeof(HeaderPanelUserControl), new PropertyMetadata(string.Empty));

        bool isClose = false;
        bool isHelp = false;
        bool isMinimize = false;

        public HeaderPanelUserControl()
        {
            InitializeComponent();
            DataContext = this;

        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            window.DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            if (isHelp || isClose)
            {
                isHelp = isClose = false;
                return;
            }
            window.WindowState = WindowState.Minimized;
                ((System.Windows.Controls.Control)sender).Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#061B6C");
                ((System.Windows.Controls.Control)sender).BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#061B6C");
                isHelp = true;
            

        }

        private void Help_Click(object sender, RoutedEventArgs e)

        {

            if (isMinimize || isClose)
            {
                isMinimize = isClose = false;
                return;
            }


            string path;
            path = System.IO.Path.GetDirectoryName(
            System.Reflection.Assembly.GetExecutingAssembly().Location);
            path += "\\" + DocumentPath;
            if (File.Exists(path))
                Process.Start(path);
            ((System.Windows.Controls.Control)sender).Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#061B6C");
            ((System.Windows.Controls.Control)sender).BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#061B6C");
            isMinimize = true;




        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            isClose = true;
            window.Close();

        }





    }
}
