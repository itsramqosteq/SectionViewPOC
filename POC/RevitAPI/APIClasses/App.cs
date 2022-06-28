#region Namespaces
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
#endregion


namespace POC
{
    class App : IExternalApplication
    {
        public static PushButton POCButton { get; set; }


        public Result OnStartup(UIControlledApplication application)
        {

            OnButtonCreate(application);
            application.ViewActivated += Application_ViewActivated;
            application.ApplicationClosing += Application_Closing;
            return Result.Succeeded;
        }
        private void Application_Closing(object sender, Autodesk.Revit.UI.Events.ApplicationClosingEventArgs e)
        {
            //TaskDialog.Show("Application_Closing", "Application_Closing");
        }

        private void Application_ViewActivated(object sender, Autodesk.Revit.UI.Events.ViewActivatedEventArgs e)
        {
            //TaskDialog.Show("Application_ViewActivated", "Application_ViewActivated");
        }


        public Result OnShutdown(UIControlledApplication a)
        {
            POCButton.Enabled = true;
            return Result.Succeeded;
        }

        //*****************************RibbonPanel()*****************************
        public RibbonPanel RibbonPanel(UIControlledApplication a)
        {
            string tab = Util.AddinRibbonTabName; // Archcorp
            string ribbonPanelText = Util.AddinRibbonPanel; // Architecture

            // Empty ribbon panel 
            RibbonPanel ribbonPanel = null;
            // Try to create ribbon tab. 
            try
            {
                a.CreateRibbonTab(tab);
            }
            catch { }
            // Try to create ribbon panel.
            try
            {
                RibbonPanel panel = a.CreateRibbonPanel(tab, ribbonPanelText);
            }
            catch { }
            // Search existing tab for your panel.
            List<RibbonPanel> panels = a.GetRibbonPanels(tab);
            foreach (RibbonPanel p in panels)
            {
                if (p.Name == ribbonPanelText)
                {
                    ribbonPanel = p;
                }
            }
            //return panel 
            return ribbonPanel;
        }


        /// <summary>
        /// Create a ribbon and panel and add a button for the revit DMS Plugin
        /// </summary>
        /// <param name="application"></param>
        private void OnButtonCreate(UIControlledApplication application)
        {
            string buttonText = Util.AddinButtonText;
            string buttonTooltip = Util.AddinButtonTooltip;

            string executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string dllLocation = Path.Combine(executableLocation, "POC.dll");

            // Create two push buttons

            PushButtonData buttondata = new PushButtonData("POCBtn", buttonText, dllLocation, "POC.Command")
            {
                ToolTip = buttonTooltip
            };

            BitmapImage pb1Image = new BitmapImage(new Uri("pack://application:,,,/POC;component/Resources/32x32.png"));
            buttondata.LargeImage = pb1Image;

            BitmapImage pb1Image2 = new BitmapImage(new Uri("pack://application:,,,/POC;component/Resources/16x16.png"));
            buttondata.Image = pb1Image2;

            var ribbonPanel = RibbonPanel(application);


            POCButton = ribbonPanel.AddItem(buttondata) as PushButton;

        }

    }

}
