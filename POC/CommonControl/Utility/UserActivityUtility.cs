using Autodesk.Revit.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace POC
{
    public partial class Utility
    {
        public static bool IsValidUser(string userId, string productType)
        {
            bool isValid = false;
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                {
                    return true;
                }
            };
            HttpClient client = new HttpClient(httpClientHandler)
            {
                BaseAddress = new Uri("http://64.62.236.10:9090/")
            };
            client.DefaultRequestHeaders.Accept.Add(
               new MediaTypeWithQualityHeaderValue("application/json"));
            try
            {
                HttpResponseMessage response = client.GetAsync(string.Format("User/GetUser?userId={0}&producttype={1}", userId, productType)).Result;
                if (response.IsSuccessStatusCode)
                {
                    User user = response.Content.ReadAsAsync<User>().Result;
                    if (user != null)
                    {
                        return DateTime.Now <= user.ExpiryDate;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return isValid;
        }
        public static void AddValidationMethod(string userName, string method)
        {
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                {
                    return true;
                }
            };
            HttpClient client = new HttpClient(httpClientHandler)
            {
                BaseAddress = new Uri("http://64.62.236.10:9090/")
            };
            client.DefaultRequestHeaders.Accept.Add(
               new MediaTypeWithQualityHeaderValue("application/json"));
            try
            {
                Validationmethod validationMethod = new Validationmethod
                {
                    RevitUserName = userName,
                    Method = method
                };
                string json = JsonConvert.SerializeObject(validationMethod);
                var postTask = client.PostAsJsonAsync<Validationmethod>("Common/ValidationMethod", validationMethod);
                postTask.Wait();
                var result = postTask.Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private static void AddUserActivityLogInternally(UserActivityLog userActivityLog)
        {
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                {
                    return true;
                }
            };
            HttpClient client = new HttpClient(httpClientHandler)
            {
                BaseAddress = new Uri("http://64.62.236.10:9090/")
            };
            client.DefaultRequestHeaders.Accept.Add(
               new MediaTypeWithQualityHeaderValue("application/json"));
            try
            {
                var postTask = client.PostAsJsonAsync<UserActivityLog>("UserActivityLog/AddUserActivity", userActivityLog);
                postTask.Wait();
                var result = postTask.Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public static string GetAssemblyFileVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersion.FileVersion;
        }
        public static async Task UserActivityLog(UIApplication _uiApp, string toolName, DateTime StartDate, string runStatus, string featureUsed,
                                        string description = null)
        {
            try
            {
                // Posting.  
                using HttpClient client = new HttpClient();
                UserActivityLog userActivityLog = new UserActivityLog
                {
                    systemusername = Environment.UserName,
                    systemdomain = Environment.UserDomainName,
                    revitusername = _uiApp.Application.Username,
                    revitversion = _uiApp.Application.VersionNumber,
                    revitfilename = _uiApp.ActiveUIDocument.Document.Title,
                    toolname = toolName,
                    toolversion = GetAssemblyFileVersion(),
                    featureused = featureUsed,
                    description = description,
                    runstatus = runStatus,
                    startdate = StartDate,
                    enddate = DateTime.UtcNow
                };
                // Setting Base address.  
                client.BaseAddress = new Uri("https://log-useractivity.herokuapp.com");
                // Setting content type.  
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                // Setting timeout.  
                client.Timeout = TimeSpan.FromSeconds(Convert.ToDouble(60000));
                // Initialization.  
                HttpResponseMessage response = new HttpResponseMessage();
                // HTTP POST  
                response = await client.PostAsJsonAsync("/logs", userActivityLog).ConfigureAwait(false);
                // Verification  
                if (response.IsSuccessStatusCode)
                {
                    response.Dispose();
                }
                else
                {
                    TaskDialog.Show("WARNING", "There was an issue while posting user activity log to Api Server");
                }
                AddUserActivityLogInternally(userActivityLog);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("WARNING", "There was an issue while posting user activity log to Api Server:" + ex.Message);
            }
        }
        //set expiry date here to avoid misusage of tools outside of organization
        public static bool HasExpired()
        {
            DateTime expiryDate = new DateTime(2022, 12, 31);
            return DateTime.Now > expiryDate;
        }
        /// <summary>
        /// verify user belongs to Sanveo or not. 
        /// If user is a sanveo employee and logged in with autodesk360 then he/she is a valid user
        /// </summary>
        public static bool IsValidUser(string userId)
        {
            bool IsValid = false;
            var ApplicationPath = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location);
            string LogFile = @"C:\Users\ADMIN\AppData\Local\Autodesk\Web Services\Log\";
            LogFile = LogFile.Replace("ADMIN", Environment.UserName);
            if (File.Exists(LogFile))
            {
                DirectoryInfo di = new DirectoryInfo(LogFile);
                FileInfo[] files = di.GetFiles("*.log");
                if (files != null && files.Length > 0)
                {
                    FileInfo fi = files.OrderByDescending(r => r.LastWriteTime).FirstOrDefault();
                    string tempLog = LogFile + "\\" + fi.Name.Split('.')[0] + "_temp.log";
                    if (File.Exists(tempLog))
                    {
                        File.Delete(tempLog);
                    }
                    File.Copy(fi.FullName, tempLog);
                    if (File.ReadAllLines(tempLog).Length > 0 && File.ReadAllLines(tempLog).Any(r => r.Contains(userId)))
                    {
                        IsValid = File.ReadAllLines(tempLog).Any(r => r.Contains(userId) && (r.ToLower().Contains("sanveo.com") || r.ToLower().Contains("sanveotech.com")));
                    }
                    File.Delete(tempLog);
                }
            }
            if (!IsValid)
            {
                string domainName = Environment.UserDomainName;
                IsValid = domainName.ToLower().Contains("sanveo") || domainName.ToLower().Contains("sanveotech.com");
            }
            return IsValid;
        }

        public static void DownloadUpdates(string filePath)
        {
            string downloadPath = string.Empty;          
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                {
                    return true;
                }
            };
            HttpClient client = new HttpClient(httpClientHandler)
            {
                BaseAddress = new Uri("http://64.62.236.10:9090/")
            };
            client.DefaultRequestHeaders.Accept.Add(
               new MediaTypeWithQualityHeaderValue("application/json"));
            try
            {
                HttpResponseMessage response = client.GetAsync(string.Format("Common/GetFileAsStream?filePath={0}", filePath)).Result;
                if (response.IsSuccessStatusCode)
                {
                    var fileStream = response.Content.ReadAsStreamAsync();
                    string TempPath = "C:\\Users\\ADMIN\\Downloads\\";
                    TempPath = TempPath.Replace("ADMIN", Environment.UserName);
                    downloadPath = Path.Combine(TempPath, "SNVAddins.exe");
                    using (FileStream file = new FileStream(downloadPath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        fileStream.Result.CopyTo(file);
                    }
                    if (File.Exists(downloadPath))
                    {
                       Process.Start(downloadPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public static async void CheckforUpdates(string folderPath)
        {
            string updatedInstallerPath = string.Empty;
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            string assemblyName = fvi.FileDescription;
            string assemblyVersion = fvi.FileVersion;
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                {
                    return true;
                }
            };
            HttpClient client = new HttpClient(httpClientHandler)
            {
                BaseAddress = new Uri("http://64.62.236.10:9090/")
            };
            client.DefaultRequestHeaders.Accept.Add(
               new MediaTypeWithQualityHeaderValue("application/json"));
            try
            {
                HttpResponseMessage response = client.GetAsync(string.Format("Common/CheckforUpdates?assemblyName={0}&assemblyVersion={1}&folderPath={2}", assemblyName, assemblyVersion, folderPath)).Result;
                if (response.IsSuccessStatusCode)
                {
                    updatedInstallerPath = response.Content.ReadAsAsync<string>().Result;
                }
                if (!string.IsNullOrEmpty(updatedInstallerPath))
                {
                    MessageBoxResult confirmMessage = MessageBox.Show("Updates are available. Do you want to download the updates and install it?",
                        "Ready to Install", System.Windows.MessageBoxButton.OKCancel);
                    if (confirmMessage == MessageBoxResult.OK)
                    {
                        confirmMessage = MessageBox.Show("You will be asked to close Revit during installation. Please save the project before clicking Ok",
                        "Notes", System.Windows.MessageBoxButton.OKCancel);
                        if (confirmMessage == MessageBoxResult.OK)
                        {
                            await Task.Run(() => { DownloadUpdates(updatedInstallerPath); });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
