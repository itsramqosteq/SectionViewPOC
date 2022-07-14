#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media.Imaging;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using POC;
#endregion

namespace POC
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {       
        /// <summary>
        /// External command mainline
        /// </summary>
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            if (!Application.IsLoggedIn)
            {
                TaskDialog.Show("Entitlement API", "Please login to Autodesk 360 first\n");
                return Result.Failed;
            }
            string userId = commandData.Application.Application.Username;
            bool isValidUser = false;
            if (true || Utility.IsValidUser(userId, Util.ProductVersion))
            {
                Utility.AddValidationMethod(userId, "Server");
                isValidUser = true;
            }
            else if (true || Utility.IsValidUser(commandData.Application.Application.LoginUserId) && !Utility.HasExpired())
            {
                Utility.AddValidationMethod(userId, "Local");
                isValidUser = true;
            }
            try
            {
               
                if (true)
                {
                    Utility.CheckforUpdates(Util.InstallerFolderName);
                    CustomUIApplication customUIApplication = new CustomUIApplication
                    {
                        CommandData = commandData
                    };
                    System.Windows.Window window = new MainWindow(customUIApplication);
                    window.Show();
                    window.Closed += OnClosing;
                    if (App.POCButton != null)
                        App.POCButton.Enabled = false;
                }
                else
                {
                    TaskDialog.Show("License", "license has been expired or you do not have a valid one");
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
        public void OnClosing(object senPOCr, EventArgs e)
        {
            if (App.POCButton != null)
                App.POCButton.Enabled = true;
        }
    }

}
