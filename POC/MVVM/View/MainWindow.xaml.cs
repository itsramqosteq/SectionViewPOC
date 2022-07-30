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
using Autodesk.Revit.UI;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using POC;

namespace POC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region UI
        private void InitializeMaterialDesign()
        {
            var card = new Card();
            var hue = new Hue("Dummy", Colors.Black, Colors.White);

        }
        private void InitializeWindowProperty()
        {
            BitmapImage pb1Image = new BitmapImage(new Uri("pack://application:,,,/POC;component/Resources/16x16.png"));
            this.Icon = pb1Image;
            this.Title = Util.ApplicationWindowTitle;
            this.MinHeight = Util.ApplicationWindowHeight;
            this.Height = Util.ApplicationWindowHeight;
            this.Topmost = Util.IsApplicationWindowTopMost;
            this.MinWidth = Util.IsApplicationWindowAlowToReSize ? Util.ApplicationWindowWidth : 100;
            this.Width = Util.ApplicationWindowWidth;
            this.ResizeMode = Util.IsApplicationWindowAlowToReSize ? System.Windows.ResizeMode.CanResize : System.Windows.ResizeMode.NoResize;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HeaderPanel.Instance = this;
        }
        #endregion

        public static MainWindow Instance;
        public UIApplication _UIApp = null;
        readonly List<ExternalEvent> _externalEvents = new List<ExternalEvent>();
        public MainWindow(CustomUIApplication application)

        {
            InitializeWindowProperty();
            InitializeMaterialDesign();
            InitializeComponent();
            InitializeHandlers();
            Instance = this;
            HeaderPanel.Instance = this;
           UserControl userControl = new ParentUserControl(_externalEvents, application, this);
            Container.Children.Add(userControl);
            

        }
        private void InitializeHandlers()
        {

            _externalEvents.Add(ExternalEvent.Create(new SectionViewHandler()));
            _externalEvents.Add(ExternalEvent.Create(new StrutHandler()));
            _externalEvents.Add(ExternalEvent.Create(new SampleHandlerWhiteSpace()));

        }

        
    }
}
