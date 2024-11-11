using System;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Xml.Linq;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Threading;
using ColorPicker;
using System.Windows.Media.Imaging;
using System.Threading;
using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;
using System.Collections.Concurrent;
using System.Globalization;
using System.Xml;
using HomeTools.AFS;
using HomeTools.BARFramework;
using HomeTools.ChannelID;
using HomeTools.UnBAR;
using CustomLogger;
using System.IO.Compression;
using System.Net.Http;
using System.Net;
using Microsoft.Web.WebView2.Core;
using System.Net.NetworkInformation;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SQLite;
using System.Buffers;
using CsvHelper;
using System.Collections.ObjectModel;
using HomeTools.Crypto;
using HomeTools.CDS;

namespace NautilusXP2024
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private AppSettings _settings;

        private SolidColorBrush _selectedThemeColor;
        public ObservableCollection<SceneListEditor> Scenes { get; set; } = new ObservableCollection<SceneListEditor>();
        private XElement loadedXmlData;
        private Dictionary<int, string> uuidMappings;
        private static bool DeployHCDBFlag = false;
        private static bool DeploySceneListFlag = false;
        public SolidColorBrush SelectedThemeColor
        {
            get { return _selectedThemeColor; }
            set
            {
                if (_selectedThemeColor != value)
                {
                    _selectedThemeColor = value;
                    OnPropertyChanged(nameof(SelectedThemeColor));
                }
            }
        }
        private FileScannerStandalone fileScanner;
        public MainWindow()
        {
            InitializeComponent();

            InitializeWebView2();

            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            LoggerAccessor.SetupLogger("NautilusXP2024", Directory.GetCurrentDirectory());
            fileScanner = new FileScannerStandalone(Sha1TextBox);

            DataContext = this;
            // Load settings when the window initializes
            _settings = SettingsManager.LoadSettings();

            // Convert the theme color from settings to a Color object
            Color themeColor = (Color)ColorConverter.ConvertFromString(_settings.ThemeColor);

            // Convert the theme color to a SolidColorBrush
            _selectedThemeColor = new SolidColorBrush(themeColor);

            // Apply the theme color to the UI
            ApplySettingsToUI();

            // Update the taskbar icon with the theme color from settings
            UpdateTaskbarIconWithTint(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dependencies\\icon.png"), themeColor);

            AFSClass.MapperHelperFolder = Directory.GetCurrentDirectory();

            _ = new Timer(AFSClass.ScheduledUpdate, null, TimeSpan.Zero, TimeSpan.FromMinutes(1440));

            logFlushTimer = new Timer(_ => FlushLogBuffer(), null, Timeout.Infinite, Timeout.Infinite);

            PS3IPforFTPTextBox.TextChanged += PS3IPforFTPTextBox_TextChanged;

            InitializeComboBoxEventHandlers();
            this.Loaded += MainWindow_Loaded;

            txtSceneType.SelectedItem = txtSceneType.Items.OfType<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == "GlobalSpace");
            txtdHost.SelectedItem = txtdHost.Items.OfType<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == "en-US");
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Set the default state for CheckBoxArchiveCreatorRenameCDN
            CheckBoxArchiveCreatorRenameCDN.IsChecked = true;
            CheckBoxArchiveMapperDeleteFilestxt.IsChecked = true;
        }

        public Color ThemeColor
        {
            get => (Color)ColorConverter.ConvertFromString(_settings.ThemeColor);
            set
            {
                if (_settings.ThemeColor != value.ToString())
                {
                    _settings.ThemeColor = value.ToString();
                    SettingsManager.SaveSettings(_settings);
                    OnPropertyChanged(nameof(ThemeColor));
                }
            }
        }

        private void ApplySettingsToUI()
        {
            // Applying settings to UI elements
            CdsEncryptOutputDirectoryTextBox.Text = _settings.CdsEncryptOutputDirectory;
            CdsDecryptOutputDirectoryTextBox.Text = _settings.CdsDecryptOutputDirectory;
            BarSdatSharcOutputDirectoryTextBox.Text = _settings.BarSdatSharcOutputDirectory;
            MappedOutputDirectoryTextBox.Text = _settings.MappedOutputDirectory;
            HcdbOutputDirectoryTextBox.Text = _settings.HcdbOutputDirectory;
            SqlOutputDirectoryTextBox.Text = _settings.SqlOutputDirectory;
            TicketListOutputDirectoryTextBox.Text = _settings.TicketListOutputDirectory;
            LuacOutputDirectoryTextBox.Text = _settings.LuacOutputDirectory;
            LuaOutputDirectoryTextBox.Text = _settings.LuaOutputDirectory;
            VideoOutputDirectoryTextBox.Text = _settings.VideoOutputDirectory;
            AudioOutputDirectoryTextBox.Text = _settings.AudioOutputDirectory;
            Rpcs3Dev_hdd0_DirectoryTextBox.Text = _settings.RPCS3OutputDirectory;
            PS3IPforFTPTextBox.Text = _settings.PS3IPforFTP;
            PS3TitleIDTextBox.Text = _settings.PS3TitleID;
            SceneListSavePathtextbox.Text = _settings.SceneListSavePath;
            SceneListPathURLtextbox2.Text = _settings.SceneListPathURL;
            TSSURLtextbox.Text = _settings.TSSURL;
            TSSeditorSavePathtextbox.Text = _settings.TSSeditorSavePath;
            ToggleSwitchLiteCatalogue.IsChecked = _settings.LiteCatalogueEnabled;
            ThemeColorPicker.SelectedColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_settings.ThemeColor);
            switch (_settings.ArchiveTypeSettingRem)
            {
                case ArchiveTypeSetting.BAR:
                    RadioButtonArchiveCreatorBAR.IsChecked = true;
                    break;
                case ArchiveTypeSetting.BAR_S:
                    RadioButtonArchiveCreatorBAR_S.IsChecked = true;
                    break;
                case ArchiveTypeSetting.SDAT:
                    RadioButtonArchiveCreatorSDAT.IsChecked = true;
                    break;
                case ArchiveTypeSetting.SDAT_SHARC:
                    RadioButtonArchiveCreatorSDAT_SHARC.IsChecked = true;
                    break;
                case ArchiveTypeSetting.CORE_SHARC:
                    RadioButtonArchiveCreatorCORE_SHARC.IsChecked = true;
                    break;
                // Added setting for creating config sharcs
                case ArchiveTypeSetting.CONFIG_SHARC:
                    RadioButtonArchiveCreatorCONFIG_SHARC.IsChecked = true;
                    break;

            };
            switch (_settings.ArchiveMapperSettingRem)
            {
                case ArchiveMapperSetting.NORM:
                    CheckBoxArchiveMapperFAST.IsChecked = false;

                    break;
                case ArchiveMapperSetting.FAST:
                    CheckBoxArchiveMapperFAST.IsChecked = true;
                    break;
                case ArchiveMapperSetting.EXP:
                    CheckBoxArchiveMapperEXP.IsChecked = true;
                    break;

            };
            switch (_settings.SaveDebugLogToggle)
            {
                case SaveDebugLog.True:
                    ToggleSwitchDebugLogEnable.IsChecked = true;
                    break;
                case SaveDebugLog.False:
                    ToggleSwitchDebugLogEnable.IsChecked = false;
                    break;
            }

            ToggleSwitchOfflinePshome.IsChecked = _settings.IsOfflineMode; // Apply offline mode setting

            SelectLastUsedTab(_settings.LastTabUsed);
        }

        private void ToggleSwitchLiteCatalogue_Checked(object sender, RoutedEventArgs e)
        {
            _settings.LiteCatalogueEnabled = ToggleSwitchLiteCatalogue.IsChecked == true;
            SettingsManager.SaveSettings(_settings);
        }

        private async void InitializeWebView2()
        {
            var environment = await CoreWebView2Environment.CreateAsync(null, null, new CoreWebView2EnvironmentOptions("--disable-web-security --user-data-dir=C:\\temp"));
            await WebView2Control.EnsureCoreWebView2Async(environment);

            // Set the source or load the custom HTML as per your existing logic
            if (ToggleSwitchOfflinePshome.IsChecked == false)
            {
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string htmlFileName = ToggleSwitchLiteCatalogue.IsChecked == false ? "index_Lite.html" : "index.html";
                string htmlFilePath = Path.Combine(exeDirectory, "dependencies", "psho.me", htmlFileName);

                if (File.Exists(htmlFilePath))
                {
                    string htmlFileUri = new Uri(htmlFilePath).AbsoluteUri;
                    WebView2Control.Source = new Uri(htmlFileUri);
                }
                else
                {
                    // Load the custom HTML if the file is not found
                    string customHtml = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {
            background-color: #111111;
            color: #fff;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            font-family: Arial, sans-serif;
            text-align: center;
            margin: 0;
        }
        .container {
            max-width: 600px;
            padding: 20px;
            border: 2px solid #fff;
            border-radius: 10px;
            box-shadow: 0 0 10px rgba(255, 255, 255, 0.2);
        }
        h1 {
            font-size: 24px;
            margin-bottom: 10px;
        }
        p {
            font-size: 16px;
            line-height: 1.5;
        }
        a {
            color: #00f;
            text-decoration: underline;
            cursor: pointer;
        }
    </style>
    <script>
        function openGoogleDrive() {
            window.chrome.webview.postMessage('openGoogleDrive');
        }
        function openGithub() {
            window.chrome.webview.postMessage('openGithub');
        }
    </script>
</head>
<body>
    <div class='container'>
        <h1>Files Not Found!</h1>
        <p>Local database files could not be found.</p>
        <p>To use a local catalogue, You must provide your own database.</p>
        <p>Please ensure the extracted files are placed in the 'dependencies/psho.me/' directory.</p>
        <p>For information on how to create your own database, See Multiserver 3 Nautilus Fork on Github <a onclick='openGithub()'>HERE</a></p>
    </div>
</body>
</html>";

                    WebView2Control.NavigateToString(customHtml);
                }
            }
            else
            {
                WebView2Control.Source = new Uri("http://psho.me/index.html");
            }

            WebView2Control.ZoomFactor = 0.80;
            WebView2Control.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            WebView2Control.CoreWebView2.DownloadStarting += CoreWebView2_DownloadStarting; // Add this line
        }

        private void CoreWebView2_DownloadStarting(object sender, CoreWebView2DownloadStartingEventArgs e)
        {
            // Define the desired download folder relative to the application
            string downloadFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "Downloads");

            // Ensure the download folder exists
            Directory.CreateDirectory(downloadFolder);

            // Set the initial download path to the specified folder
            string fileName = Path.GetFileName(e.ResultFilePath); // Extract the file name from the default path
            string newFilePath = Path.Combine(downloadFolder, fileName);

            // Check if the file already exists, and adjust the filename accordingly
            newFilePath = GetUniqueFilePath(newFilePath);

            // Set the new file path
            e.ResultFilePath = newFilePath;

            // Automatically approve the download without asking the user
            e.Handled = true;

            // Optional: Show a notification for the download start
            var notificationMessage = JsonConvert.SerializeObject(new { action = "showNotification", text = $"Downloading to {newFilePath}" });
            WebView2Control.CoreWebView2.PostWebMessageAsString(notificationMessage);

            // Handle download completion event to open the folder after download completes
            e.DownloadOperation.StateChanged += (s, args) =>
            {
                if (e.DownloadOperation.State == CoreWebView2DownloadState.Completed)
                {
                    // Open the folder after download completes successfully
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = downloadFolder,
                        UseShellExecute = true,
                        Verb = "open"
                    });
                }
                else if (e.DownloadOperation.State == CoreWebView2DownloadState.Interrupted)
                {
                    // Optionally handle download failure
                    var failedNotification = JsonConvert.SerializeObject(new { action = "showNotification", text = "Download failed" });
                    WebView2Control.CoreWebView2.PostWebMessageAsString(failedNotification);
                }
            };
        }

        // Helper method to generate a unique file path by appending _2, _3, etc., if the file already exists
        private string GetUniqueFilePath(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);
            int fileIndex = 1;

            // Generate a unique file path
            while (File.Exists(filePath))
            {
                filePath = Path.Combine(directory, $"{fileNameWithoutExtension}_{fileIndex}{extension}");
                fileIndex++;
            }

            return filePath;
        }



        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var message = e.TryGetWebMessageAsString();
            if (message == "openGoogleDrive")
            {
                OpenUrlInDefaultBrowser("https://google.ie");
            }
            else if (message == "openGithub")
            {
                OpenUrlInDefaultBrowser("https://github.com/DeViL303/MultiServer3-NuatilusFork");
            }
            else
            {
                try
                {
                    var jsonMessage = JsonConvert.DeserializeObject<Dictionary<string, object>>(message);
                    if (jsonMessage != null && jsonMessage.ContainsKey("action"))
                    {
                        if (jsonMessage["action"].ToString() == "sendToPS3" && jsonMessage.ContainsKey("data") && jsonMessage.ContainsKey("index"))
                        {
                            var listToSend = JsonConvert.SerializeObject(jsonMessage["data"]);
                            int listNumber = Convert.ToInt32(jsonMessage["index"]);
                            SaveListToFileAsync(listToSend, listNumber);

                            // Send notification to web content
                            var notificationMessage = JsonConvert.SerializeObject(new { action = "showNotification", text = "List sent to PS3" });
                            WebView2Control.CoreWebView2.PostWebMessageAsString(notificationMessage);
                        }
                        else if (jsonMessage["action"].ToString() == "saveAsSQL" && jsonMessage.ContainsKey("data") && jsonMessage.ContainsKey("index"))
                        {
                            var listToSend = JsonConvert.SerializeObject(jsonMessage["data"]);
                            int listNumber = Convert.ToInt32(jsonMessage["index"]);
                            SaveListToFileAsSQL(listToSend, listNumber);
                        }
                        else if (jsonMessage["action"].ToString() == "sendToRPCS3" && jsonMessage.ContainsKey("data") && jsonMessage.ContainsKey("index"))
                        {
                            var listToSend = JsonConvert.SerializeObject(jsonMessage["data"]);
                            int listNumber = Convert.ToInt32(jsonMessage["index"]);
                            SaveListToFileRPCS3Async(listToSend, listNumber);

                            // Send notification to web content
                            var notificationMessage = JsonConvert.SerializeObject(new { action = "showNotification", text = "List sent to RPCS3" });
                            WebView2Control.CoreWebView2.PostWebMessageAsString(notificationMessage);
                        }
                        else if (jsonMessage["action"].ToString() == "saveAsXML" && jsonMessage.ContainsKey("data") && jsonMessage.ContainsKey("index"))
                        {
                            var listToSend = JsonConvert.SerializeObject(jsonMessage["data"]);
                            int listNumber = Convert.ToInt32(jsonMessage["index"]);
                            SaveListToFileAsXML(listToSend, listNumber);
                        }
                        else if (jsonMessage["action"].ToString() == "saveAsPKG" && jsonMessage.ContainsKey("data") && jsonMessage.ContainsKey("index"))
                        {
                            var listToSend = JsonConvert.SerializeObject(jsonMessage["data"]);
                            int listNumber = Convert.ToInt32(jsonMessage["index"]);
                            SaveListToFileAsPKG(listToSend, listNumber);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to process message: {ex.Message}");
                }
            }
        }

        private async void SaveListToFileAsPKG(string listJson, int listNumber)
        {
            try
            {
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string outputDir = Path.Combine(exeDirectory, "Output", "Catalogue");
                ShowNotification("Creating pkg!", 2);
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                string filePath = Path.Combine(outputDir, $"UserList_{listNumber}.json");

                var updatedJson = ProcessJsonAndAddRewardType(listJson);

                File.WriteAllText(filePath, updatedJson);

                GenerateSqlFromJson(updatedJson, outputDir);

                string sqlFilePath = Path.Combine(outputDir, "POSTINSTALL.SQL");

                // Retrieve the dynamic value from the TextBox
                string ps3TitleID = PS3TitleIDTextBox.Text;

                // Copy the SQL file to the designated directory
                string pkgDirectory = Path.Combine(exeDirectory, "dependencies", "create_pkg", "temp", "dev_hdd0", "game", ps3TitleID, "USRDIR", "OBJECTCATALOGUE");
                if (!Directory.Exists(pkgDirectory))
                {
                    Directory.CreateDirectory(pkgDirectory);
                }

                string destinationSqlFilePath = Path.Combine(pkgDirectory, "POSTINSTALL.SQL");
                File.Copy(sqlFilePath, destinationSqlFilePath, true);

                // Run the executable to create the PKG file
                string exePath = Path.Combine(exeDirectory, "dependencies", "create_pkg", "pkg_custom.exe");
                string contentId = $"EP9000-{ps3TitleID}_00-HOME000000000001";
                string pkgFileName = $"ITEM_LIST_{listNumber}.pkg";
                string pkgOutputPath = Path.Combine(outputDir, pkgFileName);

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = $"--contentid {contentId} \"{Path.Combine(exeDirectory, "dependencies", "create_pkg", "temp")}\" \"{pkgOutputPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = new Process { StartInfo = processStartInfo };
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    MessageBox.Show($"Failed to create PKG file: {error}");
                    return;
                }

                // Read the PKG file and encode it as base64
                byte[] pkgFileBytes = File.ReadAllBytes(pkgOutputPath);
                string base64PkgFile = Convert.ToBase64String(pkgFileBytes);
                var downloadMessage = new { action = "downloadPkgFile", fileName = pkgFileName, fileContent = base64PkgFile };
                var downloadMessageJson = JsonConvert.SerializeObject(downloadMessage);

                WebView2Control.CoreWebView2.PostWebMessageAsString(downloadMessageJson);
                ShowNotification("Pkg Created!", 2);
                Console.WriteLine($"Successfully created the PKG file at {pkgOutputPath}");

                // Rename the POSTINSTALL.SQL file to include the list number
                string renamedSqlFilePath = Path.Combine(outputDir, $"POSTINSTALL_{listNumber}.SQL");
                if (File.Exists(renamedSqlFilePath))
                {
                    File.Delete(renamedSqlFilePath);
                }
                File.Move(sqlFilePath, renamedSqlFilePath);

                // Delete the temp folder after PKG creation
                string tempFolderPath = Path.Combine(exeDirectory, "dependencies", "create_pkg", "temp");
                if (Directory.Exists(tempFolderPath))
                {
                    Directory.Delete(tempFolderPath, true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save the list as PKG: {ex.Message}");
            }
        }

      
        private async void SaveListToFileAsSQL(string listJson, int listNumber)
        {
            try
            {
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string outputDir = Path.Combine(exeDirectory, "Output", "Catalogue");

                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                string filePath = Path.Combine(outputDir, $"UserList_{listNumber}.json");

                var updatedJson = ProcessJsonAndAddRewardType(listJson);

                File.WriteAllText(filePath, updatedJson);

                GenerateSqlFromJson(updatedJson, outputDir);

                string sqlFilePath = Path.Combine(outputDir, "POSTINSTALL.SQL");

                // Read the SQL file and encode it as base64
                byte[] sqlFileBytes = File.ReadAllBytes(sqlFilePath);
                string base64SqlFile = Convert.ToBase64String(sqlFileBytes);
                var downloadMessage = new { action = "downloadSqlFile", fileName = "POSTINSTALL.SQL", fileContent = base64SqlFile };
                var downloadMessageJson = JsonConvert.SerializeObject(downloadMessage);

                WebView2Control.CoreWebView2.PostWebMessageAsString(downloadMessageJson);

                // Rename the POSTINSTALL.SQL file to include the list number after download
                string renamedSqlFilePath = Path.Combine(outputDir, $"POSTINSTALL_{listNumber}.SQL");
                if (File.Exists(renamedSqlFilePath))
                {
                    File.Delete(renamedSqlFilePath);
                }
                File.Move(sqlFilePath, renamedSqlFilePath);
                ShowNotification("Created SQL!", 2); // Show notification for 5 seconds
                Console.WriteLine($"Successfully saved the list to {filePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save the list: {ex.Message}");
            }
        }

        private void OpenUrlInDefaultBrowser(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open URL: {ex.Message}");
            }
        }

        private async void SaveListToFileAsync(string listJson, int listNumber)
        {
            try
            {
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string outputDir = Path.Combine(exeDirectory, "Output", "Catalogue");

                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                string filePath = Path.Combine(outputDir, $"UserList_{listNumber}.json");

                var updatedJson = ProcessJsonAndAddRewardType(listJson);

                File.WriteAllText(filePath, updatedJson);

                GenerateSqlFromJson(updatedJson, outputDir);

                string sqlFilePath = Path.Combine(outputDir, "POSTINSTALL.SQL");

                string ipAddress = PS3IPforFTPTextBox.Text;
                if (!string.IsNullOrEmpty(ipAddress))
                {
                    // Use the text from the GUI textbox to set the folder name dynamically
                    string titleID = PS3TitleIDTextBox.Text;
                    string ftpUploadUri = $"ftp://{ipAddress}/dev_hdd0/game/{titleID}/USRDIR/OBJECTCATALOGUE/POSTINSTALL.SQL";
                    await UploadFileToFtpAsync(ftpUploadUri, sqlFilePath);
                }

                // Rename the POSTINSTALL.SQL file to include the list number after sending to FTP
                string renamedSqlFilePath = Path.Combine(outputDir, $"POSTINSTALL_{listNumber}.SQL");
                if (File.Exists(renamedSqlFilePath))
                {
                    File.Delete(renamedSqlFilePath);  // Delete the existing file if it already exists
                }
                if (File.Exists(sqlFilePath))
                {
                    File.Move(sqlFilePath, renamedSqlFilePath);
                }
                ShowNotification("Sent to PS3!", 2); // Show notification for 5 seconds
                Console.WriteLine($"Successfully saved the list to {filePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save the list: {ex.Message}");
            }
        }

        private async void SaveListToFileRPCS3Async(string listJson, int listNumber)
        {
            try
            {
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string outputDir = Path.Combine(exeDirectory, "Output", "Catalogue");

                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                string filePath = Path.Combine(outputDir, $"UserList_{listNumber}.json");

                var updatedJson = ProcessJsonAndAddRewardType(listJson);

                File.WriteAllText(filePath, updatedJson);

                GenerateSqlFromJson(updatedJson, outputDir);

                string sqlFilePath = Path.Combine(outputDir, "POSTINSTALL.SQL");

                // Get the PS3TitleID from the TextBox
                string ps3TitleID = PS3TitleIDTextBox.Text;

                string rpcs3Dir = Rpcs3Dev_hdd0_DirectoryTextBox.Text;
                if (!string.IsNullOrEmpty(rpcs3Dir))
                {
                    string destinationFilePath = Path.Combine(rpcs3Dir, "game", ps3TitleID, "USRDIR", "OBJECTCATALOGUE", "POSTINSTALL.SQL");

                    // Ensure the directory exists
                    string destinationDir = Path.GetDirectoryName(destinationFilePath);
                    if (!Directory.Exists(destinationDir))
                    {
                        Directory.CreateDirectory(destinationDir);
                    }

                    // Copy the SQL file to the destination directory
                    File.Copy(sqlFilePath, destinationFilePath, true);
                    ShowNotification("Sent to RPCS3!", 2); // Show notification for 5 seconds
                    Console.WriteLine($"Successfully copied POSTINSTALL.SQL to {destinationFilePath}");
                }

                // Rename the POSTINSTALL.SQL file to include the list number after copying to RPCS3 folder
                string renamedSqlFilePath = Path.Combine(outputDir, $"POSTINSTALL_{listNumber}.SQL");
                if (File.Exists(renamedSqlFilePath))
                {
                    File.Delete(renamedSqlFilePath);  // Delete the existing file if it already exists
                }
                if (File.Exists(sqlFilePath))
                {
                    File.Move(sqlFilePath, renamedSqlFilePath);
                }

                Console.WriteLine($"Successfully saved the list to {filePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save the list: {ex.Message}");
            }
        }


        private string ProcessJsonAndAddRewardType(string listJson)
        {
            var folderNames = ExtractFolderNames(listJson);
            var rewardAndCategories = GetRewardTypesAndCategoriesFromXml(folderNames);
            var jsonArray = JArray.Parse(listJson);

            foreach (var item in jsonArray)
            {
                var folderName = item["folderName"]?.ToString();
                if (folderName != null && rewardAndCategories.TryGetValue(folderName, out var rewardAndCategory))
                {
                    item["rewardType"] = rewardAndCategory.RewardType;
                    item["category"] = rewardAndCategory.Category;
                }
            }

            return jsonArray.ToString();
        }


        private List<string> ExtractFolderNames(string listJson)
        {
            var folderNames = new List<string>();
            var jsonArray = JArray.Parse(listJson);

            foreach (var item in jsonArray)
            {
                var folderName = item["folderName"]?.ToString();
                if (!string.IsNullOrEmpty(folderName))
                {
                    folderNames.Add(folderName);
                }
            }

            return folderNames;
        }

        private Dictionary<string, (string RewardType, string Category)> GetRewardTypesAndCategoriesFromXml(List<string> folderNames)
        {
            var rewardAndCategories = new Dictionary<string, (string RewardType, string Category)>();
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string xmlFilePath = Path.Combine(exeDirectory, "dependencies", "psho.me", "categories", "All_Items.xml");

            if (File.Exists(xmlFilePath))
            {
                var lines = File.ReadAllLines(xmlFilePath);

                foreach (var line in lines)
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 9)
                    {
                        var folderName = parts[0].Trim();
                        if (folderNames.Contains(folderName))
                        {
                            var category = parts[5].Trim();  // Assuming the 6th part is the category
                            var rewardType = parts[8].Trim(); // Assuming the 9th part is the reward type
                            rewardAndCategories[folderName] = (rewardType, category);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("All_Items.xml file not found.");
            }

            return rewardAndCategories;
        }

        private void GenerateSqlFromJson(string listJson, string outputDir)
        {
            var jsonArray = JArray.Parse(listJson);
            var premiumUuids = new List<string>();
            var rewardUuids = new List<string>();

            foreach (var item in jsonArray)
            {
                var folderName = item["folderName"]?.ToString();
                var rewardType = item["rewardType"]?.ToString();

                if (!string.IsNullOrEmpty(folderName) && !string.IsNullOrEmpty(rewardType))
                {
                    if (rewardType == "NOT-RWRD")
                    {
                        premiumUuids.Add(folderName);
                    }
                    else if (rewardType == "LUA-REWARD" || rewardType == "AUTO-REWARD")
                    {
                        rewardUuids.Add(folderName);
                    }
                }
            }

            var sqlScript = new StringBuilder();
            sqlScript.AppendLine("CREATE TEMPORARY TABLE TempActives");
            sqlScript.AppendLine("(");
            sqlScript.AppendLine("\tObjectIndex\tINTEGER");
            sqlScript.AppendLine(");");
            sqlScript.AppendLine("INSERT INTO TempActives SELECT ObjectIndex FROM Metadata WHERE KeyName == 'ACTIVE' EXCEPT SELECT ObjectIndex FROM Metadata WHERE KeyName == 'MAIN_HEAT';");
            sqlScript.AppendLine("INSERT INTO Metadata SELECT ObjectIndex, \"MAIN_HEAT\", 22 FROM TempActives;");
            sqlScript.AppendLine("INSERT INTO Metadata SELECT ObjectIndex, \"HOST_HEAT\", 22 FROM TempActives;");
            sqlScript.AppendLine("INSERT INTO Metadata SELECT ObjectIndex, \"VRAM_HEAT\", 22 FROM TempActives;");
            sqlScript.AppendLine("INSERT INTO Metadata SELECT ObjectIndex, \"PPU_HEAT\", 22 FROM TempActives;");
            sqlScript.AppendLine("INSERT INTO Metadata SELECT ObjectIndex, \"NET_HEAT\", 22 FROM TempActives;");
            sqlScript.AppendLine("DROP TABLE TempActives;");

            int entitlementIndex = 1;
            foreach (var uuid in premiumUuids)
            {
                sqlScript.AppendLine($"UPDATE Objects SET EntitlementIndex = (SELECT MAX(EntitlementIndex) FROM Entitlements) + 1, InventoryEntryType = '2', UserLocation = '1' WHERE ObjectId = '{uuid}';");
                entitlementIndex++;
            }
            int rewardIndex = 1;
            foreach (var uuid in rewardUuids)
            {
                sqlScript.AppendLine($"UPDATE Objects SET RewardIndex = (SELECT MAX(RewardIndex) FROM Rewards) + 1, InventoryEntryType = '1', UserLocation = '1' WHERE ObjectId = '{uuid}';");
                rewardIndex++;
            }

            string sqlFilePath = Path.Combine(outputDir, "POSTINSTALL.SQL");
            File.WriteAllText(sqlFilePath, sqlScript.ToString());

            Console.WriteLine($"Successfully generated the SQL script to {sqlFilePath}");
        }



        private void ToggleSwitchOfflineCheckedChanged(object sender, RoutedEventArgs e)
        {
            // Update the setting based on the toggle switch state
            _settings.IsOfflineMode = ToggleSwitchOfflinePshome.IsChecked == true;

            // Save settings to persist the change
            SettingsManager.SaveSettings(_settings);

        }




        public static Dictionary<string, string> SceneFileMappings { get; private set; } = new Dictionary<string, string>();

        private void CdsEncryptOutputDirectoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.CdsEncryptOutputDirectory = CdsEncryptOutputDirectoryTextBox.Text;
                SettingsManager.SaveSettings(_settings);
            }
        }


        private void CdsDecryptOutputDirectoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.CdsDecryptOutputDirectory = CdsDecryptOutputDirectoryTextBox.Text;
                SettingsManager.SaveSettings(_settings);
            }
        }


        private void BarSdatSharcOutputDirectoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null && BarSdatSharcOutputDirectoryTextBox != null)
            {
                _settings.BarSdatSharcOutputDirectory = BarSdatSharcOutputDirectoryTextBox.Text;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void MappedOutputDirectoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null && MappedOutputDirectoryTextBox != null)
            {
                _settings.MappedOutputDirectory = MappedOutputDirectoryTextBox.Text;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void HcdbOutputDirectoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.HcdbOutputDirectory = HcdbOutputDirectoryTextBox.Text;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void SqlOutputDirectoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.SqlOutputDirectory = SqlOutputDirectoryTextBox.Text;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void TicketListOutputDirectoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.TicketListOutputDirectory = TicketListOutputDirectoryTextBox.Text;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void LuacOutputDirectoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.LuacOutputDirectory = LuacOutputDirectoryTextBox.Text;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void LuaOutputDirectoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.LuaOutputDirectory = LuaOutputDirectoryTextBox.Text;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void VideoOutputDirectoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.VideoOutputDirectory = VideoOutputDirectoryTextBox.Text;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void AudioOutputDirectoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.AudioOutputDirectory = AudioOutputDirectoryTextBox.Text;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void Rpcs3Dev_hdd0_DirectoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.RPCS3OutputDirectory = Rpcs3Dev_hdd0_DirectoryTextBox.Text;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void PS3IPforFTPTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.PS3IPforFTP = PS3IPforFTPTextBox.Text;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void PS3TitleIDTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.PS3TitleID = PS3TitleIDTextBox.Text;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void SceneListPathURLtextbox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.SceneListPathURL = SceneListPathURLtextbox2.Text;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void SceneListSavePathtextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.SceneListSavePath = SceneListSavePathtextbox.Text;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void TSSURLtextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.TSSURL = TSSURLtextbox.Text;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void TSSeditorSavePathtextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.TSSeditorSavePath = TSSeditorSavePathtextbox.Text;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void SceneListSavePathtextbox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Create a FolderBrowserDialog to allow the user to select a folder
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog();

            // Show the dialog and check if the user selected a folder
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Set the selected folder path in the TextBox
                SceneListSavePathtextbox.Text = folderDialog.SelectedPath;
            }
        }

        private void TSSeditorSavePathtextbox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Create a FolderBrowserDialog to allow the user to select a folder
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog();

            // Show the dialog and check if the user selected a folder
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Set the selected folder path in the TextBox
                TSSeditorSavePathtextbox.Text = folderDialog.SelectedPath;
            }
        }

        private void TSSURLtextbox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Create an OpenFileDialog to allow the user to select an XML file
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                Title = "Select an XML file"
            };

            // Show the dialog and check if the user selected a file
            if (openFileDialog.ShowDialog() == true)
            {
                // Set the selected file path in the TextBox
                TSSURLtextbox.Text = openFileDialog.FileName;
            }
        }



        private void SceneListPathURLtextbox2_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Create an OpenFileDialog to allow the user to select an XML file
            var openFileDialog = new OpenFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                Title = "Select an XML file"
            };

            // Show the dialog and check if the user selected a file
            if (openFileDialog.ShowDialog() == true)
            {
                // Set the selected file path in the TextBox
                SceneListPathURLtextbox2.Text = openFileDialog.FileName;
            }
        }

        private void Border_ODCMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Create an OpenFileDialog to allow the user to select an .odc or .xml file
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "ODC files (*.odc)|*.odc|XML files (*.xml)|*.xml|All files (*.*)|*.*",
                Title = "Select an ODC or XML file"
            };

            // Show the dialog and check if the user selected a file
            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;

                // Simulate the drop functionality
                LogDebugInfo($"File selected: {selectedFilePath}");
                ProcessDroppedFile(selectedFilePath);
            }
        }

        private void Border_SQLMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Create an OpenFileDialog to allow the user to select an .sql file
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "SQL files (*.sql)|*.sql|All files (*.*)|*.*",
                Title = "Select an SQL file"
            };

            // Show the dialog and check if the user selected a file
            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;
                string fileExtension = Path.GetExtension(selectedFilePath).ToLower();

                // Ensure the selected file is a .sql file
                if (fileExtension == ".sql")
                {
                    HandleSQLFile(selectedFilePath);
                }
                else
                {
                    MessageBox.Show("Only SQL files are supported.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void RadioButtonFileSkip_Checked(object sender, RoutedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.FileOverwriteBehavior = OverwriteBehavior.Skip;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void RadioButtonFileRename_Checked(object sender, RoutedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.FileOverwriteBehavior = OverwriteBehavior.Rename;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void RadioButtonFileOverwrite_Checked(object sender, RoutedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.FileOverwriteBehavior = OverwriteBehavior.Overwrite;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void CheckBoxDebugLogsEnable_Checked(object sender, RoutedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.SaveDebugLogToggle = SaveDebugLog.True;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void CheckBoxDebugLogsEnable_UnChecked(object sender, RoutedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.SaveDebugLogToggle = SaveDebugLog.False;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void RadioButtonArchiveCreatorBAR_Checked(object sender, RoutedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.ArchiveTypeSettingRem = ArchiveTypeSetting.BAR;
                SettingsManager.SaveSettings(_settings);
               
            }
        }

        private void RadioButtonArchiveCreatorBAR_S_Checked(object sender, RoutedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.ArchiveTypeSettingRem = ArchiveTypeSetting.BAR_S;
                SettingsManager.SaveSettings(_settings);
                
            }
        }

        private void RadioButtonArchiveCreatorSDAT_Checked(object sender, RoutedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.ArchiveTypeSettingRem = ArchiveTypeSetting.SDAT;
                SettingsManager.SaveSettings(_settings);
               
            }
        }

        private void RadioButtonArchiveCreatorSDAT_SHARC_Checked(object sender, RoutedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.ArchiveTypeSettingRem = ArchiveTypeSetting.SDAT_SHARC;
                SettingsManager.SaveSettings(_settings);
                
            }
        }

        private void RadioButtonArchiveCreatorCORE_SHARC_Checked(object sender, RoutedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.ArchiveTypeSettingRem = ArchiveTypeSetting.CORE_SHARC;
                SettingsManager.SaveSettings(_settings);
                
            }
        }

        private void RadioButtonArchiveCreatorCONFIG_SHARC_Checked(object sender, RoutedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.ArchiveTypeSettingRem = ArchiveTypeSetting.CONFIG_SHARC;
                SettingsManager.SaveSettings(_settings);
               
            }
        }

      


        private List<string> logBuffer = new List<string>();
        private string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs\\debug.log");
        private System.Threading.Timer logFlushTimer;
        private readonly object logLock = new object();

        private void FlushLogBuffer()
        {
            lock (logLock)
            {
                if (logBuffer.Count > 0)
                {
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(logFilePath, true)) // true to append data to the file
                        {
                            foreach (var message in logBuffer)
                            {
                                sw.WriteLine(message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error writing to log file: " + ex.Message);
                    }
                    logBuffer.Clear();
                }
                logFlushTimer.Change(Timeout.Infinite, Timeout.Infinite); // Stop the timer
            }
        }

        public void LogDebugInfo(string message)
        {
            if (_settings.SaveDebugLogToggle == SaveDebugLog.True)
            {
                lock (logLock)
                {
                    logBuffer.Add($"{message}");

                    if (logBuffer.Count >= 100) // Flush every 10 messages, adjust as needed
                    {
                        FlushLogBuffer();
                    }
                    else
                    {
                        logFlushTimer.Change(1000, Timeout.Infinite); // Reset the timer to 1 second
                    }
                }
            }
            // If SaveDebugLogToggle is False, do nothing
        }


        // Title BAR Controls

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        // Theme Controls

        private void ThemeColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue.HasValue)
            {
                SelectedThemeColor = new SolidColorBrush(e.NewValue.Value);
                _settings.ThemeColor = e.NewValue.Value.ToString();
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void NewColorPicker_ColorChanged(object sender, EventArgs e)
        {
            var colorPicker = sender as StandardColorPicker;
            if (colorPicker != null)
            {
                // Directly use SelectedColor as it is not nullable
                Color newColor = colorPicker.SelectedColor;
                SelectedThemeColor = new SolidColorBrush(newColor);
                _settings.ThemeColor = newColor.ToString();
                SettingsManager.SaveSettings(_settings);

                // Assuming your icon is a resource in the project, otherwise provide a full path
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dependencies\\icon.png");
                UpdateTaskbarIconWithTint(iconPath, newColor);
            }
        }


        private void UpdateTaskbarIconWithTint(string iconPath, System.Windows.Media.Color tint)
        {
            // Load the existing icon
            BitmapImage originalIcon = new BitmapImage(new Uri(iconPath));

            // Create a WriteableBitmap from the original icon
            WriteableBitmap writeableBitmap = new WriteableBitmap(originalIcon);

            // Iterate over all pixels to apply the tint only on non-transparent pixels
            writeableBitmap.Lock();
            unsafe
            {
                // Pointer to the back buffer
                IntPtr buffer = writeableBitmap.BackBuffer;
                int stride = writeableBitmap.BackBufferStride;

                // Tint strength (0-255)
                byte tintStrength = 255; // Maximum strength

                for (int y = 0; y < writeableBitmap.PixelHeight; y++)
                {
                    for (int x = 0; x < writeableBitmap.PixelWidth; x++)
                    {
                        // Calculate the pixel's position
                        int position = y * stride + x * 4;

                        // Apply tint only to non-transparent pixels
                        byte* pixel = (byte*)buffer.ToPointer() + position;
                        byte alpha = pixel[3];
                        if (alpha > 0) // This checks if the pixel is not fully transparent
                        {
                            pixel[0] = (byte)((pixel[0] * (255 - tintStrength) + tint.B * tintStrength) / 255);
                            pixel[1] = (byte)((pixel[1] * (255 - tintStrength) + tint.G * tintStrength) / 255);
                            pixel[2] = (byte)((pixel[2] * (255 - tintStrength) + tint.R * tintStrength) / 255);
                        }
                    }
                }
            }
            writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight));
            writeableBitmap.Unlock();

            // Use the WriteableBitmap as the window's icon
            this.Icon = writeableBitmap;
        }

        private ImageSource BitmapToImageSource(RenderTargetBitmap bitmapSource)
        {
            // Convert the bitmap source to a PNG byte array
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            MemoryStream stream = new MemoryStream();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(stream);
            stream.Position = 0;

            // Create a new BitmapImage from the PNG byte array
            BitmapImage bImg = new BitmapImage();
            bImg.BeginInit();
            bImg.StreamSource = stream;
            bImg.EndInit();

            return bImg;
        }

        private void ClearListHandler(object sender, RoutedEventArgs e)
        {
            var textBox = GetAssociatedTextBox(sender as FrameworkElement);
            if (textBox != null)
            {
                textBox.Clear();

                // Derive the TextBlock name from the TextBox name
                string textBlockName = textBox.Name.Replace("TextBox", "DragAreaText");

                // Find the TextBlock in the current context
                var textBlock = this.FindName(textBlockName) as TextBlock;
                if (textBlock != null)
                {
                    // Show the temporary message
                    TemporaryMessageHelper.ShowTemporaryMessage(textBlock, "List Cleared", 500); // 1000 milliseconds = 1 second
                }
            }
        }

        private TextBox? GetAssociatedTextBox(FrameworkElement? element)
        {
            if (element == null) return null;
            var baseName = element.Name;
            var suffixes = new string[] { "DragArea", "ClearButton" };
            foreach (var suffix in suffixes)
            {
                if (baseName.EndsWith(suffix))
                {
                    baseName = baseName.Substring(0, baseName.Length - suffix.Length);
                    break;
                }
            }

            var textBoxName = baseName + "TextBox";
            var textBox = this.FindName(textBoxName) as TextBox;

            if (textBox == null)
            {
                throw new InvalidOperationException($"A TextBox with name '{textBoxName}' could not be found.");
            }

            return textBox;
        }

        public static class TemporaryMessageHelper
        {
            private static Dictionary<TextBlock, (DispatcherTimer Timer, string OriginalText)> messageTimers = new Dictionary<TextBlock, (DispatcherTimer, string)>();

            public static void ShowTemporaryMessage(TextBlock textBlock, string message, int displayTimeInMilliseconds)
            {
                if (messageTimers.ContainsKey(textBlock))
                {
                    var (timer, _) = messageTimers[textBlock];
                    timer.Stop();  // Stop the existing timer if it's running
                }
                else
                {
                    var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(displayTimeInMilliseconds) };
                    timer.Tick += (sender, args) =>
                    {
                        timer.Stop();
                        textBlock.Text = messageTimers[textBlock].OriginalText;
                    };
                    messageTimers[textBlock] = (timer, textBlock.Text); // Save the original text and timer
                }

                textBlock.Text = message;
                messageTimers[textBlock].Timer.Start();
            }
        }



        // TAB 1: Logic for Archive Creation

        private async void ArchiveCreatorExecuteButtonClick(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("Archive Creation: Process Initiated");
            TemporaryMessageHelper.ShowTemporaryMessage(ArchiveCreatorDragAreaText, "Processing....", 1000000);

            // Check and create output directory if it doesn't exist
            if (!Directory.Exists(_settings.BarSdatSharcOutputDirectory))
            {
                Directory.CreateDirectory(_settings.BarSdatSharcOutputDirectory);
                LogDebugInfo($"Archive Creation: Output directory created at {_settings.BarSdatSharcOutputDirectory}");
            }

            string itemsToArchive = ArchiveCreatorTextBox.Text;
            if (!string.IsNullOrWhiteSpace(itemsToArchive))
            {
                string[] itemsPaths = itemsToArchive.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                LogDebugInfo($"Archive Creation: Starting creation for {itemsPaths.Length} items");
                bool archiveCreationSuccess = await CreateArchiveAsync(itemsPaths, _settings.ArchiveTypeSettingRem);

                string message = archiveCreationSuccess ? "Success: Archives Created" : "Archive Creation Failed";
                TemporaryMessageHelper.ShowTemporaryMessage(ArchiveCreatorDragAreaText, message, 2000);

                LogDebugInfo($"Archive Creation: Result - {message}");
            }
            else
            {
                LogDebugInfo("Archive Creation: Aborted - No items listed for Archive Creation.");
                TemporaryMessageHelper.ShowTemporaryMessage(ArchiveCreatorDragAreaText, "No items listed for Archive Creation.", 2000);
            }
        }



        private void ArchiveCreatorDragDropHandler(object sender, DragEventArgs e)
        {
            LogDebugInfo("Archive Creation: Drag and Drop Initiated");
            TemporaryMessageHelper.ShowTemporaryMessage(ArchiveCreatorDragAreaText, "Processing....", 1000000);

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedItems = (string[])e.Data.GetData(DataFormats.FileDrop);
                List<string> itemsToAdd = new List<string>();

                foreach (var item in droppedItems)
                {
                    LogDebugInfo($"Processing item: {item}");

                    if (Directory.Exists(item))
                    {
                        string itemWithTrailingSlash = item.EndsWith(Path.DirectorySeparatorChar.ToString()) ? item : item + Path.DirectorySeparatorChar;
                        itemsToAdd.Add(itemWithTrailingSlash);
                        LogDebugInfo($"Directory added with trailing slash: {itemWithTrailingSlash}");
                    }
                    else if (File.Exists(item) && Path.GetExtension(item).ToLowerInvariant() == ".zip")
                    {
                        itemsToAdd.Add(item);
                        LogDebugInfo($"ZIP file added: {item}");
                    }
                }

                if (itemsToAdd.Count > 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var existingItemsSet = new HashSet<string>(ArchiveCreatorTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        int initialCount = existingItemsSet.Count;

                        existingItemsSet.UnionWith(itemsToAdd);
                        int newItemsCount = existingItemsSet.Count - initialCount;
                        int duplicatesCount = itemsToAdd.Count - newItemsCount;

                        ArchiveCreatorTextBox.Text = string.Join(Environment.NewLine, existingItemsSet);

                        string message = $"{newItemsCount} item(s) added";
                        if (duplicatesCount > 0)
                        {
                            message += $", {duplicatesCount} duplicate(s) filtered";
                        }
                        TemporaryMessageHelper.ShowTemporaryMessage(ArchiveCreatorDragAreaText, message, 2000);

                        LogDebugInfo($"Archive Creation: {newItemsCount} items added, {duplicatesCount} duplicates filtered from Drag and Drop.");
                    });
                }
                else
                {
                    LogDebugInfo("Archive Creation: Drag and Drop - No valid ZIP files or folders found.");
                    TemporaryMessageHelper.ShowTemporaryMessage(ArchiveCreatorDragAreaText, "No valid ZIP files or folders found.", 2000);
                }
            }
            else
            {
                LogDebugInfo("Archive Creation: Drag and Drop - No Data Present.");
                TemporaryMessageHelper.ShowTemporaryMessage(ArchiveCreatorDragAreaText, "Drag and Drop operation failed - No Data Present.", 2000);
            }
        }



        private async void ClickToBrowseArchiveCreatorHandler(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("Archive Creation: Browsing for files Initiated");

            // Disable the main window to prevent interactions while the dialog is open
            this.IsEnabled = false;

            var openFileDialog = new VistaOpenFileDialog
            {
                Filter = "ZIP files (*.zip)|*.zip",
                Multiselect = true
            };

            string message = "No items selected.";
            int displayTime = 2000; // Default to 2 seconds for the message display.

            bool? result = openFileDialog.ShowDialog();

            // Await a delay to absorb any unintentional clicks that happen after the dialog closes
            await Task.Delay(10); // 500 ms delay

            // Re-enable the main window after the delay
            this.IsEnabled = true;

            if (result == true)
            {
                var selectedItems = openFileDialog.FileNames;

                if (selectedItems.Any())
                {
                    LogDebugInfo($"Archive Creation: {selectedItems.Length} items selected via File Browser.");

                    Dispatcher.Invoke(() =>
                    {
                        var existingItemsSet = new HashSet<string>(ArchiveCreatorTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        existingItemsSet.UnionWith(selectedItems); // Adds selected zip files, removes duplicates
                        ArchiveCreatorTextBox.Text = string.Join(Environment.NewLine, existingItemsSet);

                        message = $"{selectedItems.Length} items added to the list";
                        displayTime = 1000; // Change display time since items were added
                    });
                }
                else
                {
                    LogDebugInfo("Archive Creation: File Browser - No ZIP files were selected.");
                    message = "No ZIP files selected.";
                }
            }
            else
            {
                LogDebugInfo("Archive Creation: File Browser - Dialog Cancelled.");
            }

            TemporaryMessageHelper.ShowTemporaryMessage(ArchiveCreatorDragAreaText, message, displayTime);
        }



        private void CheckBoxArchiveMapperFAST_Checked(object sender, RoutedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.ArchiveMapperSettingRem = ArchiveMapperSetting.FAST;
                SettingsManager.SaveSettings(_settings);
            }
            // Uncheck the CORE CheckBox if it's checked

            CheckBoxArchiveMapperEXP.IsChecked = false;
        }

        private void CheckBoxArchiveMapperFAST_Unchecked(object sender, RoutedEventArgs e)
        {

            {
                _settings.ArchiveMapperSettingRem = ArchiveMapperSetting.NORM;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void CheckBoxArchiveMapperEXP_Checked(object sender, RoutedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.ArchiveMapperSettingRem = ArchiveMapperSetting.EXP;
                SettingsManager.SaveSettings(_settings);
            }
            // Uncheck the SLOW CheckBox if it's checked
            CheckBoxArchiveMapperFAST.IsChecked = false;

        }

        private void CheckBoxArchiveMapperEXP_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_settings != null && !CheckBoxArchiveMapperFAST.IsChecked.Value)
            {
                _settings.ArchiveMapperSettingRem = ArchiveMapperSetting.NORM;
                SettingsManager.SaveSettings(_settings);
            }
        }




        private async Task<bool> CreateArchiveAsync(string[] itemPaths, ArchiveTypeSetting type)
        {
            // Check if the "Ignore timestamp.txt" checkbox is enabled and delete the timestamp.txt file if it exists
            if (CheckBoxArchiveCreatorDeleteTimestamp.IsChecked == true)
            {
                foreach (string itemPath in itemPaths)
                {
                    string directoryPath = Path.GetDirectoryName(itemPath);
                    if (directoryPath != null)
                    {
                        string timestampFilePath = Path.Combine(directoryPath, "timestamp.txt");
                        if (File.Exists(timestampFilePath))
                        {
                            File.Delete(timestampFilePath);
                            LogDebugInfo($"timestamp.txt file ignored in directory: {directoryPath}");
                            await Dispatcher.InvokeAsync(() =>
                            {
                                ArchiveCreatorTextBox.Text += $"{Environment.NewLine}timestamp.txt file ignored in directory: {directoryPath}";
                                ArchiveCreatorTextBox.ScrollToEnd();
                            });
                        }
                    }
                }
            }

            // Filter out lines starting with "Archive:" and log the start of the archive creation process
            var validItemPaths = itemPaths.Where(p => !p.Trim().StartsWith("Archive Creator:")).ToArray();
            LogDebugInfo($"Archive Creation: Beginning Archive Creation for {validItemPaths.Length} items with type {type}.");
            await Dispatcher.InvokeAsync(() =>
            {
                ArchiveCreatorTextBox.Text += $"{Environment.NewLine}Archive Creator: Beginning Archive Creation for {validItemPaths.Length} items with type {type}.";
                ArchiveCreatorTextBox.ScrollToEnd();
            });

            bool allItemsProcessed = true;
            int i = 0;

            foreach (string itemPath in validItemPaths)
            {
                try
                {
                    LogDebugInfo($"Archive Creation: Processing item {i + 1}: {itemPath}");
                    await Dispatcher.InvokeAsync(() =>
                    {
                        ArchiveCreatorTextBox.Text += $"{Environment.NewLine}Archive Creator: Processing item {i + 1}: {itemPath}";
                        ArchiveCreatorTextBox.ScrollToEnd();
                    });

                    if (itemPath.ToLower().EndsWith(".zip"))
                    {
                        string filename = Path.GetFileNameWithoutExtension(itemPath);
                        LogDebugInfo($"Archive Creation: Processing item {i + 1}: Extracting ZIP: {filename}");

                        await Dispatcher.InvokeAsync(() =>
                        {
                            ArchiveCreatorTextBox.Text += $"{Environment.NewLine}Archive Creator: Processing item {i + 1}: Extracting ZIP: {filename}";
                            ArchiveCreatorTextBox.ScrollToEnd();
                        });

                        // Combine the temporary folder path with the unique folder name
                        string temppath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                        LogDebugInfo($"Archive Creation: Processing item {i + 1}: Temporary extraction path: {temppath}");

                        await Dispatcher.InvokeAsync(() =>
                        {
                            ArchiveCreatorTextBox.Text += $"{Environment.NewLine}Archive Creator: Processing item {i + 1}: Temporary extraction path: {temppath}";
                            ArchiveCreatorTextBox.ScrollToEnd();
                        });

                        UncompressFile(itemPath, temppath);

                        bool sdat = false;
                        IEnumerable<string> enumerable = Directory.EnumerateFiles(temppath, "*.*", SearchOption.AllDirectories);
                        BARArchive? bararchive = null;

                        // Declare the fileExtension variable
                        string fileExtension = "";

                        switch (_settings.ArchiveTypeSettingRem)
                        {
                            case ArchiveTypeSetting.BAR:
                                bararchive = new BARArchive($"{_settings.BarSdatSharcOutputDirectory}/{filename}.BAR", temppath, (int)CdnMode.RETAIL, Convert.ToInt32(ArchiveCreatorTimestampTextBox.Text, 16), false, true);
                                fileExtension = ".BAR"; // Set file extension
                                break;
                            case ArchiveTypeSetting.BAR_S:
                                bararchive = new BARArchive($"{_settings.BarSdatSharcOutputDirectory}/{filename}.bar", temppath, (int)CdnMode.RETAIL, Convert.ToInt32(ArchiveCreatorTimestampTextBox.Text, 16), true, true);
                                fileExtension = ".bar"; // Set file extension
                                break;
                            case ArchiveTypeSetting.SDAT:
                                bararchive = new BARArchive($"{_settings.BarSdatSharcOutputDirectory}/{filename}.BAR", temppath, (int)CdnMode.RETAIL, Convert.ToInt32(ArchiveCreatorTimestampTextBox.Text, 16), false, true);
                                sdat = true;
                                fileExtension = ".sdat";  // Set file extension
                                break;
                            case ArchiveTypeSetting.CORE_SHARC:
                                bararchive = new BARArchive($"{_settings.BarSdatSharcOutputDirectory}/{filename}.SHARC", temppath, (int)CdnMode.RETAIL, Convert.ToInt32(ArchiveCreatorTimestampTextBox.Text, 16), true, true, ToolsImplementation.base64DefaultSharcKey);
                                fileExtension = ".SHARC"; // Set file extension
                                break;
                            case ArchiveTypeSetting.SDAT_SHARC:
                                sdat = true;
                                bararchive = new BARArchive($"{_settings.BarSdatSharcOutputDirectory}/{filename}.SHARC", temppath, (int)CdnMode.RETAIL, Convert.ToInt32(ArchiveCreatorTimestampTextBox.Text, 16), true, true, ToolsImplementation.base64CDNKey2);
                                fileExtension = ".sdat"; // Set file extension
                                break;
                            case ArchiveTypeSetting.CONFIG_SHARC:
                                bararchive = new BARArchive($"{_settings.BarSdatSharcOutputDirectory}/{filename}.sharc", temppath, (int)CdnMode.RETAIL, Convert.ToInt32(ArchiveCreatorTimestampTextBox.Text, 16), true, true, ToolsImplementation.base64CDNKey2);
                                fileExtension = ".sharc"; // Set file extension
                                break;
                        }

                        if (File.Exists(temppath + "/timestamp.txt"))
                            bararchive.BARHeader.UserData = Convert.ToInt32(File.ReadAllText(temppath + "/timestamp.txt"), 16);

                        bararchive.AllowWhitespaceInFilenames = true;

                        foreach (string path in enumerable)
                        {
                            var fullPath = Path.Combine(temppath, path);
                            bararchive.AddFile(fullPath);
                            LogDebugInfo($"Archive Creation: Processing item {i + 1}: Added file to archive: {fullPath}");
                        }

                        // Get the name of the directory
                        string directoryName = new DirectoryInfo(temppath).Name;
                        LogDebugInfo($"Archive Creation: Processing item {i + 1}: Processing directory: {directoryName}");

                        // Create a text file to write the paths to
                        StreamWriter writer = new(temppath + @"/files.txt");
                        LogDebugInfo($"Archive Creation: Processing item {i + 1}: Creating file list text file at: {temppath}files.txt");

                        // Get all files in the directory and its immediate subdirectories
                        string[] files = Directory.GetFiles(temppath, "*.*", SearchOption.AllDirectories);

                        // Loop through the files and write their paths to the text file
                        foreach (string file in files)
                        {
                            string relativePath = $"file=\"{file.Replace(temppath, "").TrimStart(Path.DirectorySeparatorChar)}\"";
                            writer.WriteLine(relativePath.Replace(@"\", "/"));
                            LogDebugInfo($"Archive Creation: Processing item {i + 1}: Writing file path to text: {relativePath.Replace(@"\", "/")}");
                        }

                        LogDebugInfo("Archive Creation: Completed writing file paths to text file.");

                        writer.Close();

                        bararchive.AddFile(temppath + @"/files.txt");

                        bararchive.CreateManifest();

                        bararchive.Save();

                        bararchive = null;

                        if (sdat && File.Exists(_settings.BarSdatSharcOutputDirectory + $"/{filename}.SHARC"))
                        {
                            LogDebugInfo($"Archive Creation: Starting SDAT encryption for SHARC file: {filename}.SHARC");
                            RunUnBAR.RunEncrypt(_settings.BarSdatSharcOutputDirectory + $"/{filename}.SHARC", _settings.BarSdatSharcOutputDirectory + $"/{filename}.sdat");
                            File.Delete(_settings.BarSdatSharcOutputDirectory + $"/{filename}.SHARC");
                            LogDebugInfo($"Archive Creation: SDAT encryption completed and original SHARC file deleted for: {filename}.SHARC");
                        }
                        else if (sdat && File.Exists(_settings.BarSdatSharcOutputDirectory + $"/{filename}.BAR"))
                        {
                            LogDebugInfo($"Archive Creation: Starting SDAT encryption for BAR file: {filename}.BAR");
                            RunUnBAR.RunEncrypt(_settings.BarSdatSharcOutputDirectory + $"/{filename}.BAR", _settings.BarSdatSharcOutputDirectory + $"/{filename}.sdat");
                            File.Delete(_settings.BarSdatSharcOutputDirectory + $"/{filename}.BAR");
                            LogDebugInfo($"Archive Creation: SDAT encryption completed and original BAR file deleted for: {filename}.BAR");
                        }

                        // Log to the textbox after creating the archive
                        await Dispatcher.InvokeAsync(() =>
                        {
                            ArchiveCreatorTextBox.Text += $"{Environment.NewLine}Archive Creator: Success {filename}{fileExtension} Created.";
                            ArchiveCreatorTextBox.ScrollToEnd();
                        });

                        // Allow UI to process its queue
                        await Task.Delay(50);
                    }
                    else
                    {
                        string? folderPath = Path.GetDirectoryName(itemPath);
                        string? filename = Path.GetFileName(folderPath);

                        bool sdat = false;
                        IEnumerable<string> enumerable = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories);
                        BARArchive? bararchive = null;

                        string fileExtension = "";

                        switch (_settings.ArchiveTypeSettingRem)
                        {
                            case ArchiveTypeSetting.BAR:
                                bararchive = new BARArchive($"{_settings.BarSdatSharcOutputDirectory}/{filename}.BAR", folderPath, (int)CdnMode.RETAIL, Convert.ToInt32(ArchiveCreatorTimestampTextBox.Text, 16), false, true);
                                fileExtension = ".BAR"; // Set the file extension
                                break;
                            case ArchiveTypeSetting.BAR_S:
                                bararchive = new BARArchive($"{_settings.BarSdatSharcOutputDirectory}/{filename}.bar", folderPath, (int)CdnMode.RETAIL, Convert.ToInt32(ArchiveCreatorTimestampTextBox.Text, 16), true, true);
                                fileExtension = ".bar"; // Set the file extension
                                break;
                            case ArchiveTypeSetting.SDAT:
                                bararchive = new BARArchive($"{_settings.BarSdatSharcOutputDirectory}/{filename}.BAR", folderPath, (int)CdnMode.RETAIL, Convert.ToInt32(ArchiveCreatorTimestampTextBox.Text, 16), false, true);
                                sdat = true;
                                fileExtension = ".sdat"; // Set the file extension
                                break;
                            case ArchiveTypeSetting.CORE_SHARC:
                                bararchive = new BARArchive($"{_settings.BarSdatSharcOutputDirectory}/{filename}.SHARC", folderPath, (int)CdnMode.RETAIL, Convert.ToInt32(ArchiveCreatorTimestampTextBox.Text, 16), true, true, ToolsImplementation.base64DefaultSharcKey);
                                fileExtension = ".SHARC"; // Set the file extension
                                break;
                            case ArchiveTypeSetting.SDAT_SHARC:
                                sdat = true;
                                bararchive = new BARArchive($"{_settings.BarSdatSharcOutputDirectory}/{filename}.SHARC", folderPath, (int)CdnMode.RETAIL, Convert.ToInt32(ArchiveCreatorTimestampTextBox.Text, 16), true, true, ToolsImplementation.base64CDNKey2);
                                fileExtension = ".sdat"; // Set the file extension
                                break;
                            case ArchiveTypeSetting.CONFIG_SHARC:
                                bararchive = new BARArchive($"{_settings.BarSdatSharcOutputDirectory}/{filename}.sharc", folderPath, (int)CdnMode.RETAIL, Convert.ToInt32(ArchiveCreatorTimestampTextBox.Text, 16), true, true, ToolsImplementation.base64CDNKey2);
                                fileExtension = ".sharc"; // Set the file extension
                                break;
                        }

                        if (File.Exists(folderPath + "/timestamp.txt"))
                            bararchive.BARHeader.UserData = Convert.ToInt32(File.ReadAllText(folderPath + "/timestamp.txt"), 16);

                        bararchive.AllowWhitespaceInFilenames = true;

                        foreach (string path in enumerable)
                        {
                            var fullPath = Path.Combine(folderPath, path);
                            bararchive.AddFile(fullPath);
                            LogDebugInfo($"Archive Creation: Processing item {i + 1}: Added file to archive from directory: {fullPath}");
                        }

                        // Get the name of the directory
                        string directoryName = new DirectoryInfo(folderPath).Name;
                        LogDebugInfo($"Archive Creation: Processing item {i + 1}: Processing directory into archive: {directoryName}");

                        // Create a text file to write the paths to
                        StreamWriter writer = new(folderPath + @"/files.txt");
                        LogDebugInfo($"Archive Creation: Processing item {i + 1}: Creating list of files at: {folderPath}files.txt for archive manifest.");

                        // Get all files in the directory and its immediate subdirectories
                        string[] files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);

                        // Loop through the files and write their paths to the text file
                        foreach (string file in files)
                        {
                            string relativePath = $"file=\"{file.Replace(folderPath, "").TrimStart(Path.DirectorySeparatorChar)}\"";
                            writer.WriteLine(relativePath.Replace(@"\", "/"));
                            LogDebugInfo($"Archive Creation: Processing item {i + 1}: Logging file path for archive manifest: {relativePath.Replace(@"\", "/")}");
                        }

                        writer.Close();
                        LogDebugInfo($"Archive Creation: Processing item {i + 1}: File list for archive manifest completed and file closed.");

                        bararchive.AddFile(folderPath + @"/files.txt");
                        LogDebugInfo($"Archive Creation: Processing item {i + 1}: Added file list to archive: {folderPath}files.txt");

                        bararchive.CreateManifest();
                        LogDebugInfo("Archive Creation: Manifest created for archive.");

                        bararchive.Save();

                        bararchive = null;
                        LogDebugInfo($"Archive Creation: New Archive Saved at: {_settings.BarSdatSharcOutputDirectory}\\{filename}{fileExtension}.");

                        if (sdat && File.Exists(_settings.BarSdatSharcOutputDirectory + $"/{filename}.SHARC"))
                        {
                            RunUnBAR.RunEncrypt(_settings.BarSdatSharcOutputDirectory + $"/{filename}.SHARC", _settings.BarSdatSharcOutputDirectory + $"/{filename}.sdat");
                            File.Delete(_settings.BarSdatSharcOutputDirectory + $"/{filename}.SHARC");
                            LogDebugInfo($"Archive Creation: SDAT encryption completed and original SHARC file deleted for: {filename}.SHARC");
                        }
                        else if (sdat && File.Exists(_settings.BarSdatSharcOutputDirectory + $"/{filename}.BAR"))
                        {
                            RunUnBAR.RunEncrypt(_settings.BarSdatSharcOutputDirectory + $"/{filename}.BAR", _settings.BarSdatSharcOutputDirectory + $"/{filename}.sdat");
                            File.Delete(_settings.BarSdatSharcOutputDirectory + $"/{filename}.BAR");
                            LogDebugInfo($"Archive Creation: SDAT encryption completed and original BAR file deleted for: {filename}.BAR");
                        }

                        // Log to the textbox after creating the archive
                        await Dispatcher.InvokeAsync(() =>
                        {
                            ArchiveCreatorTextBox.Text += $"{Environment.NewLine}Archive Creator: Success {filename}{fileExtension} Created.";
                            ArchiveCreatorTextBox.ScrollToEnd();
                        });

                        // Allow UI to process its queue
                        await Task.Delay(50);
                    }

                    i++;
                }
                catch (Exception ex)
                {
                    LogDebugInfo($"Archive Creation: Error processing item {i + 1}: {itemPath}, {ex.Message}");
                    await Dispatcher.InvokeAsync(() =>
                    {
                        ArchiveCreatorTextBox.Text += $"{Environment.NewLine}Archive Creator: Error processing item {i + 1}: {itemPath}, {ex.Message}";
                        ArchiveCreatorTextBox.ScrollToEnd();
                    });
                    allItemsProcessed = false;
                }
            }

            // Check if renaming is enabled
            if (CheckBoxArchiveCreatorRenameCDN.IsChecked == true &&
                (RadioButtonArchiveCreatorBAR.IsChecked == true || RadioButtonArchiveCreatorSDAT.IsChecked == true || RadioButtonArchiveCreatorSDAT_SHARC.IsChecked == true))
            {
                LogDebugInfo("Rename for CDN is enabled.");

                // Get all files in the output directory
                var outputFiles = Directory.GetFiles(_settings.BarSdatSharcOutputDirectory);

                foreach (var outputFilePath in outputFiles)
                {
                    string outputFileName = Path.GetFileName(outputFilePath);
                    LogDebugInfo($"Processing file: {outputFileName}");

                    // Delete .map files found in the root directory
                    if (Path.GetExtension(outputFileName).Equals(".map", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            File.Delete(outputFilePath);
                            LogDebugInfo($"Deleted .map file: {outputFileName}");
                            await Dispatcher.InvokeAsync(() =>

                            {
                                ArchiveCreatorTextBox.Text += $"{Environment.NewLine}Archive Creator: Deleted {outputFileName}";
                                ArchiveCreatorTextBox.ScrollToEnd();
                            });
                        }
                        catch (Exception ex)
                        {
                            LogDebugInfo($"Failed to delete .map file: {outputFileName}, {ex.Message}");
                            await Dispatcher.InvokeAsync(() =>
                            {
                                ArchiveCreatorTextBox.Text += $"{Environment.NewLine}Archive Creator: Failed to delete {outputFileName}: {ex.Message}";
                                ArchiveCreatorTextBox.ScrollToEnd();
                            });
                        }

                        continue; // Skip further processing for this file
                    }

                    string pattern = @"^[A-F0-9]{8}-[A-F0-9]{8}-[A-F0-9]{8}-[A-F0-9]{8}_T\d{3}\..+$";

                    LogDebugInfo($"Checking if file matches pattern: {pattern}");

                    if (Regex.IsMatch(outputFileName, pattern, RegexOptions.IgnoreCase))
                    {
                        LogDebugInfo($"File matches pattern: {outputFileName}");
                        string newFileName = Regex.Replace(outputFileName, @"(_T\d{3})(\..+)$", "/object$1$2", RegexOptions.IgnoreCase);

                        // Remove '_T000' from the file name if present
                        newFileName = Regex.Replace(newFileName, @"_T000(?=\..+$)", "", RegexOptions.IgnoreCase);

                        string newFilePath = Path.Combine(_settings.BarSdatSharcOutputDirectory, "Objects", newFileName);

                        // Ensure the target directory exists
                        Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));

                        // Ensure unique filename
                        int counter = 1;
                        string uniqueFilePath = newFilePath;
                        while (File.Exists(uniqueFilePath))
                        {
                            uniqueFilePath = Path.Combine(Path.GetDirectoryName(newFilePath), $"{Path.GetFileNameWithoutExtension(newFilePath)}({counter}){Path.GetExtension(newFilePath)}");
                            counter++;
                        }

                        // Additional debug check to avoid incorrect naming
                        while (File.Exists(uniqueFilePath))
                        {
                            // Log each step of the filename modification
                            LogDebugInfo($"File exists, generating new filename to avoid conflict: {uniqueFilePath}");

                            uniqueFilePath = Path.Combine(Path.GetDirectoryName(newFilePath), $"{Path.GetFileNameWithoutExtension(newFilePath)}({counter}){Path.GetExtension(newFilePath)}");
                            counter++;
                        }

                        try
                        {
                            File.Move(outputFilePath, uniqueFilePath);
                            LogDebugInfo($"Renamed file: {outputFileName} to {Path.GetFileName(uniqueFilePath)}");
                            await Dispatcher.InvokeAsync(() =>
                            {
                                ArchiveCreatorTextBox.Text += $"{Environment.NewLine}Archive Creator: Renamed {outputFileName} to {Path.GetFileName(uniqueFilePath)}";
                                ArchiveCreatorTextBox.ScrollToEnd();
                            });
                        }
                        catch (Exception ex)
                        {
                            LogDebugInfo($"Failed to rename file: {outputFileName} to {Path.GetFileName(uniqueFilePath)}, {ex.Message}");
                            await Dispatcher.InvokeAsync(() =>
                            {
                                ArchiveCreatorTextBox.Text += $"{Environment.NewLine}Archive Creator: Failed to rename {outputFileName} to {Path.GetFileName(uniqueFilePath)}: {ex.Message}";
                                ArchiveCreatorTextBox.ScrollToEnd();
                            });
                        }
                    }
                    else
                    {
                        LogDebugInfo($"File does not match pattern: {outputFileName}");

                        // Check for a $ in the filename if it doesn't match the pattern
                        if (outputFileName.Contains("$"))
                        {
                            string newFileName = outputFileName.Replace("$", "\\");

                            string newFilePath = Path.Combine(_settings.BarSdatSharcOutputDirectory, "Scenes", newFileName);

                            // Ensure the target directory exists
                            Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));

                            // Ensure unique filename
                            int counter = 1;
                            string uniqueFilePath = newFilePath;
                            while (File.Exists(uniqueFilePath))
                            {
                                uniqueFilePath = Path.Combine(Path.GetDirectoryName(newFilePath), $"{Path.GetFileNameWithoutExtension(newFilePath)}({counter}){Path.GetExtension(newFilePath)}");
                                counter++;
                            }

                            try
                            {
                                File.Move(outputFilePath, uniqueFilePath);
                                LogDebugInfo($"Moved file: {outputFileName} to {Path.GetFileName(uniqueFilePath)}");
                                await Dispatcher.InvokeAsync(() =>
                                {
                                    ArchiveCreatorTextBox.Text += $"{Environment.NewLine}Archive Creator: Moved {outputFileName} to {Path.GetFileName(uniqueFilePath)}";
                                    ArchiveCreatorTextBox.ScrollToEnd();
                                });
                            }
                            catch (Exception ex)
                            {
                                LogDebugInfo($"Failed to move file: {outputFileName} to {Path.GetFileName(uniqueFilePath)}, {ex.Message}");
                                await Dispatcher.InvokeAsync(() =>
                                {
                                    ArchiveCreatorTextBox.Text += $"{Environment.NewLine}Archive Creator: Failed to move {outputFileName} to {Path.GetFileName(uniqueFilePath)}: {ex.Message}";
                                    ArchiveCreatorTextBox.ScrollToEnd();
                                });
                            }
                        }
                    }
                }
            }

            if (CheckBoxArchiveCreatorRenameLocal.IsChecked == true &&
                (RadioButtonArchiveCreatorBAR.IsChecked == true || RadioButtonArchiveCreatorCORE_SHARC.IsChecked == true))
            {
                var outputFiles = Directory.GetFiles(_settings.BarSdatSharcOutputDirectory);

                foreach (var outputFilePath in outputFiles)
                {
                    string outputFileName = Path.GetFileName(outputFilePath);

                    // Delete .map files found in the root directory
                    if (Path.GetExtension(outputFileName).Equals(".map", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            File.Delete(outputFilePath);
                            LogDebugInfo($"Archive Creation: Deleted {outputFileName}");
                            await Dispatcher.InvokeAsync(() =>
                            {
                                ArchiveCreatorTextBox.Text += $"{Environment.NewLine}Archive Creator: Deleted {outputFileName}";
                                ArchiveCreatorTextBox.ScrollToEnd();
                            });
                        }
                        catch (Exception ex)
                        {
                            LogDebugInfo($"Archive Creation: Failed to delete {outputFileName}: {ex.Message}");
                            await Dispatcher.InvokeAsync(() =>
                            {
                                ArchiveCreatorTextBox.Text += $"{Environment.NewLine}Archive Creator: Failed to delete {outputFileName}: {ex.Message}";
                                ArchiveCreatorTextBox.ScrollToEnd();
                            });
                        }

                        continue; // Skip further processing for this file
                    }

                    string pattern = @"^[A-F0-9]{8}-[A-F0-9]{8}-[A-F0-9]{8}-[A-F0-9]{8}_T\d{3}\..+$";

                    if (Regex.IsMatch(outputFileName, pattern, RegexOptions.IgnoreCase))
                    {
                        string uuid = Regex.Match(outputFileName, @"^[A-F0-9]{8}-[A-F0-9]{8}-[A-F0-9]{8}-[A-F0-9]{8}").Value;

                        // Create the new folder path with the parent OBJECTS folder
                        string newFolder = Path.Combine(_settings.BarSdatSharcOutputDirectory, "OBJECTS", uuid);

                        // Ensure the target directory exists
                        Directory.CreateDirectory(newFolder);

                        // Define the new file path
                        string newFilePath = Path.Combine(newFolder, $"{uuid}{Path.GetExtension(outputFileName)}");

                        // Ensure unique filename
                        int counter = 1;
                        string uniqueFilePath = newFilePath;
                        while (File.Exists(uniqueFilePath))
                        {
                            uniqueFilePath = Path.Combine(newFolder, $"{uuid}({counter}){Path.GetExtension(outputFileName)}");
                            counter++;
                        }

                        try
                        {
                            File.Move(outputFilePath, uniqueFilePath);
                            LogDebugInfo($"Archive Creation: Renamed {outputFileName} to {Path.GetFileName(uniqueFilePath)}");
                            await Dispatcher.InvokeAsync(() =>
                            {
                                ArchiveCreatorTextBox.Text += $"{Environment.NewLine}Archive Creator: Renamed {outputFileName} to {Path.GetFileName(uniqueFilePath)}";
                                ArchiveCreatorTextBox.ScrollToEnd();
                            });
                        }
                        catch (Exception ex)
                        {
                            LogDebugInfo($"Archive Creation: Failed to rename {outputFileName} to {Path.GetFileName(uniqueFilePath)}: {ex.Message}");
                            await Dispatcher.InvokeAsync(() =>
                            {
                                ArchiveCreatorTextBox.Text += $"{Environment.NewLine}Archive Creator: Failed to rename {outputFileName} to {Path.GetFileName(uniqueFilePath)}: {ex.Message}";
                                ArchiveCreatorTextBox.ScrollToEnd();
                            });
                        }
                    }
                }
            }

            // Log the completion and result of the archive creation process
            LogDebugInfo("Archive Creation: Process Success");
            await Dispatcher.InvokeAsync(() =>
            {
                ArchiveCreatorTextBox.Text += $"{Environment.NewLine}Archive Creator: Process Success\nArchive Creator: Double click here to open the output folder";
                ArchiveCreatorTextBox.ScrollToEnd();
            });

            return allItemsProcessed;
        }



        // TAB 1: Logic for Unpacking Archives

        private async void ArchiveUnpackerExecuteButtonClick(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("Archive Unpacker: Process Initiated");
            TemporaryMessageHelper.ShowTemporaryMessage(ArchiveUnpackerDragAreaText, "Processing....", 2000);

            Directory.CreateDirectory(_settings.MappedOutputDirectory);
            LogDebugInfo($"Archive Unpacker: Output directory created at {_settings.MappedOutputDirectory}");

            string filesToUnpack = ArchiveUnpackerTextBox.Text;
            if (!string.IsNullOrWhiteSpace(filesToUnpack))
            {
                string[] filePaths = filesToUnpack.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                LogDebugInfo($"Archive Unpacker: Starting unpacking for {filePaths.Length} files");
                bool unpackingSuccess = await UnpackFilesAsync(filePaths);

                string message = unpackingSuccess ? $"Success: {filePaths.Length} Files Unpacked" : "Unpacking Failed";
                TemporaryMessageHelper.ShowTemporaryMessage(ArchiveUnpackerDragAreaText, message, 2000);

                LogDebugInfo($"Archive Unpacker: Result - {message}");
            }
            else
            {
                LogDebugInfo("Archive Unpacker: Aborted - No files listed for Unpacking.");
                TemporaryMessageHelper.ShowTemporaryMessage(ArchiveUnpackerDragAreaText, "No files listed for Unpacking.", 2000);
            }
        }



       private void ArchiveUnpackerDragDropHandler(object sender, DragEventArgs e)
{
    if (e.Data.GetDataPresent(DataFormats.FileDrop))
    {
        string[] droppedItems = (string[])e.Data.GetData(DataFormats.FileDrop);
        List<string> validFiles = new List<string>();
        var validExtensions = new HashSet<string>(new[] { ".bar", ".sdat", ".sharc", ".dat" }, StringComparer.OrdinalIgnoreCase);

        // Use parallel processing to speed up file discovery
        Parallel.ForEach(droppedItems, item =>
        {
            if (Directory.Exists(item))
            {
                var filesInDirectory = Directory.EnumerateFiles(item, "*.*", SearchOption.AllDirectories)
                    .Where(file => validExtensions.Contains(Path.GetExtension(file)))
                    .ToList();

                lock (validFiles) // Locking to avoid race condition when adding to the shared list
                {
                    validFiles.AddRange(filesInDirectory);
                }
            }
            else if (File.Exists(item) && validExtensions.Contains(Path.GetExtension(item)))
            {
                lock (validFiles) // Locking for thread safety
                {
                    validFiles.Add(item);
                }
            }
        });

        if (validFiles.Count > 0)
        {
            Dispatcher.Invoke(() =>
            {
                var existingFilesSet = new HashSet<string>(ArchiveUnpackerTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                int initialCount = existingFilesSet.Count;

                existingFilesSet.UnionWith(validFiles); // Automatically removes duplicates

                // Use StringBuilder for efficient string concatenation
                ArchiveUnpackerTextBox.Text = string.Join(Environment.NewLine, existingFilesSet);
            });
        }
    }
}





        private async void ClickToBrowseArchiveUnpackerHandler(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("Archive Unpacker: Browsing for files Initiated");

            // Disable the main window to prevent interactions while the dialog is open
            this.IsEnabled = false;

            var openFileDialog = new VistaOpenFileDialog
            {
                Filter = "Supported files (*.bar;*.sdat;*.sharc;*.dat)|*.bar;*.sdat;*.sharc;*.dat",
                Multiselect = true
            };

            string message = "No files selected.";
            int displayTime = 2000; // Default to 2 seconds for the message display.

            bool? result = openFileDialog.ShowDialog();

            // Await a delay to absorb any unintentional clicks that happen after the dialog closes
            await Task.Delay(10); // 500 ms delay

            // Re-enable the main window after the delay
            this.IsEnabled = true;

            if (result == true)
            {
                var selectedFiles = openFileDialog.FileNames;

                if (selectedFiles.Any())
                {
                    LogDebugInfo($"Archive Unpacker: {selectedFiles.Length} files selected via File Browser.");

                    Dispatcher.Invoke(() =>
                    {
                        var existingFilesSet = new HashSet<string>(ArchiveUnpackerTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        int initialCount = existingFilesSet.Count;
                        existingFilesSet.UnionWith(selectedFiles);
                        int newFilesCount = existingFilesSet.Count - initialCount;
                        int duplicatesCount = selectedFiles.Length - newFilesCount;

                        ArchiveUnpackerTextBox.Text = string.Join(Environment.NewLine, existingFilesSet);

                        string addedFilesMessage = newFilesCount > 0 ? $"{newFilesCount} files added" : "No new files added";
                        string duplicatesMessage = duplicatesCount > 1 ? $", {duplicatesCount} duplicates filtered" : "";
                        message = addedFilesMessage + duplicatesMessage;

                        TemporaryMessageHelper.ShowTemporaryMessage(ArchiveUnpackerDragAreaText, message, displayTime);
                    });
                }
                else
                {
                    LogDebugInfo("Archive Unpacker: File Browser - No compatible files were selected.");
                    message = "No compatible files selected.";
                }
            }
            else
            {
                LogDebugInfo("Archive Unpacker: File Browser - Dialog Cancelled.");
            }

            TemporaryMessageHelper.ShowTemporaryMessage(ArchiveUnpackerDragAreaText, message, displayTime);
        }



        private string currentFilePath;
        private string CurrentWorkFolder;
        private string UserSuppliedPathPrefix = string.Empty;
        private string UUIDFoundInPath = string.Empty;
        private string CurrentUUID = string.Empty;
        private string SceneFolderAkaID = string.Empty;
        private string GuessedUUID = string.Empty;

        private async Task<bool> UnpackFilesAsync(string[] filePaths)
        {
            Stopwatch TotalJobStopwatch = Stopwatch.StartNew();
            LogDebugInfo($"Archive Unpacker: Beginning unpacking process for {filePaths.Length} files");

            string ogfilename = string.Empty;
            string Outputpath = MappedOutputDirectoryTextBox.Text;

            Directory.CreateDirectory(Outputpath);

            // Filter out lines starting with "Archive Unpacker:"
            filePaths = filePaths.Where(path => !path.StartsWith("Archive Unpacker:")).ToArray();

            foreach (string filePath in filePaths)
            {
                Stopwatch extractionStopwatch = Stopwatch.StartNew();
                currentFilePath = filePath;

                if (filePath.IndexOf(@"\Scenes\", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    string[] pathParts = filePath.Split(new[] { @"\Scenes\" }, StringSplitOptions.None);
                    if (pathParts.Length > 1)
                    {
                        string[] subPathParts = pathParts[1].Split('\\');
                        if (subPathParts.Length > 0)
                        {
                            SceneFolderAkaID = subPathParts[0];
                            LogDebugInfo($"SceneFolderAkaID: {SceneFolderAkaID}");
                        }
                    }
                }


                LogDebugInfo($"Archive Unpacker: Now processing file: {currentFilePath}");

                // Log start of processing to ArchiveUnpackerTextBox
                await Dispatcher.InvokeAsync(() =>
                {
                  
                });

                if (File.Exists(filePath))
                {
                    string filename = Path.GetFileName(filePath);

                    // Determine the action based on file extension
                    if (filename.ToLower().EndsWith(".bar") || filename.ToLower().EndsWith(".dat"))
                    {
                        string barfile = Outputpath + $"/{filename}";
                        // Copy the file to the output directory for processing
                        File.WriteAllBytes(barfile, File.ReadAllBytes(filePath));
                        await RunUnBAR.Run(Directory.GetCurrentDirectory(), barfile, Outputpath, false, (int)CdnMode.RETAIL);
                        ogfilename = filename;
                        filename = filename[..^4];

                        extractionStopwatch.Stop();
                        LogDebugInfo($"Archive Unpacker: Extraction Complete for {filename} (Time Taken {extractionStopwatch.ElapsedMilliseconds}ms)");
                    }
                    else if (filename.ToLower().EndsWith(".sharc"))
                    {
                        string barfile = Outputpath + $"/{filename}";
                        File.WriteAllBytes(barfile, File.ReadAllBytes(filePath));
                        await RunUnBAR.Run(Directory.GetCurrentDirectory(), barfile, Outputpath, false, (int)CdnMode.RETAIL);
                        ogfilename = filename;
                        filename = filename[..^6];

                        extractionStopwatch.Stop();
                        LogDebugInfo($"Archive Unpacker: Extraction Complete for {filename} (Time Taken {extractionStopwatch.ElapsedMilliseconds}ms)");
                    }
                    else if (filename.ToLower().EndsWith(".sdat"))
                    {
                        await RunUnBAR.Run(Directory.GetCurrentDirectory(), filePath, Outputpath, true, (int)CdnMode.RETAIL);
                        ogfilename = filename;
                        filename = filename[..^5];

                        extractionStopwatch.Stop();
                        LogDebugInfo($"Archive Unpacker: Extraction Complete for {filename} (Time Taken {extractionStopwatch.ElapsedMilliseconds}ms)");
                    }
                    else if (filename.ToLower().EndsWith(".zip"))
                    {
                        string barfile = Outputpath + $"/{filename}";
                        UncompressFile(barfile, Outputpath);
                        ogfilename = filename;
                        filename = filename[..^4];
                        extractionStopwatch.Stop();
                        LogDebugInfo($"Archive Unpacker: Extraction Complete for {filename} (Time Taken {extractionStopwatch.ElapsedMilliseconds}ms)");
                    }
                    else
                    {
                        // Skip the file if it doesn't match any known types
                        continue;
                    }

                    // Determine if the directory exists and has files for mapping
                    string directoryToMap = Directory.Exists(Outputpath + $"/{filename}") ? Outputpath + $"/{filename}" : Outputpath;
                    CurrentWorkFolder = directoryToMap;
                    int fileCount = Directory.GetFiles(directoryToMap).Length;

                    if (fileCount > 0)
                    {
                        // Check if the CheckBoxArchiveMapperEXP is checked for experimental mapping
                        if (CheckBoxArchiveMapperEXP.IsChecked == true)
                        {
                            await ExperimentalMapperAsync(directoryToMap, ogfilename);
                        }
                        else
                        {
                            // Proceed with standard mapping logic
                            if (CheckBoxArchiveMapperFAST.IsChecked == true)
                                await AFSClass.AFSMapStart(directoryToMap, ArchiveUnpackerPathTextBox.Text, string.Empty);
                            else
                            {
                                LegacyMapper? map = new();
                                await map.MapperStart(directoryToMap, Directory.GetCurrentDirectory(), ArchiveUnpackerPathTextBox.Text, string.Empty);
                                map = null;
                            }
                        }
                    }

                    // Log completion of processing to ArchiveUnpackerTextBox
                    await Dispatcher.InvokeAsync(() =>
                    {
                        
                    });

                    // Allow UI to process its queue
                    await Task.Delay(50);
                }
            }

            TotalJobStopwatch.Stop();
            LogDebugInfo($"Archive Unpacker: Job Complete - All files processed (Time Taken {TotalJobStopwatch.ElapsedMilliseconds}ms)");

            // Log completion of all files to ArchiveUnpackerTextBox
            await Dispatcher.InvokeAsync(() =>
            {
               
            });

            return true;
        }


        public async Task ExperimentalMapperAsync(string directoryPath, string ogfilename)
        {
            // Check if the Coredata checkbox is checked and call MapCoreData041 if it is
            if (CheckBoxArchiveMapperCoredata.IsChecked == true)
            {
                await MapCoreData041(directoryPath, ogfilename);
                return;
            }

            CurrentUUID = string.Empty;
            GuessedUUID = string.Empty;
            UUIDFoundInPath = string.Empty;
            UserSuppliedPathPrefix = string.Empty;
            Stopwatch MappingStopwatch = Stopwatch.StartNew();
            CheckForUUID();
            if (CheckBoxArchiveMapperUUIDGuesser.IsChecked == true)
            {
                await UUIDguesserCheckAsync(directoryPath); // Now an async call
            }

            LogDebugInfo($"Archive Unpacker: Starting experimental mapping for directory: {directoryPath}");
            var allFilePaths = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);
            var filePaths = allFilePaths
                .Where(path =>
                    !path.EndsWith(".dds") &&
                    !path.EndsWith(".mp3") &&
                    !path.EndsWith(".mp4"))
                .ToArray();

            var hashesToPaths = await InitialScanForPaths(filePaths);
            LogDebugInfo($"Archive Unpacker: Total unique hashes and paths found: {hashesToPaths.Count}");

            try
            {
                await RenameFiles(allFilePaths, new ConcurrentDictionary<string, string>(hashesToPaths), directoryPath);

                // Check for unmapped files and attempt to map scene files
                bool hasUnmappedFiles = CheckForUnmappedFiles(directoryPath);
                if (hasUnmappedFiles)
                {
                    await TryMapSceneFile(directoryPath);
                }
                FinalFolderCheckAsync(directoryPath);
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error in mapping operation: {ex.Message}");
            }

            MappingStopwatch.Stop();
            LogDebugInfo($"Archive Unpacker: Mapping Completed (Time Taken {MappingStopwatch.ElapsedMilliseconds}ms)");
        }

        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        private async Task UUIDguesserCheckAsync(string directoryPath)
        {
            await semaphore.WaitAsync();
            try
            {
                string exeLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string dependencyPath = Path.Combine(Path.GetDirectoryName(exeLocation), "dependencies", "uuid_helper.txt");

                // Dictionary to store prefix to UUID mappings
                Dictionary<string, string> prefixToUUID = new Dictionary<string, string>();

                if (File.Exists(dependencyPath))
                {
                    var fileLines = await File.ReadAllLinesAsync(dependencyPath);
                    foreach (var line in fileLines)
                    {
                        var parts = line.Split(':');
                        if (parts.Length == 2)
                        {
                            // Store each prefix and its corresponding UUID
                            prefixToUUID[parts[0]] = parts[1];
                        }
                    }

                    LogDebugInfo($"UUID Guesser Check: uuid_helper.txt found with {fileLines.Length} lines.");
                }
                else
                {
                    LogDebugInfo("UUID Guesser Check: uuid_helper.txt not found.");
                    return;
                }

                // Get all files in the root of the directory path, excluding subdirectories
                var rootFiles = Directory.GetFiles(directoryPath, "*", SearchOption.TopDirectoryOnly);

                // Check for files that start with any of the prefixes
                foreach (var filePath in rootFiles)
                {
                    string filenameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                    foreach (var entry in prefixToUUID)
                    {
                        if (filenameWithoutExtension.StartsWith(entry.Key))
                        {
                            // Format and set the GuessedUUID with prefix, suffix and make it lowercase
                            string formattedUUID = $"objects/{entry.Value.ToLower()}/";
                            LogDebugInfo($"UUID Guesser Check: Guessed UUID = {formattedUUID}");

                            // Only set CurrentUUID if it's currently empty
                            if (string.IsNullOrEmpty(CurrentUUID))
                            {
                                CurrentUUID = entry.Value.ToUpper(); // Set CurrentUUID to the raw UUID value from the file
                                LogDebugInfo($"UUID Guesser Check: CurrentUUID set to {CurrentUUID}");
                            }

                            // Update GuessedUUID regardless of CurrentUUID's state
                            GuessedUUID = formattedUUID;
                            return; // Stop checking after the first match
                        }
                    }
                }

                // If no UUID is guessed
                if (string.IsNullOrEmpty(GuessedUUID))
                {
                    LogDebugInfo("UUID Guesser Check: No matching UUID guessed from file names.");
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        private static Dictionary<string, string> hashToPath = new Dictionary<string, string>();
        private static bool isHelperLoaded = false;

        public async Task MapCoreData041(string directoryPath, string ogfilename)
        {
            // Logging messages at the start
            string logMessage = "Map CoreData: NOTE: COREDATA Mode enabled. This does not scan for paths in the usual way.\nMap CoreData: Instead it uses a preset list of paths to rename the files. \nMap CoreData: Use this mode for COREDATA/DEVARCHIVE/CONFIG/COREOBJECTS/NPBOOT etc.\nMap CoreData: Also can be used for 0.41 era BARs\nMap CoreData: Discovered a filename for an Unmapped file? Add to Mapper in the Path2Hash Tool!\nMap CoreData: COREDATA Mode Static Path Processing started";
            LogDebugInfo(logMessage);
            Dispatcher.Invoke(() =>
            {
                ArchiveUnpackerTextBox.Text += $"{Environment.NewLine}{logMessage}";
                ArchiveUnpackerTextBox.ScrollToEnd();
            });

            string directoryName = new DirectoryInfo(directoryPath).Name;
            logMessage = $"Map CoreData: Handling directory: {directoryName} with original filename: {ogfilename}";
            LogDebugInfo(logMessage);
            Dispatcher.Invoke(() =>
            {
                ArchiveUnpackerTextBox.Text += $"{Environment.NewLine}{logMessage}";
                ArchiveUnpackerTextBox.ScrollToEnd();
            });

            // Load the helper file if not already loaded
            if (!isHelperLoaded)
            {
                logMessage = "Map CoreData: Loading core_data_mapper_helper.txt. Please wait...";
                LogDebugInfo(logMessage);
                Dispatcher.Invoke(() =>
                {
                    ArchiveUnpackerTextBox.Text += $"{Environment.NewLine}{logMessage}";
                    ArchiveUnpackerTextBox.ScrollToEnd();
                });

                string exeLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string dependencyPath = Path.Combine(Path.GetDirectoryName(exeLocation), "dependencies", "core_data_mapper_helper.txt");

                if (File.Exists(dependencyPath))
                {
                    var fileLines = await File.ReadAllLinesAsync(dependencyPath);
                    foreach (var line in fileLines)
                    {
                        var parts = line.Split(':');
                        if (parts.Length == 2)
                        {
                            // Store each hash and its corresponding new path
                            hashToPath[parts[0].Trim()] = parts[1].Trim();
                        }
                    }

                    isHelperLoaded = true;
                    logMessage = $"Map CoreData: core_data_mapper_helper.txt loaded successfully with {fileLines.Length} lines.";
                    LogDebugInfo(logMessage);
                    Dispatcher.Invoke(() =>
                    {
                        ArchiveUnpackerTextBox.Text += $"{Environment.NewLine}{logMessage}";
                        ArchiveUnpackerTextBox.ScrollToEnd();
                    });
                }
                else
                {
                    logMessage = "Map CoreData: core_data_mapper_helper.txt not found.";
                    LogDebugInfo(logMessage);
                    Dispatcher.Invoke(() =>
                    {
                        ArchiveUnpackerTextBox.Text += $"{Environment.NewLine}{logMessage}";
                        ArchiveUnpackerTextBox.ScrollToEnd();
                    });
                    return;
                }
            }

            // Get all files in the root of the directory path, excluding subdirectories
            var rootFiles = Directory.GetFiles(directoryPath, "*", SearchOption.TopDirectoryOnly);

            int filesRenamedCount = 0;

            foreach (var filePath in rootFiles)
            {
                string filenameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                if (hashToPath.TryGetValue(filenameWithoutExtension, out var newPath))
                {
                    string newFilePath = Path.Combine(directoryPath, newPath);

                    // Ensure the target directory exists
                    Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));

                    try
                    {
                        // Rename the file to the new path
                        File.Move(filePath, newFilePath);
                        LogDebugInfo($"Map CoreData: Renamed {filePath} to {newFilePath}");
                        filesRenamedCount++;
                    }
                    catch (Exception ex)
                    {
                        LogDebugInfo($"Map CoreData: Failed to rename {filePath} to {newFilePath}: {ex.Message}");
                    }

                    // Allow the UI to update
                    await Task.Yield();
                }
            }

            logMessage = $"Map CoreData: {filesRenamedCount} files renamed.";
            LogDebugInfo(logMessage);
            Dispatcher.Invoke(() =>
            {
                ArchiveUnpackerTextBox.Text += $"{Environment.NewLine}{logMessage}";
                ArchiveUnpackerTextBox.ScrollToEnd();
            });

            // Rescan the directory for unmapped files
            rootFiles = Directory.GetFiles(directoryPath, "*", SearchOption.TopDirectoryOnly);

            // Check for unmapped files
            var unmappedFiles = rootFiles.Where(filePath =>
            {
                string filenameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                return Regex.IsMatch(filenameWithoutExtension, @"^[A-F0-9]{8}$", RegexOptions.IgnoreCase);
            }).ToList();

            if (unmappedFiles.Any())
            {
                logMessage = $"Map CoreData: {unmappedFiles.Count} unmapped files found:";
                LogDebugInfo(logMessage);
                Dispatcher.Invoke(() =>
                {
                    ArchiveUnpackerTextBox.Text += $"{Environment.NewLine}{logMessage}";
                    ArchiveUnpackerTextBox.ScrollToEnd();
                });

                foreach (var unmappedFile in unmappedFiles)
                {
                    logMessage = $"Map CoreData: Unmapped file - {unmappedFile}";
                    LogDebugInfo(logMessage);


                    // Allow the UI to update
                    await Task.Yield();
                }
            }
            else
            {
                logMessage = "Map CoreData: No unmapped files found.";
                LogDebugInfo(logMessage);
                Dispatcher.Invoke(() =>
                {
                    ArchiveUnpackerTextBox.Text += $"{Environment.NewLine}{logMessage}";
                    ArchiveUnpackerTextBox.ScrollToEnd();
                });
            }

            logMessage = "Map CoreData: Completed handling the directory.\nMap CoreData: Double click here to open Output folder";
            LogDebugInfo(logMessage);
            Dispatcher.Invoke(() =>
            {
                ArchiveUnpackerTextBox.Text += $"{Environment.NewLine}{logMessage}";
                ArchiveUnpackerTextBox.ScrollToEnd();
            });
        }


        private async Task RenameFiles(string[] allFilePaths, ConcurrentDictionary<string, string> hashesToPaths, string directoryPath)
        {
            Stopwatch RenameFilesStopwatch = Stopwatch.StartNew();
            var options = new ParallelOptions { MaxDegreeOfParallelism = 2 };
            Parallel.ForEach(allFilePaths, options, filePath =>
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath); // Convert filename to uppercase to match hex hash
                if (hashesToPaths.TryGetValue(fileNameWithoutExtension, out var newPath))
                {
                    // Ensure newPath is uppercase
                    newPath = newPath.ToUpper();

                    // Determine if newPath already contains a file extension
                    var newPathHasExtension = !string.IsNullOrEmpty(Path.GetExtension(newPath));
                    var fileExtension = newPathHasExtension ? string.Empty : Path.GetExtension(filePath).ToUpper(); // Add original extension only if newPath doesn't have one

                    var newFileName = $"{newPath}{fileExtension}";
                    var newFilePath = Path.Combine(Path.GetDirectoryName(filePath), newFileName);

                    // Ensure the target directory exists
                    var newFileDirectory = Path.GetDirectoryName(newFilePath);
                    Directory.CreateDirectory(newFileDirectory);

                    try
                    {
                        File.Move(filePath, newFilePath, true); // Overwrites the file if it already exists
                                                                // LogDebugInfo($"Archive Unpacker: Renamed {filePath} to {newFilePath}");
                    }
                    catch (Exception ex)
                    {
                        LogDebugInfo($"Archive Unpacker: Failed to rename {filePath} to {newFilePath}: {ex.Message}");
                    }
                }
            });
            RenameFilesStopwatch.Stop();

            LogDebugInfo($"Archive Unpacker: Mapping Stage 3 Complete - Renamed files by Hash (Time Taken {RenameFilesStopwatch.ElapsedMilliseconds}ms)");
        }


        private bool CheckForUnmappedFiles(string directoryPath)
        {
            Stopwatch CheckForUnmappedStopwatch = Stopwatch.StartNew();
            // LogDebugInfo($"Archive Unpacker: Checking for unmapped files in {directoryPath}");

            string pattern = @"^[A-F0-9]{8}(\..+)?$";
            bool hasUnmappedFiles = Directory.EnumerateFiles(directoryPath, "*", SearchOption.TopDirectoryOnly)
                                              .Any(file => Regex.IsMatch(Path.GetFileNameWithoutExtension(file), pattern));

            // LogDebugInfo(hasUnmappedFiles ? "Archive Unpacker: Unmapped files found." : "Archive Unpacker: No unmapped files found.");
            CheckForUnmappedStopwatch.Stop();
            return hasUnmappedFiles;
        }

        private async Task FinalFolderCheckAsync(string directoryPath)
        {
            LogDebugInfo($"Archive Unpacker: Starting directory final check: {directoryPath}");

            string directoryName = Path.GetFileName(directoryPath);
            string parentDirectoryPath = Directory.GetParent(directoryPath)?.FullName ?? string.Empty;
            string newDirectoryName = directoryPath;

            // Handling when CurrentUUID is not set and the directory ends with "_DAT"
            if (string.IsNullOrEmpty(CurrentUUID) && directoryName.EndsWith("_DAT"))
            {
                newDirectoryName = await FindAndRenameForSceneFile(directoryPath, directoryName, parentDirectoryPath);
                if (newDirectoryName != directoryPath && !Directory.Exists(newDirectoryName))
                {
                    Directory.Move(directoryPath, newDirectoryName);
                    LogDebugInfo($"Directory renamed from '{directoryPath}' to '{newDirectoryName}'.");
                    directoryPath = newDirectoryName; // Update directoryPath for subsequent operations
                }
            }

            // Handling when CurrentUUID is set
            if (!string.IsNullOrEmpty(CurrentUUID))
            {
                if (directoryName.StartsWith("object_"))
                {
                    newDirectoryName = Path.Combine(parentDirectoryPath, CurrentUUID + directoryName.Substring("object".Length));
                }
                else if (directoryName.Equals("object", StringComparison.OrdinalIgnoreCase))
                {
                    newDirectoryName = Path.Combine(parentDirectoryPath, CurrentUUID + "_T000");
                }
                if (directoryName.EndsWith("_DAT"))
                {
                    newDirectoryName = Path.Combine(parentDirectoryPath, CurrentUUID + "_UNKN_" + directoryName);
                }

                // Refined conflict check
                if (newDirectoryName != directoryPath && !Directory.Exists(newDirectoryName))
                {
                    Directory.Move(directoryPath, newDirectoryName);
                    LogDebugInfo($"Directory renamed from '{directoryPath}' to '{newDirectoryName}'.");
                    directoryPath = newDirectoryName; // Update directoryPath for subsequent operations
                }
            }
            else if (!directoryName.StartsWith("object_", StringComparison.OrdinalIgnoreCase) && !directoryName.Contains("object", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(SceneFolderAkaID))
                {
                    newDirectoryName = Path.Combine(parentDirectoryPath, SceneFolderAkaID + "$" + directoryName);
                    SceneFolderAkaID = string.Empty;

                    // Enhanced conflict check
                    if (newDirectoryName != directoryPath && !Directory.Exists(newDirectoryName))
                    {
                        try
                        {
                            Directory.Move(directoryPath, newDirectoryName);
                            LogDebugInfo($"Directory renamed from '{directoryPath}' to '{newDirectoryName}'.");
                            directoryPath = newDirectoryName; // Update directoryPath for subsequent operations
                        }
                        catch (Exception ex)
                        {
                            LogDebugInfo($"Failed to rename directory from '{directoryPath}' to '{newDirectoryName}'. Exception: {ex.Message}");
                        }
                    }
                    else
                    {
                        LogDebugInfo($"New directory name '{newDirectoryName}' already exists or is the same as the original. No renaming necessary.");
                    }
                }
                else
                {
                    LogDebugInfo($"SceneFolderAkaID is not set. Skipping renaming.");
                }
            }

            // File handling and deletion logic remains unchanged
            string file1 = Path.Combine(directoryPath, "1BA97CA6");
            string file2 = Path.Combine(directoryPath, "7AB93954");
            string targetFile1 = Path.Combine(directoryPath, "__$manifest$__");
            string targetFile2 = Path.Combine(directoryPath, "files.txt");

            if (CheckBoxArchiveMapperDeleteFilestxt.IsChecked == true)
            {
                if (File.Exists(file1)) File.Delete(file1);
                if (File.Exists(file2)) File.Delete(file2);
                if (File.Exists(targetFile1)) File.Delete(targetFile1);
                if (File.Exists(targetFile2)) File.Delete(targetFile2);
            }
            else
            {
                if (File.Exists(file1))
                {
                    if (File.Exists(targetFile1)) File.Delete(targetFile1); // Overwrite if it exists
                    File.Move(file1, targetFile1);
                }
                if (File.Exists(file2))
                {
                    if (File.Exists(targetFile2)) File.Delete(targetFile2); // Overwrite if it exists
                    File.Move(file2, targetFile2);
                }
            }

            // Check and patch files if enabled
            if (CheckBoxArchiveMapperPatchMDLs.IsChecked == true)
            {
                var mdlFiles = Directory.GetFiles(directoryPath, "*.MDL", SearchOption.AllDirectories);
                foreach (var mdlFile in mdlFiles)
                {
                    byte[] fileBytes = File.ReadAllBytes(mdlFile);
                    if (fileBytes.Length > 4 && fileBytes[3] == 0x04)
                    {
                        fileBytes[3] = 0x03;
                        File.WriteAllBytes(mdlFile, fileBytes);
                    }
                }

                var sceneFiles = Directory.GetFiles(directoryPath, "*.scene", SearchOption.AllDirectories);
                foreach (var sceneFile in sceneFiles)
                {
                    string fileContent = File.ReadAllText(sceneFile);
                    if (fileContent.Contains("file:///"))
                    {
                        fileContent = fileContent.Replace("file:///", "file://");
                        File.WriteAllText(sceneFile, fileContent);
                    }
                }
            }

            // Scanning logic remains unchanged
            if (CheckBoxArchiveMapperVerify.IsChecked ?? false)
            {
                Stopwatch scanStopwatch = Stopwatch.StartNew();
                FileScanner scanner = new FileScanner();
                await scanner.ScanDirectoryAsync(directoryPath);
                scanStopwatch.Stop();
                LogDebugInfo($"FileScanner: Scanned directory {directoryPath} (Time Taken: {scanStopwatch.ElapsedMilliseconds}ms)");
            }

            // Moving directory to "Complete" folder with enhanced conflict resolution
            bool hasUnmappedFiles = CheckForUnmappedFiles(directoryPath);
            string checkedFolder = Path.Combine(parentDirectoryPath, "Complete");
            Directory.CreateDirectory(checkedFolder);
            string finalDirectoryPath = Path.Combine(checkedFolder, Path.GetFileName(directoryPath));
            if (hasUnmappedFiles)
            {
                finalDirectoryPath += "_CHECK";
            }

            // Enhanced conflict resolution when moving to "Complete"
            if (Directory.Exists(finalDirectoryPath))
            {
                LogDebugInfo($"Conflict detected: '{finalDirectoryPath}' already exists. Checking contents...");

                // Check if the existing directory is empty or contains expected contents
                var existingFiles = Directory.GetFiles(finalDirectoryPath);
                var existingDirs = Directory.GetDirectories(finalDirectoryPath);

                // If empty, consider removing it before moving
                if (!existingFiles.Any() && !existingDirs.Any())
                {
                    LogDebugInfo($"Existing directory '{finalDirectoryPath}' is empty. Deleting it to avoid conflict.");
                    Directory.Delete(finalDirectoryPath);
                }
                else
                {
                    // Add a suffix only if a real conflict is found
                    int suffixCounter = 2;
                    string baseNewDirectoryName = finalDirectoryPath;
                    while (Directory.Exists(finalDirectoryPath))
                    {
                        LogDebugInfo($"Attempting to resolve conflict by renaming to '{finalDirectoryPath} ({suffixCounter})'.");
                        finalDirectoryPath = $"{baseNewDirectoryName} ({suffixCounter++})";
                    }
                }
            }

            // Final move operation with updated path
            Directory.Move(directoryPath, finalDirectoryPath);
            LogDebugInfo($"Archive Unpacker: Directory moved to 'checked' at '{finalDirectoryPath}'.");

            // Copy JobReport.txt if exists
            string sourceFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "JobReport.txt");
            string destinationFile = Path.Combine(checkedFolder, "JobReport.txt");
            if (File.Exists(sourceFile))
            {
                File.Copy(sourceFile, destinationFile, overwrite: true);
                LogDebugInfo($"JobReport.txt was copied to '{destinationFile}'.");
            }

            // Check if repacking is enabled
            if (CheckBoxArchiveMapperRepackAll.IsChecked == true)
            {
                LogDebugInfo($"Repacker: Starting repacking for folder {finalDirectoryPath}");

                // Call the repacker and wait for it to finish
                bool repackSuccess = await StartRepackingAsync(finalDirectoryPath);

                if (repackSuccess)
                {
                    LogDebugInfo($"Repacker: Repacking completed successfully for folder {finalDirectoryPath}");
                }
                else
                {
                    LogDebugInfo($"Repacker: Repacking failed for folder {finalDirectoryPath}");
                }
            }
            else
            {
                LogDebugInfo("Repacker: Repacking is disabled, skipping.");
            }

            // Call CreateHDKFolderStructure if CheckBoxArchiveMapperOfflineMode is checked
            if (CheckBoxArchiveMapperOfflineMode.IsChecked ?? false)
            {
                await CreateHDKFolderStructure(finalDirectoryPath);
            }
        }


        private async Task<bool> StartRepackingAsync(string folderPath)
        {
            try
            {
                // Simulate the drag-and-drop process by adding the folder path to the items list
                List<string> itemsToAdd = new List<string>();
                string itemWithTrailingSlash = folderPath.EndsWith(Path.DirectorySeparatorChar.ToString()) ? folderPath : folderPath + Path.DirectorySeparatorChar;
                itemsToAdd.Add(itemWithTrailingSlash);

                if (itemsToAdd.Count > 0)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        // Retrieve existing items from ArchiveCreatorTextBox and add new items
                        var existingItemsSet = new HashSet<string>(ArchiveCreatorTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        int initialCount = existingItemsSet.Count;

                        existingItemsSet.UnionWith(itemsToAdd);
                        int newItemsCount = existingItemsSet.Count - initialCount;
                        int duplicatesCount = itemsToAdd.Count - newItemsCount;

                        ArchiveCreatorTextBox.Text = string.Join(Environment.NewLine, existingItemsSet);

                        // Display a message about the items added
                        string message = $"{newItemsCount} item(s) added";
                        if (duplicatesCount > 0)
                        {
                            message += $", {duplicatesCount} duplicate(s) filtered";
                        }
                        TemporaryMessageHelper.ShowTemporaryMessage(ArchiveCreatorDragAreaText, message, 2000);

                        LogDebugInfo($"Archive Creation: {newItemsCount} items added, {duplicatesCount} duplicates filtered from simulated Drag and Drop.");
                    });
                }
                else
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        LogDebugInfo("Simulated Drag and Drop - No valid ZIP files or folders found.");
                        TemporaryMessageHelper.ShowTemporaryMessage(ArchiveCreatorDragAreaText, "No valid ZIP files or folders found.", 2000);
                    });

                    return false; // Indicate failure due to no valid items
                }

                // Simulate clicking the create button after the drag-and-drop operation
                await Task.Delay(100); // Give time for UI to update
                ArchiveCreatorExecuteButtonClick(this, new RoutedEventArgs());

                return true; // Indicate success
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Repacker: An error occurred during repacking: {ex.Message}");
                return false; // Indicate failure
            }
        }

        private void CheckBoxArchiveMapperRepackAll_Checked(object sender, RoutedEventArgs e)
        {
            // Check and enable the CheckBoxArchiveMapperVerify checkbox
            CheckBoxArchiveMapperVerify.IsChecked = true;


            // Disable the other specified checkboxes
            CheckBoxArchiveMapperOfflineMode.IsChecked = false;
            CheckBoxArchiveMapperCoredata.IsChecked = false;
        }

        private void CheckBoxArchiveMapperOfflineMode_Checked(object sender, RoutedEventArgs e)
        {
            // Check and enable the CheckBoxArchiveMapperVerify checkbox
            CheckBoxArchiveMapperVerify.IsChecked = true;


            // Disable the other specified checkboxes
            CheckBoxArchiveMapperRepackAll.IsChecked = false;
            CheckBoxArchiveMapperCoredata.IsChecked = false;
        }

        private void CheckBoxArchiveMapperCoredata_Checked(object sender, RoutedEventArgs e)
        {
            // Check and enable the CheckBoxArchiveMapperVerify checkbox


            // Disable the other specified checkboxes
            CheckBoxArchiveMapperRepackAll.IsChecked = false;
            CheckBoxArchiveMapperOfflineMode.IsChecked = false;
        }

        
        private async Task<string> FindAndRenameForSceneFile(string directoryPath, string directoryName, string parentDirectoryPath)
        {
            string newDirectoryName = directoryPath;  // Default to the original directory path if no .SCENE file is found

            try
            {
                // Get all .SCENE files in the directory, including subdirectories
                var sceneFiles = Directory.GetFiles(directoryPath, "*.SCENE", SearchOption.AllDirectories);
                string sceneFilePath = sceneFiles.FirstOrDefault();

                if (!string.IsNullOrEmpty(sceneFilePath))
                {
                    string sceneFileName = Path.GetFileNameWithoutExtension(sceneFilePath);
                    newDirectoryName = Path.Combine(parentDirectoryPath, sceneFileName + "_" + directoryName);
                    LogDebugInfo($"Found .SCENE file: {sceneFileName}, renaming directory to '{newDirectoryName}'.");
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error finding .SCENE file: {ex.Message}");
            }

            return newDirectoryName;
        }


        private async Task CreateHDKFolderStructure(string directoryPath)
        {
            // Define a local function to recursively search for resources.xml
            string FindResourcesXml(string path)
            {
                foreach (var directory in Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories))
                {
                    string filePath = Path.Combine(directory, "resources.xml");
                    if (File.Exists(filePath))
                    {
                        return filePath;
                    }
                }
                return null; // Return null if not found
            }

            // Use the local function to find resources.xml
            string resourcesXmlPath = FindResourcesXml(directoryPath);
            if (!string.IsNullOrEmpty(resourcesXmlPath))
            {
                LogDebugInfo($"resources.xml found at: {resourcesXmlPath}");
                // Move files listed in resources.xml
                await MoveAllExceptSpecificFiles(resourcesXmlPath, directoryPath);
            }
            else
            {
                LogDebugInfo("resources.xml not found in the directory structure.");
            }
        }

        private async Task MoveAllExceptSpecificFiles(string resourcesXmlPath, string targetDirectoryPath)
        {
            // Determine the source directory from the resources.xml file path
            string sourceDirectoryPath = Path.GetDirectoryName(resourcesXmlPath);

            // List of files to exclude from moving, using a case-insensitive comparison
            var excludeFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "resources.xml",
        "object.xml",
        "localisation.xml",
        "editor.oxml",
        "object.odc",
        "catalogueentry.xml",
        "validation.xml",
        "main.lua",
        "large.png",
        "small.png"
    };

            // Move all files from source directory to target directory, except the excluded ones
            var files = Directory.EnumerateFiles(sourceDirectoryPath);
            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);
                if (!excludeFiles.Contains(fileName))
                {
                    string destFile = Path.Combine(targetDirectoryPath, fileName);
                    File.Move(file, destFile);
                }
            }

            // Move all directories from source directory to target directory
            var directories = Directory.EnumerateDirectories(sourceDirectoryPath);
            foreach (var dir in directories)
            {
                string dirName = Path.GetFileName(dir);
                string destDir = Path.Combine(targetDirectoryPath, dirName);
                Directory.Move(dir, destDir);
            }

            // Delete the timestamp.txt file in targetDirectoryPath if it exists
            string timestampFilePath = Path.Combine(targetDirectoryPath, "timestamp.txt");
            if (File.Exists(timestampFilePath))
            {
                File.Delete(timestampFilePath);
            }

            // Ensure that the operation does not block the main thread
            await Task.CompletedTask;
        }

        private async Task<ConcurrentDictionary<string, string>> InitialScanForPaths(IEnumerable<string> filePaths)
        {
            Stopwatch FilescanforpathsStopwatch = Stopwatch.StartNew();
            LogDebugInfo($"Archive Unpacker: Starting initial scan of {filePaths.Count()} files.");

            var hashesToPaths = new ConcurrentDictionary<string, string>();
            var options = new ParallelOptions { MaxDegreeOfParallelism = 4 };
            var allProcessedMatches = new ConcurrentBag<string>();

            string[] specialPrefixes1 = { "ptools", "tenvi", "xenvi", "natgp", "hatgp", "tatgp" };
            string[] specialPrefixes2 = { "file:///resource_root/build/", "file://resource_root/build/" };
            string[] staticPaths = { "resources.xml", "object.xml", "localisation.xml", "catalogueentry.xml", "object.odc", "editor.oxml", "large.png", "small.png", "maker.png", "files.txt", "__$manifest$__" };
            string ddsPattern = @"[a-z0-9][a-z0-9_. \\/-]*\.dds";
            string quotedPattern = @"(?:file|source|efx_filename)=""([a-z0-9_.() :\\/-]*)""";

            Parallel.ForEach(filePaths, options, filePath =>
            {
                try
                {
                    var contentString = File.ReadAllText(filePath).Replace("\\", "/");
                    var matches = new List<string>();

                    // Add dynamically found paths from regex searches
                    matches.AddRange(Regex.Matches(contentString, ddsPattern, RegexOptions.IgnoreCase).Select(m => m.Value.ToLower()));
                    matches.AddRange(Regex.Matches(contentString, quotedPattern, RegexOptions.IgnoreCase).Select(m => m.Groups[1].Value.ToLower()));

                    // Process all matches, regardless of content
                    foreach (var match in matches)
                    {
                        ProcessAndAddMatches(match, allProcessedMatches, specialPrefixes1, specialPrefixes2);
                    }

                    // Add static paths directly to allProcessedMatches without condition
                    foreach (var staticPath in staticPaths)
                    {
                        ProcessAndAddMatches(staticPath, allProcessedMatches, specialPrefixes1, specialPrefixes2);
                    }
                }
                catch (Exception ex)
                {
                    LogDebugInfo($"Archive Unpacker: Error processing file {filePath}: {ex.Message}");
                }
            });

            // Now process each unique path for hashing
            foreach (var processedMatch in allProcessedMatches.Distinct())
            {
                int hashOfPath = ComputeAFSHash(processedMatch);
                string hexHash = hashOfPath.ToString("X8");
                hashesToPaths.TryAdd(hexHash, processedMatch.ToUpper());
            }

            FilescanforpathsStopwatch.Stop();
            LogDebugInfo($"Archive Unpacker: Mapping Stage 2 - Path Scan Complete. Total matches found {hashesToPaths.Count} (Time Taken {FilescanforpathsStopwatch.ElapsedMilliseconds}ms)");

            return hashesToPaths;
        }


        private void ViewLogButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Determine the path to the log file next to the executable
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string logFilePath = Path.Combine(exeDirectory, "logs", "debug.log");

                // Check if the log file exists
                if (File.Exists(logFilePath))
                {
                    // Use Process.Start to open the log file with the default associated application
                    Process.Start(new ProcessStartInfo(logFilePath) { UseShellExecute = true });
                }
                else
                {
                    // Show a message if the log file does not exist
                    MessageBox.Show("Log file not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                // Log the exception and display an error message
                LogDebugInfo($"Error opening log file: {ex.Message}");
                MessageBox.Show($"Failed to open the log file. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ProcessAndAddMatches(string match, ConcurrentBag<string> allProcessedMatches, string[] specialPrefixes1, string[] specialPrefixes2)
        {
            string processedMatch = match;

            // Trim 1 byte from the start if the match starts with any of the specialPrefixes1
            foreach (var prefix in specialPrefixes1)
            {
                if (processedMatch.StartsWith(prefix))
                {
                    processedMatch = processedMatch.Substring(1); // Trim 1 byte from the start
                    break;
                }
            }

            // Trim the entire prefix for specialPrefixes2
            foreach (var prefix in specialPrefixes2)
            {
                if (processedMatch.StartsWith(prefix))
                {
                    processedMatch = processedMatch.Substring(prefix.Length); // Trim the whole prefix
                    break;
                }
            }

            var processedMatches = new List<string> { processedMatch };

            // Additional processing for specific endings
            if (processedMatch.EndsWith(".probe"))
            {
                processedMatches.Add(processedMatch.Replace(".probe", ".scene"));
                processedMatches.Add(processedMatch.Replace(".probe", ".ocean"));
            }
            else if (processedMatch.EndsWith(".atmos"))
            {
                processedMatches.Add(processedMatch.Replace(".atmos", ".cdata"));
            }

            // Append UUID or UserSuppliedPathPrefix if present
            if (!string.IsNullOrEmpty(UUIDFoundInPath))
            {
                processedMatches.AddRange(processedMatches.ToList().Select(pm => UUIDFoundInPath + pm));
            }
            if (!string.IsNullOrEmpty(UserSuppliedPathPrefix))
            {
                processedMatches.AddRange(processedMatches.ToList().Select(pm => UserSuppliedPathPrefix + pm));
            }

            // Append GuessedUUID if not empty and the checkbox is checked
            if (!string.IsNullOrEmpty(GuessedUUID))
            {
                processedMatches.AddRange(processedMatches.ToList().Select(pm => GuessedUUID + pm));
            }

            // Add all processed matches to the concurrent bag
            foreach (var finalMatch in processedMatches)
            {
                allProcessedMatches.Add(finalMatch);
            }
        }




        private async Task TryMapSceneFile(string directoryPath)
        {
            // Check if CurrentUUID is already set; if yes, skip the function
            if (!string.IsNullOrEmpty(CurrentUUID))
            {
                LogDebugInfo("Archive Unpacker: CurrentUUID is already set, skipping scene file mapping.");
                return;  // Exit the function early
            }

            Stopwatch SceneFileLookupStopwatch = Stopwatch.StartNew();
            LogDebugInfo($"Archive Unpacker: Unmapped file detected in: {directoryPath} - Checking if it's a known scene file in lookup table");

            string helperFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dependencies\\scene_file_mapper_helper.txt");

            if (!File.Exists(helperFilePath))
            {
                LogDebugInfo("Archive Unpacker: Helper file not found.");
                return;
            }

            LogDebugInfo("Archive Unpacker: Helper file found, processing mappings.");

            var fileMappings = new Dictionary<string, string>();
            foreach (var line in File.ReadAllLines(helperFilePath))
            {
                var parts = line.Split(new[] { ':' }, 2);
                if (parts.Length == 2)
                {
                    string hashPart = parts[0].Trim();
                    string pathPart = parts[1].Trim();
                    fileMappings[hashPart] = pathPart;
                }
            }

            var filesInRootDirectory = Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly);

            foreach (var filePath in filesInRootDirectory)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                if (fileMappings.TryGetValue(fileName, out var newRelativePath))
                {
                    var newFilePath = Path.Combine(directoryPath, newRelativePath);
                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(newFilePath)); // Ensure the directory exists
                        File.Move(filePath, newFilePath); // Attempt to move the file
                    }
                    catch (Exception ex)
                    {
                        LogDebugInfo($"Archive Unpacker: Error renaming file from {filePath} to {newFilePath}: {ex.Message}");
                    }
                }
            }

            SceneFileLookupStopwatch.Stop(); // Stop timing the extraction
            LogDebugInfo($"Archive Unpacker: Mapping Stage 4 Complete - Scene filename lookup (Time Taken {SceneFileLookupStopwatch.ElapsedMilliseconds}ms)");
        }


        public void CheckForUUID()
        {
            // Define a regex pattern for the UUID format 8-8-8-8, possibly followed by underscores and other characters
            string uuidPattern = @"\b([a-f0-9]{8}-[a-f0-9]{8}-[a-f0-9]{8}-[a-f0-9]{8})(?:_[\w]*)?\b";

            // Initialize a variable to hold any found UUID temporarily
            string tempUUID = string.Empty;

            // Check ArchiveUnpackerPathTextBox for a UUID
            Match match = Regex.Match(ArchiveUnpackerPathTextBox.Text.ToLower(), uuidPattern);
            if (!string.IsNullOrEmpty(ArchiveUnpackerPathTextBox.Text) && !match.Success)
            {
                // If there's a string but no UUID, save it as UserSuppliedPath in lowercase
                UserSuppliedPathPrefix = ArchiveUnpackerPathTextBox.Text.ToLower();
            }

            if (match.Success)
            {
                // Save the found UUID, ignoring any suffix
                tempUUID = match.Groups[1].Value.ToLower();
                // If a UUID is found, save it as UserSuppliedUUID in lowercase, prepended and appended as specified
                UserSuppliedPathPrefix = "objects/" + tempUUID + "/";
            }

            // Check currentFilePath for a UUID in lowercase
            match = Regex.Match(currentFilePath.ToLower(), uuidPattern);
            if (match.Success)
            {
                // Save the found UUID, ignoring any suffix
                tempUUID = match.Groups[1].Value.ToLower();
                // If a UUID is found in currentFilePath, save it as UUIDFoundInPath in lowercase, prepended and appended as specified
                UUIDFoundInPath = "objects/" + tempUUID + "/";
            }

            // If any UUID was found, save it to CurrentUUID
            if (!string.IsNullOrEmpty(tempUUID))
            {
                CurrentUUID = tempUUID.ToUpper();
            }

            LogDebugInfo($"UUID Check: UUIDFoundInPath set to {UUIDFoundInPath}, CurrentUUID set to {CurrentUUID}, UserSuppliedPathPrefix set to {UserSuppliedPathPrefix}");
        }


        private int ComputeAFSHash(string text)
        {
            unchecked
            {
                int hashOfPath = 0;
                foreach (char ch in text)
                    hashOfPath = hashOfPath * 37 + ch;
                LogDebugInfo($"Computed Hash: {hashOfPath} for Text: {text}");

                return hashOfPath;
            }
        }

       
        // TAB 2: Logic for CDS Encryption 

        private async void CDSEncrypterExecuteButtonClick(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("CDS Encryption: Process Initiated");
            TemporaryMessageHelper.ShowTemporaryMessage(CDSEncrypterDragAreaText, "Processing...", 2000);

            string baseOutputDirectory = _settings.CdsEncryptOutputDirectory; // Assume _settings.CdsOutputDirectory is already set
            if (!Directory.Exists(baseOutputDirectory))
            {
                Directory.CreateDirectory(baseOutputDirectory);
                LogDebugInfo($"CDS Encryption: Output directory created at {baseOutputDirectory}");
            }

            string filesToEncrypt = CDSEncrypterTextBox.Text; // Assuming CDSEncrypterTextBox contains file paths separated by new lines
            bool addSha1ToFilename = CDSAddSHA1CheckBox.IsChecked ?? false; // Check if the checkbox for adding SHA1 to filename is checked

            if (!string.IsNullOrWhiteSpace(filesToEncrypt))
            {
                string[] filePaths = filesToEncrypt.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                LogDebugInfo($"CDS Encryption: Starting encryption for {filePaths.Length} files");
                bool encryptionSuccess = await EncryptFilesAsync(filePaths, baseOutputDirectory, addSha1ToFilename); // Pass the state of the checkbox to the encryption method

                string message = encryptionSuccess ? $"Success: {filePaths.Length} Files Encrypted" : "Encryption Failed";
                TemporaryMessageHelper.ShowTemporaryMessage(CDSEncrypterDragAreaText, message, 2000);
                LogDebugInfo($"CDS Encryption: Result - {message}");
            }
            else
            {
                LogDebugInfo("CDS Encryption: Aborted - No files listed for Encryption.");
                TemporaryMessageHelper.ShowTemporaryMessage(CDSEncrypterDragAreaText, "No files listed for Encryption.", 2000);
            }
        }


        private void CDSEncrypterDragDropHandler(object sender, DragEventArgs e)
        {
            LogDebugInfo("CDS Encryption: Drag and Drop Initiated");
            TemporaryMessageHelper.ShowTemporaryMessage(CDSEncrypterDragAreaText, "Processing....", 1000000);

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedItems = (string[])e.Data.GetData(DataFormats.FileDrop);
                List<string> validFiles = new List<string>();
                var validExtensionsForFolders = new[] { ".sdc", ".odc", ".xml" }; // Extensions to look for within folders
                var validExtensionsForFiles = new[] { ".sdc", ".odc", ".xml", ".bar" }; // Extensions accepted for individual files

                foreach (var item in droppedItems)
                {
                    if (Directory.Exists(item))
                    {
                        // Scan the folder for specific file types excluding .bar files
                        var filesInDirectory = Directory.GetFiles(item, "*.*", SearchOption.AllDirectories)
                            .Where(file => validExtensionsForFolders.Contains(Path.GetExtension(file).ToLowerInvariant()))
                            .ToList();
                        validFiles.AddRange(filesInDirectory);
                    }
                    else if (File.Exists(item) && validExtensionsForFiles.Contains(Path.GetExtension(item).ToLowerInvariant()))
                    {
                        // Accept .bar files if they are dropped individually
                        validFiles.Add(item);
                    }
                }

                int newFilesCount = 0;
                int duplicatesCount = 0;
                string message = string.Empty;
                int displayTime = 2000;

                if (validFiles.Count > 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var existingFilesSet = new HashSet<string>(CDSEncrypterTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        int initialCount = existingFilesSet.Count;

                        existingFilesSet.UnionWith(validFiles); // Automatically removes duplicates
                        newFilesCount = existingFilesSet.Count - initialCount;
                        duplicatesCount = validFiles.Count - newFilesCount;

                        CDSEncrypterTextBox.Text = string.Join(Environment.NewLine, existingFilesSet);

                        message = $"{newFilesCount} file(s) added, {duplicatesCount} duplicate(s) filtered";
                        TemporaryMessageHelper.ShowTemporaryMessage(CDSEncrypterDragAreaText, message, 2000);

                        LogDebugInfo($"CDS Encryption: {newFilesCount} files added, {duplicatesCount} duplicates filtered from Drag and Drop.");
                    });
                }
                else
                {
                    LogDebugInfo("CDS Encryption: Drag and Drop - No valid files found.");
                    message = "No valid files found.";
                    TemporaryMessageHelper.ShowTemporaryMessage(CDSEncrypterDragAreaText, message, 2000);
                }
            }
            else
            {
                LogDebugInfo("CDS Encryption: Drag and Drop - No Data Present.");
                TemporaryMessageHelper.ShowTemporaryMessage(CDSEncrypterDragAreaText, "Drag and Drop operation failed - No Data Present.", 2000);
            }
        }




        private async void ClickToBrowseCDSEncryptHandler(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("CDS Encryption: Browsing for files Initiated");

            // Disable the main window to prevent interactions while the dialog is open
            this.IsEnabled = false;

            var openFileDialog = new VistaOpenFileDialog
            {
                Filter = "Supported files (*.sdc;*.odc;*.xml;*.bar)|*.sdc;*.odc;*.xml;*.bar",
                Multiselect = true
            };

            string message = "No files selected.";
            int displayTime = 2000;  // Keep your original display time setting

            bool? result = openFileDialog.ShowDialog();

            // Await a delay to absorb any unintentional clicks that happen after the dialog closes
            await Task.Delay(10); // Corrected delay to 10 ms

            // Re-enable the main window after the delay
            this.IsEnabled = true;

            if (result == true)
            {
                var selectedFiles = openFileDialog.FileNames;
                var validExtensions = new[] { ".sdc", ".odc", ".xml", ".bar" }
                    .Select(ext => ext.ToLowerInvariant()).ToArray();

                List<string> validFiles = selectedFiles
                    .Where(file => validExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                    .ToList();

                if (validFiles.Any())
                {
                    Dispatcher.Invoke(() =>
                    {
                        var existingFilesSet = new HashSet<string>(CDSEncrypterTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        int initialCount = existingFilesSet.Count;

                        existingFilesSet.UnionWith(validFiles);
                        int newFilesCount = existingFilesSet.Count - initialCount;
                        int duplicatesCount = validFiles.Count - newFilesCount;

                        CDSEncrypterTextBox.Text = string.Join(Environment.NewLine, existingFilesSet);

                        string duplicatesMessage = duplicatesCount > 1 ? $", {duplicatesCount} duplicate(s) filtered" : "";
                        message = $"{newFilesCount} file(s) added" + duplicatesMessage;
                        TemporaryMessageHelper.ShowTemporaryMessage(CDSEncrypterDragAreaText, message, 2000);

                        LogDebugInfo($"CDS Encryption: {newFilesCount} files added{duplicatesMessage} via File Browser.");
                    });
                }
                else
                {
                    message = "No compatible files selected.";
                    LogDebugInfo("CDS Encryption: File Browser - No compatible files were selected.");
                }
            }
            else
            {
                message = "Dialog cancelled.";
                LogDebugInfo("CDS Encryption: File Browser - Dialog Cancelled.");
            }

            TemporaryMessageHelper.ShowTemporaryMessage(CDSEncrypterDragAreaText, message, 2000);
        }


        public async Task<bool> EncryptFilesAsync(string[] filePaths, string baseOutputDirectory, bool addSha1ToFilename)
        {
            bool allFilesProcessed = true;
            foreach (var filePath in filePaths)
            {
                string filename = Path.GetFileName(filePath);
                try
                {
                    byte[] fileContent = await File.ReadAllBytesAsync(filePath);
                    byte[]? encryptedContent = null;
                    string inputSHA1 = "";

                    // Generate SHA1 hash from file content before encryption
                    using (SHA1 sha1 = SHA1.Create())
                    {
                        byte[] SHA1Data = sha1.ComputeHash(fileContent);
                        inputSHA1 = BitConverter.ToString(SHA1Data).Replace("-", "").ToLowerInvariant(); // Full SHA1 of input content
                        LogDebugInfo($"Input SHA1 for {filename}: {inputSHA1}");

                        // Encrypt the file content
                        string computedSha1 = inputSHA1.Substring(0, 16); // Use first 16 characters for some process, if needed
                        encryptedContent = CDSProcess.CDSEncrypt_Decrypt(fileContent, computedSha1, (int)CdnMode.RETAIL);
                    }

                    // Log the success message and the SHA1 of the input file
                    Dispatcher.Invoke(() =>
                    {
                        CDSEncrypterTextBox.Text += $"{Environment.NewLine}CDS: Success file encrypted for {filename}";
                        CDSEncrypterTextBox.Text += $"{Environment.NewLine}File: {filename} SHA1: {inputSHA1}";
                        CDSEncrypterTextBox.ScrollToEnd();
                    });

                    // Append SHA1 of the original content to filename if requested
                    if (addSha1ToFilename && encryptedContent != null)
                    {
                        string extension = Path.GetExtension(filename);
                        string filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
                        filename = $"{filenameWithoutExtension}_{inputSHA1}{extension}"; // Using inputSHA1 for naming
                    }

                    // Rename for CDN if the checkbox is checked and conditions are met
                    if (CDSRenameForCDNCheckBox.IsChecked == true && Path.GetExtension(filename).ToLower() == ".odc" &&
                        System.Text.RegularExpressions.Regex.IsMatch(filename, @"^[a-fA-F0-9]{8}-[a-fA-F0-9]{8}-[a-fA-F0-9]{8}-[a-fA-F0-9]{8}"))
                    {
                        string uuidPart = filename.Substring(0, 35); // Extract the 35-character UUID part
                        string extraPart = filename.Substring(35); // Extract the rest of the filename after the UUID
                        string newFolderName = uuidPart;
                        string newFileName = $"object{extraPart}";

                        string outputDirectory = Path.Combine(baseOutputDirectory, "Objects", newFolderName);
                        if (!Directory.Exists(outputDirectory))
                        {
                            Directory.CreateDirectory(outputDirectory);
                            LogDebugInfo($"Output directory {outputDirectory} created.");
                        }
                        string outputPath = Path.Combine(outputDirectory, newFileName);
                        await File.WriteAllBytesAsync(outputPath, encryptedContent);
                        LogDebugInfo($"File {newFileName} encrypted and written to {outputPath}.");
                    }
                    else if (encryptedContent != null)
                    {
                        string outputDirectory = Path.Combine(baseOutputDirectory, Path.GetFileName(Path.GetDirectoryName(filePath)));
                        if (!Directory.Exists(outputDirectory))
                        {
                            Directory.CreateDirectory(outputDirectory);
                            LogDebugInfo($"Output directory {outputDirectory} created.");
                        }
                        string outputPath = Path.Combine(outputDirectory, filename);
                        await File.WriteAllBytesAsync(outputPath, encryptedContent);
                        LogDebugInfo($"File {filename} encrypted and written to {outputPath}.");
                    }
                    else
                    {
                        allFilesProcessed = false;
                        LogDebugInfo($"Encryption failed for {filename}, no data written.");
                    }
                }
                catch (Exception ex)
                {
                    LogDebugInfo($"Error encrypting {filename}: {ex.Message}");
                    allFilesProcessed = false;
                }
            }
            return allFilesProcessed;
        }

        // TAB 2: Logic for CDS Decryption

        private async void CDSDecrypterExecuteButtonClick(object sender, RoutedEventArgs e)
        {
            TemporaryMessageHelper.ShowTemporaryMessage(CDSDecrypterDragAreaText, "Processing....", 2000);

            string baseOutputDirectory = _settings.CdsDecryptOutputDirectory;
            if (!Directory.Exists(baseOutputDirectory))
            {
                Directory.CreateDirectory(baseOutputDirectory);
            }

            string filesToDecrypt = CDSDecrypterTextBox.Text;
            string manualSha1 = CDSDecrypterSha1TextBox.Text.Trim();
            bool manualSha1IsValid = !string.IsNullOrWhiteSpace(manualSha1) && manualSha1.Length == 40;

            if (!string.IsNullOrWhiteSpace(filesToDecrypt))
            {
                string[] filePaths = filesToDecrypt.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                bool decryptionSuccess = true;

                foreach (string filePath in filePaths)
                {
                    string filename = Path.GetFileName(filePath);
                    string detectedSha1 = Regex.Match(filename, @"[a-fA-F0-9]{40}").Value;
                    string effectiveSha1 = manualSha1IsValid ? manualSha1 : detectedSha1;

                    if (manualSha1IsValid)
                    {
                        decryptionSuccess &= await DecryptFilesSHA1Async(new string[] { filePath }, baseOutputDirectory, manualSha1);
                    }
                    else if (!string.IsNullOrWhiteSpace(detectedSha1))
                    {
                        decryptionSuccess &= await DecryptFilesSHA1Async(new string[] { filePath }, baseOutputDirectory, detectedSha1);
                    }
                    else
                    {
                        decryptionSuccess &= await DecryptFilesAsync(new string[] { filePath }, baseOutputDirectory);
                    }
                }

                string message = decryptionSuccess ? "Success: All files decrypted" : "Decryption Failed for one or more files";
                TemporaryMessageHelper.ShowTemporaryMessage(CDSDecrypterDragAreaText, message, 2000);
            }
            else
            {
                TemporaryMessageHelper.ShowTemporaryMessage(CDSDecrypterDragAreaText, "No files listed for Decryption.", 2000);
            }
        }

        public async Task<bool> DecryptFilesAsync(string[] filePaths, string baseOutputDirectory)
        {
            bool allFilesProcessed = true;
            foreach (var filePath in filePaths)
            {
                string filename = Path.GetFileName(filePath);
                string extension = Path.GetExtension(filename);
                try
                {
                    string parentFolderName = Path.GetFileName(Path.GetDirectoryName(filePath));

                    byte[] fileContent = await File.ReadAllBytesAsync(filePath);

                    BruteforceProcess proc = new BruteforceProcess(fileContent);
                    byte[] decryptedContent = proc.StartBruteForce((int)CdnMode.RETAIL);

                    if (decryptedContent != null)
                    {
                        string outputDirectory = Path.Combine(baseOutputDirectory, parentFolderName);
                        if (!Directory.Exists(outputDirectory))
                        {
                            Directory.CreateDirectory(outputDirectory);
                        }
                        string outputPath = Path.Combine(outputDirectory, filename);
                        await File.WriteAllBytesAsync(outputPath, decryptedContent);

                        if (!IsValidDecryptedFile(outputPath, extension))
                        {
                            File.Delete(outputPath);
                            allFilesProcessed = false;
                        }
                    }
                    else
                    {
                        allFilesProcessed = false;
                    }
                }
                catch (Exception)
                {
                    allFilesProcessed = false;
                }
            }

            return allFilesProcessed;
        }



        public async Task<bool> DecryptFilesSHA1Async(string[] filePaths, string baseOutputDirectory, string sha1Hash)
        {
            bool allFilesProcessed = true;
            foreach (var filePath in filePaths)
            {
                string filename = Path.GetFileName(filePath);
                string extension = Path.GetExtension(filename);
                try
                {
                    string parentFolderName = Path.GetFileName(Path.GetDirectoryName(filePath));
                    byte[] fileContent = await File.ReadAllBytesAsync(filePath);
                    string fileSha1 = Regex.Match(filename, @"\b[0-9a-fA-F]{40}\b").Value;
                    byte[]? processedContent = null;
                    string useSha1;

                    if (!string.IsNullOrWhiteSpace(fileSha1))
                    {
                        useSha1 = fileSha1;
                        LogDebugInfo($"SHA1 {useSha1} detected in filename for {filename}.");
                    }
                    else if (!string.IsNullOrWhiteSpace(sha1Hash))
                    {
                        useSha1 = sha1Hash;
                        LogDebugInfo($"SHA1 {useSha1} provided manually for {filename}.");
                    }
                    else
                    {
                        using (SHA1 sha1 = SHA1.Create())
                        {
                            byte[] SHA1Data = sha1.ComputeHash(fileContent);
                            useSha1 = BitConverter.ToString(SHA1Data).Replace("-", "").Substring(0, 16);
                            LogDebugInfo($"No SHA1 found in filename or provided. Generated SHA1 for {filename}: {useSha1}");
                        }
                    }

                    processedContent = CDSProcess.CDSEncrypt_Decrypt(fileContent, useSha1.Substring(0, 16), (int)CdnMode.RETAIL);
                    if (processedContent != null)
                    {
                        string outputDirectory = Path.Combine(baseOutputDirectory, parentFolderName);
                        if (!Directory.Exists(outputDirectory))
                        {
                            Directory.CreateDirectory(outputDirectory);
                        }
                        string outputPath = Path.Combine(outputDirectory, filename);
                        await File.WriteAllBytesAsync(outputPath, processedContent);
                        LogDebugInfo($"Decryption successful for {filename}. Output written to {outputPath}.");

                        // Validate the decrypted file content
                        if (!IsValidDecryptedFile(outputPath, extension))
                        {
                            File.Delete(outputPath);
                            LogDebugInfo($"Validation failed for {filename}. File deleted.");
                            allFilesProcessed = false;
                        }
                        else
                        {
                            LogDebugInfo($"Validation passed for {filename}. Decryption successful and file content is valid.");
                        }
                    }
                    else
                    {
                        allFilesProcessed = false;
                        LogDebugInfo($"Decryption failed for {filename}. No output written.");
                    }
                }
                catch (Exception ex)
                {
                    LogDebugInfo($"Error processing {filename}: {ex.Message}");
                    allFilesProcessed = false;
                }
            }
            return allFilesProcessed;
        }


        private bool IsValidDecryptedFile(string filePath, string extension)
        {
            if (string.Equals(extension, ".hcdb", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            try
            {
                if (string.Equals(extension, ".sdc", StringComparison.OrdinalIgnoreCase))
                {
                    return ValidateSdcFile(filePath);
                }
                else if (string.Equals(extension, ".odc", StringComparison.OrdinalIgnoreCase))
                {
                    return ValidateOdcFile(filePath);
                }
                else if (string.Equals(extension, ".xml", StringComparison.OrdinalIgnoreCase))
                {
                    return ValidateXmlFile(filePath);
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool ValidateSdcFile(string filePath)
        {
            try
            {
                var doc = XDocument.Load(filePath);
                var nameElement = doc.Descendants("NAME").FirstOrDefault();
                if (nameElement != null)
                {
                    string elementValue = nameElement.Value;
                    string sha1Hash = CalculateSha1Hash(filePath);
                    string fileName = Path.GetFileName(filePath);

                    if (Path.GetFileName(filePath).ToUpper().Contains("_DAT.SDC"))
                    {
                        var archiveElement = doc.Descendants("ARCHIVE").FirstOrDefault();
                        if (archiveElement != null)
                        {
                            string archiveValue = archiveElement.Value;
                            string trimmedFileName = archiveValue.Replace("[CONTENT_SERVER_ROOT]", "").Replace(".sdat", ".sdc");
                        }
                    }

                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string CalculateSha1Hash(string filePath)
        {
            using (var sha1 = System.Security.Cryptography.SHA1.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = sha1.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
                }
            }
        }

        private bool ValidateOdcFile(string filePath)
        {
            try
            {
                var doc = XDocument.Load(filePath);
                var uuidElement = doc.Descendants("uuid").FirstOrDefault();
                if (uuidElement != null)
                {
                    string uuidValue = uuidElement.Value;
                    string sha1Hash = CalculateSha1Hash(filePath);
                    string fileName = Path.GetFileName(filePath);

                    if (Path.GetFileName(filePath).ToUpper().Contains("_DAT.ODC"))
                    {
                        var imageElement = doc.Descendants("small_image").FirstOrDefault();
                        if (imageElement == null)
                        {
                            imageElement = doc.Descendants("large_image").FirstOrDefault();
                        }

                        if (imageElement != null)
                        {
                            string imageValue = imageElement.Value;
                            string trimmedImageValue = imageValue.Replace("[THUMBNAIL_ROOT]small", "").Replace("[THUMBNAIL_ROOT]large", "").Replace(".png", "");
                            string finalName = $"Objects/{uuidValue}/object{trimmedImageValue}.odc";
                        }
                    }

                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool ValidateXmlFile(string filePath)
        {
            try
            {
                var doc = XDocument.Load(filePath);

                // For now, just return true without any specific checks
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        private void CDSDecrypterDragDropHandler(object sender, DragEventArgs e)
        {
            LogDebugInfo("CDS Decryption: Drag and Drop Initiated");
            TemporaryMessageHelper.ShowTemporaryMessage(CDSDecrypterDragAreaText, "Processing....", 1000000);

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedItems = (string[])e.Data.GetData(DataFormats.FileDrop);
                List<string> validFiles = new List<string>();
                var validExtensionsForFolders = new[] { ".sdc", ".odc", ".xml" }; // Extensions for files in folders
                var validExtensionsForFiles = new[] { ".sdc", ".odc", ".xml", ".bar" }; // Extensions for individual files

                foreach (var item in droppedItems)
                {
                    if (Directory.Exists(item))
                    {
                        var filesInDirectory = Directory.GetFiles(item, "*.*", SearchOption.AllDirectories)
                            .Where(file => validExtensionsForFolders.Contains(Path.GetExtension(file).ToLowerInvariant()))
                            .ToList();
                        validFiles.AddRange(filesInDirectory);
                    }
                    else if (File.Exists(item) && validExtensionsForFiles.Contains(Path.GetExtension(item).ToLowerInvariant()))
                    {
                        validFiles.Add(item);
                    }
                }

                int newFilesCount = 0;
                int duplicatesCount = 0;
                int displayTime = 2000;

                if (validFiles.Count > 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var existingFilesSet = new HashSet<string>(CDSDecrypterTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        int initialCount = existingFilesSet.Count;

                        existingFilesSet.UnionWith(validFiles);
                        newFilesCount = existingFilesSet.Count - initialCount;
                        duplicatesCount = validFiles.Count - newFilesCount;

                        CDSDecrypterTextBox.Text = string.Join(Environment.NewLine, existingFilesSet);

                        string message = $"{newFilesCount} file(s) added";
                        if (duplicatesCount > 0)
                        {
                            message += $", {duplicatesCount} duplicate(s) filtered";
                        }

                        TemporaryMessageHelper.ShowTemporaryMessage(CDSDecrypterDragAreaText, message, 2000);

                        LogDebugInfo($"CDS Decryption: {newFilesCount} files added, {duplicatesCount} duplicates filtered from Drag and Drop.");
                    });
                }
                else
                {
                    LogDebugInfo("CDS Decryption: Drag and Drop - No valid files found.");
                    string message = "No valid files found.";
                    TemporaryMessageHelper.ShowTemporaryMessage(CDSDecrypterDragAreaText, message, 2000);
                }
            }
            else
            {
                LogDebugInfo("CDS Decryption: Drag and Drop - No Data Present.");
                TemporaryMessageHelper.ShowTemporaryMessage(CDSDecrypterDragAreaText, "Drag and Drop operation failed - No Data Present.", 2000);
            }
        }



        private async void ClickToBrowseCDSDecryptHandler(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("CDS Decryption: Browsing for files Initiated");

            // Disable the main window to prevent interactions while the dialog is open
            this.IsEnabled = false;

            var openFileDialog = new VistaOpenFileDialog
            {
                Filter = "Supported files (*.sdc;*.odc;*.xml;*.bar)|*.sdc;*.odc;*.xml;*.bar",
                Multiselect = true
            };

            string message = "No files selected.";
            int displayTime = 2000; // Default to 2 seconds for the message display.

            bool? result = openFileDialog.ShowDialog();

            // Await a delay to absorb any unintentional clicks that happen after the dialog closes
            await Task.Delay(10); // 10 ms delay

            // Re-enable the main window after the delay
            this.IsEnabled = true;

            if (result == true)
            {
                var selectedFiles = openFileDialog.FileNames;
                var validExtensions = new[] { ".sdc", ".odc", ".xml", ".bar" }
                    .Select(ext => ext.ToLowerInvariant()).ToArray();

                List<string> validFiles = selectedFiles
                    .Where(file => validExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                    .ToList();

                if (validFiles.Any())
                {
                    Dispatcher.Invoke(() =>
                    {
                        var existingFilesSet = new HashSet<string>(CDSDecrypterTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        int initialCount = existingFilesSet.Count;
                        existingFilesSet.UnionWith(validFiles);
                        int newFilesCount = existingFilesSet.Count - initialCount;
                        int duplicatesCount = validFiles.Count - newFilesCount;

                        CDSDecrypterTextBox.Text = string.Join(Environment.NewLine, existingFilesSet);

                        string addedFilesMessage = newFilesCount > 0 ? $"{newFilesCount} file(s) added" : "No new files added";
                        string duplicatesMessage = duplicatesCount > 1 ? $", {duplicatesCount} duplicate(s) filtered" : "";
                        message = addedFilesMessage + duplicatesMessage;

                        LogDebugInfo($"CDS Decryption: {message} via File Browser");
                    });
                }
                else
                {
                    message = "No compatible files selected.";
                    LogDebugInfo("CDS Decryption: File Browser - No compatible files were selected.");
                }
            }
            else
            {
                message = "Dialog cancelled.";
                LogDebugInfo("CDS Decryption: File Browser - Dialog Cancelled.");
            }

            TemporaryMessageHelper.ShowTemporaryMessage(CDSDecrypterDragAreaText, message, 2000);
        }


        private Task<bool> DecryptFilesAsync(string[] filePaths)
        {
            LogDebugInfo($"CDS Decryption: Starting decryption for {filePaths.Length} file(s)");

            // TODO: Implement the actual decryption logic here
            // The following line is just a placeholder to simulate successful decryption.
            bool decryptionResult = true; // Simulate success for now

            if (decryptionResult)
            {
                LogDebugInfo("CDS Decryption: Decryption process completed successfully");
            }
            else
            {
                LogDebugInfo("CDS Decryption: Decryption process failed");
            }

            // Return true if decryption is successful, false otherwise
            return Task.FromResult(decryptionResult);
        }

        // TAB 3: Logic for packing SQL to HCDB

        // HCDB Encrypter execute button click
        private async void HCDBEncrypterExecuteButtonClick(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("HCDB Conversion: Process Initiated");
            TemporaryMessageHelper.ShowTemporaryMessage(HCDBEncrypterDragAreaText, "Processing....", 1000000);

            if (!Directory.Exists(_settings.HcdbOutputDirectory))
            {
                Directory.CreateDirectory(_settings.HcdbOutputDirectory);
                LogDebugInfo($"HCDB Conversion: Output directory created at {_settings.HcdbOutputDirectory}");
            }

            string filesToConvert = HCDBEncrypterTextBox.Text;
            if (!string.IsNullOrWhiteSpace(filesToConvert))
            {
                string[] filePaths = filesToConvert.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                string[] segsPaths = filePaths.Select(fp => fp + ".segs").ToArray(); // Assuming LZMA outputs files with .segs extension

                LogDebugInfo($"HCDB Conversion: Starting conversion for {filePaths.Length} files");
                bool conversionSuccess = await ConvertSqlToHcdbAsync(filePaths);

                if (conversionSuccess)
                {
                    bool encryptionSuccess = await EncryptHCDBFilesAsync(segsPaths, _settings.HcdbOutputDirectory); // Encrypt the .segs files
                    string message = encryptionSuccess ? $"Success: {filePaths.Length} Files Converted and Encrypted" : "Encryption Failed";
                    TemporaryMessageHelper.ShowTemporaryMessage(HCDBEncrypterDragAreaText, message, 2000);
                    LogDebugInfo($"HCDB Conversion: Result - {message}");
                }
                else
                {
                    string message = "Conversion Failed";
                    TemporaryMessageHelper.ShowTemporaryMessage(HCDBEncrypterDragAreaText, message, 2000);
                    LogDebugInfo($"HCDB Conversion: Result - {message}");
                }
            }
            else
            {
                LogDebugInfo("HCDB Conversion: Aborted - No SQL files listed for conversion.");
                TemporaryMessageHelper.ShowTemporaryMessage(HCDBEncrypterDragAreaText, "No SQL files listed for conversion.", 2000);
            }
        }


        private async Task<bool> ConvertSqlToHcdbAsync(string[] filePaths)
        {
            bool allFilesProcessed = true;
            string lzmaPath = @"dependencies\lzma_segs.exe";

            foreach (var filePath in filePaths)
            {
                string filename = Path.GetFileName(filePath);
                string expectedOutputPath = filePath + ".segs";  // Expecting LZMA to append ".segs" to the input file's name

                try
                {
                    // Process the file with LZMA without explicitly specifying the output path
                    if (!ExecuteLzmaProcess(lzmaPath, filePath))
                    {
                        LogDebugInfo($"Conversion failed for {filename}.");
                        allFilesProcessed = false;
                    }
                    else
                    {
                        LogDebugInfo($"Conversion successful for {filename}, output likely written to {expectedOutputPath}.");
                    }
                }
                catch (Exception ex)
                {
                    LogDebugInfo($"Error processing {filename}: {ex.Message}");
                    allFilesProcessed = false;
                }
            }

            return allFilesProcessed;
        }

        private bool ExecuteLzmaProcess(string lzmaPath, string inputFilePath)
        {
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = lzmaPath;
                    process.StartInfo.Arguments = $"\"{inputFilePath}\"";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();

                    // To log output or errors (optional but useful for debugging)
                    string output = process.StandardOutput.ReadToEnd();
                    string errors = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    // Check if the process exited successfully and the expected output file was created
                    if (process.ExitCode == 0 && File.Exists(inputFilePath + ".segs"))
                    {
                        LogDebugInfo($"LZMA compression successful for {Path.GetFileName(inputFilePath)}");
                        return true;
                    }
                    else
                    {
                        LogDebugInfo($"LZMA compression failed for {Path.GetFileName(inputFilePath)}: {errors}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error during LZMA compression: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EncryptHCDBFilesAsync(string[] filePaths, string baseOutputDirectory)
        {
            bool addSha1ToFilename = HCDBAddSHA1CheckBox.IsChecked ?? false;
            bool allFilesProcessed = true;
            foreach (var filePath in filePaths)
            {
                string filename = Path.GetFileName(filePath);
                // Remove ".sql" and ".segs" if present, then append ".hcdb"
                string cleanFilename = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(filename)) + ".hcdb";

                try
                {
                    byte[] fileContent = await File.ReadAllBytesAsync(filePath);
                    byte[]? encryptedContent = null;
                    string inputSHA1 = "";

                    using (SHA1 sha1 = SHA1.Create())
                    {
                        byte[] SHA1Data = sha1.ComputeHash(fileContent);
                        inputSHA1 = BitConverter.ToString(SHA1Data).Replace("-", ""); // Full SHA1 of input content
                        LogDebugInfo($"Input SHA1 for {cleanFilename}: {inputSHA1}");

                        string computedSha1 = inputSHA1.Substring(0, 16);
                        encryptedContent = CDSProcess.CDSEncrypt_Decrypt(fileContent, computedSha1, (int)CdnMode.RETAIL);
                    }

                    // Append SHA1 to the filename without extensions if requested
                    if (addSha1ToFilename && encryptedContent != null)
                    {
                        string filenameWithoutExtension = Path.GetFileNameWithoutExtension(cleanFilename);
                        cleanFilename = $"{filenameWithoutExtension}_{inputSHA1}.hcdb";  // Append SHA1 and new extension
                    }

                    if (encryptedContent != null)
                    {
                        string outputPath = Path.Combine(baseOutputDirectory, cleanFilename);
                        await File.WriteAllBytesAsync(outputPath, encryptedContent);
                        LogDebugInfo($"File {cleanFilename} encrypted and written to {outputPath}.");

                        // After successful encryption, delete the .segs file
                        File.Delete(filePath);
                        LogDebugInfo($"Temporary file {filePath} deleted after successful encryption.");
                    }
                    else
                    {
                        allFilesProcessed = false;
                        LogDebugInfo($"Encryption failed for {cleanFilename}, no data written.");
                    }
                }
                catch (Exception ex)
                {
                    LogDebugInfo($"Error encrypting {cleanFilename}: {ex.Message}");
                    allFilesProcessed = false;
                }
            }
            return allFilesProcessed;
        }



        private void HCDBEncrypterDragDropHandler(object sender, DragEventArgs e)
        {
            LogDebugInfo("HCDB Conversion: Drag and Drop Initiated");
            TemporaryMessageHelper.ShowTemporaryMessage(HCDBEncrypterDragAreaText, "Processing....", 1000000);

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedItems = (string[])e.Data.GetData(DataFormats.FileDrop);
                List<string> validFiles = new List<string>();
                string message = string.Empty;
                int displayTime = 2000; // Default to 2 seconds

                foreach (var item in droppedItems)
                {
                    if (Directory.Exists(item))
                    {
                        var filesInDirectory = Directory.GetFiles(item, "*.sql", SearchOption.AllDirectories).ToList();
                        validFiles.AddRange(filesInDirectory);
                    }
                    else if (File.Exists(item) && Path.GetExtension(item).ToLowerInvariant() == ".sql")
                    {
                        validFiles.Add(item);
                    }
                }

                if (validFiles.Count > 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var existingFilesSet = new HashSet<string>(HCDBEncrypterTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        int initialCount = existingFilesSet.Count;
                        existingFilesSet.UnionWith(validFiles); // Automatically removes duplicates
                        int newFilesCount = existingFilesSet.Count - initialCount;
                        int duplicatesCount = validFiles.Count - newFilesCount;

                        HCDBEncrypterTextBox.Text = string.Join(Environment.NewLine, existingFilesSet);

                        string addedFilesMessage = newFilesCount > 0 ? $"{newFilesCount} SQL files added" : "No new SQL files added";
                        string duplicatesMessage = duplicatesCount > 0 ? $", {duplicatesCount} duplicates filtered" : "";
                        message = addedFilesMessage + duplicatesMessage;

                        LogDebugInfo($"HCDB Conversion: {newFilesCount} SQL files added, {duplicatesCount} duplicates filtered");
                    });
                    displayTime = 1000; // Change display time since files were added
                }
                else
                {
                    message = "No SQL files found.";
                    LogDebugInfo("HCDB Conversion: No SQL files found in Drag and Drop.");
                }

                TemporaryMessageHelper.ShowTemporaryMessage(HCDBEncrypterDragAreaText, message, 2000);
            }
            else
            {
                LogDebugInfo("HCDB Conversion: Drag and Drop - No Data Present.");
                TemporaryMessageHelper.ShowTemporaryMessage(HCDBEncrypterDragAreaText, "Drag and Drop operation failed - No Data Present.", 2000);
            }
        }



        private async void ClickToBrowseHCDBEncryptHandler(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("HCDB Conversion: Browsing for SQL files Initiated");

            // Disable the main window to prevent interactions while the dialog is open
            this.IsEnabled = false;

            var openFileDialog = new VistaOpenFileDialog
            {
                Filter = "SQL files (*.sql)|*.sql",
                Multiselect = true
            };

            string message = "No files selected.";
            int displayTime = 2000; // Default to 2 seconds for the message display.

            bool? result = openFileDialog.ShowDialog();

            // Await a delay to absorb any unintentional clicks that happen after the dialog closes
            await Task.Delay(10); // 10 ms delay

            // Re-enable the main window after the delay
            this.IsEnabled = true;

            if (result == true)
            {
                var selectedFiles = openFileDialog.FileNames;

                if (selectedFiles.Any())
                {
                    Dispatcher.Invoke(() =>
                    {
                        var existingFilesSet = new HashSet<string>(HCDBEncrypterTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        int initialCount = existingFilesSet.Count;
                        existingFilesSet.UnionWith(selectedFiles); // Automatically removes duplicates
                        int newFilesCount = existingFilesSet.Count - initialCount;
                        int duplicatesCount = selectedFiles.Length - newFilesCount;

                        HCDBEncrypterTextBox.Text = string.Join(Environment.NewLine, existingFilesSet);

                        string addedFilesMessage = newFilesCount > 0 ? $"{newFilesCount} SQL files added" : "No new SQL files added";
                        string duplicatesMessage = duplicatesCount > 1 ? $", {duplicatesCount} duplicates filtered" : "";
                        message = addedFilesMessage + duplicatesMessage;

                        LogDebugInfo($"HCDB Conversion: {newFilesCount} SQL files added, {duplicatesCount} duplicates filtered via File Browser");
                    });
                    displayTime = 1000; // Change display time since files were added
                }
                else
                {
                    message = "No SQL files selected.";
                    LogDebugInfo("HCDB Conversion: No SQL files selected in File Browser.");
                }
            }
            else
            {
                LogDebugInfo("HCDB Conversion: File Browser - Dialog Cancelled.");
            }

            TemporaryMessageHelper.ShowTemporaryMessage(HCDBEncrypterDragAreaText, message, 2000);
        }



        // TAB 3: Logic for packing HCDB to SQL

        // HCDB Decrypter execute button click
        private async void HCDBDecrypterExecuteButtonClick(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("HCDB Decryption: Process Initiated");
            TemporaryMessageHelper.ShowTemporaryMessage(HCDBDecrypterDragAreaText, "Processing...", 2000);

            string outputDirectory = SqlOutputDirectoryTextBox.Text;
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
                LogDebugInfo($"HCDB Decryption: Output directory created at {outputDirectory}");
            }

            string filesToDecrypt = HCDBDecrypterTextBox.Text;
            string manualSha1 = HCDBDecrypterSha1TextBox.Text.Trim();
            bool manualSha1IsValid = !string.IsNullOrWhiteSpace(manualSha1) && manualSha1.Length == 40;

            if (!string.IsNullOrWhiteSpace(filesToDecrypt))
            {
                string[] filePaths = filesToDecrypt.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                bool decryptionSuccess = true;

                foreach (string filePath in filePaths)
                {
                    string filename = Path.GetFileName(filePath);
                    string detectedSha1 = Regex.Match(filename, @"[a-fA-F0-9]{40}").Value;
                    string effectiveSha1 = manualSha1IsValid ? manualSha1 : detectedSha1;
                    string cleanFilename = manualSha1IsValid ? filename.Replace(effectiveSha1, "") : filename;  // Remove SHA1 from filename if used

                    byte[]? decryptedData = null;
                    if (!string.IsNullOrWhiteSpace(effectiveSha1) && effectiveSha1.Length == 40)
                    {
                        LogDebugInfo($"Using SHA1 for {filename}: {effectiveSha1}. Starting decryption.");
                        decryptedData = await DecryptHCDBFilesSHA1Async(filePath, outputDirectory, effectiveSha1);
                    }
                    else
                    {
                        LogDebugInfo($"No valid SHA1 detected or provided for {filename}. Using standard decryption.");
                        decryptedData = await DecryptHCDBFilesAsync(filePath, outputDirectory);
                    }

                    if (decryptedData != null)
                    {
                        if (!await ProcessDecryptedHCDB(filePath, decryptedData, outputDirectory, cleanFilename))
                        {
                            decryptionSuccess = false;
                            LogDebugInfo($"Post-decryption processing failed for {filename}.");
                        }
                        else
                        {
                            LogDebugInfo($"Post-decryption processing succeeded for {filename}.");
                        }
                    }
                    else
                    {
                        decryptionSuccess = false;
                        LogDebugInfo($"Decryption failed for {filename}.");
                    }
                }

                string message = decryptionSuccess ? "Success: All HCDB files decrypted and processed" : "Decryption or processing failed for one or more files";
                TemporaryMessageHelper.ShowTemporaryMessage(HCDBDecrypterDragAreaText, message, 2000);
                LogDebugInfo($"HCDB Decryption: Result - {message}");
            }
            else
            {
                LogDebugInfo("HCDB Decryption: Aborted - No HCDB files listed for decryption.");
                TemporaryMessageHelper.ShowTemporaryMessage(HCDBDecrypterDragAreaText, "No HCDB files listed for decryption.", 2000);
            }
        }


        public async Task<byte[]?> DecryptHCDBFilesAsync(string filePath, string outputDirectory)
        {
            string filename = Path.GetFileName(filePath);
            try
            {
                byte[] fileContent = await File.ReadAllBytesAsync(filePath);
                LogDebugInfo($"File content read successfully for {filename}, starting decryption.");

                // Assuming decryption process
                BruteforceProcess? proc = new BruteforceProcess(fileContent);
                byte[]? decryptedContent = proc.StartBruteForce((int)CdnMode.RETAIL);  // Simplify, assuming this method does the decryption

                if (decryptedContent != null)
                {
                    LogDebugInfo($"Decryption successful for {filename}.");
                    return decryptedContent;
                }
                else
                {
                    LogDebugInfo($"Decryption process returned null for {filename}. No data written.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error processing {filename}: {ex.Message}");
                return null;
            }
        }



        public async Task<byte[]?> DecryptHCDBFilesSHA1Async(string filePath, string outputDirectory, string sha1Hash)
        {
            string filename = Path.GetFileName(filePath);
            try
            {
                byte[] fileContent = await File.ReadAllBytesAsync(filePath);
                LogDebugInfo($"Reading file content for {filename}.");

                // Using SHA1 hash to decrypt the file
                byte[]? processedContent = CDSProcess.CDSEncrypt_Decrypt(fileContent, sha1Hash.Substring(0, 16), (int)CdnMode.RETAIL);

                if (processedContent != null)
                {
                    LogDebugInfo($"Decryption successful for {filename} using SHA1.");
                    return processedContent;
                }
                else
                {
                    LogDebugInfo($"Decryption failed for {filename}. No output written.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error processing {filename}: {ex.Message}");
                return null;
            }
        }



        private async Task<bool> ProcessDecryptedHCDB(string filePath, byte[] decryptedData, string outputDirectory, string filename)
        {
            try
            {
                // If SHA1 was part of the filename, remove it before saving
                string baseFilename = Path.GetFileNameWithoutExtension(filename);
                string cleanFilename = Regex.Replace(baseFilename, "[a-fA-F0-9]{40}", ""); // Remove SHA1 hash
                cleanFilename = cleanFilename.TrimEnd('_') + ".SQL"; // Ensure it ends with ".sql"

                byte[]? processedData = HCDBUnpack(decryptedData, LogDebugInfo);
                if (processedData != null)
                {
                    string outputPath = Path.Combine(outputDirectory, cleanFilename);
                    await File.WriteAllBytesAsync(outputPath, processedData);
                    LogDebugInfo($"Processed HCDB file successfully written to {outputPath}.");

                    // Validate the processed SQL file
                    if (IsValidSqlFile(outputPath))
                    {
                        LogDebugInfo($"Validation passed for SQL file at {outputPath}.");
                        return true;
                    }
                    else
                    {
                        File.Delete(outputPath);
                        LogDebugInfo($"Invalid SQL file deleted at {outputPath}.");
                        return false;
                    }
                }
                else
                {
                    LogDebugInfo($"Failed to process decrypted data for {filename}.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error processing decrypted data for {filename}: {ex.Message}");
                return false;
            }
        }


        public static byte[]? HCDBUnpack(byte[] decryptedData, Action<string> log)
        {
            try
            {
                byte[]? decompressedData = CompressionLibrary.Edge.LZMA.Decompress(decryptedData, true);

                if (decompressedData != null && decompressedData.Length >= 4 &&
                    decompressedData[0] != 0x73 && decompressedData[1] != 0x65 &&
                    decompressedData[2] != 0x67 && decompressedData[3] != 0x73)
                {
                    return decompressedData; // Returns the decompressed data array
                }
                else
                {
                    log("Decompression failed or data did not start with the expected header.");
                    return null; // Returns null if decompression fails or header is incorrect
                }
            }
            catch (Exception ex)
            {
                log($"Error during HCDB unpacking: {ex.Message}");
                return null;
            }
        }

        private bool IsValidSqlFile(string filePath)
        {
            // Expected header bytes for "SQLite" files
            byte[] expectedHeader = { 0x53, 0x51, 0x4C, 0x69, 0x74, 0x65 };

            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] fileHeader = new byte[expectedHeader.Length];
                    int bytesRead = fs.Read(fileHeader, 0, fileHeader.Length);

                    // Check if read bytes match the expected header
                    if (bytesRead == expectedHeader.Length)
                    {
                        for (int i = 0; i < expectedHeader.Length; i++)
                        {
                            if (fileHeader[i] != expectedHeader[i])
                            {
                                LogDebugInfo($"SQL file validation failed: Header does not match at {filePath}.");
                                return false;
                            }
                        }
                        return true;  // Header matches
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error validating SQL file at {filePath}: {ex.Message}");
            }

            LogDebugInfo($"SQL file validation failed: Insufficient header bytes at {filePath}.");
            return false;  // Header does not match or an error occurred
        }



        // HCDB Decrypter drag and drop handler
        private void HCDBDecrypterDragDropHandler(object sender, DragEventArgs e)
        {
            LogDebugInfo("HCDB to SQL Conversion: Drag and Drop Initiated");
            TemporaryMessageHelper.ShowTemporaryMessage(HCDBDecrypterDragAreaText, "Processing....", 1000000);

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedItems = (string[])e.Data.GetData(DataFormats.FileDrop);
                List<string> validFiles = new List<string>();
                string message = string.Empty;
                int displayTime = 2000; // Default to 2 seconds

                foreach (var item in droppedItems)
                {
                    if (Directory.Exists(item))
                    {
                        var filesInDirectory = Directory.GetFiles(item, "*.hcdb", SearchOption.AllDirectories).ToList();
                        validFiles.AddRange(filesInDirectory);
                    }
                    else if (File.Exists(item) && Path.GetExtension(item).ToLowerInvariant() == ".hcdb")
                    {
                        validFiles.Add(item);
                    }
                }

                if (validFiles.Count > 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var existingFilesSet = new HashSet<string>(HCDBDecrypterTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        int initialCount = existingFilesSet.Count;
                        existingFilesSet.UnionWith(validFiles);
                        int newFilesCount = existingFilesSet.Count - initialCount;
                        int duplicatesCount = validFiles.Count - newFilesCount;

                        HCDBDecrypterTextBox.Text = string.Join(Environment.NewLine, existingFilesSet);

                        string addedFilesMessage = newFilesCount > 0 ? $"{newFilesCount} HCDB files added" : "No new HCDB files added";
                        string duplicatesMessage = duplicatesCount > 0 ? $", {duplicatesCount} duplicates filtered" : "";
                        message = addedFilesMessage + duplicatesMessage;

                        LogDebugInfo($"HCDB to SQL Conversion: {newFilesCount} HCDB files added, {duplicatesCount} duplicates filtered");
                    });
                    displayTime = 1000; // Change display time since files were added
                }
                else
                {
                    message = "No HCDB files found.";
                    LogDebugInfo("HCDB to SQL Conversion: No HCDB files found in Drag and Drop.");
                }

                TemporaryMessageHelper.ShowTemporaryMessage(HCDBDecrypterDragAreaText, message, 2000);
            }
            else
            {
                LogDebugInfo("HCDB to SQL Conversion: Drag and Drop - No Data Present.");
                TemporaryMessageHelper.ShowTemporaryMessage(HCDBDecrypterDragAreaText, "Drag and Drop operation failed - No Data Present.", 2000);
            }
        }


        private async void ClickToBrowseHCDBDecryptHandler(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("HCDB to SQL Conversion: Browsing for HCDB files Initiated");

            // Disable the main window to prevent interactions while the dialog is open
            this.IsEnabled = false;

            var openFileDialog = new VistaOpenFileDialog
            {
                Filter = "HCDB files (*.hcdb)|*.hcdb",
                Multiselect = true
            };

            string message = "No files selected.";
            int displayTime = 2000; // Default to 2 seconds for the message display.

            bool? result = openFileDialog.ShowDialog();

            // Await a short delay to absorb any unintentional clicks that happen after the dialog closes
            await Task.Delay(10); // Using a 10 ms delay 

            // Re-enable the main window after the delay
            this.IsEnabled = true;

            if (result == true)
            {
                var selectedFiles = openFileDialog.FileNames;

                if (selectedFiles.Any())
                {
                    Dispatcher.Invoke(() =>
                    {
                        var existingFilesSet = new HashSet<string>(HCDBDecrypterTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        int initialCount = existingFilesSet.Count;
                        existingFilesSet.UnionWith(selectedFiles); // Automatically removes duplicates
                        int newFilesCount = existingFilesSet.Count - initialCount;
                        int duplicatesCount = selectedFiles.Length - newFilesCount;

                        HCDBDecrypterTextBox.Text = string.Join(Environment.NewLine, existingFilesSet);

                        string addedFilesMessage = newFilesCount > 0 ? $"{newFilesCount} HCDB files added" : "No new HCDB files added";
                        string duplicatesMessage = duplicatesCount > 1 ? $", {duplicatesCount} duplicates filtered" : "";
                        message = addedFilesMessage + duplicatesMessage;

                        LogDebugInfo($"HCDB to SQL Conversion: {newFilesCount} HCDB files added{duplicatesMessage} via File Browser");
                    });
                    displayTime = 1000; // Change display time since files were added
                }
                else
                {
                    message = "No HCDB files selected.";
                    LogDebugInfo("HCDB to SQL Conversion: No HCDB files selected in File Browser.");
                }
            }
            else
            {
                message = "Dialog cancelled.";
                LogDebugInfo("HCDB to SQL Conversion: File Browser - Dialog Cancelled.");
            }

            TemporaryMessageHelper.ShowTemporaryMessage(HCDBDecrypterDragAreaText, message, 2000);
        }
        private Task<bool> ConvertHcdbToSqlAsync(string[] filePaths)
        {
            LogDebugInfo($"HCDB to SQL Conversion: Starting conversion for {filePaths.Length} HCDB file(s)");

            // TODO: Implement the actual conversion logic here
            bool conversionResult = true; // Simulate success for now

            if (conversionResult)
            {
                LogDebugInfo("HCDB to SQL Conversion: Conversion process completed successfully");
            }
            else
            {
                LogDebugInfo("HCDB to SQL Conversion: Conversion process failed");
            }

            return Task.FromResult(conversionResult);
        }


        // TAB 5: Scene IDs 

        private void SceneIDEncrypt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string input = SceneIDnumberInputTextBox.Text;
                bool isLegacyMode = legacyModeCheckBox.IsChecked ?? false;
                StringBuilder output = new StringBuilder();

                if (input.Contains('-'))
                {
                    var parts = input.Split('-');
                    if (parts.Length == 2 &&
                        ushort.TryParse(parts[0], out ushort start) &&
                        ushort.TryParse(parts[1], out ushort end) &&
                        start >= 1 && end <= 65535 && start <= end)
                    {
                        for (ushort i = start; i <= end; i++)
                        {
                            SceneKey key = isLegacyMode ? SIDKeyGenerator.Instance.Generate(i)
                                                         : SIDKeyGenerator.Instance.GenerateNewerType(i);
                            output.AppendLine($"{i}: {key.ToString()}");
                        }
                        encryptedSceneIDTextBox.Text = output.ToString();
                    }
                    else
                    {
                        encryptedSceneIDTextBox.Text = "Please enter a valid range (e.g., 1-50).\n";
                    }
                }
                else if (ushort.TryParse(input, out ushort sceneID) && sceneID >= 1 && sceneID <= 65535)
                {
                    SceneKey key = isLegacyMode ? SIDKeyGenerator.Instance.Generate(sceneID)
                                                 : SIDKeyGenerator.Instance.GenerateNewerType(sceneID);
                    encryptedSceneIDTextBox.Text = $"{sceneID}: {key.ToString()}";
                }
                else
                {
                    encryptedSceneIDTextBox.Text = "Please enter a valid number between 1 and 65535\nor a range (e.g., 1-5000).\n";
                }
            }
            catch (Exception ex)
            {
                encryptedSceneIDTextBox.Text += $"Error during encryption: {ex.Message}\n";
            }
        }



        private void SceneIDDecrypt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string input = SceneIDDecryptInputTextBox.Text;
                bool isLegacyMode = decrypterLegacyModeCheckBox.IsChecked ?? false;
                StringBuilder output = new StringBuilder();

                // Regular expression to match GUID format
                string guidPattern = @"\b[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\b";
                MatchCollection matches = Regex.Matches(input, guidPattern);

                foreach (Match match in matches)
                {
                    string encryptedID = match.Value;
                    if (Guid.TryParse(encryptedID, out Guid sceneGuid))
                    {
                        SceneKey key = new SceneKey(sceneGuid);
                        ushort sceneID = isLegacyMode ? SIDKeyGenerator.Instance.ExtractSceneID(key)
                                                      : SIDKeyGenerator.Instance.ExtractSceneIDNewerType(key);
                        output.AppendLine($"{encryptedID}: {sceneID}");
                    }
                    else
                    {
                        output.AppendLine($"{encryptedID}: Invalid SceneID");
                    }
                }

                DecryptedSceneIDTextBox.Text = output.ToString();
            }
            catch (Exception ex)
            {
                DecryptedSceneIDTextBox.Text += $"Error during decryption: {ex.Message}\n";
            }
        }


        private void SceneIDTabItem_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                StringBuilder inputText = new StringBuilder();

                foreach (string file in files)
                {
                    if (Path.GetExtension(file).Equals(".txt", StringComparison.OrdinalIgnoreCase) ||
                        Path.GetExtension(file).Equals(".xml", StringComparison.OrdinalIgnoreCase) ||
                        Path.GetExtension(file).Equals(".lua", StringComparison.OrdinalIgnoreCase))
                    {
                        string fileContent = File.ReadAllText(file);
                        inputText.AppendLine(fileContent);
                    }
                }

                SceneIDDecryptInputTextBox.Text = inputText.ToString();

                // Optionally, automatically click the Decrypt button
                SceneIDDecrypt_Click(this, new RoutedEventArgs());
            }
        }

        private void ClearSceneIDTextHandler(object sender, RoutedEventArgs e)
        {
            // Clear the input and output text boxes
            SceneIDDecryptInputTextBox.Text = string.Empty;
            DecryptedSceneIDTextBox.Text = string.Empty;
            LogDebugInfo("SceneID Decrypter: List Cleared");
        }




        // TAB 6: Logic for Compiling LUA to LUAC

        private bool isParseOnly = false;
        private bool isStripDebug = false;
        private CancellationTokenSource _luaCompilerCancellationTokenSource;


        private async void LUACompilerExecuteButtonClick(object sender, RoutedEventArgs e)
        {
            _luaCompilerCancellationTokenSource?.Cancel();
            _luaCompilerCancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = _luaCompilerCancellationTokenSource.Token;

            LogDebugInfo("LUA Compilation to LUAC: Process Started");
            AppendTextToLUATextBox(Environment.NewLine + Environment.NewLine);

            string filesToCompile = LUACompilerTextBox.Text;
            if (!string.IsNullOrWhiteSpace(filesToCompile))
            {
                string[] allLines = filesToCompile.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                var validFilePaths = allLines
                    .Where(line => !line.TrimStart().StartsWith("--") &&
                                   !line.TrimStart().StartsWith("##") &&
                                   (line.EndsWith(".lua", StringComparison.OrdinalIgnoreCase) ||
                                    line.EndsWith(".LUA", StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                var existingFiles = validFilePaths.Where(File.Exists).ToList();
                var nonExistingFiles = validFilePaths.Except(existingFiles).ToList();

                int totalSuccessCount = 0;
                int totalFailureCount = 0;

                string compilerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dependencies", "HomeLuaC.exe");



                if (!isParseOnly && !Directory.Exists(_settings.LuacOutputDirectory))
                {
                    Directory.CreateDirectory(_settings.LuacOutputDirectory);
                    LogDebugInfo($"LUA Compilation to LUAC: Output directory created at {_settings.LuacOutputDirectory}");
                    TemporaryMessageHelper.ShowTemporaryMessage(LUACompilerDragAreaText, $"Created {_settings.LuacOutputDirectory}", 1000);

                }

                if (existingFiles.Any())
                {
                    foreach (string file in existingFiles)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            LUACompilerTextBox.Clear();
                            break;
                        }

                        if (isParseOnly)
                        {
                            TemporaryMessageHelper.ShowTemporaryMessage(LUACompilerDragAreaText, $"Parsing {Path.GetFileName(file)}...", 1000);

                            var (successCount, failureCount, parseResult) = await LUACompilerParseOnlyAsync(compilerPath, file, cancellationToken);
                            totalSuccessCount += successCount;
                            totalFailureCount += failureCount;
                            AppendTextToLUATextBox(parseResult);
                        }
                        else
                        {
                            TemporaryMessageHelper.ShowTemporaryMessage(LUACompilerDragAreaText, $"Compiling {Path.GetFileName(file)}...", 1000);

                            var compileResult = await LUACompilerCompileFileAsync(file, isStripDebug, cancellationToken);
                            AppendTextToLUATextBox(compileResult);

                            if (compileResult.Contains("-- Compile Success:"))
                            {
                                totalSuccessCount++;
                            }
                            else if (compileResult.Contains("--- ERROR:"))
                            {
                                totalFailureCount++;
                            }
                        }
                    }

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        string summaryMessage = isParseOnly ?
                            $"{totalSuccessCount} File(s) Parsed Successfully, {totalFailureCount} File(s) Failed to Parse" :
                            $"{totalSuccessCount} File(s) Compiled Successfully, {totalFailureCount} File(s) Failed to Compile";
                        TemporaryMessageHelper.ShowTemporaryMessage(LUACompilerDragAreaText, summaryMessage, 3000);
                    }
                }

                if (nonExistingFiles.Any() && !cancellationToken.IsCancellationRequested)
                {
                    string missingFilesMessage = string.Join(Environment.NewLine, nonExistingFiles.Select(file => "--- ERROR: File Not Found " + file));
                    LogDebugInfo("LUA Compilation to LUAC: Some files could not be found");
                    TemporaryMessageHelper.ShowTemporaryMessage(LUACompilerDragAreaText, "Some files could not be found", 3000);
                    AppendTextToLUATextBox(missingFilesMessage);
                }
            }
            else
            {
                LogDebugInfo("LUA Compilation to LUAC: Failed Initialisation as no input files received");
                TemporaryMessageHelper.ShowTemporaryMessage(LUACompilerDragAreaText, "No LUA files listed for compilation.", 3000);
            }
        }




        private async void AppendTextToLUATextBox(string text)
        {
            // If called from a non-UI thread, use Dispatcher
            await Dispatcher.InvokeAsync(async () =>
            {
                LUACompilerTextBox.AppendText(text);

                // Introduce a short delay
                await Task.Delay(20); // Delay for 100 milliseconds

                // Scrolls the text box to the end after appending text
                LUACompilerTextBox.ScrollToEnd();
            });
        }

        private async void StopandClearLUAListHandler(object sender, RoutedEventArgs e)
        {
            // Cancel the ongoing process
            _luaCompilerCancellationTokenSource?.Cancel();

            // Clear the LUACompilerTextBox
            LUACompilerTextBox.Clear();

            // Reset UI elements if necessary
            TemporaryMessageHelper.ShowTemporaryMessage(LUACompilerDragAreaText, "List Cleared", 2000);
        }


        private async Task<string> LUACompilerCompileFileAsync(string file, bool stripDebug, CancellationToken cancellationToken)
        {
            StringBuilder compileResult = new StringBuilder();

            string outputDirectory = _settings.LuacOutputDirectory;
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            string compilerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dependencies", "HomeLuaC.exe");


            if (!File.Exists(compilerPath))
            {
                compileResult.AppendLine("Error: Compiler executable not found at " + compilerPath);
                return compileResult.ToString();
            }

            if (string.IsNullOrWhiteSpace(file) || file.TrimStart().StartsWith("--") || file.TrimStart().StartsWith("##"))
            {
                return "";
            }

            if (!Path.GetExtension(file).Equals(".lua", StringComparison.OrdinalIgnoreCase))
            {
                return $"--- Warning: {Path.GetFileName(file)} is not a LUA file. Skipped.";
            }

            string outputFileName = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(file) + ".LUAC");
            bool wasRenamed = false;

            switch (_settings.FileOverwriteBehavior)
            {
                case OverwriteBehavior.Rename:
                    int counter = 1;
                    string baseOutputFileName = outputFileName;
                    while (File.Exists(baseOutputFileName))
                    {
                        baseOutputFileName = Path.Combine(outputDirectory, $"{Path.GetFileNameWithoutExtension(file)}_{counter:D2}.LUAC");
                        counter++;
                    }
                    if (baseOutputFileName != outputFileName)
                    {
                        wasRenamed = true;
                        outputFileName = baseOutputFileName;
                    }
                    break;
                case OverwriteBehavior.Skip:
                    if (File.Exists(outputFileName))
                    {
                        return $"\n-- Skipping {Path.GetFileName(file)} as output already exists";
                    }
                    break;
                case OverwriteBehavior.Overwrite:
                default:
                    break;
            }

            string arguments = $"-o \"{outputFileName}\" \"{file}\"";
            if (stripDebug)
            {
                arguments = "-s " + arguments;
            }

            compileResult.AppendLine($"-- Compiling: {Path.GetFileName(file)}...");

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = compilerPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = startInfo })
            {
                process.Start();
                await process.WaitForExitAsync();

                if (cancellationToken.IsCancellationRequested)
                {
                    return ""; // Exit if cancellation is requested
                }

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                if (File.Exists(outputFileName))
                {
                    string successMessage = $"-- Compile Success: '{outputFileName}' Created.";
                    if (wasRenamed)
                    {
                        successMessage += " (Renamed)";
                    }
                    compileResult.AppendLine(successMessage);
                }
                else
                {
                    string formattedError = ProcessCompileLUAErrorMessage(error);
                    compileResult.AppendLine($"--- ERROR: Compiling '{Path.GetFileName(file)}' failed. {formattedError}");
                }
            }

            return compileResult.ToString();
        }


        private string ProcessCompileLUAErrorMessage(string errorMessage)
        {
            int lastIndex = errorMessage.LastIndexOf(':');
            if (lastIndex >= 0)
            {
                int secondLastIndex = errorMessage.LastIndexOf(':', lastIndex - 1);
                if (secondLastIndex >= 0)
                {
                    string errorPart = errorMessage.Substring(secondLastIndex + 1).Trim();
                    return $"at Line {errorPart}";
                }
            }
            return errorMessage;
        }



        private async Task<(int successCount, int failureCount, string resultText)> LUACompilerParseOnlyAsync(string compilerPath, string file, CancellationToken cancellationToken)
        {
            LogDebugInfo($"LUA Compilation to LUAC (Parse Only): Processing {file}");

            int successCount = 0;
            int failureCount = 0;
            StringBuilder compileResults = new StringBuilder();

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = compilerPath,
                Arguments = $"-p \"{file}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = startInfo })
            {
                process.Start();
                await process.WaitForExitAsync();

                if (cancellationToken.IsCancellationRequested)
                {
                    return (0, 0, ""); // Exit if cancellation is requested
                }

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                // Log the full error message for debugging purposes
                if (!string.IsNullOrWhiteSpace(error))
                {
                    LogDebugInfo($"LUA Compilation to LUAC (Parse Only): Error while parsing {file}: {error}");
                }

                string standardVersionMessage = "Playstation Home version of luac compiled at 14:36:10 on Jan 26 2009.";
                string[] outputLines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                if (outputLines.Length == 1 && outputLines[0].Trim() == standardVersionMessage && string.IsNullOrWhiteSpace(error))
                {
                    // Successful parsing
                    successCount++;
                    compileResults.AppendLine($"-- Parse Success: {Path.GetFileName(file)} No syntax errors found.");
                }
                else
                {
                    // Failure detected
                    failureCount++;

                    // Process error message to remove the full file path and shorten the message
                    string shortErrorMessage = ProcessLUAPARSEErrorMessage(error);
                    compileResults.AppendLine($"--- ERROR Parsing {Path.GetFileName(file)} {shortErrorMessage}");
                }
            }

            return (successCount, failureCount, compileResults.ToString());
        }


        private string ProcessLUAPARSEErrorMessage(string errorMessage)
        {
            // Find the last colon
            int lastIndex = errorMessage.LastIndexOf(':');

            if (lastIndex >= 0)
            {
                // Find the second-to-last colon before the last colon
                int secondLastIndex = errorMessage.LastIndexOf(':', lastIndex - 1);

                if (secondLastIndex >= 0)
                {
                    // Extract the relevant part of the error message
                    string errorPart = errorMessage.Substring(secondLastIndex + 1).Trim();

                    // Return the formatted error message
                    return $"at Line {errorPart}";
                }
            }

            // If the format is not as expected, return the original message
            return errorMessage;
        }




        private void LuaDragDropHandler(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedItems = (string[])e.Data.GetData(DataFormats.FileDrop);
                List<string> validFiles = new List<string>();
                string message = string.Empty;
                int displayTime = 2000; // Default to 2 seconds

                int newFilesCount = 0; // Declare outside of Dispatcher.Invoke
                int duplicatesCount = 0; // Declare outside of Dispatcher.Invoke

                foreach (var item in droppedItems)
                {
                    if (Directory.Exists(item))
                    {
                        var filesInDirectory = Directory.GetFiles(item, "*.lua", SearchOption.AllDirectories).ToList();
                        if (filesInDirectory.Count > 0)
                        {
                            validFiles.AddRange(filesInDirectory);
                        }
                    }
                    else if (File.Exists(item) && Path.GetExtension(item).ToLowerInvariant() == ".lua")
                    {
                        validFiles.Add(item);
                    }
                }

                if (validFiles.Count > 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var existingFilesSet = new HashSet<string>(LUACompilerTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        int initialCount = existingFilesSet.Count;
                        existingFilesSet.UnionWith(validFiles);
                        newFilesCount = existingFilesSet.Count - initialCount;
                        duplicatesCount = validFiles.Count - newFilesCount;

                        LUACompilerTextBox.Text = string.Join(Environment.NewLine, existingFilesSet);
                    });
                    displayTime = 2000; // Change display time since files were added
                }
                else
                {
                    LogDebugInfo("LUA Compilation to LUAC: Dropped Files/Folders scanned - No LUA files found.");
                    message = "No LUA files found.";
                }

                // Construct the message after Dispatcher.Invoke has executed
                string addedFilesMessage = newFilesCount > 0 ? $"{newFilesCount} LUA files added" : "No new LUA files added";
                string duplicatesMessage = duplicatesCount > 1 ? $", {duplicatesCount} duplicates filtered" : "";
                message = addedFilesMessage + duplicatesMessage;
                string logPrefix = "LUA Compilation to LUAC: Drag and Drop - ";
                LogDebugInfo(logPrefix + addedFilesMessage); // This will log "Drag and Drop Info: X LUA files added" or "Drag and Drop Info: No new LUA files added"
                if (duplicatesCount > 0)
                {
                    // Ensure we clean up the message by trimming any leading comma and space
                    string cleanDuplicatesMessage = duplicatesMessage.TrimStart(',', ' ').Trim();
                    LogDebugInfo(logPrefix + cleanDuplicatesMessage); // This will log "Drag and Drop Info: X duplicates filtered" if there are any duplicates
                }

                TemporaryMessageHelper.ShowTemporaryMessage(LUACompilerDragAreaText, message, 2000);
            }
        }


        private async void ClickToBrowseHandlerLUACompiler(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("LUA Compilation: Browsing for LUA files Initiated");

            // Disable the main window to prevent interactions while the dialog is open
            this.IsEnabled = false;

            var openFileDialog = new VistaOpenFileDialog
            {
                Filter = "LUA files (*.lua)|*.lua",
                Multiselect = true
            };

            string message = "No files selected.";
            int displayTime = 2000; // Default to 2 seconds for the message display.

            bool? result = openFileDialog.ShowDialog();

            // Await a delay to absorb any unintentional clicks that happen after the dialog closes
            await Task.Delay(10); // 10 ms delay

            // Re-enable the main window after the delay
            this.IsEnabled = true;

            if (result == true)
            {
                var selectedFiles = openFileDialog.FileNames;

                if (selectedFiles.Any())
                {
                    int newFilesCount = 0; // Declare outside of Dispatcher.Invoke
                    int duplicatesCount = 0; // Declare outside of Dispatcher.Invoke

                    Dispatcher.Invoke(() =>
                    {
                        var existingFilesSet = new HashSet<string>(LUACompilerTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        int initialCount = existingFilesSet.Count;
                        existingFilesSet.UnionWith(selectedFiles);
                        newFilesCount = existingFilesSet.Count - initialCount;
                        duplicatesCount = selectedFiles.Length - newFilesCount;

                        LUACompilerTextBox.Text = string.Join(Environment.NewLine, existingFilesSet);
                    });

                    // Construct the message outside of the Dispatcher.Invoke block using the counts
                    string addedFilesMessage = newFilesCount > 0 ? $"{newFilesCount} LUA files added" : "No new LUA files added";
                    string duplicatesMessage = duplicatesCount > 1 ? $", {duplicatesCount} duplicates filtered" : "";
                    message = addedFilesMessage + duplicatesMessage;

                    displayTime = 1000; // Change display time since files were added
                    string logPrefix = "LUA Compilation to LUAC: Click to Browse - ";
                    LogDebugInfo(logPrefix + addedFilesMessage); // Logs added files
                    if (duplicatesCount > 0)
                    {
                        LogDebugInfo(logPrefix + $"{duplicatesCount} duplicates filtered"); // Logs duplicates if any
                    }
                }
                else
                {
                    LogDebugInfo("LUA Compilation to LUAC: Click to Browse - No LUA files were selected.");
                    message = "No LUA files selected.";
                }
            }
            else
            {
                LogDebugInfo("LUA Compilation to LUAC: File Browser - Dialog Cancelled.");
                message = "Dialog cancelled.";
            }

            TemporaryMessageHelper.ShowTemporaryMessage(LUACompilerDragAreaText, message, 2000);
        }


        private void LUACompilerTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Assuming LUACOutputDirectoryTextBox is the name of your TextBox containing the output directory path
            string outputDirectory = LuacOutputDirectoryTextBox.Text;

            if (!string.IsNullOrEmpty(outputDirectory) && Directory.Exists(outputDirectory))
            {
                try
                {
                    // Open the directory in File Explorer
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName = outputDirectory,
                        UseShellExecute = true,
                        Verb = "open"
                    });
                }
                catch (Exception ex)
                {
                    // Handle any exceptions (e.g., directory does not exist)
                    LogDebugInfo($"LUA Compilation to LUAC: Unable to open directory: {ex.Message}");
                    MessageBox.Show($"Unable to open directory: {ex.Message}");
                }
            }
            else
            {
                LogDebugInfo($"LUA Compilation to LUAC: Output directory {outputDirectory} is not set or does not exist.");
                MessageBox.Show("Output directory is not set or does not exist.");
            }
        }

        private void CheckBoxLUACParseOnly_Checked(object sender, RoutedEventArgs e)
        {
            // Update the 'isParseOnly' property when the checkbox is checked
            isParseOnly = true;
            // If Parse Only is checked, we want to uncheck Strip Debug
            isStripDebug = false;
            CheckBoxStripDebug.IsChecked = false;
        }

        private void CheckBoxLUACParseOnly_Unchecked(object sender, RoutedEventArgs e)
        {
            // Update the 'isParseOnly' property when the checkbox is unchecked
            isParseOnly = false;
        }

        private void CheckBoxLUACStripDebug_Checked(object sender, RoutedEventArgs e)
        {
            // Update the 'isStripDebug' property when the checkbox is checked
            isStripDebug = true;
            // If Strip Debug is checked, we want to uncheck Parse Only
            isParseOnly = false;
            CheckBoxParseOnly.IsChecked = false;
        }

        private void CheckBoxLUACStripDebug_Unchecked(object sender, RoutedEventArgs e)
        {
            // Update the 'isStripDebug' property when the checkbox is unchecked
            isStripDebug = false;
        }




        // TAB 6: Logic for Decompiling LUAC to LUA

        private bool isNewTask = true; // Flag to check if a new task has started
        private string unluac122Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dependencies", "unluac122.jar");
        private string unluac2023Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dependencies", "unluac_2023_12_24.jar");
        private string unluacNETPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dependencies", "unluac.exe");


        private string currentUnluacJarPath = "";


        // Event handlers for the radio buttons
        private void LUACDecompilerRadioButtonUnluac122_Checked(object sender, RoutedEventArgs e)
        {
            currentUnluacJarPath = unluac122Path;
        }

        private void LUACDecompilerRadioButtonUnluac2023_Checked(object sender, RoutedEventArgs e)
        {
            currentUnluacJarPath = unluac2023Path;
        }

        private void LUACDecompilerRadioButtonUnluacNET_Checked(object sender, RoutedEventArgs e)
        {
            currentUnluacJarPath = unluacNETPath;
        }

        // Modified LUACDecompilerExecuteButtonClick method
        private async void LUACDecompilerExecuteButtonClick(object sender, RoutedEventArgs e)
        {
            isNewTask = true;
            LogDebugInfo("LUAC Decompilation to LUA: Process Started");
            TemporaryMessageHelper.ShowTemporaryMessage(LUACDecompilerDragAreaText, "Initializing...", 3000);

            string unluacJarPath = currentUnluacJarPath;
            if (!File.Exists(unluacJarPath) && !unluacJarPath.Equals(unluacNETPath))
            {
                LogDebugInfo($"LUAC Decompilation to LUA: Error: {unluacJarPath} was not found.");
                TemporaryMessageHelper.ShowTemporaryMessage(LUACDecompilerDragAreaText, $"Error: {unluacJarPath} was not found.", 3000);
                return;
            }

            if (!Directory.Exists(_settings.LuaOutputDirectory))
            {
                Directory.CreateDirectory(_settings.LuaOutputDirectory);
                LogDebugInfo($"LUAC Decompilation to LUA: Output directory created at {_settings.LuaOutputDirectory}");
                TemporaryMessageHelper.ShowTemporaryMessage(LUACDecompilerDragAreaText, $"Created {_settings.LuaOutputDirectory}", 1000);
            }

            string filesToDecompile = LUACDecompilerTextBox.Text;
            if (!string.IsNullOrWhiteSpace(filesToDecompile))
            {
                string[] filePaths = filesToDecompile
                    .Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(line => !line.TrimStart().StartsWith("--") && !line.TrimStart().StartsWith("##") && line.EndsWith(".luac", StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                int successCount = 0, failureCount = 0;

                foreach (string file in filePaths)
                {
                    TemporaryMessageHelper.ShowTemporaryMessage(LUACDecompilerDragAreaText, $"Decompiling {Path.GetFileName(file)}...", 10000);

                    var (isSuccess, message) = unluacJarPath.Equals(unluacNETPath) ?
                        await DecompileLuacFileWithExeAsync(file, unluacJarPath) :
                        await DecompileLuacFileAsync(file, unluacJarPath);

                    AppendTextToLUACDecompilerTextBox(message);

                    if (isSuccess)
                    {
                        successCount++;
                    }
                    else
                    {
                        failureCount++;
                    }
                }

                string summaryMessage = $"-- {successCount} file(s) decompiled successfully, {failureCount} file(s) failed/skipped.";
                AppendTextToLUACDecompilerTextBox(summaryMessage);
                LogDebugInfo($"LUAC Decompilation to LUA: {summaryMessage}");
                TemporaryMessageHelper.ShowTemporaryMessage(LUACDecompilerDragAreaText, summaryMessage, 6000);
            }
            else
            {
                LogDebugInfo("LUAC Decompilation to LUA: No input files were provided for decompilation.");
                TemporaryMessageHelper.ShowTemporaryMessage(LUACDecompilerDragAreaText, "No LUAC files listed for decompilation.", 3000);
            }
        }


        private async Task<(bool isSuccess, string message)> DecompileLuacFileWithExeAsync(string inputFile, string unluacExePath)
        {
            LogDebugInfo($"LUAC Decompilation: Starting decompilation for {inputFile}");

            string baseOutputFileName = Path.Combine(_settings.LuaOutputDirectory, Path.GetFileNameWithoutExtension(inputFile) + ".lua");
            string outputFileName = baseOutputFileName;
            bool renamed = false;

            switch (_settings.FileOverwriteBehavior)
            {
                case OverwriteBehavior.Rename:
                    int counter = 1;
                    while (File.Exists(outputFileName))
                    {
                        outputFileName = Path.Combine(_settings.LuaOutputDirectory, $"{Path.GetFileNameWithoutExtension(inputFile)}_{counter:D2}.lua");
                        counter++;
                        renamed = true;
                    }
                    break;
                case OverwriteBehavior.Skip:
                    if (File.Exists(outputFileName))
                    {
                        LogDebugInfo($"LUAC Decompilation: SKIPPED '{inputFile}' because '{outputFileName}' already exists.");
                        return (false, $"-- SKIPPED: {Path.GetFileName(inputFile)} output already exists - See settings to change");
                    }
                    break;
                case OverwriteBehavior.Overwrite:
                default:
                    break;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = unluacExePath,
                Arguments = $"\"{inputFile}\" \"{outputFileName}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            try
            {
                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    bool exited = await Task.Run(() => process.WaitForExit(3000)); // 3-second timeout

                    if (!exited)
                    {
                        process.Kill();
                        LogDebugInfo($"LUAC Decompilation: Timeout occurred for {inputFile}");
                        return (false, $"--- ERROR: Decompilation timed out for '{Path.GetFileName(inputFile)}'.");
                    }

                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();

                    if (process.ExitCode == 0)
                    {
                        // Check if the output file exists
                        if (File.Exists(outputFileName))
                        {
                            LogDebugInfo($"LUAC Decompilation: Successfully decompiled {inputFile} to {outputFileName}");
                            return (true, $"-- Success: Decompiled '{Path.GetFileName(inputFile)}' to '{outputFileName}'" + (renamed ? " (Renamed)." : "."));
                        }
                        else
                        {
                            LogDebugInfo($"LUAC Decompilation: Output file '{outputFileName}' not found after decompilation for {inputFile}");
                            return (false, $"--- ERROR: Output file '{outputFileName}' not found after decompilation.");
                        }
                    }
                    else
                    {
                        LogDebugInfo($"LUAC Decompilation: Decompilation failed for {inputFile}");
                        return (false, $"--- ERROR: Decompiling '{Path.GetFileName(inputFile)}' failed. Error: {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"LUAC Decompilation: Exception occurred for {inputFile}: {ex.Message}");
                return (false, $"--- ERROR: Exception while decompiling '{Path.GetFileName(inputFile)}'. Exception: {ex.Message}");
            }
        }



        private async Task<(bool isSuccess, string message)> DecompileLuacFileAsync(string inputFile, string unluacJarPath)
        {
            LogDebugInfo($"LUAC Decompilation: Starting decompilation for {inputFile}");

            string baseOutputFileName = Path.Combine(_settings.LuaOutputDirectory, Path.GetFileNameWithoutExtension(inputFile) + ".lua");
            string outputFileName = baseOutputFileName;
            bool renamed = false;

            switch (_settings.FileOverwriteBehavior)
            {
                case OverwriteBehavior.Rename:
                    int counter = 1;
                    while (File.Exists(outputFileName))
                    {
                        outputFileName = Path.Combine(_settings.LuaOutputDirectory, $"{Path.GetFileNameWithoutExtension(inputFile)}_{counter:D2}.lua");
                        counter++;
                        renamed = true;
                    }
                    break;
                case OverwriteBehavior.Skip:
                    if (File.Exists(outputFileName))
                    {
                        return (false, $"-- Decompiling: {Path.GetFileName(inputFile)}... Skipped '{outputFileName}' already exists.");
                    }
                    break;
                case OverwriteBehavior.Overwrite:
                default:
                    break;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "java",
                Arguments = $"-jar \"{unluacJarPath}\" \"{inputFile}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            try
            {
                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    bool exited = await Task.Run(() => process.WaitForExit(2000)); // 1-second timeout

                    if (!exited)
                    {
                        process.Kill();
                        LogDebugInfo($"LUAC Decompilation: Timeout occurred for {inputFile}");
                        return (false, $"--- ERROR: Decompilation timed out for '{Path.GetFileName(inputFile)}'.");
                    }

                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();

                    if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                    {
                        await File.WriteAllTextAsync(outputFileName, output);
                        if (File.Exists(outputFileName))
                        {
                            LogDebugInfo($"LUAC Decompilation: Successfully decompiled {inputFile} to {outputFileName}");
                            return (true, $"-- Success: Decompiled '{Path.GetFileName(inputFile)}' to '{outputFileName}'" + (renamed ? " (Renamed)." : "."));
                        }
                        else
                        {
                            LogDebugInfo($"LUAC Decompilation: Output file not found after decompilation for {inputFile}");
                            return (false, $"--- ERROR: Output file '{outputFileName}' not found after decompilation.");
                        }
                    }
                    else
                    {
                        LogDebugInfo($"LUAC Decompilation: Decompilation failed for {inputFile}");
                        return (false, $"--- ERROR: Decompiling '{Path.GetFileName(inputFile)}' failed. Error: {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"LUAC Decompilation: Exception occurred for {inputFile}: {ex.Message}");
                return (false, $"--- ERROR: Exception while decompiling '{Path.GetFileName(inputFile)}'. Exception: {ex.Message}");
            }
        }



        private void AppendTextToLUACDecompilerTextBox(string text)
        {
            // If called from a non-UI thread, use Dispatcher
            Dispatcher.InvokeAsync(async () =>
            {
                // Check if this is a new task and prepend two new lines if it is
                if (isNewTask && !string.IsNullOrEmpty(LUACDecompilerTextBox.Text))
                {
                    LUACDecompilerTextBox.AppendText(Environment.NewLine + Environment.NewLine);
                    isNewTask = false; // Reset flag as this is no longer a new task
                }

                LUACDecompilerTextBox.AppendText(text + Environment.NewLine);

                // Introduce a short delay
                await Task.Delay(20); // Delay for 20 milliseconds

                // Scrolls the text box to the end after appending text
                LUACDecompilerTextBox.ScrollToEnd();
            });
        }





        private void LuacDragDropHandler(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedItems = (string[])e.Data.GetData(DataFormats.FileDrop);
                List<string> validFiles = new List<string>();
                string message = string.Empty;
                int displayTime = 2000; // Default to 2 seconds

                foreach (var item in droppedItems)
                {
                    if (Directory.Exists(item))
                    {
                        var filesInDirectory = Directory.GetFiles(item, "*.luac", SearchOption.AllDirectories).ToList();
                        if (filesInDirectory.Count > 0)
                        {
                            validFiles.AddRange(filesInDirectory);
                        }
                    }
                    else if (File.Exists(item) && Path.GetExtension(item).ToLowerInvariant() == ".luac")
                    {
                        validFiles.Add(item);
                    }
                }

                int newFilesCount = 0;
                int duplicatesCount = 0;

                if (validFiles.Count > 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var existingFilesSet = new HashSet<string>(LUACDecompilerTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        int initialCount = existingFilesSet.Count;
                        existingFilesSet.UnionWith(validFiles);
                        newFilesCount = existingFilesSet.Count - initialCount;
                        duplicatesCount = validFiles.Count - newFilesCount;

                        LUACDecompilerTextBox.Text = string.Join(Environment.NewLine, existingFilesSet);
                    });
                    displayTime = 2000; // Change display time since files were added
                    LogDebugInfo($"LUAC Decompilation to LUA: {newFilesCount} new files added for decompilation.");
                    if (duplicatesCount > 0)
                    {
                        LogDebugInfo($"LUAC Decompilation to LUA: {duplicatesCount} duplicate files ignored.");
                    }
                }
                else
                {
                    LogDebugInfo("LUAC Decompilation to LUA: No valid .luac files found to decompile.");
                    message = "No LUAC files found";
                }

                // Construct the message after Dispatcher.Invoke has executed
                string addedFilesMessage = newFilesCount > 0 ? $"{newFilesCount} LUAC files added" : "No new LUAC files added";
                string duplicatesMessage = duplicatesCount > 1 ? $", {duplicatesCount} duplicates filtered" : "";
                message = addedFilesMessage + duplicatesMessage;

                // Log final message
                LogDebugInfo("LUAC Decompilation to LUA: Drag and Drop - " + message);

                TemporaryMessageHelper.ShowTemporaryMessage(LUACDecompilerDragAreaText, message, 2000);
            }
        }


        private async void ClickToBrowseHandlerLUACDecompiler(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("LUAC Decompilation to LUA: Browsing for LUAC files Initiated");

            // Disable the main window to prevent interactions while the dialog is open
            this.IsEnabled = false;

            var openFileDialog = new VistaOpenFileDialog
            {
                Filter = "LUAC files (*.luac)|*.luac",
                Multiselect = true
            };

            string message = "No files selected.";
            int displayTime = 2000; // Default to 2 seconds for the message display.

            bool? result = openFileDialog.ShowDialog();

            // Await a delay to absorb any unintentional clicks that happen after the dialog closes
            await Task.Delay(10); // 10 ms delay

            // Re-enable the main window after the delay
            this.IsEnabled = true;

            if (result == true)
            {
                var selectedFiles = openFileDialog.FileNames;

                if (selectedFiles.Any())
                {
                    int newFilesCount = 0;
                    int duplicatesCount = 0;

                    Dispatcher.Invoke(() =>
                    {
                        var existingFilesSet = new HashSet<string>(LUACDecompilerTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        int initialCount = existingFilesSet.Count;
                        existingFilesSet.UnionWith(selectedFiles);
                        newFilesCount = existingFilesSet.Count - initialCount;
                        duplicatesCount = selectedFiles.Length - newFilesCount;

                        LUACDecompilerTextBox.Text = string.Join(Environment.NewLine, existingFilesSet);
                    });

                    string addedFilesMessage = newFilesCount > 0 ? $"{newFilesCount} LUAC files added" : "No new LUAC files added";
                    string duplicatesMessage = duplicatesCount > 1 ? $", {duplicatesCount} duplicates filtered" : "";
                    message = addedFilesMessage + duplicatesMessage;
                    displayTime = 1000; // Change display time since files were added

                    string logPrefix = "LUAC Decompilation to LUA: Click to Browse - ";
                    LogDebugInfo(logPrefix + addedFilesMessage);
                    if (duplicatesCount > 0)
                    {
                        LogDebugInfo(logPrefix + $"{duplicatesCount} duplicates filtered");
                    }
                }
                else
                {
                    LogDebugInfo("LUAC Decompilation to LUA: Click to Browse - No LUAC files were selected.");
                    message = "No LUAC files selected.";
                }
            }
            else
            {
                message = "Dialog cancelled.";
                LogDebugInfo("LUAC Decompilation to LUA: File Browser - Dialog Cancelled.");
            }

            TemporaryMessageHelper.ShowTemporaryMessage(LUACDecompilerDragAreaText, message, 2000);
        }


        private void LUACDecompilerTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Assuming LUAOutputDirectoryTextBox is the name of your TextBox containing the output directory path
            string outputDirectory = LuaOutputDirectoryTextBox.Text;

            if (!string.IsNullOrEmpty(outputDirectory) && Directory.Exists(outputDirectory))
            {
                try
                {
                    // Open the directory in File Explorer
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName = outputDirectory,
                        UseShellExecute = true,
                        Verb = "open"
                    });
                    LogDebugInfo($"LUAC Decompilation to LUA: Opened output directory {outputDirectory}");
                }
                catch (Exception ex)
                {
                    // Handle any exceptions
                    LogDebugInfo($"LUAC Decompilation to LUA: ERROR: Unable to open directory - {ex.Message}");
                    MessageBox.Show($"Unable to open directory: {ex.Message}");
                }
            }
            else
            {
                LogDebugInfo($"LUAC Decompilation to LUA: ERROR: Directory {outputDirectory} is not set or does not exist.");
                MessageBox.Show("Output directory is not set or does not exist.");
            }
        }

        // TAB 8: Logic for SDC Creation

        private async void CreateSdcButton_Click(object sender, RoutedEventArgs e)
        {
            // Gather data from the form
            string name = sdcNameTextBox.Text;
            string description = sdcDescriptionTextBox.Text;
            string maker = sdcMakerTextBox.Text;
            string hdkVersion = sdcHDKVersionTextBox.Text;
            string archivePath = sdcServerArchivePathTextBox.Text;
            string archiveSize = sdcArchiveSizeTextBox.Text;
            string timestamp = sdcTimestampTextBox.Text;
            string thumbnailSuffix = sdcThumbnailSuffixTextBox.Text;

            // Get the text from sdcServerArchivePathTextBox
            string sdcServerArchivePath = sdcServerArchivePathTextBox.Text;

            // Trim "Scenes/" off the start if it exists
            if (sdcServerArchivePath.StartsWith("Scenes/", StringComparison.OrdinalIgnoreCase))
            {
                sdcServerArchivePath = sdcServerArchivePath.Substring(7);
            }

            // Replace ".BAR" or ".sdat" with ".sdc" (case insensitive)
            sdcServerArchivePath = Regex.Replace(sdcServerArchivePath, @"(\.BAR|\.sdat)$", ".sdc", RegexOptions.IgnoreCase);

            // Write the result to txtSdcPath
            txtSdcPath.Text = sdcServerArchivePath;

            // Extract the name for txtSdcName
            string sdcName = string.Empty;

            // Find the first occurrence of '/' or '\'
            int slashIndex = sdcServerArchivePath.IndexOfAny(new char[] { '/', '\\' });
            if (slashIndex != -1)
            {
                sdcName = sdcServerArchivePath.Substring(0, slashIndex);
            }
            else
            {
                // If no '/' or '\' is found, find the first occurrence of '.'
                int dotIndex = sdcServerArchivePath.IndexOf('.');
                if (dotIndex != -1)
                {
                    sdcName = sdcServerArchivePath.Substring(0, dotIndex);
                }
            }

            // Write the result to txtSdcName
            txtSdcName.Text = sdcName;
            EncryptAndSetChannelID();

            // Pad the thumbnail suffix to 3 digits if it is not empty
            if (!string.IsNullOrEmpty(thumbnailSuffix))
            {
                thumbnailSuffix = thumbnailSuffix.PadLeft(3, '0');
            }

            // Write the version to txtVersion with leading zeros trimmed off
            if (string.IsNullOrEmpty(thumbnailSuffix))
            {
                txtSDCversion.Text = "0";
            }
            else
            {
                txtSDCversion.Text = thumbnailSuffix.TrimStart('0');
            }
            // Construct the thumbnail image file names
            string makerImageSuffix = string.IsNullOrEmpty(thumbnailSuffix) ? "" : $"_T{thumbnailSuffix}";
            string smallImageSuffix = string.IsNullOrEmpty(thumbnailSuffix) ? "" : $"_T{thumbnailSuffix}";
            string largeImageSuffix = string.IsNullOrEmpty(thumbnailSuffix) ? "" : $"_T{thumbnailSuffix}";

            string makerImage = $"[THUMBNAIL_ROOT]maker{makerImageSuffix}.png";
            string smallImage = $"[THUMBNAIL_ROOT]small{smallImageSuffix}.png";
            string largeImage = $"[THUMBNAIL_ROOT]large{largeImageSuffix}.png";

            // Extract the SDC file name from the archive path and handle special suffix
            string archiveFileName = System.IO.Path.GetFileNameWithoutExtension(archivePath);
            string sdcFileName = ConvertToSdcFileName(archiveFileName, thumbnailSuffix);

            // Create the XML document
            XDocument sdcXml = new XDocument(
                new XElement("XML", new XAttribute("hdk_version", hdkVersion),
                    new XElement("SDC_VERSION", "1.0"),
                    new XElement("LANGUAGE", new XAttribute("REGION", "en-GB"),
                        new XElement("NAME", name),
                        new XElement("DESCRIPTION", description),
                        new XElement("MAKER", maker),
                        new XElement("MAKER_IMAGE", makerImage),
                        new XElement("SMALL_IMAGE", smallImage),
                        new XElement("LARGE_IMAGE", largeImage),

                        // Conditionally add the ARCHIVES element
                        (offlineSDCCheckBox.IsChecked != true) ?
                            new XElement("ARCHIVES", CreateArchiveElement(archivePath, archiveSize, timestamp)) : null
                    ),

                    // Duplicated language sections
                    AddDuplicatedLanguageSection("en-US", name, description, archivePath, archiveSize, timestamp),
                    AddDuplicatedLanguageSection("en-SG", name, description, archivePath, archiveSize, timestamp),
                    AddDuplicatedLanguageSection("en-CA", name, description, archivePath, archiveSize, timestamp),

                    AddDuplicatedLanguageSection("fr-FR", name, description, archivePath, archiveSize, timestamp),
                    AddDuplicatedLanguageSection("de-DE", name, description, archivePath, archiveSize, timestamp),
                    AddDuplicatedLanguageSection("it-IT", name, description, archivePath, archiveSize, timestamp),
                    AddDuplicatedLanguageSection("es-ES", name, description, archivePath, archiveSize, timestamp),
                    AddDuplicatedLanguageSection("ja-JP", name, description, archivePath, archiveSize, timestamp),
                    AddDuplicatedLanguageSection("ko-KR", name, description, archivePath, archiveSize, timestamp),
                    AddDuplicatedLanguageSection("zh-TW", name, description, archivePath, archiveSize, timestamp),
                    AddDuplicatedLanguageSection("zh-HK", name, description, archivePath, archiveSize, timestamp),

                    new XElement("age_rating", new XAttribute("minimum_age", "0"), new XAttribute("parental_control_level", "1"))
                )
            );

            // Save the XML to a temporary file
            string tempFilePath = Path.Combine(Path.GetTempPath(), sdcFileName);
            sdcXml.Save(tempFilePath);

            // Calculate SHA1 hash and update the text box
            string sha1Hash = await Task.Run(() => CalculateSha1Hash(tempFilePath));
            Createdsdcsha1TextBox.Text = sha1Hash;
            txtSdcsha1Digest.Text = sha1Hash;
            // Read the content of the temporary file and set it to the sdcCreatorTextbox
            string sdcContent = await File.ReadAllTextAsync(tempFilePath);
            SDCCreatorTextbox.Text = sdcContent;

            byte[] fileContent;

            // Check if encryption is needed
            if (EncryptSDCCheckBox.IsChecked == true)
            {
                fileContent = await EncryptSDCODCFileAsync(tempFilePath);
            }
            else
            {
                fileContent = await File.ReadAllBytesAsync(tempFilePath);
            }

            // Show save file dialog for final file
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "SDC files (*.sdc)|*.sdc",
                DefaultExt = "sdc",
                FileName = sdcFileName
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                await File.WriteAllBytesAsync(saveFileDialog.FileName, fileContent);
            }
            LoadSceneListXMLv2intoEditorButton_Click(sender, e);
            // Clean up temporary file
            File.Delete(tempFilePath);
        }

        private string ConvertToSdcFileName(string archiveFileName, string thumbnailSuffix)
        {
            // Remove _TXXX suffix if it exists
            string sdcFileName = Regex.Replace(archiveFileName, "_T\\d{3}$", "");

            // Replace .sdat with .sdc
            sdcFileName = sdcFileName + ".sdc";

            // Append thumbnail suffix if it exists
            if (!string.IsNullOrEmpty(thumbnailSuffix))
            {
                sdcFileName = sdcFileName.Replace(".sdc", $"_T{thumbnailSuffix}.sdc");
            }

            return sdcFileName;
        }



        private XElement CreateArchiveElement(string archivePath, string archiveSize, string timestamp)
        {
            var archiveElement = new XElement("ARCHIVE", $"[CONTENT_SERVER_ROOT]{archivePath}");

            if (!string.IsNullOrEmpty(archiveSize))
            {
                archiveElement.Add(new XAttribute("size", archiveSize));
            }

            if (!string.IsNullOrEmpty(timestamp))
            {
                archiveElement.Add(new XAttribute("timestamp", timestamp));
            }

            return archiveElement;
        }

        private async Task<byte[]> EncryptSDCODCFileAsync(string filePath)
        {
            try
            {
                byte[] fileContent = await File.ReadAllBytesAsync(filePath);

                // Generate SHA1 hash from file content before encryption
                using (SHA1 sha1 = SHA1.Create())
                {
                    byte[] SHA1Data = sha1.ComputeHash(fileContent);
                    string inputSHA1 = BitConverter.ToString(SHA1Data).Replace("-", "").ToLowerInvariant();

                    // Encrypt the file content
                    string computedSha1 = inputSHA1.Substring(0, 16); // Use first 16 characters for encryption key
                    byte[] encryptedContent = CDSProcess.CDSEncrypt_Decrypt(fileContent, computedSha1, (int)CdnMode.RETAIL);

                    return encryptedContent;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }



        // Function to add a language placeholder
        private XElement AddLanguagePlaceholder(string languageCode)
        {
            // Create the archive element conditionally
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            XElement archivesElement = (offlineSDCCheckBox.IsChecked != true) ?
                new XElement("ARCHIVES",
                    new XElement("ARCHIVE",
                        new XAttribute("size", sdcArchiveSizeTextBox.Text),
                        new XAttribute("timestamp", sdcTimestampTextBox.Text),
                        $"[CONTENT_SERVER_ROOT]{sdcServerArchivePathTextBox.Text}")
                ) : null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            // Create and return the language element
            return new XElement("LANGUAGE", new XAttribute("REGION", languageCode),
                new XElement("NAME", ""),
                new XElement("DESCRIPTION", ""),
                archivesElement
            );
        }

        private XElement AddDuplicatedLanguageSection(string languageCode, string name, string description, string archivePath, string archiveSize, string timestamp)
        {
            // Create the archive element conditionally
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            XElement archivesElement = (offlineSDCCheckBox.IsChecked != true) ?
                new XElement("ARCHIVES",
                    new XElement("ARCHIVE",
                        new XAttribute("size", archiveSize),
                        new XAttribute("timestamp", timestamp),
                        $"[CONTENT_SERVER_ROOT]{archivePath}")
                ) : null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            // Create and return the language element
            return new XElement("LANGUAGE",
                new XAttribute("REGION", languageCode),
                new XElement("NAME", name),
                new XElement("DESCRIPTION", description),
                archivesElement
            );
        }

        public event PropertyChangedEventHandler? PropertyChanged = delegate { };

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl && e.AddedItems.Count > 0)
            {
                // Assuming each TabItem's Tag property is set to the corresponding enum value
                var selectedTab = (TabItem)e.AddedItems[0];
                if (Enum.TryParse<RememberLastTabUsed>(selectedTab.Tag.ToString(), out var lastTabUsed))
                {
                    _settings.LastTabUsed = lastTabUsed;
                    SettingsManager.SaveSettings(_settings);
                }
            }
        }


        private void SelectLastUsedTab(RememberLastTabUsed lastTabUsed)
        {
            switch (lastTabUsed)
            {
                case RememberLastTabUsed.ArchiveTool:
                    MainTabControl.SelectedIndex = 0; // Index of ArchiveTool tab
                    break;
                case RememberLastTabUsed.CDSTool:
                    MainTabControl.SelectedIndex = 1; // Index of CDSTool tab
                    break;
                case RememberLastTabUsed.HCDBTool:
                    MainTabControl.SelectedIndex = 2; // Index of HCDBTool tab
                    break;
                case RememberLastTabUsed.SQLTool:
                    MainTabControl.SelectedIndex = 3; // Index of SQLTool tab
                    break;
                case RememberLastTabUsed.TickLSTTool:
                    MainTabControl.SelectedIndex = 4; // Index of TickLSTTool tab
                    break;
                case RememberLastTabUsed.SceneIDTool:
                    MainTabControl.SelectedIndex = 5; // Index of SceneIDTool tab
                    break;
                case RememberLastTabUsed.LUACTool:
                    MainTabControl.SelectedIndex = 6; // Index of LUACTool tab
                    break;
                case RememberLastTabUsed.SDCODCTool:
                    MainTabControl.SelectedIndex = 7; // Index of SDCODCTool tab
                    break;
                case RememberLastTabUsed.Path2Hash:
                    MainTabControl.SelectedIndex = 8; // Index of Path2Hash tab
                    break;
                case RememberLastTabUsed.EbootPatcher:
                    MainTabControl.SelectedIndex = 9; // Index of EbootPatcher tab
                    break;
                case RememberLastTabUsed.SHAChecker:
                    MainTabControl.SelectedIndex = 10; // Index of SHAChecker tab
                    break;
                case RememberLastTabUsed.MediaTool:
                    MainTabControl.SelectedIndex = 11; // Index of Video tab
                    break;
                default:
                    MainTabControl.SelectedIndex = 0; // Default to ArchiveTool tab if none is matched
                    break;
            }
        }




        private void sdcServerArchivePathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void sdcHDKVersionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void sdcMakerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void sdcDescriptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void sdcNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TabContent_IsVisibleChanged(object sender, TextChangedEventArgs e)
        {

        }

        // TAB 8: Logic for SDC Creation

        private async void ODCCreateODC_Click(object sender, RoutedEventArgs e)
        {

            // Generate a UUID if odcUUIDTextBox is empty
            if (string.IsNullOrEmpty(odcUUIDTextBox.Text))
            {
                odcUUIDTextBox.Text = GenerateUUID();
            }
            txtObjectId.Text = odcUUIDTextBox.Text;
            txtArchiveTimeStampHex.Text = odcTimestampTextBox.Text;
            // Gather data from the form
            string name = odcNameTextBox.Text;
            string description = odcDescriptionTextBox.Text;
            string maker = odcMakerTextBox.Text;
            string hdkVersion = odcHDKVersionTextBox.Text;
            string thumbnailSuffix = odcThumbnailSuffixTextBox.Text;
            string uuid = odcUUIDTextBox.Text;
            string timestamp = odcTimestampTextBox.Text;

            // Pad the thumbnail suffix to 3 digits if it is not empty
            if (!string.IsNullOrEmpty(thumbnailSuffix))
            {
                thumbnailSuffix = thumbnailSuffix.PadLeft(3, '0');
            }

            // Write the version to txtVersion with leading zeros trimmed off
            if (string.IsNullOrEmpty(thumbnailSuffix))
            {
                txtVersion.Text = "0";
            }
            else
            {
                txtVersion.Text = thumbnailSuffix.TrimStart('0');
            }
            // Construct the thumbnail image file names
            string makerImage = $"[THUMBNAIL_ROOT]maker{(string.IsNullOrEmpty(thumbnailSuffix) ? "" : $"_T{thumbnailSuffix}")}.png";
            string smallImage = $"[THUMBNAIL_ROOT]small{(string.IsNullOrEmpty(thumbnailSuffix) ? "" : $"_T{thumbnailSuffix}")}.png";
            string largeImage = $"[THUMBNAIL_ROOT]large{(string.IsNullOrEmpty(thumbnailSuffix) ? "" : $"_T{thumbnailSuffix}")}.png";

            // Define the language codes and default values for name and description
            string[] languageCodes = new string[] { "en-GB", "fr-FR", "de-DE", "it-IT", "es-ES", "ja-JP", "ko-KR", "en-SG", "zh-HK", "zh-TW", "en-US" };
            bool[] defaultValues = new bool[] { true, false, false, false, false, false, false, false, false, false, false };

            // Create the legal section
            XElement legalSection = new XElement("legal",
                new XElement("age_rating", new XAttribute("minimum_age", "0"), new XAttribute("parental_control_level", "1")),
                new XElement("age_rating", new XAttribute("country", "US"), new XAttribute("minimum_age", "0"), new XAttribute("parental_control_level", "1")),
                new XElement("age_rating", new XAttribute("country", "JP"), new XAttribute("minimum_age", "0"), new XAttribute("parental_control_level", "1")),
                new XElement("age_rating", new XAttribute("country", "KR"), new XAttribute("minimum_age", "0"), new XAttribute("parental_control_level", "1")),
                new XElement("age_rating", new XAttribute("country", "SG"), new XAttribute("minimum_age", "0"), new XAttribute("parental_control_level", "1")),
                new XElement("age_rating", new XAttribute("country", "CA"), new XAttribute("minimum_age", "0"), new XAttribute("parental_control_level", "1"))
            );

            // Create the XML document
            XDocument odcXml = new XDocument(
                new XElement("odc",
                    new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                    new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema"),
                    new XAttribute("hdk_version", hdkVersion),
                    new XAttribute("version", "1.0"),
                    new XElement("version", "1.0"),
                    new XElement("uuid", uuid),
                    // Add name elements for different languages
                    languageCodes.Select((code, index) =>
                        new XElement("name",
                            new XAttribute("lang", code),
                            new XAttribute("default", defaultValues[index].ToString().ToLower()),
                            name)),
                    // Add description elements for different languages
                    languageCodes.Select((code, index) =>
                        new XElement("description",
                            new XAttribute("lang", code),
                            new XAttribute("default", defaultValues[index].ToString().ToLower()),
                            description)),
                    new XElement("maker", new XAttribute("lang", "en-GB"), new XAttribute("default", "true"), maker),
                    new XElement("maker_image", new XAttribute("lang", "en-GB"), new XAttribute("default", "true"), makerImage),
                    new XElement("small_image", new XAttribute("lang", "en-GB"), new XAttribute("default", "true"), smallImage),
                    new XElement("large_image", new XAttribute("lang", "en-GB"), new XAttribute("default", "true"), largeImage),
                    new XElement("entitlements", string.Empty),
                    legalSection,
                    // Conditionally add the timestamp element
                    !string.IsNullOrEmpty(timestamp) ? new XElement("timestamp", timestamp) : null
                )
            );

            // Construct the output file name
            string outputFileName = $"{uuid}{(string.IsNullOrEmpty(thumbnailSuffix) ? "" : $"_T{thumbnailSuffix}")}.odc";

            // Save the XML to a temporary file
            string tempFilePath = Path.Combine(Path.GetTempPath(), outputFileName);
            odcXml.Save(tempFilePath);

            // Calculate SHA1 hash and update the text box
            string sha1Hash = await Task.Run(() => CalculateSDCODCSha1Hash(tempFilePath));
            Createdodcsha1TextBox.Text = sha1Hash;
            txtOdcSha1Digest.Text = sha1Hash;
            // Read the content of the temporary file and set it to the ODCCreatorTextbox
            string odcContent = await File.ReadAllTextAsync(tempFilePath);
            ODCCreatorTextbox.Text = odcContent;

            byte[] fileContent;

            // Check if encryption is needed
            if (EncryptODCCheckBox.IsChecked == true)
            {
                fileContent = await EncryptSDCODCFileAsync(tempFilePath);
            }
            else
            {
                fileContent = await File.ReadAllBytesAsync(tempFilePath);
            }

            // Show save file dialog for final file
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "ODC files (*.odc)|*.odc",
                DefaultExt = "odc",
                FileName = outputFileName
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                await File.WriteAllBytesAsync(saveFileDialog.FileName, fileContent);
            }

            // Clean up temporary file
            File.Delete(tempFilePath);
        }

        private string CalculateSDCODCSha1Hash(string filePath)
        {
            using (var sha1 = System.Security.Cryptography.SHA1.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = sha1.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
                }
            }
        }

        private void CreateOdcClearButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear each TextBox by setting its Text property to an empty string
            odcNameTextBox.Text = string.Empty;
            odcDescriptionTextBox.Text = string.Empty;
            odcMakerTextBox.Text = "Sony Computer Entertainment"; // Reset to default text
            odcHDKVersionTextBox.Text = "1.86.0.17.PUB"; // Reset to default text
            odcThumbnailSuffixTextBox.Text = string.Empty;
            odcUUIDTextBox.Text = string.Empty;
            odcTimestampTextBox.Text = "FFFFFFFF"; // Reset to default text
            Createdodcsha1TextBox.Text = string.Empty;
            ODCCreatorTextbox.Text = string.Empty;
        }
        private void CreateSdcClearButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear each TextBox by setting its Text property to an empty string
            sdcNameTextBox.Text = string.Empty;
            sdcDescriptionTextBox.Text = string.Empty;
            sdcMakerTextBox.Text = "Sony Computer Entertainment"; // Reset to default text
            sdcHDKVersionTextBox.Text = "1.86.0.17.PUB"; // Reset to default text
            sdcThumbnailSuffixTextBox.Text = string.Empty;
            sdcServerArchivePathTextBox.Text = "Scenes/"; // Reset to default text
            sdcTimestampTextBox.Text = "FFFFFFFF"; // Reset to default text
            sdcArchiveSizeTextBox.Text = string.Empty;
            Createdsdcsha1TextBox.Text = string.Empty;
            SDCCreatorTextbox.Text = string.Empty;

            // Reset CheckBox states
            offlineSDCCheckBox.IsChecked = false;
            EncryptSDCCheckBox.IsChecked = false;
        }

        private async void SDCCopySHA1Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Createdsdcsha1TextBox.Text))
            {
                string originalText = Createdsdcsha1TextBox.Text;
                Clipboard.SetText(originalText);
                Createdsdcsha1TextBox.Text = "SHA1 Copied";
                await Task.Delay(500);
                Createdsdcsha1TextBox.Text = originalText;
            }
        }

        private async void ODCCopySHA1Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Createdodcsha1TextBox.Text))
            {
                string originalText = Createdodcsha1TextBox.Text;
                Clipboard.SetText(originalText);
                Createdodcsha1TextBox.Text = "SHA1 Copied";
                await Task.Delay(500);
                Createdodcsha1TextBox.Text = originalText;
            }
        }




        private string GenerateUUID()
        {
            Random random = new Random();
            Func<int, string> randomHex = length => new string(Enumerable.Repeat("0123456789ABCDEF", length)
                                                                    .Select(s => s[random.Next(s.Length)]).ToArray());

            return $"{randomHex(8)}-{randomHex(8)}-{randomHex(8)}-{randomHex(8)}";
        }

        private void ODCGenerateUUIDButton_Click(object sender, RoutedEventArgs e)
        {
            string uuid = GenerateUUID();
            odcUUIDTextBox.Text = uuid;
            txtObjectId.Text = uuid;  // Assuming 'anotherUUIDTextBox' is the name of the second textbox
        }



        private void odcUUIDTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void odcNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void odcDescriptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void odcHDKVersionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void odcEntitlementIdTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void odcTimestampTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void RadioButtonBAR_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void RadioButtonBARSECURE_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void RadioButtonSDAT_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void RadioButtonSHARC_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void RadioButtonCORE_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBoxBruteforceUUID_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBoxSceneIDEncrypterLegacy_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBoxSceneIDDecrypterLegacy_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBoxTicketLSTLegacy_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void ArchiveCreatorTimestamp_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void MapperPathPrefixTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void CheckBoxMapCoredata_Checked(object sender, RoutedEventArgs e)
        {

        }


        private void sdcServerArchivePathTextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

        private void IncreaseCpuCores_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DecreaseCpuCores_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Path2HashButton_Click(object sender, RoutedEventArgs e)
        {
            string inputPath = Path2HashInputTextBox.Text;
            int hash = BarHash(inputPath);
            Path2HashOutputTextBox.Text = hash.ToString("X8").ToUpper();
            LogDebugInfo($"Path hashed: {inputPath}, Hash (Hex): {hash.ToString("X8").ToUpper()}, Hash (Int): {hash}");

            if (!string.IsNullOrWhiteSpace(Path2HashOutputExtraTextBox.Text))
            {
                var paths = Path2HashOutputExtraTextBox.Text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                Path2HashOutputExtraTextBox.Clear();

                foreach (var path in paths)
                {
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        string modifiedPath = path.Contains(":") ? path.Substring(path.IndexOf(":") + 1) : path;
                        int pathHash = BarHash(modifiedPath);
                        Path2HashOutputExtraTextBox.AppendText($"{pathHash.ToString("X8").ToUpper()}:{modifiedPath.ToUpper()}\n");
                        LogDebugInfo($"Path hashed: {modifiedPath}, Hash (Hex): {pathHash.ToString("X8").ToUpper()}, Hash (Int): {pathHash}");
                    }
                }
            }
        }

        public static int BarHash(string path)
        {
            path = path.Replace('\\', '/').ToLower();

            int crc = 0;
            foreach (char c in path)
            {
                crc *= 0x25;
                crc += c;
            }
            return crc;
        }

        private void Path2HashClearListHandler(object sender, RoutedEventArgs e)
        {
            // Clear the input and output text boxes
            Path2HashInputTextBox.Text = string.Empty;
            Path2HashOutputTextBox.Text = string.Empty;
            Path2HashOutputExtraTextBox.Text = string.Empty;
            LogDebugInfo("Path to Hash: List Cleared");
        }

        private bool isAddToMapperButtonClickedOnce = false;

        // Field to keep track of the button click count.
        private int addToMapperButtonClickCount = 0;

        private async void AddToMapperButton_Click(object sender, RoutedEventArgs e)
        {
            // Handle the first click.
            if (addToMapperButtonClickCount == 0)
            {
                Path2HashDragAreaText.Text = "Add these paths to the mapper helper? Click again to confirm";
                addToMapperButtonClickCount++;
                return;
            }
            // Handle the second click.
            else if (addToMapperButtonClickCount == 1)
            {
                Path2HashDragAreaText.Text = "Adding too many useless paths here could slow down mapping, Are you sure?";
                addToMapperButtonClickCount++;
                return;
            }

            // Handle the third click - Proceed with adding paths.
            if (addToMapperButtonClickCount >= 2)
            {
                // Paths to mapper helper files
                string sceneHelperFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dependencies\\scene_file_mapper_helper.txt");
                string coreDataHelperFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dependencies\\core_data_mapper_helper.txt");

                // Method to process helper files
                void ProcessHelperFile(string helperFilePath)
                {
                    using (var writer = new StreamWriter(helperFilePath, true)) // Append mode
                    {
                        // Process paths from text boxes
                        var uniquePaths = new HashSet<string>();
                        ProcessTextBoxForUniquePaths(Path2HashOutputExtraTextBox.Text, uniquePaths);
                        ProcessTextBoxForUniquePaths(Path2HashInputTextBox.Text, uniquePaths, true);

                        foreach (var path in uniquePaths)
                        {
                            if (!string.IsNullOrWhiteSpace(path))
                            {
                                int hash = BarHash(path);
                                string hashString = hash.ToString("X8").ToUpper();
                                string newEntry = $"{hashString}:{path.ToUpper()}";
                                writer.WriteLine(newEntry);
                            }
                        }
                    }
                }

                // Process both helper files
                ProcessHelperFile(sceneHelperFilePath);
                ProcessHelperFile(coreDataHelperFilePath);

                // After adding paths, update the text and use a delay to revert the message.
                Path2HashDragAreaText.Text = "Paths added to mapper helper files";
                await Task.Delay(1000); // Wait for one second
                Path2HashDragAreaText.Text = "Drag a folder or TXT/XML here to hash all paths";
            }
        }

        private void ProcessTextBoxForUniquePaths(string textBoxContent, HashSet<string> uniquePaths, bool isInput = false)
        {
            var lines = textBoxContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                string path = isInput ? line : line.Contains(":") ? line.Split(':')[1] : line;
                path = path.Trim();
                if (!uniquePaths.Contains(path.ToUpper()) && !string.IsNullOrWhiteSpace(path))
                {
                    uniquePaths.Add(path.ToUpper());
                }
            }
        }




        private void Path2HashTabItem_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedObjects = (string[])e.Data.GetData(DataFormats.FileDrop);
                StringBuilder outputText = new StringBuilder();
                HashSet<string> uniquePaths = new HashSet<string>();

                foreach (string droppedObject in droppedObjects)
                {
                    if (Directory.Exists(droppedObject))
                    {
                        // Process each file in the directory and add to uniquePaths
                        IEnumerable<string> fileEntries = Directory.EnumerateFiles(droppedObject, "*", SearchOption.AllDirectories);
                        foreach (string fullFileName in fileEntries)
                        {
                            string relativePath = fullFileName.Substring(droppedObject.Length + 1).ToUpper(); // +1 to remove the leading backslash and convert to uppercase
                            uniquePaths.Add(relativePath);
                        }
                    }
                    else if (File.Exists(droppedObject))
                    {
                        // Check if the file is of a specific type
                        var extension = Path.GetExtension(droppedObject).ToLower();
                        if (extension == ".txt" || extension == ".xml")
                        {
                            // Search within the file for paths
                            var fileContent = File.ReadAllText(droppedObject);
                            Regex regex = new Regex("\"([^\"]*\\.scene)\"");
                            var matches = regex.Matches(fileContent);

                            foreach (Match match in matches)
                            {
                                // Extract the path from the match, convert to uppercase, and add to uniquePaths
                                string path = match.Groups[1].Value.ToUpper();
                                uniquePaths.Add(path);
                            }
                        }
                    }
                }

                // Process unique paths, hash them, and format the output in uppercase
                foreach (string path in uniquePaths)
                {
                    int hash = BarHash(path);
                    outputText.AppendLine($"{hash:X8}:{path}");
                }

                Path2HashOutputExtraTextBox.Text = outputText.ToString();
            }
        }



        private void TabItem_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }




        private int previousFileCount = 0;
        private CancellationTokenSource cancellationTokenSource;

        private async void Sha1DragArea_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedItems = (string[])e.Data.GetData(DataFormats.FileDrop);

                int fileCount = droppedItems.Select(item =>
                    Directory.Exists(item) ? Directory.GetFiles(item, "*", SearchOption.AllDirectories).Length : 1)
                    .Sum();

                if (fileCount > 2000)
                {
                    if (previousFileCount == fileCount)
                    {
                        cancellationTokenSource?.Cancel();
                        cancellationTokenSource = new CancellationTokenSource();
                        ProcessFiles(droppedItems, cancellationTokenSource.Token);
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                            TemporaryMessageHelper.ShowTemporaryMessage(SHA1DragAreaText, "Over 2000 files - Drag same files again to confirm?", 5000)
                        );
                        previousFileCount = fileCount;
                    }
                }
                else
                {
                    cancellationTokenSource?.Cancel();
                    cancellationTokenSource = new CancellationTokenSource();
                    ProcessFiles(droppedItems, cancellationTokenSource.Token);
                }
            }
        }

        private async void ProcessFiles(string[] droppedItems, CancellationToken cancellationToken)
        {
            Sha1TextBox.Clear();
            Dispatcher.Invoke(() =>
                TemporaryMessageHelper.ShowTemporaryMessage(SHA1DragAreaText, "Generating SHA1s...", 10000)
            );

            int fileCount = 0;
            await Task.Run(() =>
            {
                foreach (string item in droppedItems)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    if (Directory.Exists(item))
                    {
                        string[] files = Directory.GetFiles(item, "*", SearchOption.AllDirectories);
                        foreach (string file in files)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                return;
                            }
                            ComputeAndAppendSha1(file);
                            fileCount++;
                            UpdateMessageWithCount(fileCount);
                        }
                    }
                    else if (File.Exists(item))
                    {
                        ComputeAndAppendSha1(item);
                        fileCount++;
                        UpdateMessageWithCount(fileCount);
                    }
                }
            });

            Dispatcher.Invoke(() =>
                TemporaryMessageHelper.ShowTemporaryMessage(SHA1DragAreaText, $"{fileCount} Files Done", 1000)
            );
            previousFileCount = 0;
        }

        private void UpdateMessageWithCount(int count)
        {
            Dispatcher.Invoke(() =>
            {
                TemporaryMessageHelper.ShowTemporaryMessage(SHA1DragAreaText, $"Generating SHA1s... {count} files done", 10000);
            });
        }

        private void ComputeAndAppendSha1(string file)
        {
            LogDebugInfo($"SHA1 Generator: Processing file: {file}");
            try
            {
                string sha1Hash = ComputeSha1(file);
                string fileName = Path.GetFileName(file);
                Dispatcher.Invoke(() => Sha1TextBox.AppendText($"SHA1: {sha1Hash} - File: {file}{Environment.NewLine}"));
                LogDebugInfo($"SHA1 Generator: {fileName} = {sha1Hash}");
            }
            catch (Exception ex)
            {
                string fileName = Path.GetFileName(file);
                Dispatcher.Invoke(() => Sha1TextBox.AppendText($"Error processing {file}: {ex.Message}{Environment.NewLine}"));
                LogDebugInfo($"SHA1 Generator: Error processing {fileName}: {ex.Message}");
            }
        }

        private string ComputeSha1(string filePath)
        {
            using (var sha1 = System.Security.Cryptography.SHA1.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] checksum = sha1.ComputeHash(stream);
                    return BitConverter.ToString(checksum).Replace("-", String.Empty);
                }
            }
        }

        private async void ClearSHA1Button_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource?.Cancel();


            await Task.Delay(20);


            Sha1TextBox.Clear();
            Dispatcher.Invoke(() =>
            {
                TemporaryMessageHelper.ShowTemporaryMessage(SHA1DragAreaText, "SHA1 list cleared", 1000);
            });
        }


        private void CopySHA1Button_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Sha1TextBox.Text);
            Dispatcher.Invoke(() =>
            {
                TemporaryMessageHelper.ShowTemporaryMessage(SHA1DragAreaText, "List Copied to Clipboard", 2000);
            });
        }

        private async void SHA1BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "All files (*.*)|*.*", // You can adjust this to specific file types
                Multiselect = true // Allow multiple file selection
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var selectedFiles = openFileDialog.FileNames;

                // Following the same logic as your Sha1DragArea_Drop event
                int fileCount = selectedFiles.Select(item =>
                    Directory.Exists(item) ? Directory.GetFiles(item, "*", SearchOption.AllDirectories).Length : 1)
                    .Sum();

                if (fileCount > 2000)
                {
                    if (previousFileCount == fileCount)
                    {
                        cancellationTokenSource?.Cancel();
                        cancellationTokenSource = new CancellationTokenSource();
                        ProcessFiles(selectedFiles, cancellationTokenSource.Token);
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                            TemporaryMessageHelper.ShowTemporaryMessage(SHA1DragAreaText, "Over 2000 files - Drag same files again to confirm?", 5000)
                        );
                        previousFileCount = fileCount;
                    }
                }
                else
                {
                    cancellationTokenSource?.Cancel();
                    cancellationTokenSource = new CancellationTokenSource();
                    ProcessFiles(selectedFiles, cancellationTokenSource.Token);
                }
            }
        }


        // Tab 9 video converter

        private async void VideoDragArea_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedItems = (string[])e.Data.GetData(DataFormats.FileDrop);
                cancellationTokenSource?.Cancel();
                cancellationTokenSource = new CancellationTokenSource();
                await ProcessVideoFiles(droppedItems, cancellationTokenSource.Token);
            }
        }

        private async Task ProcessVideoFiles(string[] videoFiles, CancellationToken cancellationToken)
        {
            VideoTextBox.Clear();
            int fileCount = 0;
            await Task.Run(() =>
            {
                foreach (string file in videoFiles)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    if (File.Exists(file) &&
                        (file.EndsWith(".mp4") || file.EndsWith(".avi") || file.EndsWith(".mov") ||
                         file.EndsWith(".mpeg") || file.EndsWith(".mpg") || file.EndsWith(".mkv") ||
                         file.EndsWith(".wmv") || file.EndsWith(".flv") || file.EndsWith(".webm") ||
                         file.EndsWith(".ogv") || file.EndsWith(".3gp"))) // Add other video formats as needed

                    {
                        Dispatcher.Invoke(() => VideoTextBox.AppendText($"Video file: {file}{Environment.NewLine}"));
                        fileCount++;
                    }
                }
            });

            Dispatcher.Invoke(() =>
                TemporaryMessageHelper.ShowTemporaryMessage(VideoDragAreaText, $"{fileCount} video files processed.", 1000)
            );
        }

        private async void VideoBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Video files (*.mp4; *.avi; *.mov; *.mpeg; *.mpg; *.mkv; *.wmv; *.flv; *.webm; *.ogv; *.3gp)|*.mp4;*.avi;*.mov;*.mpeg;*.mpg;*.mkv;*.wmv;*.flv;*.webm;*.ogv;*.3gp|All files (*.*)|*.*",

                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var selectedFiles = openFileDialog.FileNames;
                cancellationTokenSource?.Cancel();
                cancellationTokenSource = new CancellationTokenSource();
                await ProcessVideoFiles(selectedFiles, cancellationTokenSource.Token);
            }
        }


        private void ClearVideoButton_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource?.Cancel();
            VideoTextBox.Clear();
            Dispatcher.Invoke(() =>
                TemporaryMessageHelper.ShowTemporaryMessage(VideoDragAreaText, "Video list cleared", 1000)
            );
        }

        private async void ConvertVideoButton_Click(object sender, RoutedEventArgs e)
        {
            // Gather all input from UI
            string videoUrls = VideoYoutubeURLTextBox.Text.Trim();
            string localVideoEntries = VideoTextBox.Text.Trim(); // This should contain lines that may start with "Video file:"
            string outputDirectory = VideoOutputDirectoryTextBox.Text.Trim();

            // Ensure there's meaningful input
            if (string.IsNullOrEmpty(videoUrls) && string.IsNullOrEmpty(localVideoEntries))
            {
                Dispatcher.Invoke(() => TemporaryMessageHelper.ShowTemporaryMessage(VideoDragAreaText, "Please enter a video URL or check for local video paths.", 2000));
                return;
            }

            // Prepare the output directory
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // Define paths to dependencies
            string dependenciesPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "dependencies");
            string ytDlpPath = Path.Combine(dependenciesPath, "yt-dlp.exe");
            string ffmpegPath = Path.Combine(dependenciesPath, "ffmpeg.exe");

            // Check for required dependencies
            await EnsureDependencies(ytDlpPath, ffmpegPath);

            // Process local video files detected in the VideoTextBox
            var localVideoPaths = localVideoEntries.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Where(line => line.StartsWith("Video file:"))
                                        .Select(line => line.Replace("Video file:", "").Trim());

            foreach (string localPath in localVideoPaths)
            {
                await ConvertLocalVideoFile(localPath, outputDirectory);
            }

            // Process each URL for download and conversion
            var urls = videoUrls.Split(new[] { ' ', ',', '\n', '\r', '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var url in urls)
            {
                await DownloadVideo(url, outputDirectory, ytDlpPath);
            }
        }

        private async Task ConvertLocalVideoFile(string filePath, string outputDirectory)
        {
            var (selectedResolution, selectedBitrate) = GetResolutionAndBitrate();
            string exeLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string ffmpegPath = Path.Combine(Path.GetDirectoryName(exeLocation), "dependencies\\ffmpeg.exe");

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string outputFilePath = $"{outputDirectory}\\{fileNameWithoutExtension}_{selectedResolution.Replace(':', 'x')}.mp4";

            string audioBitrate = "160k"; // Default audio bitrate

            if (Audio160kRadioButton.IsChecked == true)
                audioBitrate = "160k";
            else if (Audio256kRadioButton.IsChecked == true)
                audioBitrate = "256k";
            else if (Audio320kRadioButton.IsChecked == true)
                audioBitrate = "320k";

            int audioBoost = 0;
            if (VideoAudioBoost3CheckBox.IsChecked == true) audioBoost += 3;
            if (VideoAudioBoost6CheckBox.IsChecked == true) audioBoost += 6;
            string audioFilter = audioBoost > 0 ? $",volume={audioBoost}dB" : "";

            // Set video codec and additional arguments for MPEG-4
            string videoCodec = "mpeg4";
            string additionalArgs = "";
            string aspectRatio = AspectRatioFourThreeCheckBox.IsChecked ?? false ? "4/3" : "16/9";

            string ffmpegArguments = $"-i \"{filePath}\" " +
                $"-c:v {videoCodec} {additionalArgs} -b:v {selectedBitrate} -minrate {selectedBitrate} -maxrate {selectedBitrate} -bufsize {int.Parse(selectedBitrate.Replace("k", "")) * 2}k " +
                $"-vf setdar={aspectRatio} -s {selectedResolution} -r 29.970 " +
                $"-c:a aac -b:a {audioBitrate} -ar 48000 -ac 2 " +
                $"-af \"loudnorm=I=-23:LRA=7:tp=-2{audioFilter}\" " +
                $"-movflags +faststart " +
                $"-metadata:s:a title=\"Stereo\" " +
                $"\"{outputFilePath}\"";

            ProcessStartInfo ffmpegStartInfo = new ProcessStartInfo()
            {
                FileName = ffmpegPath,
                Arguments = ffmpegArguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process ffmpegProcess = Process.Start(ffmpegStartInfo))
            {
                ffmpegProcess.OutputDataReceived += (s, args) =>
                {
                    if (args.Data != null)
                    {
                        Dispatcher.Invoke(() => VideoTextBox.AppendText(args.Data + Environment.NewLine));
                    }
                };
                ffmpegProcess.BeginOutputReadLine();
                ffmpegProcess.ErrorDataReceived += (s, args) =>
                {
                    if (args.Data != null)
                        Dispatcher.Invoke(() => VideoTextBox.AppendText("Conversion: " + args.Data + Environment.NewLine));
                };
                ffmpegProcess.BeginErrorReadLine();

                await ffmpegProcess.WaitForExitAsync();
                if (ffmpegProcess.ExitCode == 0)
                {
                    Dispatcher.Invoke(() => VideoTextBox.AppendText("Conversion successful. Converted video located at: " + outputFilePath + Environment.NewLine));
                    File.Delete(filePath); // Optionally delete the original file after successful conversion
                }
                else
                {
                    Dispatcher.Invoke(() => TemporaryMessageHelper.ShowTemporaryMessage(VideoDragAreaText, "Conversion failed, check output for details.", 2000));
                }
            }
        }




        private (string resolution, string bitrate) GetResolutionAndBitrate()
        {
            // Check if the 4:3 aspect ratio option is selected
            bool isFourThree = AspectRatioFourThreeCheckBox.IsChecked == true;

            if (Video360pRadioButton.IsChecked == true)
                return isFourThree ? ("480:360", "1800k") : ("640:360", "1800k");
            else if (Video480pRadioButton.IsChecked == true)
                return isFourThree ? ("640:480", "2500k") : ("854:480", "2500k");
            else if (Video576pRadioButton.IsChecked == true)
                return isFourThree ? ("768:576", "3500k") : ("1024:576", "3500k");
            else if (Video720pRadioButton.IsChecked == true)
                return isFourThree ? ("960:720", "4500k") : ("1280:720", "4500k");
            else if (Video1080pRadioButton.IsChecked == true)
                return isFourThree ? ("1440:1080", "6000k") : ("1920:1080", "7000k");
            else
                return isFourThree ? ("768:576", "2500k") : ("1024:576", "2500k"); // Default case if none is selected
        }




        private async Task EnsureDependencies(string ytDlpPath, string ffmpegPath)
        {
            // Ensure yt-dlp is downloaded
            if (!File.Exists(ytDlpPath))
            {
                if (MessageBox.Show("yt-dlp.exe was not found. You can manually add it to the dependencies folder and try again, OR Would you like to download it from https://github.com/yt-dlp/yt-dlp/releases/ now?", "Download yt-dlp", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    await DownloadFileAsync("https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe", ytDlpPath);
                }
                else
                {
                    throw new InvalidOperationException("yt-dlp is required to proceed.");
                }
            }

            // Ensure ffmpeg is downloaded
            if (!File.Exists(ffmpegPath))
            {
                if (MessageBox.Show("ffmpeg.exe was not found. You can manually add it to the dependencies folder and try again, OR Would you like to download it from https://github.com/yt-dlp/FFmpeg-Builds/releases/ now?", "Download ffmpeg", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    await DownloadAndExtractFfmpeg(Path.GetDirectoryName(ffmpegPath), ffmpegPath);
                }
                else
                {
                    throw new InvalidOperationException("ffmpeg is required to proceed.");
                }
            }
        }


        private async Task DownloadFileAsync(string url, string path)
        {
            Dispatcher.Invoke(() => VideoTextBox.AppendText($"Starting download from {url}...\n"));
            using (var client = new WebClient())
            {
                await client.DownloadFileTaskAsync(new Uri(url), path);
            }
            Dispatcher.Invoke(() => VideoTextBox.AppendText("Download completed.\n"));
        }

        private async Task DownloadAndExtractFfmpeg(string dependenciesPath, string ffmpegPath)
        {
            string zipPath = Path.Combine(dependenciesPath, "ffmpeg.zip");
            string extractPath = Path.Combine(dependenciesPath, "ffmpeg-extract");

            // Download ffmpeg
            Dispatcher.Invoke(() => VideoTextBox.AppendText("Downloading ffmpeg...\n"));
            using (var client = new WebClient())
            {
                await client.DownloadFileTaskAsync(new Uri("https://github.com/yt-dlp/FFmpeg-Builds/releases/download/autobuild-2024-06-30-14-06/ffmpeg-n7.0.1-11-g40ddddca45-win64-gpl-7.0.zip"), zipPath);
            }
            Dispatcher.Invoke(() => VideoTextBox.AppendText("ffmpeg downloaded. Extracting...\n"));

            // Clean up existing extraction directory if it exists
            if (Directory.Exists(extractPath))
            {
                Directory.Delete(extractPath, true);
            }

            // Extract ffmpeg
            try
            {
                ZipFile.ExtractToDirectory(zipPath, extractPath);
                File.Move(Path.Combine(extractPath, "ffmpeg-n7.0.1-11-g40ddddca45-win64-gpl-7.0", "bin", "ffmpeg.exe"), ffmpegPath);
                Dispatcher.Invoke(() => VideoTextBox.AppendText("ffmpeg extracted successfully.\n"));
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => VideoTextBox.AppendText($"Error extracting ffmpeg: {ex.Message}\n"));
            }

            // Cleanup
            try
            {
                Directory.Delete(extractPath, true);
                File.Delete(zipPath);
                Dispatcher.Invoke(() => VideoTextBox.AppendText("Cleanup completed.\n"));
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => VideoTextBox.AppendText($"Error during cleanup: {ex.Message}\n"));
            }
        }


        private async Task DownloadVideo(string videoUrls, string outputDirectory, string ytDlpPath)
        {
            if (string.IsNullOrEmpty(videoUrls))
            {
                Dispatcher.Invoke(() => TemporaryMessageHelper.ShowTemporaryMessage(VideoDragAreaText, "Please enter a video URL.", 2000));
                return;
            }

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            var urls = videoUrls.Split(new[] { ' ', ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var videoUrl in urls)
            {
                string downloadArguments = $"-f bestvideo+bestaudio/best {videoUrl} -o \"{outputDirectory}\\%(title)s.%(ext)s\"";
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = ytDlpPath,
                    Arguments = downloadArguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    process.OutputDataReceived += (s, args) =>
                    {
                        if (args.Data != null)
                        {
                            Dispatcher.Invoke(() => VideoTextBox.AppendText(args.Data + Environment.NewLine));
                        }
                    };
                    process.BeginOutputReadLine();

                    process.ErrorDataReceived += (s, args) =>
                    {
                        if (args.Data != null)
                        {
                            Dispatcher.Invoke(() => VideoTextBox.AppendText("Error: " + args.Data + Environment.NewLine));
                        }
                    };
                    process.BeginErrorReadLine();

                    await process.WaitForExitAsync();

                    if (process.ExitCode == 0)
                    {
                        Dispatcher.Invoke(() => TemporaryMessageHelper.ShowTemporaryMessage(VideoDragAreaText, "Download successful.", 2000));
                        await ScanAndConvertWebmFiles(outputDirectory); // Scan and convert any webm files after successful download
                    }
                    else
                    {
                        Dispatcher.Invoke(() => TemporaryMessageHelper.ShowTemporaryMessage(VideoDragAreaText, "Download may have failed, check the output for details.", 2000));
                    }
                }
            }
        }


        private async Task ScanAndConvertWebmFiles(string directory)
        {
            var webmFiles = Directory.GetFiles(directory, "*.webm");
            foreach (var file in webmFiles)
            {
                await ConvertLocalVideoFile(file, directory);
                File.Delete(file); // Delete the webm file after conversion
            }
        }


        private string ParseForMergedFilePath(Queue<string> lastFewLines)
        {
            string mergePrefix = "[Merger] Merging formats into ";
            foreach (var line in lastFewLines)
            {
                if (line.StartsWith(mergePrefix))
                {
                    return line.Substring(mergePrefix.Length).Trim('"');
                }
            }
            return null;
        }




        private void CacheTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void CacheButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CacheOutputTextBox(object sender, TextChangedEventArgs e)
        {

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {

        }

        private void InfTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void CheckBoxOfflineSDC_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CacheZipExtractandRename_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CacheZipClearFileList_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CacheZipTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void INFEncryptDecryptFileList_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CacheZipDeleteReservedCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CacheZipRenameCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        // Settings Tab - Set directories
        private void CdsEncryptBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var folderPath = OpenFolderDialog(CdsEncryptOutputDirectoryTextBox.Text);
            if (folderPath != null)
            {
                CdsEncryptOutputDirectoryTextBox.Text = folderPath;
            }
        }

        private void CdsDecryptBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var folderPath = OpenFolderDialog(CdsDecryptOutputDirectoryTextBox.Text);
            if (folderPath != null)
            {
                CdsDecryptOutputDirectoryTextBox.Text = folderPath;
            }
        }

        private void CdsDecryptOpenButton_Click(object sender, RoutedEventArgs e)
        {
            string outputDirectory = CdsDecryptOutputDirectoryTextBox.Text;

            try
            {
                // Check if the directory exists
                if (!Directory.Exists(outputDirectory))
                {
                    // Create the directory if it does not exist
                    Directory.CreateDirectory(outputDirectory);
                    MessageBox.Show($"Directory created: {outputDirectory}");
                }

                // Open the directory in File Explorer
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = outputDirectory,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., invalid path or lack of permissions)
                MessageBox.Show($"Error opening directory: {ex.Message}");
            }
        }

        private void CdsEncryptOpenButton_Click(object sender, RoutedEventArgs e)
        {
            string outputDirectory = CdsEncryptOutputDirectoryTextBox.Text;

            try
            {
                // Check if the directory exists
                if (!Directory.Exists(outputDirectory))
                {
                    // Create the directory if it does not exist
                    Directory.CreateDirectory(outputDirectory);
                    MessageBox.Show($"Directory created: {outputDirectory}");
                }

                // Open the directory in File Explorer
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = outputDirectory,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., invalid path or lack of permissions)
                MessageBox.Show($"Error opening directory: {ex.Message}");
            }
        }


        private void BarSdatSharcBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var folderPath = OpenFolderDialog(BarSdatSharcOutputDirectoryTextBox.Text);
            if (folderPath != null)
            {
                BarSdatSharcOutputDirectoryTextBox.Text = folderPath;
            }
        }

        private void BarSdatSharcOpenButton_Click(object sender, RoutedEventArgs e)
        {
            string outputDirectory = BarSdatSharcOutputDirectoryTextBox.Text;

            try
            {
                // Check if the directory exists
                if (!Directory.Exists(outputDirectory))
                {
                    // Create the directory if it does not exist
                    Directory.CreateDirectory(outputDirectory);
                    MessageBox.Show($"Directory created: {outputDirectory}");
                }

                // Open the directory in File Explorer
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = outputDirectory,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., invalid path or lack of permissions)
                MessageBox.Show($"Error opening directory: {ex.Message}");
            }
        }

        private void MappedBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var folderPath = OpenFolderDialog(MappedOutputDirectoryTextBox.Text);
            if (folderPath != null)
            {
                MappedOutputDirectoryTextBox.Text = folderPath;
            }
        }

        private void MappedOpenButton_Click(object sender, RoutedEventArgs e)
        {
            string outputDirectory = MappedOutputDirectoryTextBox.Text;

            try
            {
                // Check if the directory exists
                if (!Directory.Exists(outputDirectory))
                {
                    // Create the directory if it does not exist
                    Directory.CreateDirectory(outputDirectory);
                    MessageBox.Show($"Directory created: {outputDirectory}");
                }

                // Open the directory in File Explorer
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = outputDirectory,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., invalid path or lack of permissions)
                MessageBox.Show($"Error opening directory: {ex.Message}");
            }
        }


        private void HcdbBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var folderPath = OpenFolderDialog(HcdbOutputDirectoryTextBox.Text);
            if (folderPath != null)
            {
                HcdbOutputDirectoryTextBox.Text = folderPath;
            }
        }

        private void HcdbOpenButton_Click(object sender, RoutedEventArgs e)
        {
            string outputDirectory = HcdbOutputDirectoryTextBox.Text;

            try
            {
                // Check if the directory exists
                if (!Directory.Exists(outputDirectory))
                {
                    // Create the directory if it does not exist
                    Directory.CreateDirectory(outputDirectory);
                    MessageBox.Show($"Directory created: {outputDirectory}");
                }

                // Open the directory in File Explorer
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = outputDirectory,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., invalid path or lack of permissions)
                MessageBox.Show($"Error opening directory: {ex.Message}");
            }
        }

        private void SqlBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var folderPath = OpenFolderDialog(SqlOutputDirectoryTextBox.Text);
            if (folderPath != null)
            {
                SqlOutputDirectoryTextBox.Text = folderPath;
            }
        }

        private void SqlOpenButton_Click(object sender, RoutedEventArgs e)
        {
            string outputDirectory = SqlOutputDirectoryTextBox.Text;

            try
            {
                // Check if the directory exists
                if (!Directory.Exists(outputDirectory))
                {
                    // Create the directory if it does not exist
                    Directory.CreateDirectory(outputDirectory);
                    MessageBox.Show($"Directory created: {outputDirectory}");
                }

                // Open the directory in File Explorer
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = outputDirectory,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., invalid path or lack of permissions)
                MessageBox.Show($"Error opening directory: {ex.Message}");
            }
        }

        private void VideoOpenButton_Click(object sender, RoutedEventArgs e)
        {
            string outputDirectory = VideoOutputDirectoryTextBox.Text;

            try
            {
                // Check if the directory exists
                if (!Directory.Exists(outputDirectory))
                {
                    // Create the directory if it does not exist
                    Directory.CreateDirectory(outputDirectory);
                    MessageBox.Show($"Directory created: {outputDirectory}");
                }

                // Open the directory in File Explorer
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = outputDirectory,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., invalid path or lack of permissions)
                MessageBox.Show($"Error opening directory: {ex.Message}");
            }
        }

        private void AudioBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var folderPath = OpenFolderDialog(AudioOutputDirectoryTextBox.Text);
            if (folderPath != null)
            {
                AudioOutputDirectoryTextBox.Text = folderPath;
            }
        }

        private void AudioOpenButton_Click(object sender, RoutedEventArgs e)
        {
            string outputDirectory = AudioOutputDirectoryTextBox.Text;

            try
            {
                // Check if the directory exists
                if (!Directory.Exists(outputDirectory))
                {
                    // Create the directory if it does not exist
                    Directory.CreateDirectory(outputDirectory);
                    MessageBox.Show($"Directory created: {outputDirectory}");
                }

                // Open the directory in File Explorer
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = outputDirectory,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., invalid path or lack of permissions)
                MessageBox.Show($"Error opening directory: {ex.Message}");
            }
        }

        private void RPCS3BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var folderPath = OpenFolderDialog(Rpcs3Dev_hdd0_DirectoryTextBox.Text);
            if (folderPath != null)
            {
                Rpcs3Dev_hdd0_DirectoryTextBox.Text = folderPath;
            }
        }

        private void RPCS3OpenButton_Click(object sender, RoutedEventArgs e)
        {
            string outputDirectory = Rpcs3Dev_hdd0_DirectoryTextBox.Text;

            try
            {
                // Check if the directory exists
                if (!Directory.Exists(outputDirectory))
                {
                    // Create the directory if it does not exist
                    Directory.CreateDirectory(outputDirectory);
                    MessageBox.Show($"Directory created: {outputDirectory}");
                }

                // Open the directory in File Explorer
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = outputDirectory,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., invalid path or lack of permissions)
                MessageBox.Show($"Error opening directory: {ex.Message}");
            }
        }


        private void TicketListBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var folderPath = OpenFolderDialog(TicketListOutputDirectoryTextBox.Text);
            if (folderPath != null)
            {
                TicketListOutputDirectoryTextBox.Text = folderPath;
            }
        }

        private void TicketListOpenButton_Click(object sender, RoutedEventArgs e)
        {
            string outputDirectory = TicketListOutputDirectoryTextBox.Text;

            try
            {
                // Check if the directory exists
                if (!Directory.Exists(outputDirectory))
                {
                    // Create the directory if it does not exist
                    Directory.CreateDirectory(outputDirectory);
                    MessageBox.Show($"Directory created: {outputDirectory}");
                }

                // Open the directory in File Explorer
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = outputDirectory,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., invalid path or lack of permissions)
                MessageBox.Show($"Error opening directory: {ex.Message}");
            }
        }

        private void LuacBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var folderPath = OpenFolderDialog(LuacOutputDirectoryTextBox.Text);
            if (folderPath != null)
            {
                LuacOutputDirectoryTextBox.Text = folderPath;
            }
        }

        private void LuacOpenButton_Click(object sender, RoutedEventArgs e)
        {
            string outputDirectory = LuacOutputDirectoryTextBox.Text;

            try
            {
                // Check if the directory exists
                if (!Directory.Exists(outputDirectory))
                {
                    // Create the directory if it does not exist
                    Directory.CreateDirectory(outputDirectory);
                    MessageBox.Show($"Directory created: {outputDirectory}");
                }

                // Open the directory in File Explorer
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = outputDirectory,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., invalid path or lack of permissions)
                MessageBox.Show($"Error opening directory: {ex.Message}");
            }
        }

        private void LuaBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var folderPath = OpenFolderDialog(LuaOutputDirectoryTextBox.Text);
            if (folderPath != null)
            {
                LuaOutputDirectoryTextBox.Text = folderPath;
            }
        }

        private void LuaOpenButton_Click(object sender, RoutedEventArgs e)
        {
            string outputDirectory = LuaOutputDirectoryTextBox.Text;

            try
            {
                // Check if the directory exists
                if (!Directory.Exists(outputDirectory))
                {
                    // Create the directory if it does not exist
                    Directory.CreateDirectory(outputDirectory);
                    MessageBox.Show($"Directory created: {outputDirectory}");
                }

                // Open the directory in File Explorer
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = outputDirectory,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., invalid path or lack of permissions)
                MessageBox.Show($"Error opening directory: {ex.Message}");
            }
        }


        // Helper method to open the folder dialog and return the selected path
        private string OpenFolderDialog(string initialDirectory)
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;
                dialog.InitialDirectory = initialDirectory;
                dialog.EnsurePathExists = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
#pragma warning disable CS8603 // Possible null reference return.
                    return dialog.FileName;
#pragma warning restore CS8603 // Possible null reference return.
                }
            }
#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.
        }


        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }

        private void Grid_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy; // Allows the drop
            }
            else
            {
                e.Effects = DragDropEffects.None; // Disallows the drop
            }
            e.Handled = true; // Mark the event as handled
        }


        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                // Determine which half of the grid the file was dropped on based on the drop position
                Point dropPosition = e.GetPosition((IInputElement)sender);
                var grid = sender as Grid;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                int column = dropPosition.X < grid.ActualWidth / 2 ? 0 : 1;
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                // Call different functions based on which column the file was dropped
                if (column == 0)
                {
                    HandleSdcFileDrop(files);
                }
                else
                {
                    HandleOdcFileDrop(files);
                }
            }
        }

        private async void HandleSdcFileDrop(string[] files)
        {
            string filePath = files.FirstOrDefault();
            LogDebugInfo("HandleSdcFileDrop: File dropped - " + filePath);

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                MessageBox.Show("Invalid file. Please drop a valid SDC XML file.");
                LogDebugInfo("HandleSdcFileDrop: Invalid file path or file does not exist.");
                return;
            }

            try
            {
                // Try to load the SDC XML file
                LogDebugInfo("HandleSdcFileDrop: Attempting to load SDC XML file.");
                XDocument sdcXml = XDocument.Load(filePath);
                ProcessSdcXml(sdcXml);
                LogDebugInfo("HandleSdcFileDrop: SDC XML file loaded successfully.");
            }
            catch (Exception ex)
            {
                LogDebugInfo("HandleSdcFileDrop: Failed to load SDC XML file. Attempting decryption. Exception: " + ex.Message);

                string tempDirectory = Path.Combine(Path.GetTempPath(), "DecryptedFiles");
                Directory.CreateDirectory(tempDirectory);

                bool decryptionSuccess = await DecryptFilesAsync(new[] { filePath }, tempDirectory);
                if (decryptionSuccess)
                {
                    string parentFolderName = Path.GetFileName(Path.GetDirectoryName(filePath));
                    string decryptedFilePath = Path.Combine(tempDirectory, parentFolderName, Path.GetFileName(filePath));
                    LogDebugInfo("HandleSdcFileDrop: Decryption successful. Decrypted file path - " + decryptedFilePath);

                    if (File.Exists(decryptedFilePath))
                    {
                        try
                        {
                            XDocument sdcXml = XDocument.Load(decryptedFilePath);
                            ProcessSdcXml(sdcXml);
                            LogDebugInfo("HandleSdcFileDrop: Decrypted SDC XML file loaded successfully.");

                            // Compute and display the SHA1 hash
                            string sha1Hash = CalculateSDCODCSha1Hash(decryptedFilePath);
                            Createdsdcsha1TextBox.Text = sha1Hash;
                            LogDebugInfo("HandleSdcFileDrop: SHA1 hash computed - " + sha1Hash);
                        }
                        catch (Exception ex2)
                        {
                            MessageBox.Show($"Error reading decrypted SDC file: {ex2.Message}");
                            LogDebugInfo("HandleSdcFileDrop: Error reading decrypted SDC file. Exception: " + ex2.Message);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Error reading or decrypting SDC file.");
                    LogDebugInfo("HandleSdcFileDrop: Decryption failed.");
                }
            }
        }


        private void ProcessSdcXml(XDocument sdcXml)
        {
            LogDebugInfo("ProcessSdcXml: Processing SDC XML file.");
            var englishLanguageElement = sdcXml.Root.Elements("LANGUAGE").FirstOrDefault(el => el.Attribute("REGION")?.Value == "en-GB");

            if (englishLanguageElement != null)
            {
                sdcNameTextBox.Text = englishLanguageElement.Element("NAME")?.Value ?? "";
                sdcDescriptionTextBox.Text = englishLanguageElement.Element("DESCRIPTION")?.Value ?? "";
                sdcMakerTextBox.Text = englishLanguageElement.Element("MAKER")?.Value ?? "";

                var archiveElement = englishLanguageElement.Element("ARCHIVES")?.Element("ARCHIVE");
                if (archiveElement != null)
                {
                    sdcServerArchivePathTextBox.Text = (archiveElement.Value ?? "").Replace("[CONTENT_SERVER_ROOT]", string.Empty);
                    sdcArchiveSizeTextBox.Text = archiveElement.Attribute("size")?.Value ?? "";
                    sdcTimestampTextBox.Text = archiveElement.Attribute("timestamp")?.Value ?? "";
                }

                var makerImagePath = englishLanguageElement.Element("MAKER_IMAGE")?.Value ?? "";
                sdcThumbnailSuffixTextBox.Text = ExtractSDCThumbnailSuffix(makerImagePath);

                offlineSDCCheckBox.IsChecked = englishLanguageElement.Element("ARCHIVES") == null;
            }

            sdcHDKVersionTextBox.Text = sdcXml.Root.Attribute("hdk_version")?.Value ?? "";
            LogDebugInfo("ProcessSdcXml: SDC XML file processed successfully.");
        }


        private string ExtractSDCThumbnailSuffix(string imagePath)
        {
            // Assuming the thumbnail suffix is in the format "_Txxx.png"
            var match = Regex.Match(imagePath, @"_T(\d+)\.png");
            return match.Success ? match.Groups[1].Value : "";
        }


        private async void HandleOdcFileDrop(string[] files)
        {
            string filePath = files.FirstOrDefault();
            LogDebugInfo("HandleOdcFileDrop: File dropped - " + filePath);

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                MessageBox.Show("Invalid file. Please drop a valid ODC XML file.");
                LogDebugInfo("HandleOdcFileDrop: Invalid file path or file does not exist.");
                return;
            }

            try
            {
                // Try to load the ODC XML file
                LogDebugInfo("HandleOdcFileDrop: Attempting to load ODC XML file.");
                XDocument odcXml = XDocument.Load(filePath);
                ProcessOdcXml(odcXml);
                LogDebugInfo("HandleOdcFileDrop: ODC XML file loaded successfully.");
            }
            catch (Exception ex)
            {
                LogDebugInfo("HandleOdcFileDrop: Failed to load ODC XML file. Attempting decryption. Exception: " + ex.Message);

                string tempDirectory = Path.Combine(Path.GetTempPath(), "DecryptedFiles");
                Directory.CreateDirectory(tempDirectory);

                bool decryptionSuccess = await DecryptFilesAsync(new[] { filePath }, tempDirectory);
                if (decryptionSuccess)
                {
                    // Adjust decryptedFilePath to reflect the expected folder structure
                    string parentFolderName = Path.GetFileName(Path.GetDirectoryName(filePath));
                    string decryptedFilePath = Path.Combine(tempDirectory, parentFolderName, Path.GetFileName(filePath));
                    LogDebugInfo("HandleOdcFileDrop: Decryption successful. Decrypted file path - " + decryptedFilePath);

                    if (File.Exists(decryptedFilePath))
                    {
                        try
                        {
                            // Compute SHA1 of the decrypted file using the existing function
                            string sha1Hash = CalculateSDCODCSha1Hash(decryptedFilePath);
                            Createdodcsha1TextBox.Text = sha1Hash;

                            XDocument odcXml = XDocument.Load(decryptedFilePath);
                            ProcessOdcXml(odcXml);
                            LogDebugInfo("HandleOdcFileDrop: Decrypted ODC XML file loaded successfully.");
                        }
                        catch (Exception ex2)
                        {
                            MessageBox.Show($"Error reading decrypted ODC file: {ex2.Message}");
                            LogDebugInfo("HandleOdcFileDrop: Error reading decrypted ODC file. Exception: " + ex2.Message);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Error reading or decrypting ODC file.");
                    LogDebugInfo("HandleOdcFileDrop: Decryption failed.");
                }
            }
        }

        private void ProcessOdcXml(XDocument odcXml)
        {
            LogDebugInfo("ProcessOdcXml: Processing ODC XML file.");
            odcNameTextBox.Text = odcXml.Root.Elements("name").FirstOrDefault()?.Value ?? "";
            odcDescriptionTextBox.Text = odcXml.Root.Elements("description").FirstOrDefault()?.Value ?? "";
            odcMakerTextBox.Text = odcXml.Root.Elements("maker").FirstOrDefault()?.Value ?? "";
            odcHDKVersionTextBox.Text = odcXml.Root.Attribute("hdk_version")?.Value ?? "";
            odcUUIDTextBox.Text = odcXml.Root.Element("uuid")?.Value ?? "";
            odcTimestampTextBox.Text = odcXml.Root.Element("timestamp")?.Value ?? "";

            var makerImagePath = odcXml.Root.Elements("maker_image").FirstOrDefault()?.Value ?? "";
            odcThumbnailSuffixTextBox.Text = ExtractODCThumbnailSuffix(makerImagePath);
            LogDebugInfo("ProcessOdcXml: ODC XML file processed successfully.");
        }


        private string ExtractODCThumbnailSuffix(string imagePath)
        {
            // Assuming the thumbnail suffix is in the format "_Txxx.png"
            var match = Regex.Match(imagePath, @"_T(\d+)\.png");
            return match.Success ? match.Groups[1].Value : "";
        }

        private void RadioButtonEBOOTBIN(object sender, RoutedEventArgs e)
        {

        }

        private void RadioButtonEBOOTELF(object sender, RoutedEventArgs e)
        {

        }

        private void RadioButtonEBOOTHDSELF(object sender, RoutedEventArgs e)
        {

        }

        private void RadioButtonEBOOTSELF(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBoxPSPLUS_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBoxSkipEula_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBoxCMDConsole_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBoxBlockProfanity_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBoxDrawDistanceHack_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void EbootPatcherDragDropHandler(object sender, DragEventArgs e)
        {
            LogDebugInfo("EBOOT Patcher: Drag and Drop initiated.");

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                LogDebugInfo("EBOOT Patcher: Data format is FileDrop.");

                string[] droppedItems = (string[])e.Data.GetData(DataFormats.FileDrop);
                string message = "Invalid file format.";

                if (droppedItems.Length > 0)
                {
                    LogDebugInfo($"EBOOT Patcher: Number of items dropped: {droppedItems.Length}");

                    if (Directory.Exists(droppedItems[0]))
                    {
                        LogDebugInfo("EBOOT Patcher: Dropped item is a directory.");
                        message = "This tool only supports one file at a time.";
                    }
                    else if (File.Exists(droppedItems[0]))
                    {
                        LogDebugInfo("EBOOT Patcher: Dropped item is a file.");
                        string file = droppedItems[0];

                        if (new[] { ".bin", ".elf", ".self" }.Contains(Path.GetExtension(file).ToLowerInvariant()))
                        {
                            LogDebugInfo("EBOOT Patcher: File format is valid.");
                            LoadEBOOT(file);
                            string fileName = Path.GetFileName(file);
                            string dirPath = Path.GetDirectoryName(file);
                            string shortenedPath = dirPath.Length > 17 ? "..." + dirPath.Substring(dirPath.Length - 17) : dirPath;
                            message = $"{shortenedPath}\\{fileName} Loaded";
                            LogDebugInfo($"EBOOT Patcher: {message}");
                        }
                        else
                        {
                            LogDebugInfo("EBOOT Patcher: File format is not supported.");
                            message = "This tool only supports bin, elf, and self files.";
                        }
                    }
                    else
                    {
                        LogDebugInfo("EBOOT Patcher: Dropped item is neither a file nor a directory.");
                    }
                }

                Dispatcher.Invoke(() =>
                {
                });
            }
            else
            {
                LogDebugInfo("EBOOT Patcher: Drag and Drop data format is not FileDrop.");
            }
        }





        private async void ClickToBrowseHandlerEbootPatcher(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("EBOOT Patcher: Click to Browse EBOOT File selected - Checking...");
            this.IsEnabled = false;  // Disable the main window to prevent interactions

            var openFileDialog = new VistaOpenFileDialog
            {
                Filter = "Eboot files (*.bin;*.elf;*.self)|*.bin;*.elf;*.self",
                Multiselect = false
            };
            string message = "No file selected.";

            bool? result = openFileDialog.ShowDialog();

            // Await a delay to absorb any unintentional clicks that happen after the dialog closes
            await Task.Delay(10); // 10 ms delay

            this.IsEnabled = true;  // Re-enable the main window after the delay

            if (result == true)
            {
                var selectedFile = openFileDialog.FileName;
                LoadEBOOT(selectedFile);

                if (!string.IsNullOrEmpty(selectedFile))
                {
                    string fileName = Path.GetFileName(selectedFile);
                    string dirPath = Path.GetDirectoryName(selectedFile);
                    string shortenedPath = dirPath.Length > 17 ? "..." + dirPath.Substring(dirPath.Length - 17) : dirPath;
                    message = $"{shortenedPath}\\{fileName} Loaded";
                }
                else
                {
                    message = "No Eboot file selected.";
                }
            }
            else
            {
                message = "File selection was canceled.";
            }

            Dispatcher.Invoke(() =>
            {
                EbootPatcherDragAreaText.Text = message;
            });
        }



        private void LoadEBOOT(string ebootFilePath)
        {
            LoadedEbootTitleID.Text = "";
            LoadedEbootServiceID.Text = "";
            LoadedEbootNPCommID.Text = "";
            LoadedEbootMuisVersion.Text = "";
            LoadedEbootAppID.Text = "";
            LoadedEbootTSSURL.Text = "";
            LoadedEbootOfflineName.Text = "";
            LogDebugInfo("EBOOT Patcher: Loading EBOOT " + ebootFilePath);

            // Update LoadedEbootFilePath TextBox
            LoadedEbootFilePath.Text = ebootFilePath;

            string destinationFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dependencies", "temp");
            string destinationFile = Path.Combine(destinationFolder, Path.GetFileName(ebootFilePath));

            // Check if the destination folder exists, create if not
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            // Clean the folder (delete all files in the folder)
            foreach (var file in Directory.GetFiles(destinationFolder))
            {
                File.Delete(file);
            }

            // Copy the Eboot file
            File.Copy(ebootFilePath, destinationFile, true);

            // Determine the file type and handle accordingly
            string extension = Path.GetExtension(ebootFilePath).ToLowerInvariant();
            switch (extension)
            {
                case ".bin":
                    HandleBINFile(destinationFile);
                    break;
                case ".elf":
                    HandleELFFile(destinationFile);
                    break;
                case ".self":
                    HandleSELFFile(destinationFile);
                    break;
                default:
                    // Handle unknown file type or add logging
                    break;
            }
        }


        private void HandleBINFile(string filePath)
        {
            string scetoolDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dependencies", "scetool");
            string scetoolPath = Path.Combine(scetoolDirectory, "scetool.exe");
            string tempDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dependencies", "temp");
            string originalFileName = Path.GetFileName(filePath);
            string copiedFilePath = Path.Combine(scetoolDirectory, originalFileName);
            string outputFilePath = Path.Combine(scetoolDirectory, "EBOOT.ELF");
            string message;
            int displayTime = 3000; // Default to 3 seconds for the message display

            // Check if scetool exists
            if (!File.Exists(scetoolPath))
            {
                message = "scetool.exe not found. Please ensure it is installed at " + scetoolDirectory;
                LogDebugInfo("EBOOT Patcher: " + message);
                Dispatcher.Invoke(() =>
                {
                    TemporaryMessageHelper.ShowTemporaryMessage(EbootPatcherDragAreaText, message, 2000);
                });
                return;
            }

            // Copy EBOOT.BIN to the scetool directory
            try
            {
                File.Copy(filePath, copiedFilePath, overwrite: true);
            }
            catch (IOException ioEx)
            {
                message = "Failed to copy EBOOT.BIN to scetool directory: " + ioEx.Message;
                LogDebugInfo("EBOOT Patcher: " + message);
                Dispatcher.Invoke(() =>
                {
                    TemporaryMessageHelper.ShowTemporaryMessage(EbootPatcherDragAreaText, message, 2000);
                });
                return;
            }

            // Prepare the process start info
            var startInfo = new ProcessStartInfo
            {
                FileName = scetoolPath,
                Arguments = $"--decrypt \"{originalFileName}\" \"{outputFilePath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = scetoolDirectory
            };

            // Set the CYGWIN environment variable to not warn about DOS-style paths
            startInfo.EnvironmentVariables["CYGWIN"] = "nodosfilewarning";

            // Execute the scetool command
            using (var process = Process.Start(startInfo))
            {
                process.WaitForExit();

                // Read the standard error stream to capture any error messages
                string stderr = process.StandardError.ReadToEnd();
                if (process.ExitCode != 0 || !string.IsNullOrEmpty(stderr))
                {
                    message = $"scetool failed to decrypt EBOOT.BIN: {stderr}";
                    LogDebugInfo("EBOOT Patcher: " + message);
                }
                else
                {
                    // Check if the EBOOT.ELF file exists before moving it
                    if (!File.Exists(outputFilePath))
                    {
                        message = "EBOOT failed to decrypt.";
                        LogDebugInfo("EBOOT Patcher: " + message);
                        Dispatcher.Invoke(() =>
                        {
                            TemporaryMessageHelper.ShowTemporaryMessage(EbootPatcherDragAreaText, message, 2000);
                        });
                        return; // Early return to skip the file moving process
                    }

                    message = $"Eboot decrypted successfully to: {outputFilePath}";
                    LogDebugInfo("EBOOT Patcher: " + message);

                    // Move the ELF file to the temp directory
                    string finalOutputPath = Path.Combine(tempDirectory, "EBOOT.ELF");
                    if (File.Exists(finalOutputPath)) File.Delete(finalOutputPath); // Ensure there's no conflict
                    File.Move(outputFilePath, finalOutputPath);

                    // Delete the copied EBOOT.BIN from scetool directory
                    File.Delete(copiedFilePath);

                    message = $"Eboot.ELF moved to: {finalOutputPath}";
                    LogDebugInfo("EBOOT Patcher: " + message);
                }

                // Assuming ParseEbootInfo() is a method you want to call after processing
                ParseEbootInfo();

                Dispatcher.Invoke(() =>
                {
                });
            }
        }

        private void HandleELFFile(string unusedFilePath)
        {
            string sourceFilePath = LoadedEbootFilePath.Text; // Get the path from the TextBox
            string tempDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dependencies", "temp");
            string finalOutputPath = Path.Combine(tempDirectory, "EBOOT.ELF");
            string message;
            int displayTime = 3000; // Default to 3 seconds for the message display

            LogDebugInfo("EBOOT Patcher: Handling ELF file.");
            LogDebugInfo("EBOOT Patcher: Source ELF file path from TextBox: " + sourceFilePath);

            // Check if the source ELF file exists
            if (!File.Exists(sourceFilePath))
            {
                message = "Failed to load ELF file: Source file does not exist.";
                LogDebugInfo("EBOOT Patcher: " + message);
                Dispatcher.Invoke(() =>
                {
                    TemporaryMessageHelper.ShowTemporaryMessage(EbootPatcherDragAreaText, message, 2000);
                });
                return;
            }

            // Check if the temp directory exists, create if not
            if (!Directory.Exists(tempDirectory))
            {
                LogDebugInfo("EBOOT Patcher: Temp directory does not exist, creating...");
                Directory.CreateDirectory(tempDirectory);
            }

            // Clean the temp folder (delete all files in the folder)
            foreach (var file in Directory.GetFiles(tempDirectory))
            {
                File.Delete(file);
            }

            try
            {
                // Move the ELF file to the temp directory (renamed to EBOOT.ELF)
                File.Copy(sourceFilePath, finalOutputPath);

                message = $"ELF file moved successfully to: {finalOutputPath}";
                LogDebugInfo("EBOOT Patcher: " + message);

                // Assuming ParseEbootInfo() is a method you want to call after processing
                ParseEbootInfo();
            }
            catch (IOException ioEx)
            {
                message = "Failed to move ELF file to temp directory: " + ioEx.Message;
                LogDebugInfo("EBOOT Patcher: " + message);
            }

            // Display the message in the GUI
            Dispatcher.Invoke(() =>
            {
            });
        }


        private void LoadEbootDefinitions()
        {
            string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dependencies", "ebootdefs.json");
            try
            {
                string jsonContent = File.ReadAllText(jsonFilePath);
                signatureToInfoMap = JsonConvert.DeserializeObject<Dictionary<string, EbootInfo>>(jsonContent);

                if (signatureToInfoMap == null)
                {
                    // Log an error or throw an exception
                    LogDebugInfo("Failed to deserialize the JSON content into a dictionary. JSON content might be empty.");
                }
                else if (signatureToInfoMap.Count == 0)
                {
                    // The JSON deserialized but the dictionary is empty
                    LogDebugInfo("EBOOT Patcher: The JSON content deserialized into an empty dictionary. Check the JSON content.");
                }
            }
            catch (Exception ex)
            {
                // Log the exception details
                LogDebugInfo($"EBOOT Patcher: Error loading EBOOT definitions: {ex.Message}");
                // Consider rethrowing the exception or handling it to prevent further execution
            }
        }




        private string UniqueSig;

        private void ParseEbootInfo()
        {
            string ebootElfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dependencies", "temp", "EBOOT.ELF");
            LoadEbootDefinitions();

            try
            {
                using (FileStream fs = new FileStream(ebootElfPath, FileMode.Open, FileAccess.Read))
                {
                    // Read the first 500KB of the file
                    const int bytesToRead = 500 * 1024; // 500KB
                    byte[] buffer = new byte[bytesToRead];
                    int bytesRead = fs.Read(buffer, 0, bytesToRead);

                    // Compute the SHA1 hash of the first 500KB
                    using (SHA1 sha1 = SHA1.Create())
                    {
                        byte[] hashBytes = sha1.ComputeHash(buffer, 0, bytesRead);
                        string sha1Hash = BitConverter.ToString(hashBytes).Replace("-", "");

                        // Log the SHA1 hash
                        LogDebugInfo($"EBOOT Patcher: SHA1 Hash: {sha1Hash}");

                        // Use the SHA1 hash for identification instead of unique signature
                        UniqueSig = sha1Hash; // Assuming you want to keep using UniqueSig for compatibility
                        CheckUniqueSigAndUpdateFields();
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo("EBOOT Patcher: Error parsing EBOOT.ELF: " + ex.Message);
            }
        }

        public class EbootInfo
        {
            public string Version { get; set; }
            public string Type { get; set; }
            public Dictionary<string, string> Offsets { get; set; } // Changed from long to string

            public EbootInfo(string version, string type, Dictionary<string, string> offsets)
            {
                Version = version;
                Type = type;
                Offsets = offsets;
            }
        }


        private Dictionary<string, EbootInfo> signatureToInfoMap;


        private void CheckUniqueSigAndUpdateFields()
        {
            if (signatureToInfoMap.TryGetValue(UniqueSig, out var ebootInfo))
            {
                string ebootElfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dependencies", "temp", "EBOOT.ELF");


                using (FileStream fs = new FileStream(ebootElfPath, FileMode.Open, FileAccess.Read))
                {
                    string firstPartOfMuis = "";
                    string secondPartOfMuis = "";

                    foreach (var offsetKey in ebootInfo.Offsets.Keys)
                    {
                        var offsetValue = ebootInfo.Offsets[offsetKey];

                        if (offsetValue != "0x0")
                        {
                            long offset = Convert.ToInt64(offsetValue, 16);
                            fs.Seek(offset, SeekOrigin.Begin);

                            var dataList = new List<byte>();
                            int nextByte;

                            if (offsetKey.Equals("AppIdOffset"))
                            {
                                byte[] appIdData = new byte[5];
                                fs.Read(appIdData, 0, appIdData.Length);
                                string appId = Encoding.UTF8.GetString(appIdData);
                                Dispatcher.Invoke(() =>
                                {
                                    UpdateGUIElement(offsetKey, appId);
                                });
                            }
                            else
                            {
                                while ((nextByte = fs.ReadByte()) != 0 && nextByte != -1)
                                {
                                    dataList.Add((byte)nextByte);
                                }
                                string data = Encoding.UTF8.GetString(dataList.ToArray());

                                if (offsetKey.Equals("MuisVersionOffset1"))
                                {
                                    firstPartOfMuis = data;
                                }
                                else if (offsetKey.Equals("MuisVersionOffset2"))
                                {
                                    secondPartOfMuis = data;
                                }
                                else
                                {
                                    Dispatcher.Invoke(() =>
                                    {
                                        UpdateGUIElement(offsetKey, data);
                                    });
                                }
                            }
                        }
                        else
                        {
                            Dispatcher.Invoke(() =>
                            {
                                UpdateGUIElement(offsetKey, null);
                            });
                        }
                    }

                    string fullMuisVersion = secondPartOfMuis != "" ? $"{firstPartOfMuis}.{secondPartOfMuis}" : firstPartOfMuis;
                    Dispatcher.Invoke(() =>
                    {
                        LoadedEbootMuisVersion.Text = fullMuisVersion;

                        // Determine the text based on whether the version matches
                        string displayText = ebootInfo.Version == fullMuisVersion
                            ? $"EBOOT Loaded: {ebootInfo.Type} {ebootInfo.Version}"
                            : $"EBOOT Loaded: {ebootInfo.Type} {ebootInfo.Version} (Spoofed to {fullMuisVersion})";

                        EbootPatcherDragAreaText.Text = displayText;

                        foreach (var key in new[] { "TitleIdOffset", "ServiceIdOffset", "NPCommIDOffset", "TssUrlOffset", "AppIdOffset", "OfflineNameOffset", "PSplusOffset", "EulaOffset", "CMDConsoleOffset", "ProfFilterOffset", "DrawDistOffset" })
                        {
                            if (!ebootInfo.Offsets.ContainsKey(key))
                            {
                                UpdateGUIElement(key, null);
                            }
                        }
                    });
                }
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    EbootPatcherDragAreaText.Text = "EBOOT version/type unknown. Please ensure you're using a supported EBOOT file.";
                });
            }
        }





        private void UpdateGUIElement(string key, string value)
        {
            switch (key)
            {
                case "TitleIdOffset":
                    LoadedEbootTitleID.Text = !string.IsNullOrEmpty(value) ? value : "Title ID Not Available";
                    break;
                case "ServiceIdOffset":
                    LoadedEbootServiceID.Text = !string.IsNullOrEmpty(value) ? value : "Service ID Not Available";
                    break;
                case "NPCommIDOffset":
                    LoadedEbootNPCommID.Text = !string.IsNullOrEmpty(value) ? value : "NP Comm ID Not Available";
                    break;
                case "TssUrlOffset":
                    LoadedEbootTSSURL.Text = !string.IsNullOrEmpty(value) ? value : "TSS URL Not Available";
                    break;
                case "AppIdOffset":
                    LoadedEbootAppID.Text = !string.IsNullOrEmpty(value) ? value : "App ID Not Found";
                    break;
                case "OfflineNameOffset":
                    LoadedEbootOfflineName.Text = !string.IsNullOrEmpty(value) ? value : "N/A (Online Only)";
                    break;
                case "PSplusOffset":
                    CheckBoxPSPLUS.IsChecked = value == "1" ? true : false; // Assuming unchecked if not "1"
                    break;
                case "CMDConsoleOffset":
                    CheckBoxCMDConsole.IsChecked = value == "1" ? true : false; // Assuming unchecked if not "1"
                    break;
                case "ProfFilterOffset":
                    CheckBoxBlockProfanity.IsChecked = value == "1" ? true : false; // Assuming unchecked if not "1"
                    break;
                case "DrawDistOffset":
                    CheckBoxDrawDistanceHack.IsChecked = value == "1" ? true : false; // Assuming unchecked if not "1"
                    break;
                    // Add additional cases as necessary with their own default messages or values
            }
        }

        private void PatchButton_Click(object sender, RoutedEventArgs e)
        {
            string ebootElfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dependencies", "temp", "EBOOT.ELF");
            PatchTitleID(ebootElfPath);
            PatchAppID(ebootElfPath);
            PatchNPCommID(ebootElfPath);
            PatchServiceID(ebootElfPath);
            PatchMuisVersion(ebootElfPath);
            PatchTSSURL(ebootElfPath);
            PatchOfflineName(ebootElfPath);
            PatchPSPLUS(ebootElfPath);
            PatchCMDConsole(ebootElfPath);
            PatchProfanity(ebootElfPath);
            PatchDrawDistance1(ebootElfPath);
            PatchDrawDistance2(ebootElfPath);

            // Check the radio button setting and act accordingly
            if (RadioButtonEBOOTBINitem.IsChecked == true)
            {
                EncryptEbootWithScetool();
            }
            else if (RadioButtonEBOOTELFitem.IsChecked == true)
            {
                SaveEbootElfCopy();
            }
            else
            {
                LogDebugInfo("EBOOT Patcher: No output format selected.");
            }

            TemporaryMessageHelper.ShowTemporaryMessage(EbootPatcherDragAreaText, "Eboot patched successfully.", 2000);
        }


        private void EncryptEbootWithScetool()
        {
            string elfFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dependencies", "temp", "EBOOT.ELF");
            string scetoolDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dependencies", "scetool");
            string scetoolPath = Path.Combine(scetoolDirectory, "scetool.exe");

            if (!File.Exists(scetoolPath))
            {
                string message = "scetool.exe not found. Please ensure it is installed at " + scetoolDirectory;
                LogDebugInfo(message);
                Dispatcher.Invoke(() =>
                {
                    TemporaryMessageHelper.ShowTemporaryMessage(EbootPatcherDragAreaText, message, 2000);
                });
                return;
            }

            string arguments = $"-l 72F990788F9CFF745725F08E4C128387 --sce-type=SELF --compress-data=TRUE --skip-sections=FALSE --key-revision=04 --self-ctrl-flags=4000000000000000000000000000000000000000000000000000000000000002 --self-auth-id=1010000001000003 --self-add-shdrs=TRUE --self-vendor-id=01000002 --self-app-version=0001008600000000 --self-type=NPDRM --self-fw-version=0003004000000000 --np-license-type=FREE --np-content-id=EP9000-NPIA00005_00-HOME000000000001 --np-app-type=EXEC --np-real-fname=\"EBOOT.BIN\" --encrypt \"{elfFilePath}\" \"EBOOT.BIN\"";


            var startInfo = new ProcessStartInfo
            {
                FileName = scetoolPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = scetoolDirectory
            };

            using (var process = Process.Start(startInfo))
            {
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    string error = process.StandardError.ReadToEnd();
                    LogDebugInfo($"scetool failed: {error}");
                    return;
                }
            }

            Dispatcher.Invoke(() =>
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "EBOOT File (*.BIN)|*.BIN",
                    Title = "Save the EBOOT.BIN file",
                    FileName = "EBOOT.BIN" // This sets the default file name in the dialog
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string savePath = saveFileDialog.FileName;
                    string binPath = Path.Combine(scetoolDirectory, "EBOOT.BIN");

                    if (File.Exists(binPath))
                    {
                        File.Move(binPath, savePath, overwrite: true);
                        LogDebugInfo($"EBOOT.BIN saved successfully to: {savePath}");
                    }
                    else
                    {
                        LogDebugInfo("EBOOT.BIN was not found after scetool process.");
                    }
                }
            });
        }

        private void SaveEbootElfCopy()
        {
            string elfFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dependencies", "temp", "EBOOT.ELF");

            // Check if the ELF file exists
            if (!File.Exists(elfFilePath))
            {
                Dispatcher.Invoke(() =>
                {
                    TemporaryMessageHelper.ShowTemporaryMessage(EbootPatcherDragAreaText, "EBOOT.ELF not found for saving.", 2000);
                });
                return;
            }

            // Prompt the user to save the EBOOT.ELF file
            Dispatcher.Invoke(() =>
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "ELF File (*.ELF)|*.ELF",
                    Title = "Save the EBOOT.ELF file",
                    FileName = "EBOOT.elf" // Pre-fill the file name
                };

                // Show the Save File Dialog. If the user clicked OK, save the file
                if (saveFileDialog.ShowDialog() == true)
                {
                    string savePath = saveFileDialog.FileName;
                    // Copy the EBOOT.ELF to the user's chosen path
                    File.Copy(elfFilePath, savePath, overwrite: true);
                    LogDebugInfo($"EBOOT.ELF saved successfully to: {savePath}");
                }
            });
        }


        private void PatchPSPLUS(string ebootElfPath)
        {
            // Prepare the byte arrays for both states
            byte[] enabledValue = Encoding.ASCII.GetBytes("xxxxxxxxxx"); // "xxxxxxxxxx" for checkbox enabled
            byte[] disabledValue = Encoding.ASCII.GetBytes("PSPlusOnly"); // "PSPlusOnly" for checkbox disabled

            try
            {
                using (FileStream fs = new FileStream(ebootElfPath, FileMode.Open, FileAccess.Write))
                {
                    if (signatureToInfoMap.TryGetValue(UniqueSig, out var ebootInfo))
                    {
                        // Define the list of keys for PS Plus offsets
                        var psPlusOffsetKeys = new List<string>
                {
                    "PSplusOffset",
                    "PSplusOffset2",
                };

                        foreach (var offsetKey in psPlusOffsetKeys)
                        {
                            // Check if the dictionary contains the offset
                            if (ebootInfo.Offsets.TryGetValue(offsetKey, out var offsetValue))
                            {
                                long offset = Convert.ToInt64(offsetValue, 16); // Convert hex string to long
                                fs.Seek(offset, SeekOrigin.Begin);

                                // Write the appropriate byte array based on the checkbox state
                                byte[] valueToWrite = CheckBoxPSPLUS.IsChecked == true ? enabledValue : disabledValue;
                                fs.Write(valueToWrite, 0, valueToWrite.Length);
                            }
                        }

                        LogDebugInfo("EBOOT Patcher: PS Plus patched successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"EBOOT Patcher: Error patching PS Plus: {ex.Message}");
            }
        }


        private void PatchCMDConsole(string ebootElfPath)
        {
            // Prepare the byte arrays for both states
            byte[] enabledValue = Encoding.ASCII.GetBytes("xxxxxxx"); // "xxxxxxx" for checkbox enabled
            byte[] disabledValue = Encoding.ASCII.GetBytes("**DEV**"); // "**DEV**" for checkbox disabled

            try
            {
                using (FileStream fs = new FileStream(ebootElfPath, FileMode.Open, FileAccess.Write))
                {
                    if (signatureToInfoMap.TryGetValue(UniqueSig, out var ebootInfo))
                    {
                        // Corrected variable name for CMD Console offsets
                        var cmdConsoleOffsetKeys = new List<string>
                {
                    "CMDConsoleOffset",
                };

                        foreach (var offsetKey in cmdConsoleOffsetKeys)
                        {
                            // Check if the dictionary contains the offset
                            if (ebootInfo.Offsets.TryGetValue(offsetKey, out var offsetValue))
                            {
                                long offset = Convert.ToInt64(offsetValue, 16); // Convert hex string to long
                                fs.Seek(offset, SeekOrigin.Begin);

                                // Write the appropriate byte array based on the checkbox state
                                byte[] valueToWrite = CheckBoxCMDConsole.IsChecked == true ? enabledValue : disabledValue;
                                fs.Write(valueToWrite, 0, valueToWrite.Length);
                            }
                        }

                        LogDebugInfo("EBOOT Patcher: CMD Console patched successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"EBOOT Patcher: Error patching CMD Console: {ex.Message}");
            }
        }


        private void PatchProfanity(string ebootElfPath)
        {
            // Prepare the byte arrays for both states
            byte[] enabledValue = Encoding.ASCII.GetBytes("xxxxxxxxxxxxxxx"); // "xxxxxxxxxxxxxxx" for checkbox enabled
            byte[] disabledValue = Encoding.ASCII.GetBytes("ProfanityFilter"); // "ProfanityFilter" for checkbox disabled

            try
            {
                using (FileStream fs = new FileStream(ebootElfPath, FileMode.Open, FileAccess.Write))
                {
                    if (signatureToInfoMap.TryGetValue(UniqueSig, out var ebootInfo))
                    {
                        // Corrected variable name for CMD Console offsets
                        var ProfanityOffsetKeys = new List<string>
                {
                    "ProfFilterOffset",
                };

                        foreach (var offsetKey in ProfanityOffsetKeys)
                        {
                            // Check if the dictionary contains the offset
                            if (ebootInfo.Offsets.TryGetValue(offsetKey, out var offsetValue))
                            {
                                long offset = Convert.ToInt64(offsetValue, 16); // Convert hex string to long
                                fs.Seek(offset, SeekOrigin.Begin);

                                // Write the appropriate byte array based on the checkbox state
                                byte[] valueToWrite = CheckBoxBlockProfanity.IsChecked == true ? enabledValue : disabledValue;
                                fs.Write(valueToWrite, 0, valueToWrite.Length);
                            }
                        }

                        LogDebugInfo("EBOOT Patcher: Profanity patched successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"EBOOT Patcher: Error patching Profanity filter: {ex.Message}");
            }
        }



        private void PatchDrawDistance1(string ebootElfPath)
        {
            // Prepare the byte arrays for both states
            byte[] enabledValue = Encoding.ASCII.GetBytes("lod1"); // "lod1" for checkbox enabled
            byte[] disabledValue = Encoding.ASCII.GetBytes("lod2"); // "lod2" for checkbox disabled

            try
            {
                using (FileStream fs = new FileStream(ebootElfPath, FileMode.Open, FileAccess.Write))
                {
                    if (signatureToInfoMap.TryGetValue(UniqueSig, out var ebootInfo))
                    {

                        var DDOffsetKeys = new List<string>
                {
                    "DrawDistOffset",
                    "DrawDistOffset3",
                    "DrawDistOffset5",
                    "DrawDistOffset7",
                };

                        foreach (var offsetKey in DDOffsetKeys)
                        {
                            // Check if the dictionary contains the offset
                            if (ebootInfo.Offsets.TryGetValue(offsetKey, out var offsetValue))
                            {
                                long offset = Convert.ToInt64(offsetValue, 16); // Convert hex string to long
                                fs.Seek(offset, SeekOrigin.Begin);

                                // Write the appropriate byte array based on the checkbox state
                                byte[] valueToWrite = CheckBoxDrawDistanceHack.IsChecked == true ? enabledValue : disabledValue;
                                fs.Write(valueToWrite, 0, valueToWrite.Length);
                            }
                        }

                        LogDebugInfo("EBOOT Patcher: PS Plus patched successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"EBOOT Patcher: Error patching PS Plus: {ex.Message}");
            }
        }

        private void PatchDrawDistance2(string ebootElfPath)
        {
            // Prepare the byte arrays for both states
            byte[] enabledValue = Encoding.ASCII.GetBytes("lod1"); // "lod1" for checkbox enabled
            byte[] disabledValue = Encoding.ASCII.GetBytes("lod3"); // "lod3" for checkbox disabled

            try
            {
                using (FileStream fs = new FileStream(ebootElfPath, FileMode.Open, FileAccess.Write))
                {
                    if (signatureToInfoMap.TryGetValue(UniqueSig, out var ebootInfo))
                    {
                        // Define the list of keys for PS Plus offsets
                        var DD2OffsetKeys = new List<string>
                {
                    "DrawDistOffset2",
                    "DrawDistOffset4",
                    "DrawDistOffset6",
                    "DrawDistOffset8",
                };

                        foreach (var offsetKey in DD2OffsetKeys)
                        {
                            // Check if the dictionary contains the offset
                            if (ebootInfo.Offsets.TryGetValue(offsetKey, out var offsetValue))
                            {
                                long offset = Convert.ToInt64(offsetValue, 16); // Convert hex string to long
                                fs.Seek(offset, SeekOrigin.Begin);

                                // Write the appropriate byte array based on the checkbox state
                                byte[] valueToWrite = CheckBoxDrawDistanceHack.IsChecked == true ? enabledValue : disabledValue;
                                fs.Write(valueToWrite, 0, valueToWrite.Length);
                            }
                        }

                        LogDebugInfo("EBOOT Patcher: PS Plus patched successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"EBOOT Patcher: Error patching PS Plus: {ex.Message}");
            }
        }


        private void PatchTitleID(string ebootElfPath)
        {
            // Retrieve the Title ID value from the textbox and ensure it's 9 bytes long, padding with '0' if necessary
            string titleIdValue = LoadedEbootTitleID.Text.PadRight(9, '0');

            // Reflect the padded value back in the GUI, so it matches what gets written to the file
            Dispatcher.Invoke(() =>
            {
                LoadedEbootTitleID.Text = titleIdValue;
            });

            byte[] bytesToWrite = Encoding.UTF8.GetBytes(titleIdValue); // Convert to bytes

            try
            {
                using (FileStream fs = new FileStream(ebootElfPath, FileMode.Open, FileAccess.Write))
                {
                    if (signatureToInfoMap.TryGetValue(UniqueSig, out var ebootInfo))
                    {
                        // List of possible Title ID offset keys
                        var titleIdOffsetKeys = new List<string>
                {
                    "TitleIdOffset",
                    "TitleIdOffset2",
                    "TitleIdOffset3",
                    "TitleIdOffset4",
                    "TitleIdOffset5",
                    "TitleIdOffset6",
                };

                        foreach (var offsetKey in titleIdOffsetKeys)
                        {
                            // Check if the offset exists in the dictionary
                            if (ebootInfo.Offsets.TryGetValue(offsetKey, out var offsetValue))
                            {
                                long offset = Convert.ToInt64(offsetValue, 16); // Convert hex string to long
                                fs.Seek(offset, SeekOrigin.Begin);
                                fs.Write(bytesToWrite, 0, bytesToWrite.Length); // Write exactly 9 bytes
                            }
                        }

                        LogDebugInfo("EBOOT Patcher: Title ID(s) patched successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"EBOOT Patcher: Error patching Title ID(s): {ex.Message}");
            }
        }

        private void PatchAppID(string ebootElfPath)
        {
            // Retrieve the App ID value from the textbox and ensure it's 5 bytes long, padding with '0' if necessary
            string appIdValue = LoadedEbootAppID.Text.PadRight(5, '0');

            // Reflect the padded value back in the GUI, so it matches what gets written to the file
            Dispatcher.Invoke(() =>
            {
                LoadedEbootAppID.Text = appIdValue;
            });

            byte[] bytesToWrite = Encoding.UTF8.GetBytes(appIdValue); // Convert to bytes

            try
            {
                using (FileStream fs = new FileStream(ebootElfPath, FileMode.Open, FileAccess.Write))
                {
                    if (signatureToInfoMap.TryGetValue(UniqueSig, out var ebootInfo))
                    {
                        // Check if the AppIdOffset exists in the dictionary
                        if (ebootInfo.Offsets.TryGetValue("AppIdOffset", out var offsetValue))
                        {
                            long offset = Convert.ToInt64(offsetValue, 16); // Convert hex string to long
                            fs.Seek(offset, SeekOrigin.Begin);
                            fs.Write(bytesToWrite, 0, bytesToWrite.Length); // Write exactly 5 bytes
                        }

                        LogDebugInfo("EBOOT Patcher: App ID patched successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"EBOOT Patcher: Error patching App ID: {ex.Message}");
            }
        }

        private void PatchNPCommID(string ebootElfPath)
        {
            // Retrieve the NPCommID value from the textbox, ensure it's at least 9 chars long, padding with '\0' (null) if necessary
            string npCommIdValue = LoadedEbootNPCommID.Text;
            // Ensure the string does not exceed 15 characters
            npCommIdValue = npCommIdValue.Length > 15 ? npCommIdValue.Substring(0, 15) : npCommIdValue.PadRight(9, '\0');

            // Reflect the padded/truncated value back in the GUI, so it matches what gets written to the file
            Dispatcher.Invoke(() =>
            {
                LoadedEbootNPCommID.Text = npCommIdValue.Replace("\0", ""); // Display without nulls in the GUI for clarity
            });

            byte[] bytesToWrite = Encoding.UTF8.GetBytes(npCommIdValue); // Convert to bytes

            try
            {
                using (FileStream fs = new FileStream(ebootElfPath, FileMode.Open, FileAccess.Write))
                {
                    if (signatureToInfoMap.TryGetValue(UniqueSig, out var ebootInfo))
                    {
                        // Check if the NPCommIDOffset exists in the dictionary
                        if (ebootInfo.Offsets.TryGetValue("NPCommIDOffset", out var offsetValue))
                        {
                            long offset = Convert.ToInt64(offsetValue, 16); // Convert hex string to long
                            fs.Seek(offset, SeekOrigin.Begin);
                            fs.Write(bytesToWrite, 0, bytesToWrite.Length); // Write the bytes, padded/truncated as necessary
                        }

                        LogDebugInfo("EBOOT Patcher: NPCommID patched successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"EBOOT Patcher: Error patching NPCommID: {ex.Message}");
            }
        }

        private void PatchServiceID(string ebootElfPath)
        {
            // Retrieve the Service ID value from the textbox and ensure it does not exceed 19 characters, padding with '0' if necessary
            string serviceIdValue = LoadedEbootServiceID.Text.PadRight(19, '0');

            // Reflect the padded value back in the GUI, so it matches what gets written to the file
            Dispatcher.Invoke(() =>
            {
                LoadedEbootServiceID.Text = serviceIdValue;
            });

            byte[] bytesToWrite = Encoding.UTF8.GetBytes(serviceIdValue); // Convert to bytes

            try
            {
                using (FileStream fs = new FileStream(ebootElfPath, FileMode.Open, FileAccess.Write))
                {
                    if (signatureToInfoMap.TryGetValue(UniqueSig, out var ebootInfo))
                    {
                        // Check if the ServiceIdOffset exists in the dictionary
                        if (ebootInfo.Offsets.TryGetValue("ServiceIdOffset", out var offsetValue))
                        {
                            long offset = Convert.ToInt64(offsetValue, 16); // Convert hex string to long
                            fs.Seek(offset, SeekOrigin.Begin);
                            fs.Write(bytesToWrite, 0, bytesToWrite.Length); // Write the bytes, padded as necessary
                        }

                        LogDebugInfo("EBOOT Patcher: Service ID patched successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"EBOOT Patcher: Error patching Service ID: {ex.Message}");
            }
        }

        private void PatchMuisVersion(string ebootElfPath)
        {
            // Ensure the MuisVersion input is exactly 8 characters long, padding with '0' and '.00' as necessary
            string muisVersionInput = LoadedEbootMuisVersion.Text;
            if (muisVersionInput.Length < 8)
            {
                muisVersionInput = muisVersionInput.PadRight(5, '0') + ".00";
            }

            // Split the processed input into parts for MuisVersionOffset1 and MuisVersionOffset2
            string muisVersionPart1 = muisVersionInput.Substring(0, 5); // First 5 characters for MuisVersionOffset1
            string muisVersionPart2 = muisVersionInput.Substring(5, 3); // Last 3 characters for MuisVersionOffset2, includes the period

            // If muisVersionPart2 starts with a period, remove it
            if (muisVersionPart2.StartsWith("."))
            {
                muisVersionPart2 = muisVersionPart2.TrimStart('.');
            }

            // Reflect the processed value back in the GUI
            Dispatcher.Invoke(() =>
            {
                LoadedEbootMuisVersion.Text = muisVersionInput;
            });

            try
            {
                using (FileStream fs = new FileStream(ebootElfPath, FileMode.Open, FileAccess.Write))
                {
                    if (signatureToInfoMap.TryGetValue(UniqueSig, out var ebootInfo))
                    {
                        // Patch MuisVersionOffset1
                        if (ebootInfo.Offsets.TryGetValue("MuisVersionOffset1", out var offsetValue1))
                        {
                            PatchOffsetWithBytes(fs, offsetValue1, muisVersionPart1);
                        }

                        // Patch MuisVersionOffset2, if it exists
                        if (ebootInfo.Offsets.TryGetValue("MuisVersionOffset2", out var offsetValue2))
                        {
                            PatchOffsetWithBytes(fs, offsetValue2, muisVersionPart2);
                        }

                        LogDebugInfo("EBOOT Patcher: MuisVersion patched successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"EBOOT Patcher: Error patching MuisVersion: {ex.Message}");
            }
        }

        private void PatchOffsetWithBytes(FileStream fs, string hexOffset, string value)
        {
            long offset = Convert.ToInt64(hexOffset, 16); // Convert hex string to long
            byte[] bytesToWrite = Encoding.UTF8.GetBytes(value); // Convert string value to bytes
            fs.Seek(offset, SeekOrigin.Begin);
            fs.Write(bytesToWrite, 0, bytesToWrite.Length); // Write the bytes to the specified offset
        }


        private void PatchTSSURL(string ebootElfPath)
        {
            string tssUrlValue = LoadedEbootTSSURL.Text;

            try
            {
                using (FileStream fs = new FileStream(ebootElfPath, FileMode.Open, FileAccess.ReadWrite))
                {
                    if (signatureToInfoMap.TryGetValue(UniqueSig, out var ebootInfo))
                    {
                        if (ebootInfo.Offsets.TryGetValue("TssUrlOffset", out var offsetValue))
                        {
                            long offset = Convert.ToInt64(offsetValue, 16); // Convert hex string to long
                            byte[] urlBytes = Encoding.UTF8.GetBytes(tssUrlValue);
                            byte[] padding = new byte[1] { 0 }; // Null byte for padding

                            fs.Seek(offset, SeekOrigin.Begin);
                            fs.Write(urlBytes, 0, urlBytes.Length); // Write the URL bytes

                            // Now pad with nulls until the first null byte in the file,
                            // indicating the previous URL's end or the maximum length allowed.
                            long nextBytePosition = fs.Position; // Get the current position after writing URL
                            int nextByte = fs.ReadByte(); // Read the next byte to check if it's null
                            while (nextByte != 0 && nextByte != -1) // -1 is EOF
                            {
                                fs.Seek(nextBytePosition, SeekOrigin.Begin); // Move back to the position to overwrite with null
                                fs.Write(padding, 0, padding.Length); // Write a null byte for padding
                                nextBytePosition++; // Move to the next position
                                fs.Seek(nextBytePosition, SeekOrigin.Begin); // Move the pointer to the next byte to check
                                nextByte = fs.ReadByte(); // Read the next byte to check if it's null
                            }

                            LogDebugInfo("EBOOT Patcher: TSS URL patched and padded successfully.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"EBOOT Patcher: Error patching TSS URL: {ex.Message}");
            }
        }

        private void PatchOfflineName(string ebootElfPath)
        {
            string offlineNameValue = LoadedEbootOfflineName.Text;

            try
            {
                using (FileStream fs = new FileStream(ebootElfPath, FileMode.Open, FileAccess.ReadWrite))
                {
                    if (signatureToInfoMap.TryGetValue(UniqueSig, out var ebootInfo))
                    {
                        if (ebootInfo.Offsets.TryGetValue("OfflineNameOffset", out var offsetValue))
                        {
                            long offset = Convert.ToInt64(offsetValue, 16); // Convert hex string to long
                            fs.Seek(offset, SeekOrigin.Begin);

                            int existingLength = 0;
                            while (fs.ReadByte() != 0)
                            {
                                existingLength++;
                            }

                            int totalAvailableSpace = existingLength + CalculateAvailableNulls(fs);
                            int maxLength = Math.Min(offlineNameValue.Length, totalAvailableSpace);
                            offlineNameValue = offlineNameValue.Substring(0, maxLength);

                            // Update the textbox with the potentially trimmed string
                            Dispatcher.Invoke(() =>
                            {
                                LoadedEbootOfflineName.Text = offlineNameValue;
                            });

                            byte[] nameBytes = Encoding.UTF8.GetBytes(offlineNameValue);
                            fs.Seek(offset, SeekOrigin.Begin); // Reset position to the start of the string
                            fs.Write(nameBytes, 0, nameBytes.Length); // Write the new name

                            // Pad the remaining space with null bytes
                            for (int i = nameBytes.Length; i < totalAvailableSpace; i++)
                            {
                                fs.WriteByte(0);
                            }

                            LogDebugInfo("EBOOT Patcher: Offline name patched and textbox updated successfully.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"EBOOT Patcher: Error patching Offline Name: {ex.Message}");
            }
        }


        private int CalculateAvailableNulls(FileStream fs)
        {
            int nullCount = 0;
            int nextByte = fs.ReadByte();

            // Count consecutive null bytes from the current position
            while (nextByte == 0)
            {
                nullCount++;
                nextByte = fs.ReadByte();
            }

            // Rewind the stream back to the start of the null sequence
            fs.Seek(-(nullCount + 1), SeekOrigin.Current);

            return nullCount;
        }

        private static void UncompressFile(string compressedFilePath, string extractionFolderPath)
        {
            try
            {
                ZipFile.ExtractToDirectory(compressedFilePath, extractionFolderPath);
            }
            catch (Exception ex)
            {
                CustomLogger.LoggerAccessor.LogInfo($"[File Uncompress] - An error occurred: {ex}");
            }
        }

        private void HandleSELFFile(string filePath)
        {
            // Processing specific to .self files
            // ...
        }




        private void CheckBoxLSTEncrypterLegacyMode(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBoxLSTDecrypterLegacyMode(object sender, RoutedEventArgs e)
        {

        }



        private void CheckBoxSaveDecInfs_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBoxSaveDecInfs_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBoxSaveDecInfsCACHE_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBoxSaveDecInfsCACHE_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBoxArchiveMapperVerify_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBoxArchiveMapperVerify_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBoxTTYSpam_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBoxMLAA_Checked(object sender, RoutedEventArgs e)
        {

        }

        private async void ArchiveUnpackerVerifyTestButtonClick(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("Archive Unpacker: Browsing for directory initiated");

            // Disable the main window to prevent interactions while the dialog is open
            this.IsEnabled = false;

            var folderDialog = new VistaFolderBrowserDialog();

            bool? result = folderDialog.ShowDialog();

            // Await a delay to absorb any unintentional clicks that happen after the dialog closes
            await Task.Delay(10); // 10 ms delay

            // Re-enable the main window after the delay
            this.IsEnabled = true;

            if (result == true && !string.IsNullOrEmpty(folderDialog.SelectedPath))
            {
                string selectedDirectory = folderDialog.SelectedPath;
                await ScanDirectoryAsync(selectedDirectory);
            }
            else
            {
                LogDebugInfo("Archive Unpacker: No directory selected in folder browser.");
                MessageBox.Show("No directory selected.");
            }
        }



        private async void VerifyDragArea_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedItems = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (droppedItems.Length > 0)
                {
                    string firstItem = droppedItems[0];
                    FileScannerStandalone scanner = new FileScannerStandalone(Sha1TextBox);

                    if (System.IO.Directory.Exists(firstItem))
                    {
                        await scanner.ScanDirectoryAsync(firstItem);
                    }
                    else if (System.IO.File.Exists(firstItem))
                    {
                        await scanner.ScanFileAsync(firstItem);
                    }
                    else
                    {
                        MessageBox.Show("Please drop a valid folder or file.");
                    }
                }
            }
        }

        private async Task ScanDirectoryAsync(string directoryPath)
        {
            try
            {
                FileScannerStandalone scanner = new FileScannerStandalone(Sha1TextBox);
                await scanner.ScanDirectoryAsync(directoryPath);
                MessageBox.Show("Scan completed!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }


        private void ArchiveUnpackerTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Get the text from the MappedOutputDirectoryTextBox
            string path = MappedOutputDirectoryTextBox.Text;

            // Check if the path is a valid directory, if not create it
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Open the directory in File Explorer
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        private void ArchiveCreatorTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Get the text from the BarSdatSharcOutputDirectoryTextBox
            string path = BarSdatSharcOutputDirectoryTextBox.Text;

            // Check if the path is a valid directory, if not create it
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Open the directory in File Explorer
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        private void CDSEncrypterTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Get the text from the CdsOutputDirectoryTextBox
            string path = CdsEncryptOutputDirectoryTextBox.Text;

            // Check if the path is a valid directory, if not create it
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Open the directory in File Explorer
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        private void CDSDecrypterTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Get the text from the CdsOutputDirectoryTextBox
            string path = CdsDecryptOutputDirectoryTextBox.Text;

            // Check if the path is a valid directory, if not create it
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Open the directory in File Explorer
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        private void HCDBEncrypterTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Get the text from the HcdbOutputDirectoryTextBox
            string path = HcdbOutputDirectoryTextBox.Text;

            // Check if the path is a valid directory, if not create it
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Open the directory in File Explorer
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        private void HCDBDecrypterTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Get the text from the sqlOutputDirectoryTextBox
            string path = SqlOutputDirectoryTextBox.Text;

            // Check if the path is a valid directory, if not create it
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Open the directory in File Explorer
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
                Verb = "open"
            });
        }


        // BNKUnpacker Handlers
        private void BNKUnpackerDragDropHandler(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedItems = (string[])e.Data.GetData(DataFormats.FileDrop);
                var validFiles = droppedItems.Where(item => File.Exists(item) && Path.GetExtension(item).Equals(".bnk", StringComparison.OrdinalIgnoreCase)).ToList();

                if (validFiles.Any())
                {
                    BNKUnpackerTextBox.Text = string.Join(Environment.NewLine, validFiles);
                    TemporaryMessageHelper.ShowTemporaryMessage(BNKUnpackerDragAreaText, $"{validFiles.Count} items added", 2000);
                }
                else
                {
                    TemporaryMessageHelper.ShowTemporaryMessage(BNKUnpackerDragAreaText, "No valid BNK files found", 2000);
                }
            }
        }

        private void ClickToBrowseHandlerBNKUnpacker(object sender, MouseButtonEventArgs e)
        {
            var openFileDialog = new VistaOpenFileDialog
            {
                Filter = "BNK files (*.bnk)|*.bnk",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var selectedFiles = openFileDialog.FileNames.ToList();
                if (selectedFiles.Any())
                {
                    BNKUnpackerTextBox.Text = string.Join(Environment.NewLine, selectedFiles);
                    TemporaryMessageHelper.ShowTemporaryMessage(BNKUnpackerDragAreaText, $"{selectedFiles.Count} items added", 2000);
                }
                else
                {
                    TemporaryMessageHelper.ShowTemporaryMessage(BNKUnpackerDragAreaText, "No files selected", 2000);
                }
            }
        }

        private async void BNKUnpackerExecuteButtonClick(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("BNK Unpacker: Process Started");

            var inputFiles = BNKUnpackerTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (!inputFiles.Any())
            {
                TemporaryMessageHelper.ShowTemporaryMessage(BNKUnpackerDragAreaText, "No files to unpack", 2000);
                return;
            }

            string outputDirectory = AudioOutputDirectoryTextBox.Text;
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                TemporaryMessageHelper.ShowTemporaryMessage(BNKUnpackerDragAreaText, "Invalid output directory", 2000);
                return;
            }

            // Create the output directory if it doesn't exist
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
                TemporaryMessageHelper.ShowTemporaryMessage(BNKUnpackerDragAreaText, $"Created output directory: {outputDirectory}", 2000);
            }

            foreach (var inputFile in inputFiles)
            {
                await UnpackBNKFile(inputFile, outputDirectory);
            }

            LogDebugInfo("BNK Unpacker: Process Completed");
            TemporaryMessageHelper.ShowTemporaryMessage(BNKUnpackerDragAreaText, "Unpacking completed", 2000);
        }

        private void ClearBNKUnpackerListHandler(object sender, RoutedEventArgs e)
        {
            BNKUnpackerTextBox.Clear();
            TemporaryMessageHelper.ShowTemporaryMessage(BNKUnpackerDragAreaText, "List cleared", 2000);
        }

        private void BNKUnpackerTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string outputDirectory = AudioOutputDirectoryTextBox.Text;

            if (!string.IsNullOrEmpty(outputDirectory) && Directory.Exists(outputDirectory))
            {
                try
                {
                    // Open the directory in File Explorer
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName = outputDirectory,
                        UseShellExecute = true,
                        Verb = "open"
                    });
                    LogDebugInfo($"BNK Unpacker: Opened output directory {outputDirectory}");
                }
                catch (Exception ex)
                {
                    // Handle any exceptions
                    LogDebugInfo($"BNK Unpacker: ERROR: Unable to open directory - {ex.Message}");
                    MessageBox.Show($"Unable to open directory: {ex.Message}");
                }
            }
            else
            {
                LogDebugInfo($"BNK Unpacker: ERROR: Directory {outputDirectory} is not set or does not exist.");
                MessageBox.Show("Output directory is not set or does not exist.");
            }
        }

        private async Task UnpackBNKFile(string inputFile, string outputDirectory)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputFile);
            string outputSubDirectory = Path.Combine(outputDirectory, fileNameWithoutExtension);
            Directory.CreateDirectory(outputSubDirectory);

            string vgmstreamCliPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dependencies", "vgmstream", "vgmstream-cli.exe");
            if (!File.Exists(vgmstreamCliPath))
            {
                LogDebugInfo("Error: vgmstream-cli.exe not found");
                return;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = vgmstreamCliPath,
                Arguments = $"-o \"{Path.Combine(outputSubDirectory, $"{fileNameWithoutExtension}_?s_?n.wav")}\" -i \"{inputFile}\" -S 0",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = startInfo })
            {
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                AppendTextToBNKUnpackerTextBox($"Output for {Path.GetFileName(inputFile)}:");
                AppendTextToBNKUnpackerTextBox(output);
                AppendTextToBNKUnpackerTextBox(error);

                if (process.ExitCode == 0)
                {
                    LogDebugInfo($"Unpacked: {inputFile} to {outputSubDirectory}");
                }
                else
                {
                    LogDebugInfo($"Error unpacking {inputFile}: {error}");
                }
            }
        }

        private void AppendTextToBNKUnpackerTextBox(string text)
        {
            // If called from a non-UI thread, use Dispatcher
            Dispatcher.Invoke(() =>
            {
                BNKUnpackerTextBox.AppendText(text + Environment.NewLine);

                // Scrolls the text box to the end after appending text
                BNKUnpackerTextBox.ScrollToEnd();
            });
        }

        private void CheckBoxArchiveCreatorRenameCDN_Checked(object sender, RoutedEventArgs e)
        {
            if (CheckBoxArchiveCreatorRenameLocal.IsChecked == true)
            {
                CheckBoxArchiveCreatorRenameLocal.IsChecked = false;
            }
        }

        private void CheckBoxArchiveCreatorRenameCDN_Unchecked(object sender, RoutedEventArgs e)
        {
            // Optionally handle when the CDN checkbox is unchecked
        }

        private void CheckBoxArchiveCreatorRenameLocal_Checked(object sender, RoutedEventArgs e)
        {
            if (CheckBoxArchiveCreatorRenameCDN.IsChecked == true)
            {
                CheckBoxArchiveCreatorRenameCDN.IsChecked = false;
            }
        }

        private void CheckBoxArchiveCreatorRenameLocal_Unchecked(object sender, RoutedEventArgs e)
        {
            // Optionally handle when the Local checkbox is unchecked
        }


        private async void PS3PingButton_Click(object sender, RoutedEventArgs e)
        {
            async Task<bool> PingIpAddressAsync(string ipAddress)
            {
                try
                {
                    using (Ping ping = new Ping())
                    {
                        PingReply reply = await ping.SendPingAsync(ipAddress, 1000); // Timeout set to 1000 ms (1 second)
                        return reply.Status == IPStatus.Success;
                    }
                }
                catch
                {
                    return false;
                }
            }

            void ShowPingResult(string message, Color color)
            {
                PingResultTextBlock.Text = message;
                PingResultTextBlock.Foreground = new SolidColorBrush(color);
            }

            async Task ShowPingResultForDurationAsync(string message, Color color, int durationInMilliseconds)
            {
                ShowPingResult(message, color);
                await Task.Delay(durationInMilliseconds);
            }

            async Task<bool> CheckFileExistsOnFtpAsync(string ftpUri)
            {
                try
                {
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUri);
                    request.Method = WebRequestMethods.Ftp.GetFileSize;
                    request.Credentials = new NetworkCredential("anonymous", "anonymous@example.com"); // Use anonymous credentials

                    using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                    {
                        return true;
                    }
                }
                catch (WebException ex)
                {
                    if (ex.Response != null)
                    {
                        var response = (FtpWebResponse)ex.Response;
                        if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                        {
                            return false;
                        }
                    }
                    throw;
                }
            }

            async Task<byte[]> DownloadFileFromFtpAsync(string ftpUri)
            {
                try
                {
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUri);
                    request.Method = WebRequestMethods.Ftp.DownloadFile;
                    request.Credentials = new NetworkCredential("anonymous", "anonymous@example.com"); // Use anonymous credentials

                    using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                    using (Stream responseStream = response.GetResponseStream())
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        await responseStream.CopyToAsync(memoryStream);
                        return memoryStream.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    ShowPingResult($"Error downloading file: {ex.Message}", Colors.Red);
                    return null;
                }
            }

            string ExtractVersionString(byte[] fileContent)
            {
                byte[] versionPattern = Encoding.ASCII.GetBytes("VERSION");
                int index = SearchPatternInBytes(fileContent, versionPattern);
                if (index != -1)
                {
                    int startIndex = index + versionPattern.Length;
                    // Skip null bytes after VERSION to find the version string in 00.00 format
                    while (startIndex < fileContent.Length && fileContent[startIndex] == 0x00)
                    {
                        startIndex++;
                    }
                    if (startIndex + 4 < fileContent.Length)
                    {
                        return Encoding.ASCII.GetString(fileContent, startIndex, 5); // Extract version number in 00.00 format
                    }
                }
                return null;
            }

            int SearchPatternInBytes(byte[] data, byte[] pattern)
            {
                for (int i = 0; i <= data.Length - pattern.Length; i++)
                {
                    bool match = true;
                    for (int j = 0; j < pattern.Length; j++)
                    {
                        if (data[i + j] != pattern[j])
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                    {
                        return i;
                    }
                }
                return -1;
            }

            string ipAddress = PS3IPforFTPTextBox.Text;
            string ps3TitleID = PS3TitleIDTextBox.Text;

            if (string.IsNullOrEmpty(ipAddress))
            {
                await ShowPingResultForDurationAsync("Invalid IP!", Colors.Red, 1000);
                return;
            }

            bool pingResult = await PingIpAddressAsync(ipAddress);
            if (pingResult)
            {
                await ShowPingResultForDurationAsync("Stage 1: Success - Device Found!", Colors.LimeGreen, 800);

                // FTP connection details
                string ftpUri = $"ftp://{ipAddress}/dev_hdd0/game/{ps3TitleID}/PARAM.SFO";

                bool fileExists = await CheckFileExistsOnFtpAsync(ftpUri);
                if (fileExists)
                {
                    await ShowPingResultForDurationAsync("Stage 2: Success - This is a PS3!", Colors.LimeGreen, 800);

                    byte[] fileContent = await DownloadFileFromFtpAsync(ftpUri);
                    if (fileContent != null)
                    {
                        string versionString = ExtractVersionString(fileContent);
                        if (!string.IsNullOrEmpty(versionString))
                        {
                            ShowPingResult($"Stage 3: Success - Home Version: {versionString} Found!", Colors.LimeGreen);
                        }
                        else
                        {
                            ShowPingResult("Version string not found!", Colors.Orange);
                        }
                    }
                }
                else
                {
                    await ShowPingResultForDurationAsync("File not found on FTP!", Colors.Orange, 1000);
                }
            }
            else
            {
                await ShowPingResultForDurationAsync("Failed!", Colors.Red, 1000);
            }
        }


        private async Task UploadFileToFtpAsync(string ftpUri, string localFilePath)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUri);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential("anonymous", "anonymous@example.com"); // Use anonymous credentials

                byte[] fileContents = await File.ReadAllBytesAsync(localFilePath);
                request.ContentLength = fileContents.Length;

                using (Stream requestStream = request.GetRequestStream())
                {
                    await requestStream.WriteAsync(fileContents, 0, fileContents.Length);
                }

                using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                {
                    Console.WriteLine($"Upload File Complete, status {response.StatusDescription}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to upload file, Make sure PS3 IP is set correctly in Nautilus settings and that FTP is running on your PS3 console., Webman MOD recommended for background FTP, Multiman works fine too: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void SaveListToFileAsXML(string listJson, int listNumber)
        {
            try
            {
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string outputDir = Path.Combine(exeDirectory, "Output", "Catalogue");

                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                string filePath = Path.Combine(outputDir, $"UserList_{listNumber}.json");

                var updatedJson = ProcessJsonAndAddRewardType(listJson);

                File.WriteAllText(filePath, updatedJson);

                string xmlFilePath = GenerateXmlFromJson(updatedJson, outputDir);

                // Read the XML file and encode it as base64
                byte[] xmlFileBytes = File.ReadAllBytes(xmlFilePath);
                string base64XmlFile = Convert.ToBase64String(xmlFileBytes);
                var downloadMessage = new { action = "downloadSqlFile", fileName = "UserList.xml", fileContent = base64XmlFile };
                var downloadMessageJson = JsonConvert.SerializeObject(downloadMessage);

                WebView2Control.CoreWebView2.PostWebMessageAsString(downloadMessageJson);

                Console.WriteLine($"Successfully saved the list to {filePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save the list: {ex.Message}");
            }
        }

        private string GenerateXmlFromJson(string listJson, string outputDir)
        {
            var jsonArray = JArray.Parse(listJson);
            var xmlDocument = new XDocument(new XElement("inventory"));

            foreach (var item in jsonArray)
            {
                var folderName = item["folderName"]?.ToString();
                if (!string.IsNullOrEmpty(folderName))
                {
                    xmlDocument.Root.Add(new XElement("object", new XAttribute("uuid", folderName)));
                }
            }

            string xmlFilePath = Path.Combine(outputDir, "UserList.xml");
            xmlDocument.Save(xmlFilePath);

            Console.WriteLine($"Successfully generated the XML to {xmlFilePath}");

            return xmlFilePath;
        }

        private async void ShowNotification(string message, int durationInSeconds)
        {
            NotificationTextBlock.Text = message;
            NotificationPopup.IsOpen = true;

            await Task.Delay(durationInSeconds * 1000);

            NotificationPopup.IsOpen = false;
        }

        private int NexTIndex { get; set; }
        private Dictionary<string, Dictionary<string, string>> unsavedChanges = new Dictionary<string, Dictionary<string, string>>();
        private string sqlFilePath;

        private void BrowseSQLButton_Click(object sender, RoutedEventArgs e)
        {
            ClearUnsavedChanges();
            var initialDirectory = SqlOutputDirectoryTextBox.Text;

            // Create the directory if it does not exist
            if (!Directory.Exists(initialDirectory))
            {
                Directory.CreateDirectory(initialDirectory);
            }

            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                InitialDirectory = initialDirectory,
                RestoreDirectory = true,
                Filter = "All supported files (*.sql;*.hcdb;*.xml)|*.sql;*.hcdb;*.xml|SQL files (*.sql)|*.sql|HCDB files (*.hcdb)|*.hcdb|XML files (*.xml)|*.xml|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                string fileExtension = Path.GetExtension(filePath).ToLower();

                switch (fileExtension)
                {
                    case ".sql":
                        HandleSQLFile(filePath);
                        break;
                    case ".hcdb":
                        HandleHCDBFile(filePath);
                        break;
                    case ".xml":
                        HandleXMLFile(filePath);
                        break;
                    default:
                        MessageBox.Show("Unsupported file type selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }
            }
        }

        private void Border_DragSQLEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    string fileExtension = Path.GetExtension(files[0]).ToLower();
                    if (fileExtension == ".sql")
                    {
                        e.Effects = DragDropEffects.Copy;
                    }
                    else
                    {
                        e.Effects = DragDropEffects.None;
                    }
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void Border_SQLDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    string filePath = files[0];
                    string fileExtension = Path.GetExtension(filePath).ToLower();

                    if (fileExtension == ".sql")
                    {
                        HandleSQLFile(filePath);
                    }
                    else
                    {
                        MessageBox.Show("Only SQL files are supported for drag and drop.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            e.Handled = true;
        }


        private async void HandleSQLFile(string filePath)
        {
            LogDebugInfo($"HandleSQLFile invoked with filePath: {filePath}");
            sqlFilePath = filePath;  // Store the file path

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                int maxIndex = LoadMaxIndexFromSQLFile(filePath);

                // Set the NexTIndex property
                NexTIndex = maxIndex + 1;

                // Update the TextBox with NexTIndex
                txtObjectIndex.Text = NexTIndex.ToString();

                // Populate the grid with entries starting from index 0
                await PopulateGridWithEntries(0, 25); // Default count value of 25

                stopwatch.Stop();
                LogDebugInfo($"SQL file processed in {stopwatch.ElapsedMilliseconds} ms. Max Index: {maxIndex}, NexTIndex: {NexTIndex}");
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error loading and parsing SQL file: {ex.Message}");
                // Suppress the error message box
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        private int LoadMaxIndexFromSQLFile(string filePath)
        {
            int maxIndex = -1;
            bool retry = true;
            int attempts = 0;

            while (retry && attempts < 2)
            {
                SQLiteConnection connection = null;
                attempts++;

                try
                {
                    string connectionString = $"Data Source={filePath};Version=3;";
                    connection = new SQLiteConnection(connectionString);
                    connection.Open();
                    LogDebugInfo("SQLite connection opened successfully.");

                    string query = @"
            SELECT MAX(ObjectIndex) as MaxIndex
            FROM Objects";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        object result = command.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            maxIndex = Convert.ToInt32(result);
                        }
                    }

                    retry = false; // If successful, exit loop
                }
                catch (Exception ex)
                {
                    LogDebugInfo($"Error loading max index from SQL file on attempt {attempts}: {ex.Message}");

                    if (attempts < 2)
                    {
                        LogDebugInfo("Retrying...");
                    }
                    else
                    {
                        LogDebugInfo($"Error loading max index from SQL file: {ex.Message}");
                    }
                }
                finally
                {
                    connection?.Close();
                }
            }

            return maxIndex;
        }

        private async void GoToIndexButton_Click(object sender, RoutedEventArgs e)
        {
            ClearUnsavedChanges();
            string input = txtGoToIndex.Text;
            if (string.IsNullOrWhiteSpace(input))
            {
                LogDebugInfo("Input is empty.");
                return;
            }

            if (input.Contains('-'))
            {
                // Handle range input
                var parts = input.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[0], out int startIndex) && int.TryParse(parts[1], out int endIndex))
                {
                    if (startIndex > endIndex)
                    {
                        LogDebugInfo("Invalid range entered: start index is greater than end index.");
                        MessageBox.Show("Invalid range: start index is greater than end index.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    LogDebugInfo($"GoToIndexButton_Click invoked with range: {startIndex}-{endIndex}");
                    await PopulateGridWithEntries(startIndex, endIndex - startIndex + 1);
                }
                else
                {
                    LogDebugInfo("Invalid range entered: unable to parse start or end index.");
                    MessageBox.Show("Invalid range entered. Please enter a valid range.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (int.TryParse(input, out int startIndex))
            {
                // Handle single index input
                LogDebugInfo($"GoToIndexButton_Click invoked with startIndex: {startIndex}");
                await PopulateGridWithEntries(startIndex, 25);  // Load 50 entries starting from the specified index
            }
            else
            {
                LogDebugInfo("Invalid index entered.");
                MessageBox.Show("Invalid index entered. Please enter a valid index.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void HandleHCDBFile(string filePath)
        {
            // Add your code to handle HCDB files here
            MessageBox.Show($"Handling HCDB file: {filePath}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void HandleXMLFile(string filePath)
        {
            // Add your code to handle XML files here
            MessageBox.Show($"Handling XML file: {filePath}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private List<Dictionary<string, string>> LoadEntriesRange(string filePath, int startIndex, int count)
        {
            var entries = new List<Dictionary<string, string>>();

            try
            {
                LogDebugInfo($"Loading {count} object indexes from {filePath}, starting at index {startIndex}.");
                string connectionString = $"Data Source={filePath};Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    LogDebugInfo("SQLite connection opened successfully.");

                    // Step 1: Fetch the ObjectIndexes in the specified range
                    string fetchIndexesQuery = @"
                SELECT ObjectIndex
                FROM Objects
                WHERE ObjectIndex >= @startIndex
                ORDER BY ObjectIndex
                LIMIT @count";

                    var objectIndexes = new List<int>();
                    using (SQLiteCommand fetchIndexesCommand = new SQLiteCommand(fetchIndexesQuery, connection))
                    {
                        fetchIndexesCommand.Parameters.AddWithValue("@startIndex", startIndex);
                        fetchIndexesCommand.Parameters.AddWithValue("@count", count);

                        using (SQLiteDataReader reader = fetchIndexesCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                objectIndexes.Add(reader.GetInt32(0));
                            }
                        }
                    }

                    if (objectIndexes.Count == 0)
                    {
                        return entries; // No entries to process
                    }

                    // Step 2: Fetch the Objects and their Metadata for the selected ObjectIndexes
                    string query = @"
                SELECT 
                    o.ObjectIndex, 
                    o.ObjectId, 
                    o.Version, 
                    o.Location, 
                    o.InventoryEntryType, 
                    o.ArchiveTimeStamp, 
                    o.OdcSha1Digest, 
                    o.EntitlementIndex, 
                    o.RewardIndex, 
                    o.UserLocation, 
                    o.UserDateLastUsed,
                    m.KeyName,
                    m.Value
                FROM Objects o
                LEFT JOIN Metadata m ON o.ObjectIndex = m.ObjectIndex
                WHERE o.ObjectIndex IN (" + string.Join(",", objectIndexes) + @")
                ORDER BY o.ObjectIndex";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var entry = new Dictionary<string, string>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    string columnName = reader.GetName(i);
                                    object value = reader.GetValue(i);

                                    if (columnName == "OdcSha1Digest" && value != DBNull.Value)
                                    {
                                        // Convert the BLOB to a hex string
                                        byte[] blob = (byte[])value;
                                        entry[columnName] = BitConverter.ToString(blob).Replace("-", "").ToUpper();
                                    }
                                    else if ((columnName == "ArchiveTimeStamp" || columnName == "UserDateLastUsed") && value != DBNull.Value)
                                    {
                                        // Convert the timestamp to an 8-character hexadecimal string with leading zeros
                                        int timestamp = Convert.ToInt32(value);
                                        entry[columnName] = timestamp.ToString("X8");
                                    }
                                    else
                                    {
                                        entry[columnName] = value.ToString();
                                    }
                                }
                                entries.Add(entry);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error loading entries from SQL file: {ex.Message}");
            }

            return entries;
        }



        private int currentStartIndex = 0; // This will keep track of the current starting index

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            ClearUnsavedChanges();
            currentStartIndex += 25;
            PopulateGridWithEntriesSafe(currentStartIndex, 25);
            ScrollToTop();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ClearUnsavedChanges();
            currentStartIndex -= 25;
            if (currentStartIndex < 0)
            {
                currentStartIndex = 0; // Ensure we don't go below 0
            }
            PopulateGridWithEntriesSafe(currentStartIndex, 25);
            ScrollToTop();
        }

        private void ClearUnsavedChanges()
        {
            unsavedChanges.Clear();
            UnsavedChanges.Visibility = Visibility.Hidden;
        }


        internal async void PopulateGridWithEntriesSafe(int startIndex, int count)
        {
            LogDebugInfo($"PopulateGridWithEntriesSafe invoked with startIndex: {startIndex}, count: {count}");

            try
            {
                await PopulateGridWithEntries(startIndex, count);
                ScrollToTop(); // Scroll to the top after populating entries
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error populating grid: {ex.Message}");
                MessageBox.Show($"Error populating grid: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ScrollToTop()
        {
            sqlItemsScrollViewer?.ScrollToTop();
        }
        private async Task PopulateGridWithEntries(int startIndex, int count)
        {
            LogDebugInfo($"PopulateGridWithEntries invoked with startIndex: {startIndex}, count: {count}");

            var entries = LoadEntriesRange(sqlFilePath, startIndex, count);

            if (entries == null || entries.Count == 0)
            {
                LogDebugInfo("No entries loaded from database.");
                return;
            }

            // Define column widths
            var columnWidths = new Dictionary<string, GridLength>
    {
        { "ObjectIndex", new GridLength(50) },
        { "ObjectId", new GridLength(245) },
        { "Version", new GridLength(48) },
        { "Location", new GridLength(57) },
        { "InventoryEntryType", new GridLength(43) },
        { "ArchiveTimeStamp", new GridLength(72) },
        { "OdcSha1Digest", new GridLength(300) },
        { "EntitlementIndex", new GridLength(54) },
        { "RewardIndex", new GridLength(59) },
        { "UserLocation", new GridLength(55) },
        { "UserDateLastUsed", new GridLength(48) },
        { "KeyName", new GridLength(135) },
        { "Value", new GridLength(310) }
    };

            // Clear existing children and column definitions
            SQLItemsGrid.Children.Clear();
            SQLItemsGrid.ColumnDefinitions.Clear();
            SQLItemsGrid.RowDefinitions.Clear();

            // Add column definitions
            foreach (var column in columnWidths)
            {
                SQLItemsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = column.Value });
            }

            // Add row definitions for each entry
            foreach (var entry in entries)
            {
                SQLItemsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            // Sort and group metadata entries by ObjectIndex
            var sortedEntries = entries.GroupBy(e => e["ObjectIndex"])
                                       .Select(g => new
                                       {
                                           ObjectIndex = g.Key,
                                           Entries = g.OrderBy(e =>
                                           {
                                               var keyName = e["KeyName"];
                                               if (keyName.Contains("CLOTHING") || keyName.Contains("FURNITURE") || keyName.Contains("MINI_GAME"))
                                                   return 1;
                                               if (keyName.Contains("RIGS"))
                                                   return 2;
                                               if (keyName.Contains("ENTITLEMENT_ID"))
                                                   return 3;
                                               return keyName.Contains("_HEAT") ? 5 : 4; // Default order for other entries, _HEAT last
                                           }).ThenBy(e => e["KeyName"]).ToList() // Ensure consistent ordering within groups
                                       });

            // Add data incrementally
            int rowIndex = 0;
            foreach (var group in sortedEntries)
            {
                foreach (var entry in group.Entries)
                {
                    int colIndex = 0;
                    foreach (var column in columnWidths.Keys)
                    {
                        string displayValue = entry.ContainsKey(column) ? entry[column] : string.Empty;

                        // Modify the display value based on specific conditions
                        if (column == "Value")
                        {
                            if (displayValue == "00000000-00000000-00000010-00000000")
                            {
                                displayValue += " (Male)";
                            }
                            else if (displayValue == "00000000-00000000-00000010-00000001")
                            {
                                displayValue += " (Female)";
                            }
                        }

                        var textBox = new TextBox
                        {
                            Text = displayValue,
                            Margin = new Thickness(2),
                            Style = (Style)FindResource("SmallTextBoxStyle2")
                        };

                        // Make ObjectIndex and ObjectId read-only
                        if (column == "ObjectIndex" || column == "ObjectId")
                        {
                            textBox.IsReadOnly = true;
                        }

                        // Make other fields editable including Version, KeyName, Value, SHA1, and Timestamp
                        if (column == "Version" || column == "KeyName" || column == "Value" || column == "OdcSha1Digest" || column == "ArchiveTimeStamp")
                        {
                            // Store original key name and value
                            string originalKeyName = entry.ContainsKey("KeyName") ? entry["KeyName"] : string.Empty;
                            string originalValue = entry.ContainsKey("Value") ? entry["Value"] : string.Empty;

                            textBox.Tag = new { OriginalKeyName = originalKeyName, OriginalValue = originalValue };

                            textBox.TextChanged += (sender, e) =>
                            {
                                textBox.Foreground = Brushes.Yellow; // Change text color to yellow
                                UnsavedChanges.Visibility = Visibility.Visible;

                                // Store unsaved changes
                                if (!unsavedChanges.ContainsKey(entry["ObjectIndex"]))
                                {
                                    unsavedChanges[entry["ObjectIndex"]] = new Dictionary<string, string>();
                                }
                                unsavedChanges[entry["ObjectIndex"]][column] = textBox.Text;
                            };

                            textBox.KeyDown += async (sender, e) =>
                            {
                                if (e.Key == Key.Enter)
                                {
                                    var tag = (dynamic)textBox.Tag;

                                    if (column == "KeyName" && string.IsNullOrWhiteSpace(textBox.Text))
                                    {
                                        await DeleteMetadataEntry(entry["ObjectIndex"], tag.OriginalKeyName, tag.OriginalValue);
                                    }
                                    else
                                    {
                                        foreach (var change in unsavedChanges[entry["ObjectIndex"]])
                                        {
                                            if (change.Key == "KeyName" && string.IsNullOrWhiteSpace(change.Value))
                                            {
                                                await DeleteMetadataEntry(entry["ObjectIndex"], tag.OriginalKeyName, tag.OriginalValue);
                                            }
                                            else if (change.Key == "KeyName" || change.Key == "Value")
                                            {
                                                await UpdateMetadataEntry(entry["ObjectIndex"], change.Key, change.Value, tag.OriginalKeyName, tag.OriginalValue);
                                            }
                                            else
                                            {
                                                await UpdateDatabaseEntry(entry["ObjectIndex"], change.Key, change.Value);
                                            }
                                        }
                                    }

                                    // Clear changes after saving
                                    unsavedChanges.Remove(entry["ObjectIndex"]);
                                    UnsavedChanges.Visibility = Visibility.Hidden;
                                    await PopulateGridWithEntries(currentStartIndex, 25); // Reload the same object index range
                                }
                            };
                        }
                        else
                        {
                            textBox.IsReadOnly = true;
                        }

                        Grid.SetRow(textBox, rowIndex);
                        Grid.SetColumn(textBox, colIndex++);
                        SQLItemsGrid.Children.Add(textBox);
                    }
                    rowIndex++;
                }

                // Update the UI
                await Task.Delay(1);  // Small delay to ensure the UI updates incrementally
            }
        }

        private async Task UpdateDatabaseEntry(string objectIndex, string columnName, string newValue)
        {
            try
            {
                string connectionString = $"Data Source={sqlFilePath};Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string updateQuery = string.Empty;
                    SQLiteCommand command = new SQLiteCommand(connection);

                    if (columnName == "ArchiveTimeStamp")
                    {
                        int timestampValue = ConvertHexToInt(newValue);
                        updateQuery = $@"
                    UPDATE Objects
                    SET {columnName} = @newValue
                    WHERE ObjectIndex = @objectIndex";
                        command.Parameters.AddWithValue("@newValue", timestampValue);
                    }
                    else if (columnName == "OdcSha1Digest")
                    {
                        byte[] sha1Value = ConvertHexToByteArray(newValue);
                        updateQuery = $@"
                    UPDATE Objects
                    SET {columnName} = @newValue
                    WHERE ObjectIndex = @objectIndex";
                        command.Parameters.AddWithValue("@newValue", sha1Value);
                    }
                    else if (columnName == "Version")
                    {
                        updateQuery = $@"
                    UPDATE Objects
                    SET {columnName} = @newValue
                    WHERE ObjectIndex = @objectIndex";
                        command.Parameters.AddWithValue("@newValue", newValue);
                    }

                    command.CommandText = updateQuery;
                    command.Parameters.AddWithValue("@objectIndex", objectIndex);
                    await command.ExecuteNonQueryAsync();

                    LogDebugInfo($"Database entry updated: ObjectIndex={objectIndex}, Column={columnName}, NewValue={newValue}");
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error updating database entry: {ex.Message}");
                MessageBox.Show($"Error updating database entry: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateMetadataEntry(string objectIndex, string columnName, string newValue, string originalKeyName, string originalValue)
        {
            try
            {
                string connectionString = $"Data Source={sqlFilePath};Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string updateQuery = $@"
                UPDATE Metadata
                SET {columnName} = @newValue
                WHERE ObjectIndex = @objectIndex AND KeyName = @originalKeyName AND Value = @originalValue";

                    SQLiteCommand command = new SQLiteCommand(updateQuery, connection);
                    command.Parameters.AddWithValue("@newValue", newValue);
                    command.Parameters.AddWithValue("@objectIndex", objectIndex);
                    command.Parameters.AddWithValue("@originalKeyName", originalKeyName);
                    command.Parameters.AddWithValue("@originalValue", originalValue);

                    await command.ExecuteNonQueryAsync();

                    LogDebugInfo($"Metadata entry updated: ObjectIndex={objectIndex}, Column={columnName}, NewValue={newValue}");
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error updating metadata entry: {ex.Message}");
                MessageBox.Show($"Error updating metadata entry: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteMetadataEntry(string objectIndex, string keyName, string value)
        {
            try
            {
                string connectionString = $"Data Source={sqlFilePath};Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string deleteQuery = $@"
                DELETE FROM Metadata
                WHERE ObjectIndex = @objectIndex AND KeyName = @keyName AND Value = @value";

                    SQLiteCommand command = new SQLiteCommand(deleteQuery, connection);
                    command.Parameters.AddWithValue("@objectIndex", objectIndex);
                    command.Parameters.AddWithValue("@keyName", keyName);
                    command.Parameters.AddWithValue("@value", value);

                    await command.ExecuteNonQueryAsync();

                    LogDebugInfo($"Metadata entry deleted: ObjectIndex={objectIndex}, KeyName={keyName}, Value={value}");
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error deleting metadata entry: {ex.Message}");
                MessageBox.Show($"Error deleting metadata entry: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ClearUnsavedChanges();
            string searchQuery = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(searchQuery))
            {
                MessageBox.Show("Please enter a search query.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Perform the search and load results
            PerformSearchAndLoadResults(searchQuery);
        }

        private async void PerformSearchAndLoadResults(string searchQuery)
        {
            try
            {
                // Find the ObjectIndex based on the ObjectID search query
                int startIndex = GetObjectIndexByObjectId(searchQuery);

                if (startIndex == -1)
                {
                    MessageBox.Show("ObjectID not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Load the item and the next 25 items
                await PopulateGridWithEntries(startIndex, 25);
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error performing search: {ex.Message}");
                MessageBox.Show($"Error performing search: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetObjectIndexByObjectId(string objectId)
        {
            int objectIndex = -1;

            try
            {
                string connectionString = $"Data Source={sqlFilePath};Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    LogDebugInfo("SQLite connection opened successfully.");

                    string query = @"
                SELECT ObjectIndex
                FROM Objects
                WHERE ObjectId = @objectId
                LIMIT 1";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@objectId", objectId);
                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            objectIndex = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error retrieving ObjectIndex by ObjectId: {ex.Message}");
            }

            return objectIndex;
        }



        private async Task<List<string>> GetObjectIdsInRange(int startIndex, int endIndex)
        {
            var objectIds = new List<string>();

            try
            {
                string connectionString = $"Data Source={sqlFilePath};Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();
                    LogDebugInfo("SQLite connection opened successfully.");

                    string query = @"
                SELECT ObjectId
                FROM Objects
                WHERE ObjectIndex BETWEEN @startIndex AND @endIndex";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@startIndex", startIndex);
                        command.Parameters.AddWithValue("@endIndex", endIndex);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                objectIds.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error retrieving ObjectIDs in range: {ex.Message}");
            }

            return objectIds;
        }

        private async void DeleteSQLIndexButton_Click(object sender, RoutedEventArgs e)
        {
            ClearUnsavedChanges();
            string input = txtGoToIndex.Text;
            if (string.IsNullOrWhiteSpace(input))
            {
                LogDebugInfo("Input is empty.");
                return;
            }

            int startIndex, endIndex;

            if (input.Contains('-'))
            {
                // Handle range input
                var parts = input.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[0], out startIndex) && int.TryParse(parts[1], out int endIndexParsed))
                {
                    if (startIndex > endIndexParsed)
                    {
                        LogDebugInfo("Invalid range entered: start index is greater than end index.");
                        MessageBox.Show("Invalid range: start index is greater than end index.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    endIndex = endIndexParsed;
                }
                else
                {
                    LogDebugInfo("Invalid range entered: unable to parse start or end index.");
                    MessageBox.Show("Invalid range entered. Please enter a valid range.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else if (int.TryParse(input, out startIndex))
            {
                // Handle single index input
                endIndex = startIndex;
            }
            else
            {
                LogDebugInfo("Invalid index entered.");
                MessageBox.Show("Invalid index entered. Please enter a valid index.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var objectIds = await GetObjectIdsInRange(startIndex, endIndex);
            if (objectIds.Count == 0)
            {
                MessageBox.Show("No entries found for the specified range.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string message = "Are you sure you want to delete the following entries?\n\n" + string.Join("\n", objectIds);
            MessageBoxResult result = MessageBox.Show(message, "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                await DeleteEntriesAndReorder(startIndex, endIndex);
            }
        }

        private async Task DeleteEntriesAndReorder(int startIndex, int endIndex)
        {
            try
            {
                string connectionString = $"Data Source={sqlFilePath};Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();
                    LogDebugInfo("SQLite connection opened successfully.");

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Step 1: Delete metadata entries in the specified range
                            string deleteMetadataQuery = @"
                    DELETE FROM Metadata
                    WHERE ObjectIndex BETWEEN @startIndex AND @endIndex";
                            using (SQLiteCommand deleteMetadataCommand = new SQLiteCommand(deleteMetadataQuery, connection))
                            {
                                deleteMetadataCommand.Parameters.AddWithValue("@startIndex", startIndex);
                                deleteMetadataCommand.Parameters.AddWithValue("@endIndex", endIndex);
                                await deleteMetadataCommand.ExecuteNonQueryAsync();
                            }

                            // Step 2: Delete main entries in the specified range
                            string deleteQuery = @"
                    DELETE FROM Objects
                    WHERE ObjectIndex BETWEEN @startIndex AND @endIndex";
                            using (SQLiteCommand deleteCommand = new SQLiteCommand(deleteQuery, connection))
                            {
                                deleteCommand.Parameters.AddWithValue("@startIndex", startIndex);
                                deleteCommand.Parameters.AddWithValue("@endIndex", endIndex);
                                await deleteCommand.ExecuteNonQueryAsync();
                            }

                            // Step 3: Reorder the remaining indexes
                            string updateQuery = @"
                    UPDATE Objects
                    SET ObjectIndex = ObjectIndex - @offset
                    WHERE ObjectIndex > @endIndex";
                            using (SQLiteCommand updateCommand = new SQLiteCommand(updateQuery, connection))
                            {
                                int offset = endIndex - startIndex + 1;
                                updateCommand.Parameters.AddWithValue("@offset", offset);
                                updateCommand.Parameters.AddWithValue("@endIndex", endIndex);
                                await updateCommand.ExecuteNonQueryAsync();
                            }

                            transaction.Commit();
                            LogDebugInfo("Entries deleted and indexes reordered successfully.");
                            MessageBox.Show("Entries deleted and indexes reordered successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Update NexTIndex and the textbox
                            int maxIndex = await GetMaxObjectIndex(connection);
                            NexTIndex = maxIndex + 1;
                            txtObjectIndex.Text = NexTIndex.ToString();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            LogDebugInfo($"Error deleting entries and reordering indexes: {ex.Message}");
                            MessageBox.Show($"Error deleting entries and reordering indexes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }

                // Refresh the grid to reflect changes
                await PopulateGridWithEntries(0, 25);
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error performing delete and reorder: {ex.Message}");
                MessageBox.Show($"Error performing delete and reorder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private async Task<int> GetMaxObjectIndex(SQLiteConnection connection)
        {
            int maxIndex = -1;

            try
            {
                string query = @"
            SELECT MAX(ObjectIndex) as MaxIndex
            FROM Objects";

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    object result = await command.ExecuteScalarAsync();
                    if (result != DBNull.Value && result != null)
                    {
                        maxIndex = Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error retrieving max ObjectIndex: {ex.Message}");
            }

            return maxIndex;
        }

        // Method to handle key name selection changes and update value ComboBox options
        private void txtKeyName1_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateValueComboBox(txtKeyName1, txtValue1, txtCustomKeyName1, txtCustomValue1);
        private void txtKeyName2_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateValueComboBox(txtKeyName2, txtValue2, txtCustomKeyName2, txtCustomValue2);
        private void txtKeyName3_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateValueComboBox(txtKeyName3, txtValue3, txtCustomKeyName3, txtCustomValue3);
        private void txtKeyName4_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateValueComboBox(txtKeyName4, txtValue4, txtCustomKeyName4, txtCustomValue4);
        private void txtKeyName5_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateValueComboBox(txtKeyName5, txtValue5, txtCustomKeyName5, txtCustomValue5);
        private void txtKeyName6_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateValueComboBox(txtKeyName6, txtValue6, txtCustomKeyName6, txtCustomValue6);



        // Method to handle key name selection changes and update value ComboBox options
        // Method to handle key name selection changes and update value ComboBox options
        // Method to handle key name selection changes and update value ComboBox options
        private void UpdateValueComboBox(ComboBox keyComboBox, ComboBox valueComboBox, TextBox customKeyTextBox, TextBox customValueTextBox)
        {
            if (keyComboBox.SelectedItem == null) return;

            string selectedKey = (keyComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (selectedKey == "CUSTOM")
            {
                customKeyTextBox.Visibility = Visibility.Visible;
                keyComboBox.Visibility = Visibility.Collapsed;
                customValueTextBox.Visibility = Visibility.Visible;
                valueComboBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                customKeyTextBox.Visibility = Visibility.Collapsed;
                keyComboBox.Visibility = Visibility.Visible;

                valueComboBox.Visibility = Visibility.Visible;
                customValueTextBox.Visibility = Visibility.Collapsed;

                valueComboBox.Items.Clear();

                if (keyNameValues.ContainsKey(selectedKey))
                {
                    foreach (var value in keyNameValues[selectedKey])
                    {
                        valueComboBox.Items.Add(new ComboBoxItem { Content = value });
                    }

                    if (valueComboBox.Items.Count > 0)
                    {
                        valueComboBox.SelectedIndex = 0; // Automatically select the first value if there is one
                    }
                }
                valueComboBox.Items.Add(new ComboBoxItem { Content = "CUSTOM" });
            }

            // Automatic selection for other ComboBoxes
            if (keyComboBox == txtKeyName1)
            {
                ResetComboBox(txtKeyName2, txtCustomKeyName2);
                ResetComboBox(txtValue2, txtCustomValue2);
                ResetComboBox(txtKeyName3, txtCustomKeyName3);
                ResetComboBox(txtValue3, txtCustomValue3);

                if (selectedKey == "CLOTHING")
                {
                    SelectComboBoxItem(txtKeyName2, "RIGS");
                    SelectComboBoxItem(txtKeyName3, "ENTITLEMENT_ID");

                    // Uncheck and disable the HEAT checkbox, clear HEAT textboxes
                    chkBox1.IsChecked = false;
                    chkBox1.IsEnabled = false;
                    SetHeatTextBoxes(string.Empty, true);
                    ResetComboBox(txtKeyName4, txtCustomKeyName4);
                    ResetComboBox(txtValue4, txtCustomValue4);
                    ResetComboBox(txtKeyName5, txtCustomKeyName5);
                    ResetComboBox(txtValue5, txtCustomValue5);
                    ResetComboBox(txtKeyName6, txtCustomKeyName6);
                    ResetComboBox(txtValue6, txtCustomValue6);
                }
                else if (selectedKey == "FURNITURE")
                {
                    SelectComboBoxItem(txtKeyName2, "ENTITLEMENT_ID");
                    ResetComboBox(txtKeyName3, txtCustomKeyName3);
                    ResetComboBox(txtValue3, txtCustomValue3);
                    ResetComboBox(txtKeyName4, txtCustomKeyName4);
                    ResetComboBox(txtValue4, txtCustomValue4);
                    ResetComboBox(txtKeyName5, txtCustomKeyName5);
                    ResetComboBox(txtValue5, txtCustomValue5);
                    ResetComboBox(txtKeyName6, txtCustomKeyName6);
                    ResetComboBox(txtValue6, txtCustomValue6);

                    // Ensure the checkbox is enabled for FURNITURE
                    chkBox1.IsEnabled = true;
                }
                else if (selectedKey == "PORTABLE")
                {
                    SelectComboBoxItem(txtKeyName2, "ENTITLEMENT_ID");
                    ResetComboBox(txtKeyName3, txtCustomKeyName3);
                    ResetComboBox(txtValue3, txtCustomValue3);
                    ResetComboBox(txtKeyName4, txtCustomKeyName4);
                    ResetComboBox(txtValue4, txtCustomValue4);
                    ResetComboBox(txtKeyName5, txtCustomKeyName5);
                    ResetComboBox(txtValue5, txtCustomValue5);
                    ResetComboBox(txtKeyName6, txtCustomKeyName6);
                    ResetComboBox(txtValue6, txtCustomValue6);

                    // Ensure the checkbox is enabled for FURNITURE
                    chkBox1.IsEnabled = true;
                }
                else if (selectedKey == "MINIGAME")
                {
                    ResetComboBox(txtKeyName2, txtCustomKeyName2);
                    ResetComboBox(txtValue2, txtCustomValue2);
                    ResetComboBox(txtKeyName3, txtCustomKeyName3);
                    ResetComboBox(txtValue3, txtCustomValue3);
                    ResetComboBox(txtKeyName4, txtCustomKeyName4);
                    ResetComboBox(txtValue4, txtCustomValue4);
                    ResetComboBox(txtKeyName5, txtCustomKeyName5);
                    ResetComboBox(txtValue5, txtCustomValue5);
                    ResetComboBox(txtKeyName6, txtCustomKeyName6);
                    ResetComboBox(txtValue6, txtCustomValue6);

                    // Ensure the checkbox is enabled for FURNITURE
                    chkBox1.IsEnabled = true;
                }
                else if (selectedKey == "SCENE_ENTITLEMENT")
                {
                    SelectComboBoxItem(txtKeyName2, "SCENE_TYPE");
                    SelectComboBoxItem(txtKeyName3, "ENTITLEMENT_ID");
                    ResetComboBox(txtKeyName4, txtCustomKeyName4);
                    ResetComboBox(txtValue4, txtCustomValue4);
                    ResetComboBox(txtKeyName5, txtCustomKeyName5);
                    ResetComboBox(txtValue5, txtCustomValue5);
                    ResetComboBox(txtKeyName6, txtCustomKeyName6);
                    ResetComboBox(txtValue6, txtCustomValue6);

                    // Swap Value1 to a TextBox
                    customValueTextBox.Visibility = Visibility.Visible;
                    valueComboBox.Visibility = Visibility.Collapsed;
                }
                else if (selectedKey == "COMMUNICATION_ID")
                {
                    SelectComboBoxItem(txtKeyName2, "TITLE_ID");

                    // Swap Value1 and Value2 to TextBoxes
                    txtCustomValue1.Visibility = Visibility.Visible;
                    txtValue1.Visibility = Visibility.Collapsed;
                    txtCustomValue2.Visibility = Visibility.Visible;
                    txtValue2.Visibility = Visibility.Collapsed;
                    ResetComboBox(txtKeyName4, txtCustomKeyName4);
                    ResetComboBox(txtValue4, txtCustomValue4);
                    ResetComboBox(txtKeyName5, txtCustomKeyName5);
                    ResetComboBox(txtValue5, txtCustomValue5);
                    ResetComboBox(txtKeyName6, txtCustomKeyName6);
                    ResetComboBox(txtValue6, txtCustomValue6);
                }
                else
                {
                    chkBox1.IsEnabled = true;
                }
            }
        }


        private void SelectComboBoxItem(ComboBox comboBox, string itemText)
        {
            foreach (var item in comboBox.Items)
            {
                if ((item as ComboBoxItem)?.Content.ToString() == itemText)
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }
            // If not found, reset to default
            comboBox.SelectedIndex = -1;
        }

        private void UpdateValueComboBoxForValue(ComboBox valueComboBox, TextBox customValueTextBox)
        {
            if (valueComboBox.SelectedItem == null) return;

            string selectedValue = (valueComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (selectedValue == "CUSTOM")
            {
                customValueTextBox.Visibility = Visibility.Visible;
                valueComboBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                customValueTextBox.Visibility = Visibility.Collapsed;
                valueComboBox.Visibility = Visibility.Visible;
            }
        }

        // Event handler for selection changed on value ComboBoxes
        private void Value_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox valueComboBox = sender as ComboBox;
            if (valueComboBox != null)
            {
                if (valueComboBox == txtValue1)
                {
                    UpdateValueComboBoxForValue(txtValue1, txtCustomValue1);
                }
                else if (valueComboBox == txtValue2)
                {
                    UpdateValueComboBoxForValue(txtValue2, txtCustomValue2);
                }
                else if (valueComboBox == txtValue3)
                {
                    UpdateValueComboBoxForValue(txtValue3, txtCustomValue3);
                }
            }
        }

        // Attach Value_SelectionChanged to each value ComboBox
        private void InitializeComboBoxEventHandlers()
        {
            txtValue1.SelectionChanged += Value_SelectionChanged;
            txtValue2.SelectionChanged += Value_SelectionChanged;
            txtValue3.SelectionChanged += Value_SelectionChanged;
        }




        private void chkBox1_Checked(object sender, RoutedEventArgs e)
        {
            SetHeatTextBoxes("1", false); // Set value to "1" and make them writable
        }

        private void chkBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            SetHeatTextBoxes(string.Empty, true); // Clear the text and make them read-only
        }

        private void SetHeatTextBoxes(string text, bool isReadOnly)
        {
            txtBox1.Text = text;
            txtBox1.IsReadOnly = isReadOnly;

            txtBox2.Text = text;
            txtBox2.IsReadOnly = isReadOnly;

            txtBox4.Text = text;
            txtBox4.IsReadOnly = isReadOnly;

            txtBox5.Text = text;
            txtBox5.IsReadOnly = isReadOnly;

            txtBox6.Text = text;
            txtBox6.IsReadOnly = isReadOnly;
        }


        private void InsertMetadataEntry(SQLiteConnection connection, int objectIndex, ComboBox keyComboBox, ComboBox valueComboBox, TextBox customKeyTextBox, TextBox customValueTextBox)
        {
            string keyName = keyComboBox.Visibility == Visibility.Visible ?
                             ((ComboBoxItem)keyComboBox.SelectedItem)?.Content.ToString() : customKeyTextBox.Text;
            string value = valueComboBox.Visibility == Visibility.Visible ?
                           ((ComboBoxItem)valueComboBox.SelectedItem)?.Content.ToString() : customValueTextBox.Text;

            // Ensure keyName is not empty
            if (!string.IsNullOrEmpty(keyName))
            {
                // Trim " (Male)" or " (Female)" from the value if present
                if (!string.IsNullOrEmpty(value))
                {
                    if (value.EndsWith(" (Male)"))
                    {
                        value = value.Substring(0, value.Length - " (Male)".Length);
                    }
                    else if (value.EndsWith(" (Female)"))
                    {
                        value = value.Substring(0, value.Length - " (Female)".Length);
                    }
                }

                string insertMetadataQuery = @"
INSERT INTO Metadata (ObjectIndex, KeyName, Value)
VALUES (@objectIndex, @keyName, @value)";
                using (var insertMetadataCommand = new SQLiteCommand(insertMetadataQuery, connection))
                {
                    insertMetadataCommand.Parameters.AddWithValue("@objectIndex", objectIndex);
                    insertMetadataCommand.Parameters.AddWithValue("@keyName", keyName);
                    insertMetadataCommand.Parameters.AddWithValue("@value", value ?? string.Empty); // Use empty string if value is null
                    insertMetadataCommand.ExecuteNonQuery();
                }
            }
        }



        private async void AddNewSQLItem_Button_Click(object sender, RoutedEventArgs e)
        {
            ClearUnsavedChanges();
            try
            {
                int objectIndex = int.Parse(txtObjectIndex.Text);
                string objectId = txtObjectId.Text;
                int version = int.Parse(txtVersion.Text);
                int location = int.Parse(txtLocation.Text);
                string entryType = txtEntryType.Text == "NULL" ? null : txtEntryType.Text;
                string archiveTimeStampHex = txtArchiveTimeStampHex.Text;
                string odcSha1Digest = txtOdcSha1Digest.Text;
                int? entitlementIndex = ParseNullableInt(txtEntitlementIndex.Text);
                int? rewardIndex = ParseNullableInt(txtRewardIndex.Text);
                int userLocation = int.Parse(txtUserLocation.Text);
                string lastUsed = txtLastUsed.Text;

                // Validate SHA1 (allow it to be empty or a valid SHA1)
                if (!string.IsNullOrEmpty(odcSha1Digest) && !Regex.IsMatch(odcSha1Digest, @"^[a-fA-F0-9]{40}$"))
                {
                    MessageBox.Show("Invalid SHA1 format. It must be 40 hexadecimal characters.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Validate UUID
                if (!Regex.IsMatch(objectId, @"^[a-fA-F0-9]{8}-[a-fA-F0-9]{8}-[a-fA-F0-9]{8}-[a-fA-F0-9]{8}$"))
                {
                    MessageBox.Show("Invalid UUID format. It must be in the format 8-8-8-8 hexadecimal characters.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Validate timestamp
                if (!Regex.IsMatch(archiveTimeStampHex, @"^[a-fA-F0-9]{1,8}$"))
                {
                    MessageBox.Show("Invalid timestamp format. It must be up to 8 hexadecimal characters.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Validate last used date if not NULL
                int? lastUsedDate = null;
                if (!string.IsNullOrEmpty(lastUsed) && lastUsed != "NULL")
                {
                    if (!Regex.IsMatch(lastUsed, @"^[a-fA-F0-9]{1,8}$"))
                    {
                        MessageBox.Show("Invalid last used date format. It must be up to 8 hexadecimal characters.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    lastUsedDate = ConvertHexToInt(lastUsed);
                }

                int archiveTimeStamp = ConvertHexToInt(archiveTimeStampHex);
                byte[] odcSha1DigestBytes = !string.IsNullOrEmpty(odcSha1Digest) ? ConvertHexToByteArray(odcSha1Digest) : null;

                bool objectReplaced = false;

                using (var connection = new SQLiteConnection($"Data Source={sqlFilePath};Version=3;"))
                {
                    await connection.OpenAsync();

                    bool objectIndexExists = await CheckObjectIndexExists(connection, objectIndex);
                    bool objectIdExists = false;
                    int existingObjectIndex = -1;

                    if (!objectIndexExists)
                    {
                        (objectIdExists, existingObjectIndex) = await CheckObjectIdExists(connection, objectId);
                    }

                    if (objectIndexExists || objectIdExists)
                    {
                        string message = objectIndexExists ?
                            $"ObjectIndex {objectIndex} already exists. Do you want to replace the existing item?" :
                            $"ObjectId {objectId} already exists under ObjectIndex {existingObjectIndex}. Do you want to replace the existing item?";

                        var result = CustomMessageBoxInserttoSQL.Show(message, "Replace", "Cancel");

                        if (result == CustomMessageBoxInserttoSQL.CustomMessageBoxResult.Cancel)
                        {
                            return; // Abort the insertion process
                        }

                        // Delete the existing item
                        int indexToDelete = objectIndexExists ? objectIndex : existingObjectIndex;
                        await DeleteItemByObjectIndex(connection, indexToDelete);
                        objectReplaced = true;
                        objectIndex = indexToDelete; // Use the existing index
                    }

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Insert main object entry
                            string insertQuery = @"
                    INSERT INTO Objects (ObjectIndex, ObjectId, Version, Location, InventoryEntryType, ArchiveTimeStamp, OdcSha1Digest, EntitlementIndex, RewardIndex, UserLocation, UserDateLastUsed)
                    VALUES (@objectIndex, @objectId, @version, @location, @entryType, @archiveTimeStamp, @odcSha1Digest, @entitlementIndex, @rewardIndex, @userLocation, @lastUsedDate)";
                            using (var insertCommand = new SQLiteCommand(insertQuery, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@objectIndex", objectIndex);
                                insertCommand.Parameters.AddWithValue("@objectId", objectId);
                                insertCommand.Parameters.AddWithValue("@version", version);
                                insertCommand.Parameters.AddWithValue("@location", location);
                                insertCommand.Parameters.AddWithValue("@entryType", (object)entryType ?? DBNull.Value);
                                insertCommand.Parameters.AddWithValue("@archiveTimeStamp", archiveTimeStamp);
                                insertCommand.Parameters.AddWithValue("@odcSha1Digest", (object)odcSha1DigestBytes ?? DBNull.Value);
                                insertCommand.Parameters.AddWithValue("@entitlementIndex", (object)entitlementIndex ?? DBNull.Value);
                                insertCommand.Parameters.AddWithValue("@rewardIndex", (object)rewardIndex ?? DBNull.Value);
                                insertCommand.Parameters.AddWithValue("@userLocation", userLocation);
                                insertCommand.Parameters.AddWithValue("@lastUsedDate", (object)lastUsedDate ?? DBNull.Value);
                                await insertCommand.ExecuteNonQueryAsync();
                            }

                            // Insert metadata entries
                            InsertMetadataEntry(connection, objectIndex, txtKeyName1, txtValue1, txtCustomKeyName1, txtCustomValue1);
                            InsertMetadataEntry(connection, objectIndex, txtKeyName2, txtValue2, txtCustomKeyName2, txtCustomValue2);
                            InsertMetadataEntry(connection, objectIndex, txtKeyName3, txtValue3, txtCustomKeyName3, txtCustomValue3);
                            InsertMetadataEntry(connection, objectIndex, txtKeyName4, txtValue4, txtCustomKeyName4, txtCustomValue4);
                            InsertMetadataEntry(connection, objectIndex, txtKeyName5, txtValue5, txtCustomKeyName5, txtCustomValue5);
                            InsertMetadataEntry(connection, objectIndex, txtKeyName6, txtValue6, txtCustomKeyName6, txtCustomValue6);

                            // Insert HEAT metadata entries if checkbox is checked
                            if (chkBox1.IsChecked == true)
                            {
                                InsertHeatMetadataEntries(connection, objectIndex);
                            }

                            transaction.Commit();
                            if (objectReplaced)
                            {
                                MessageBox.Show("Object replaced successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Object inserted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            LogDebugInfo($"Error adding new item: {ex.Message}");
                            MessageBox.Show($"Error adding new item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                    // Update NexTIndex and the textbox
                    int maxIndex = await GetMaxObjectIndex(connection);
                    NexTIndex = maxIndex + 1;
                    txtObjectIndex.Text = NexTIndex.ToString();

                    // Call the range load function to load the index number in txtObjectIndex textbox and load 25 from that point on
                    await PopulateGridWithEntries(objectIndex, 25);
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error adding new item: {ex.Message}");
                MessageBox.Show($"Error adding new item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async Task<bool> CheckObjectIndexExists(SQLiteConnection connection, int objectIndex)
        {
            string query = "SELECT COUNT(*) FROM Objects WHERE ObjectIndex = @objectIndex";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@objectIndex", objectIndex);
                int count = Convert.ToInt32(await command.ExecuteScalarAsync());
                return count > 0;
            }
        }

        private async Task<(bool, int)> CheckObjectIdExists(SQLiteConnection connection, string objectId)
        {
            string query = "SELECT ObjectIndex FROM Objects WHERE ObjectId = @objectId";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@objectId", objectId);
                object result = await command.ExecuteScalarAsync();
                if (result != null)
                {
                    int existingObjectIndex = Convert.ToInt32(result);
                    return (true, existingObjectIndex);
                }
                return (false, -1);
            }
        }

        private async Task DeleteItemByObjectIndex(SQLiteConnection connection, int objectIndex)
        {
            // Delete metadata entries
            string deleteMetadataQuery = "DELETE FROM Metadata WHERE ObjectIndex = @objectIndex";
            using (var deleteMetadataCommand = new SQLiteCommand(deleteMetadataQuery, connection))
            {
                deleteMetadataCommand.Parameters.AddWithValue("@objectIndex", objectIndex);
                await deleteMetadataCommand.ExecuteNonQueryAsync();
            }

            // Delete main object entry
            string deleteQuery = "DELETE FROM Objects WHERE ObjectIndex = @objectIndex";
            using (var deleteCommand = new SQLiteCommand(deleteQuery, connection))
            {
                deleteCommand.Parameters.AddWithValue("@objectIndex", objectIndex);
                await deleteCommand.ExecuteNonQueryAsync();
            }
        }




        // Method to insert HEAT metadata entries
        private void InsertHeatMetadataEntries(SQLiteConnection connection, int objectIndex)
        {
            var heatTextBoxes = new Dictionary<string, TextBox>
    {
        { "HOST_HEAT", txtBox4 },
        { "MAIN_HEAT", txtBox1 },
        { "NET_HEAT", txtBox5 },
        { "PPU_HEAT", txtBox2 },
        { "VRAM_HEAT", txtBox6 }
    };

            foreach (var heatEntry in heatTextBoxes)
            {
                if (!string.IsNullOrWhiteSpace(heatEntry.Value.Text))
                {
                    string insertMetadataQuery = @"
            INSERT INTO Metadata (ObjectIndex, KeyName, Value)
            VALUES (@objectIndex, @keyName, @value)";
                    using (var insertMetadataCommand = new SQLiteCommand(insertMetadataQuery, connection))
                    {
                        insertMetadataCommand.Parameters.AddWithValue("@objectIndex", objectIndex);
                        insertMetadataCommand.Parameters.AddWithValue("@keyName", heatEntry.Key);
                        insertMetadataCommand.Parameters.AddWithValue("@value", heatEntry.Value.Text);
                        insertMetadataCommand.ExecuteNonQuery();
                    }
                }
            }
        }

        private void InsertHeatMetadataEntry(SQLiteConnection connection, int objectIndex, string keyName, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                string insertMetadataQuery = @"
        INSERT INTO Metadata (ObjectIndex, KeyName, Value)
        VALUES (@objectIndex, @keyName, @value)";
                using (var insertMetadataCommand = new SQLiteCommand(insertMetadataQuery, connection))
                {
                    insertMetadataCommand.Parameters.AddWithValue("@objectIndex", objectIndex);
                    insertMetadataCommand.Parameters.AddWithValue("@keyName", keyName);
                    insertMetadataCommand.Parameters.AddWithValue("@value", value);
                    insertMetadataCommand.ExecuteNonQuery();
                }
            }
        }




        // Dictionary to map key names to values
        private Dictionary<string, List<string>> keyNameValues = new Dictionary<string, List<string>>
{
    { "CLOTHING", new List<string> { "HAIR", "HAT", "HANDS", "TORS", "LEGS", "FEET", "OUTFITS", "GLASSES", "HEADPHONES", "JEWELBOTHEARS", "JEWELLEFTEAR", "JEWELRIGHTEAR", "FACIALHAIR", "RACE", "" } },
    { "FURNITURE", new List<string> { "CHAIR", "APPLIANCE", "TABLE", "FOOTSTOOL", "LIGHT", "ORNAMENT", "FRAME", "SOFA", "STORAGE", "CUBE", "FLOORING", "" } },
    { "PORTABLE", new List<string> { "" } },
    { "ENTITLEMENT_ID", new List<string> { "LUA_REWARD", "AUTOMATIC_REWARD", "FREE", "" } },
    { "MINI_GAME", new List<string> { "", "DARTS", "APPLIANCE", "HUBCONTENT", "XMAS_2010_SCEA", "CASUAL", "GAME", "SPACE" } },
    { "ARCADE_GAME", new List<string> { "" } },
    { "WORLD_MAP", new List<string> { "SCEA SCEE SCEJ SCEASIA", "SCEA SCEE SCEJ", "SCEA SCEE", "SCEJ SCEASIA", "SCEA", "SCEE", "SCEJ", "SCEASIA" } },
    { "SCENE_ENTITLEMENT", new List<string> { "CUSTOM" } },
    { "EMBEDDED_OBJECT", new List<string> { "" } },
    { "COMMUNICATION_ID", new List<string> { "CUSTOM" } },
    { "ACTIVE", new List<string> { "DEFAULT", "" } },
    { "TARGETABLE", new List<string> { "" } },
    { "RIGS", new List<string> { "00000000-00000000-00000010-00000000 (Male)", "00000000-00000000-00000010-00000001 (Female)" } },
    { "TITLE_ID", new List<string> { "CUSTOM" } },
    { "TAGS", new List<string> { "" } },
    { "LPID", new List<string> { "DEFAULT_HAIR", "DEFAULT_FACIALHAIR" } },
    { "SCENE_TYPE", new List<string> { "APARTMENT", "CLUBHOUSE" } }
};

        private int ConvertHexToInt(string hex)
        {
            try
            {
                return int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Format error converting hex to int: {ex.Message}");
                throw;
            }
        }

        private byte[] ConvertHexToByteArray(string hex)
        {
            try
            {
                int numberChars = hex.Length;
                byte[] bytes = new byte[numberChars / 2];
                for (int i = 0; i < numberChars; i += 2)
                {
                    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                }
                return bytes;
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Format error converting hex to byte array: {ex.Message}");
                throw;
            }
        }

        private int? ParseNullableInt(string value)
        {
            if (string.IsNullOrEmpty(value) || value == "NULL")
            {
                return null;
            }

            if (int.TryParse(value, out int result))
            {
                return result;
            }

            LogDebugInfo($"Invalid nullable int format: {value}");
            throw new FormatException($"Invalid nullable int format: {value}");
        }




        private int? ParseHexOrNullableInt(string text)
        {
            LogDebugInfo($"Parsing hex or nullable int: {text}");
            if (string.IsNullOrEmpty(text) || text.Trim().Equals("NULL", StringComparison.OrdinalIgnoreCase))
                return null;

            try
            {
                return ConvertHexToInt(text);
            }
            catch (FormatException)
            {
                throw new FormatException($"Invalid hex or nullable int format: {text}");
            }
        }

        private void InsertNewItem(int objectIndex, string objectId, int version, int location, char entryType, int archiveTimeStamp, byte[] odcSha1Digest, int? entitlementIndex, int? rewardIndex, int userLocation, int? userDateLastUsed)
        {
            try
            {
                string connectionString = $"Data Source={sqlFilePath};Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    LogDebugInfo("SQLite connection opened successfully.");

                    string insertQuery = @"
                INSERT INTO Objects (ObjectIndex, ObjectId, Version, Location, InventoryEntryType, ArchiveTimeStamp, OdcSha1Digest, EntitlementIndex, RewardIndex, UserLocation, UserDateLastUsed)
                VALUES (@objectIndex, @objectId, @version, @location, @entryType, @archiveTimeStamp, @odcSha1Digest, @entitlementIndex, @rewardIndex, @userLocation, @userDateLastUsed)";

                    using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@objectIndex", objectIndex);
                        command.Parameters.AddWithValue("@objectId", objectId);
                        command.Parameters.AddWithValue("@version", version);
                        command.Parameters.AddWithValue("@location", location);
                        command.Parameters.AddWithValue("@entryType", entryType == '\0' ? (object)DBNull.Value : entryType);
                        command.Parameters.AddWithValue("@archiveTimeStamp", archiveTimeStamp);
                        command.Parameters.AddWithValue("@odcSha1Digest", odcSha1Digest);
                        command.Parameters.AddWithValue("@entitlementIndex", (object)entitlementIndex ?? DBNull.Value);
                        command.Parameters.AddWithValue("@rewardIndex", (object)rewardIndex ?? DBNull.Value);
                        command.Parameters.AddWithValue("@userLocation", userLocation);
                        command.Parameters.AddWithValue("@userDateLastUsed", (object)userDateLastUsed ?? DBNull.Value);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error inserting new item into SQL file: {ex.Message}");
                throw;
            }
        }


        // Method to reset all ComboBoxes and TextBoxes to their default states
        private void ResetForm()
        {
            ClearUnsavedChanges();
            // Reset TextBoxes to default values
            txtObjectId.Text = "";
            txtVersion.Text = "31";
            txtLocation.Text = "1";
            txtEntryType.Text = "NULL";
            txtArchiveTimeStampHex.Text = "FFFFFFFF";
            txtOdcSha1Digest.Text = "";
            txtEntitlementIndex.Text = "NULL";
            txtRewardIndex.Text = "NULL";
            txtUserLocation.Text = "0";
            txtLastUsed.Text = "NULL";

            // Reset ComboBoxes to default values and hide custom TextBoxes
            ResetComboBox(txtKeyName1, txtCustomKeyName1);
            ResetComboBox(txtValue1, txtCustomValue1);
            ResetComboBox(txtKeyName2, txtCustomKeyName2);
            ResetComboBox(txtValue2, txtCustomValue2);
            ResetComboBox(txtKeyName3, txtCustomKeyName3);
            ResetComboBox(txtValue3, txtCustomValue3);
            ResetComboBox(txtKeyName4, txtCustomKeyName4);
            ResetComboBox(txtValue4, txtCustomValue4);
            ResetComboBox(txtKeyName5, txtCustomKeyName5);
            ResetComboBox(txtValue5, txtCustomValue5);
            ResetComboBox(txtKeyName6, txtCustomKeyName6);
            ResetComboBox(txtValue6, txtCustomValue6);

            // Reset CheckBoxes
            chkBox1.IsChecked = false;

            // Reset additional TextBoxes
            txtBox1.Text = "";
            txtBox2.Text = "";
            txtBox4.Text = "";
            txtBox5.Text = "";
            txtBox6.Text = "";

            // Make HEAT textboxes readonly
            txtBox1.IsReadOnly = true;
            txtBox2.IsReadOnly = true;
            txtBox4.IsReadOnly = true;
            txtBox5.IsReadOnly = true;
            txtBox6.IsReadOnly = true;
        }

        // Helper method to reset a ComboBox and its corresponding custom TextBox
        private void ResetComboBox(ComboBox comboBox, TextBox customTextBox)
        {
            comboBox.SelectedIndex = -1; // Reset the ComboBox selection
            comboBox.Visibility = Visibility.Visible; // Ensure ComboBox is visible
            customTextBox.Text = ""; // Clear the custom TextBox
            customTextBox.Visibility = Visibility.Collapsed; // Hide the custom TextBox
        }

        // Event handler for the Clear button
        private void ClearSQLButton_Click(object sender, RoutedEventArgs e)
        {
            ResetForm();
        }

        public class XmlObject
        {
            public string ObjectIndex { get; set; }
            public string ObjectId { get; set; }
            public string Version { get; set; }
            public string Location { get; set; }
            public string InventoryEntryType { get; set; }
            public string ArchiveTimeStamp { get; set; }
            public string OdcSha1Digest { get; set; }
            public string EntitlementIndex { get; set; }
            public string RewardIndex { get; set; }
            public string UserLocation { get; set; }
            public string UserDateLastUsed { get; set; }
            public List<XmlMetadata> Metadata { get; set; }
        }

        public class XmlMetadata
        {
            public string ObjectIndex { get; set; }
            public string KeyName { get; set; }
            public string Value { get; set; }
        }

        private string csvFilePath;


        private void txtValue_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private async void ExportToXMLButton_Click(object sender, RoutedEventArgs e)
        {
            ClearUnsavedChanges();
            try
            {
                List<XmlObject> xmlObjects = new List<XmlObject>();

                // Retrieve data from the SQL database
                string connectionString = $"Data Source={sqlFilePath};Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Query to get all objects
                    string objectQuery = @"
            SELECT 
                ObjectIndex, 
                ObjectId, 
                Version, 
                Location, 
                InventoryEntryType, 
                ArchiveTimeStamp, 
                OdcSha1Digest, 
                EntitlementIndex, 
                RewardIndex, 
                UserLocation, 
                UserDateLastUsed 
            FROM Objects";

                    using (SQLiteCommand objectCommand = new SQLiteCommand(objectQuery, connection))
                    {
                        using (var reader = await objectCommand.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var sqliteReader = (SQLiteDataReader)reader;

                                XmlObject xmlObject = new XmlObject
                                {
                                    ObjectIndex = sqliteReader["ObjectIndex"].ToString(),
                                    ObjectId = sqliteReader["ObjectId"].ToString(),
                                    Version = sqliteReader["Version"].ToString(),
                                    Location = sqliteReader["Location"].ToString(),
                                    InventoryEntryType = sqliteReader["InventoryEntryType"]?.ToString(),
                                    ArchiveTimeStamp = ConvertTimestampToHex(sqliteReader["ArchiveTimeStamp"]?.ToString()),
                                    OdcSha1Digest = ConvertBlobToSha1(sqliteReader["OdcSha1Digest"] as byte[]),
                                    EntitlementIndex = sqliteReader["EntitlementIndex"]?.ToString(),
                                    RewardIndex = sqliteReader["RewardIndex"]?.ToString(),
                                    UserLocation = sqliteReader["UserLocation"].ToString(),
                                    UserDateLastUsed = sqliteReader["UserDateLastUsed"]?.ToString(),
                                    Metadata = new List<XmlMetadata>()
                                };

                                // Query to get metadata for the current object
                                string metadataQuery = @"
                        SELECT ObjectIndex, KeyName, Value 
                        FROM Metadata 
                        WHERE ObjectIndex = @objectIndex";

                                using (SQLiteCommand metadataCommand = new SQLiteCommand(metadataQuery, connection))
                                {
                                    metadataCommand.Parameters.AddWithValue("@objectIndex", xmlObject.ObjectIndex);

                                    using (var metadataReader = await metadataCommand.ExecuteReaderAsync())
                                    {
                                        while (await metadataReader.ReadAsync())
                                        {
                                            var sqliteMetadataReader = (SQLiteDataReader)metadataReader;

                                            XmlMetadata metadata = new XmlMetadata
                                            {
                                                ObjectIndex = sqliteMetadataReader["ObjectIndex"].ToString(),
                                                KeyName = sqliteMetadataReader["KeyName"].ToString(),
                                                Value = sqliteMetadataReader["Value"].ToString()
                                            };

                                            xmlObject.Metadata.Add(metadata);
                                        }
                                    }
                                }

                                xmlObjects.Add(xmlObject);
                            }
                        }
                    }
                }

                // Ask user where to save the XML file
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                    DefaultExt = ".xml",
                    Title = "Save XML File"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string selectedFilePath = saveFileDialog.FileName;
                    WriteObjectsToXML(selectedFilePath, xmlObjects);

                    // Generate the CSV file path based on the XML file path
                    csvFilePath = Path.ChangeExtension(selectedFilePath, ".csv");
                    ExportToCSV(csvFilePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to XML and CSV: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void ExportToCSV(string csvFilePath)
        {
            try
            {
                using (var writer = new StreamWriter(csvFilePath))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    string connectionString = $"Data Source={sqlFilePath};Version=3;";
                    using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        string query = @"
                    SELECT 
                        o.ObjectIndex, 
                        o.ObjectId, 
                        o.Version, 
                        o.Location, 
                        o.InventoryEntryType, 
                        o.ArchiveTimeStamp, 
                        o.OdcSha1Digest, 
                        o.EntitlementIndex, 
                        o.RewardIndex, 
                        o.UserLocation, 
                        o.UserDateLastUsed, 
                        m.KeyName, 
                        m.Value
                    FROM Objects o
                    LEFT JOIN Metadata m ON o.ObjectIndex = m.ObjectIndex";

                        using (SQLiteCommand command = new SQLiteCommand(query, connection))
                        {
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                bool headerWritten = false;

                                while (await reader.ReadAsync())
                                {
                                    var sqliteReader = (SQLiteDataReader)reader;

                                    if (!headerWritten)
                                    {
                                        for (int i = 0; i < sqliteReader.FieldCount; i++)
                                        {
                                            csv.WriteField(sqliteReader.GetName(i));
                                        }
                                        csv.NextRecord();
                                        headerWritten = true;
                                    }

                                    for (int i = 0; i < sqliteReader.FieldCount; i++)
                                    {
                                        var value = sqliteReader.GetValue(i);

                                        if (value is DBNull)
                                        {
                                            csv.WriteField(string.Empty);
                                        }
                                        else if (value is byte[] blob)
                                        {
                                            csv.WriteField(BitConverter.ToString(blob).Replace("-", "").ToUpper());
                                        }
                                        else if (value is int && (sqliteReader.GetName(i) == "ArchiveTimeStamp" || sqliteReader.GetName(i) == "UserDateLastUsed"))
                                        {
                                            // Convert integer timestamps to hexadecimal strings
                                            csv.WriteField(Convert.ToInt32(value).ToString("X8"));
                                        }
                                        else
                                        {
                                            csv.WriteField(value);
                                        }
                                    }
                                    csv.NextRecord();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to CSV: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private string ConvertTimestampToHex(string timestamp)
        {
            if (int.TryParse(timestamp, out int timestampInt))
            {
                return timestampInt.ToString("X8");
            }
            return timestamp; // Return the original value if conversion fails
        }

        private string ConvertBlobToSha1(byte[] blob)
        {
            if (blob != null && blob.Length == 20) // Ensure it's a 20-byte SHA1 blob
            {
                return BitConverter.ToString(blob).Replace("-", "").ToUpper();
            }
            return string.Empty;
        }

        private void WriteObjectsToXML(string xmlFilePath, List<XmlObject> xmlObjects)
        {
            try
            {
                using (XmlWriter writer = XmlWriter.Create(xmlFilePath, new XmlWriterSettings { Indent = true }))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("object_catalogue");

                    foreach (var obj in xmlObjects)
                    {
                        if (string.IsNullOrEmpty(obj.ObjectId))
                        {
                            continue; // Skip entries without a UUID
                        }

                        writer.WriteStartElement("object");

                        writer.WriteAttributeString("uuid", obj.ObjectId);
                        writer.WriteAttributeString("timestamp", obj.ArchiveTimeStamp.ToUpper());

                        if (!string.IsNullOrEmpty(obj.OdcSha1Digest))
                        {
                            writer.WriteAttributeString("sha1", obj.OdcSha1Digest.ToUpper());
                        }

                        if (!string.IsNullOrEmpty(obj.Version))
                        {
                            writer.WriteAttributeString("vers", obj.Version);
                        }

                        // Add metadata
                        AddMetadataToXML(writer, obj.Metadata);

                        writer.WriteEndElement(); // End "object"
                    }

                    writer.WriteEndElement(); // End "object_catalogue"
                    writer.WriteEndDocument();
                }

                CleanUpXML(xmlFilePath, xmlFilePath); // Clean up the XML and save to the final file path
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing XML to file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddMetadataToXML(XmlWriter writer, List<XmlMetadata> metadataList)
        {
            Dictionary<string, List<string>> metadata = new Dictionary<string, List<string>>();
            Dictionary<string, Dictionary<string, string>> specialMetadata = new Dictionary<string, Dictionary<string, string>>();
            List<XmlMetadata> heatMetadata = new List<XmlMetadata>(); // List to store heat metadata
            bool hasSpecialEntitlement = false;

            foreach (var metadataItem in metadataList)
            {
                string keyName = metadataItem.KeyName.ToLower();
                string value = metadataItem.Value;

                if (keyName == "title_id")
                {
                    if (!metadata.ContainsKey(keyName))
                    {
                        metadata[keyName] = new List<string>();
                    }
                    metadata[keyName].Add(value);
                }
                else if (keyName == "entitlement_id" || keyName == "product_id" || keyName == "category_id")
                {
                    if (keyName == "entitlement_id" && (value == "LUA_REWARD" || value == "AUTOMATIC_REWARD" || value == "FREE"))
                    {
                        hasSpecialEntitlement = true;
                        if (!specialMetadata.ContainsKey(keyName))
                        {
                            specialMetadata[keyName] = new Dictionary<string, string>();
                        }
                        specialMetadata[keyName]["SCEE"] = value;
                        specialMetadata[keyName]["SCEA"] = value;
                        specialMetadata[keyName]["SCEJ"] = value;
                        specialMetadata[keyName]["SCEAsia"] = value;
                    }
                    else
                    {
                        string territory = GetTerritory(value);
                        if (!specialMetadata.ContainsKey(keyName))
                        {
                            specialMetadata[keyName] = new Dictionary<string, string>();
                        }
                        specialMetadata[keyName][territory] = value;
                    }
                }
                else if (keyName == "host_heat" || keyName == "main_heat" || keyName == "net_heat" || keyName == "ppu_heat" || keyName == "vram_heat")
                {
                    // Add to heat metadata list
                    heatMetadata.Add(metadataItem);
                }
                else
                {
                    if (!metadata.ContainsKey(keyName))
                    {
                        metadata[keyName] = new List<string>();
                    }
                    metadata[keyName].Add(value);
                }
            }

            // Write regular metadata
            foreach (var key in metadata.Keys)
            {
                writer.WriteStartElement(key);

                if (metadata[key].Count == 0 || string.IsNullOrEmpty(metadata[key][0]))
                {
                    writer.WriteEndElement(); // Self-closing tag for empty values
                }
                else
                {
                    writer.WriteString(string.Join(" ", metadata[key])); // Space-separated values
                    writer.WriteEndElement();
                }
            }

            // Write special metadata
            WriteSpecialMetadata(writer, specialMetadata, hasSpecialEntitlement);

            // Write heat metadata last
            foreach (var heatItem in heatMetadata)
            {
                writer.WriteStartElement(heatItem.KeyName.ToLower());
                writer.WriteString(heatItem.Value);
                writer.WriteEndElement();
            }
        }

        private void WriteSpecialMetadata(XmlWriter writer, Dictionary<string, Dictionary<string, string>> specialMetadata, bool hasSpecialEntitlement)
        {
            foreach (var key in specialMetadata.Keys)
            {
                var territories = new HashSet<string> { "SCEE", "SCEA", "SCEJ", "SCEAsia" };

                foreach (var territory in specialMetadata[key].Keys)
                {
                    writer.WriteStartElement(key);
                    writer.WriteAttributeString("territory", territory);

                    if (!string.IsNullOrEmpty(specialMetadata[key][territory]))
                    {
                        writer.WriteString(specialMetadata[key][territory]);
                    }

                    writer.WriteEndElement();

                    territories.Remove(territory);
                }

                // Add missing territories
                foreach (var territory in territories)
                {
                    writer.WriteStartElement(key);
                    writer.WriteAttributeString("territory", territory);
                    writer.WriteEndElement(); // Self-closing tag for empty values
                }
            }
        }

        private string GetTerritory(string value)
        {
            if (value.StartsWith("E"))
            {
                return "SCEE";
            }
            else if (value.StartsWith("U"))
            {
                return "SCEA";
            }
            else if (value.StartsWith("J"))
            {
                return "SCEJ";
            }
            else if (value.StartsWith("H"))
            {
                return "SCEAsia";
            }
            else
            {
                return string.Empty;
            }
        }

        private void CleanUpXML(string inputXmlFilePath, string outputXmlFilePath)
        {
            XDocument doc = XDocument.Load(inputXmlFilePath);

            // Remove entries with empty territory
            doc.Descendants()
               .Where(e => e.Attribute("territory") != null && string.IsNullOrEmpty(e.Attribute("territory").Value))
               .Remove();

            // Make LPID uppercase and format clothing and furniture values
            foreach (var element in doc.Descendants())
            {
                if (element.Name.LocalName == "lpid")
                {
                    element.Name = "LPID";
                    if (!string.IsNullOrEmpty(element.Value))
                    {
                        element.Value = TransformSpecialCases(element.Value);
                    }
                }
                else if (element.Name.LocalName == "clothing" || element.Name.LocalName == "furniture")
                {
                    if (!string.IsNullOrEmpty(element.Value))
                    {
                        element.Value = TransformSpecialCases(element.Value);
                    }
                }
            }

            doc.Save(outputXmlFilePath);
        }

        private string TransformSpecialCases(string input)
        {
            switch (input.ToLower())
            {
                case "default_hair":
                    return "Default_Hair";
                case "jewelbothears":
                    return "JewelBothEars";
                case "jewelleftear":
                    return "JewelLeftEar";
                case "jewelrightear":
                    return "JewelRightEar";
                case "headphones":
                    return "HeadPhones";
                case "facialhair":
                    return "FacialHair";
                case "default_facialhair":
                    return "Default_FacialHair";
                default:
                    return char.ToUpper(input[0]) + input.Substring(1).ToLower();
            }
        }




        private void Border_DragEnter(object sender, DragEventArgs e)
        {
            LogDebugInfo("Border_DragEnter invoked.");
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                LogDebugInfo("FileDrop detected. Copy effect set.");
            }
            else
            {
                e.Effects = DragDropEffects.None;
                LogDebugInfo("No FileDrop detected. None effect set.");
            }
        }

        private void Border_Drop(object sender, DragEventArgs e)
        {
            LogDebugInfo("Border_Drop invoked.");
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    string filePath = files[0];
                    LogDebugInfo($"File dropped: {filePath}");
                    ProcessDroppedFile(filePath);
                }
            }
        }

        private async void ProcessDroppedFile(string filePath)
        {
            LogDebugInfo($"ProcessDroppedFile invoked with filePath: {filePath}");
            try
            {
                if (!TryProcessFile(filePath))
                {
                    string baseOutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "work"); // Use static work directory
                    LogDebugInfo($"Decrypting file: {filePath}");
                    bool decrypted = await DecryptODCFilesAsync(new[] { filePath }, baseOutputDirectory);
                    if (decrypted)
                    {
                        string decryptedFilePath = Path.Combine(baseOutputDirectory, Path.GetFileName(filePath));
                        LogDebugInfo($"Decrypted file path: {decryptedFilePath}");
                        if (!TryProcessFile(decryptedFilePath))
                        {
                            MessageBox.Show("Failed to process the decrypted file.");
                            LogDebugInfo("Failed to process the decrypted file.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Failed to decrypt the file.");
                        LogDebugInfo("Failed to decrypt the file.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while processing the file: " + ex.Message);
                LogDebugInfo($"Error processing file: {ex.Message}");
            }
        }

        private bool TryProcessFile(string filePath)
        {
            LogDebugInfo($"TryProcessFile invoked with filePath: {filePath}");
            try
            {
                // Load the XML document
                XDocument doc = XDocument.Load(filePath);
                LogDebugInfo("XML document loaded.");

                // Extract the UUID and timestamp
                string uuid = doc.Root.Element("uuid")?.Value;
                if (string.IsNullOrEmpty(uuid))
                {
                    LogDebugInfo("UUID not found in XML document.");
                    return false; // UUID not found, return false
                }

                string timestamp = doc.Root.Element("timestamp")?.Value;

                // Calculate SHA1 hash of the file
                string sha1Hash = CalculateSha1(filePath);
                LogDebugInfo($"SHA1 hash calculated: {sha1Hash}");

                // Extract the image file name suffix and update the version
                string version = ExtractVersionFromImages(doc);
                LogDebugInfo($"Version extracted from images: {version}");

                // Update the TextBox controls
                txtObjectId.Text = uuid;
                txtArchiveTimeStampHex.Text = timestamp;
                txtOdcSha1Digest.Text = sha1Hash;
                txtVersion.Text = version;

                LogDebugInfo("File processed successfully.");
                return true; // Successfully processed the file
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error processing file: {ex.Message}");
                return false; // Error occurred, return false
            }
        }

        public async Task<bool> DecryptODCFilesAsync(string[] filePaths, string baseOutputDirectory)
        {
            LogDebugInfo("DecryptODCFilesAsync invoked.");
            bool allFilesProcessed = true;
            foreach (var filePath in filePaths)
            {
                string filename = Path.GetFileName(filePath);
                try
                {
                    byte[] fileContent = await File.ReadAllBytesAsync(filePath);
                    LogDebugInfo($"Read file content: {filename}");

                    BruteforceProcess proc = new BruteforceProcess(fileContent);
                    byte[] decryptedContent = proc.StartBruteForce((int)CdnMode.RETAIL);

                    if (decryptedContent != null)
                    {
                        string outputDirectory = baseOutputDirectory;
                        if (!Directory.Exists(outputDirectory))
                        {
                            Directory.CreateDirectory(outputDirectory);
                            LogDebugInfo($"Created output directory: {outputDirectory}");
                        }
                        string outputPath = Path.Combine(outputDirectory, filename);
                        await File.WriteAllBytesAsync(outputPath, decryptedContent);
                        LogDebugInfo($"Decrypted content written to: {outputPath}");

                        if (!IsValidDecryptedODCFile(outputPath))
                        {
                            File.Delete(outputPath);
                            LogDebugInfo($"Invalid decrypted ODC file, deleted: {outputPath}");
                            allFilesProcessed = false;
                        }
                    }
                    else
                    {
                        allFilesProcessed = false;
                        LogDebugInfo($"Decryption failed for file: {filename}");
                    }
                }
                catch (Exception ex)
                {
                    allFilesProcessed = false;
                    LogDebugInfo($"Error decrypting file: {ex.Message}");
                }
            }

            return allFilesProcessed;
        }

        private bool IsValidDecryptedODCFile(string filePath)
        {
            LogDebugInfo($"IsValidDecryptedODCFile invoked with filePath: {filePath}");
            try
            {
                XDocument doc = XDocument.Load(filePath);
                bool isValid = doc.Root.Element("uuid") != null;
                LogDebugInfo($"IsValidDecryptedODCFile result: {isValid}");
                return isValid;
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error in IsValidDecryptedODCFile: {ex.Message}");
                return false;
            }
        }

        private string CalculateSha1(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                using (SHA1 sha1 = SHA1.Create())
                {
                    byte[] hashBytes = sha1.ComputeHash(fs);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant();
                }
            }
        }

        private string ExtractVersionFromImages(XDocument doc)
        {
            string[] imageElements = { "maker_image", "small_image", "large_image" };
            foreach (var elementName in imageElements)
            {
                string imagePath = doc.Root.Element(elementName)?.Value;
                if (!string.IsNullOrEmpty(imagePath))
                {
                    var match = Regex.Match(imagePath, @"_T(\d{3})\.png", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        // Extract the version and trim leading zeros
                        string version = match.Groups[1].Value.TrimStart('0');
                        return version;
                    }
                }
            }
            return string.Empty; // Return empty string if no version found
        }

        private async void ExportToHCDBButton_Click(object sender, RoutedEventArgs e)
        {
            DeployHCDBFlag = false;
            ClearUnsavedChanges();
            LogDebugInfo("HCDB Conversion: Process Initiated");


            if (!Directory.Exists(_settings.HcdbOutputDirectory))
            {
                Directory.CreateDirectory(_settings.HcdbOutputDirectory);
                LogDebugInfo($"HCDB Conversion: Output directory created at {_settings.HcdbOutputDirectory}");
            }

            if (!string.IsNullOrWhiteSpace(sqlFilePath))
            {
                string[] filePaths = new string[] { sqlFilePath };
                string[] segsPaths = filePaths.Select(fp => fp + ".segs").ToArray(); // Assuming LZMA outputs files with .segs extension

                LogDebugInfo($"HCDB Conversion: Starting conversion for {filePaths.Length} file(s)");
                bool conversionSuccess = await ConvertSqlToHcdbAsync_Unique(filePaths);

                if (conversionSuccess)
                {
                    bool encryptionSuccess = await EncryptHCDBFilesAsync_Unique(segsPaths, _settings.HcdbOutputDirectory); // Encrypt the .segs files
                    if (!encryptionSuccess)
                    {
                        string message = "Encryption Failed";

                        LogDebugInfo($"HCDB Conversion: Result - {message}");
                    }
                }
                else
                {
                    string message = "Conversion Failed";

                    LogDebugInfo($"HCDB Conversion: Result - {message}");
                }
            }
            else
            {
                LogDebugInfo("HCDB Conversion: Aborted - No SQL file specified for conversion.");

            }
        }



        private async Task<bool> ConvertSqlToHcdbAsync_Unique(string[] filePaths)
        {
            bool allFilesProcessed = true;
            string lzmaPath = @"dependencies\lzma_segs.exe";

            foreach (var filePath in filePaths)
            {
                string filename = Path.GetFileName(filePath);
                string expectedOutputPath = filePath + ".segs";  // Expecting LZMA to append ".segs" to the input file's name

                try
                {
                    // Process the file with LZMA without explicitly specifying the output path
                    if (!ExecuteLzmaProcess_Unique(lzmaPath, filePath))
                    {
                        LogDebugInfo($"Conversion failed for {filename}.");
                        allFilesProcessed = false;
                    }
                    else
                    {
                        LogDebugInfo($"Conversion successful for {filename}, output likely written to {expectedOutputPath}.");
                    }
                }
                catch (Exception ex)
                {
                    LogDebugInfo($"Error processing {filename}: {ex.Message}");
                    allFilesProcessed = false;
                }
            }

            return allFilesProcessed;
        }

        private bool ExecuteLzmaProcess_Unique(string lzmaPath, string inputFilePath)
        {
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = lzmaPath;
                    process.StartInfo.Arguments = $"\"{inputFilePath}\"";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();

                    // To log output or errors (optional but useful for debugging)
                    string output = process.StandardOutput.ReadToEnd();
                    string errors = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    // Check if the process exited successfully and the expected output file was created
                    if (process.ExitCode == 0 && File.Exists(inputFilePath + ".segs"))
                    {
                        LogDebugInfo($"LZMA compression successful for {Path.GetFileName(inputFilePath)}");
                        return true;
                    }
                    else
                    {
                        LogDebugInfo($"LZMA compression failed for {Path.GetFileName(inputFilePath)}: {errors}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error during LZMA compression: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EncryptHCDBFilesAsync_Unique(string[] filePaths, string baseOutputDirectory)
        {
            bool allFilesProcessed = true;
            string segsFileSHA1 = "";

            foreach (var filePath in filePaths)
            {
                string filename = Path.GetFileName(filePath);
                string originalFilenameWithoutExtension = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(filename));

                // Create the subfolder "Database_Edits" in the existing path
                string subfolderPath = Path.Combine(baseOutputDirectory, "Database_Edits");
                if (!Directory.Exists(subfolderPath))
                {
                    Directory.CreateDirectory(subfolderPath);
                }

                try
                {
                    byte[] fileContent = await File.ReadAllBytesAsync(filePath);
                    byte[]? encryptedContent = null;
                    string inputSHA1 = "";

                    using (SHA1 sha1 = SHA1.Create())
                    {
                        byte[] SHA1Data = sha1.ComputeHash(fileContent);
                        inputSHA1 = BitConverter.ToString(SHA1Data).Replace("-", ""); // Full SHA1 of input content
                        segsFileSHA1 = inputSHA1; // Store the SHA1 of the .segs file
                        LogDebugInfo($"Input SHA1 for {filename}: {inputSHA1}");

                        // Encrypt the content
                        string computedSha1 = inputSHA1.Substring(0, 16);
                        encryptedContent = CDSProcess.CDSEncrypt_Decrypt(fileContent, computedSha1, (int)CdnMode.RETAIL);

                        if (encryptedContent != null)
                        {
                            // Define the 4 file names
                            string[] hcdbFilenames = new string[]
                            {
                        "Objects/ObjectCatalogue_5_SCEAsia.hcdb",
                        "Objects/ObjectCatalogue_5_SCEJ.hcdb",
                        "Objects/ObjectCatalogue_5_SCEA.hcdb",
                        "Objects/ObjectCatalogue_5_SCEE.hcdb"
                            };

                            foreach (var hcdbFilename in hcdbFilenames)
                            {
                                string outputPath = Path.Combine(subfolderPath, Path.GetFileName(hcdbFilename));
                                await File.WriteAllBytesAsync(outputPath, encryptedContent);
                                LogDebugInfo($"File {hcdbFilename} encrypted and written to {outputPath}.");
                            }

                            // After successful encryption, delete the .segs file
                            File.Delete(filePath);
                            LogDebugInfo($"Temporary file {filePath} deleted after successful encryption.");
                        }
                        else
                        {
                            allFilesProcessed = false;
                            LogDebugInfo($"Encryption failed for file {filename}, no data written.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogDebugInfo($"Error encrypting {filename}: {ex.Message}");
                    allFilesProcessed = false;
                }
            }

            if (allFilesProcessed)
            {
                ShowSuccessMessageBox(segsFileSHA1, Path.Combine(baseOutputDirectory, "Database_Edits")); // Display the success message box with SHA-1 and open save location
            }

            return allFilesProcessed;
        }


        private void ShowSuccessMessageBox(string segsFileSHA1, string saveDirectory)
        {
            // Update the LatestHCDBSHA1textbox and set the visibility of LatestHCDBSHA1panel
            Dispatcher.Invoke(() =>
            {
                // Find the LatestHCDBSHA1textbox and set its text to the computed SHA1 hash
                var latestHCDBSHA1textbox = (TextBox)FindName("LatestHCDBSHA1textbox");
                if (latestHCDBSHA1textbox != null)
                {
                    latestHCDBSHA1textbox.Text = segsFileSHA1;
                }
                else
                {
                    MessageBox.Show("LatestHCDBSHA1textbox not found!");
                }

                // Find the LatestHCDBSHA1panel and set its visibility to visible
                var latestHCDBSHA1panel = (StackPanel)FindName("LatestHCDBSHA1panel");
                if (latestHCDBSHA1panel != null)
                {
                    latestHCDBSHA1panel.Visibility = Visibility.Visible;
                }
                else
                {
                    MessageBox.Show("LatestHCDBSHA1panel not found!");
                }
            });

            // Create the success message box
            var messageBox = new Window
            {
                Title = "4 x HCDB Created Successfully",
                Width = 350,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                Background = (Brush)new BrushConverter().ConvertFromString("#242424"),
                Foreground = Brushes.White,
                WindowStyle = WindowStyle.None,
                BorderBrush = (Brush)new BrushConverter().ConvertFromString("#000000"),
                BorderThickness = new Thickness(1)
            };

            var stackPanel = new StackPanel
            {
                Margin = new Thickness(10)
            };

            var textBlock = new TextBlock
            {
                Text = "HCDB Creation was successful.\n\nSHA1 of the .segs file:",
                Margin = new Thickness(0, 0, 0, 10),
                FontSize = 14,
                Foreground = Brushes.White
            };

            var sha1TextBox = new TextBox
            {
                Text = segsFileSHA1,
                IsReadOnly = true,
                Margin = new Thickness(0, 0, 0, 10),
                FontSize = 14,
                Width = 330,
                Foreground = (Brush)new BrushConverter().ConvertFromString("#C0BFBF"),
                Background = (Brush)new BrushConverter().ConvertFromString("#121212"),
                BorderThickness = new Thickness(0)
            };

            var copyButton = new Button
            {
                Content = "Copy SHA1",
                Width = 100,
                Height = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 11,
                Foreground = Brushes.White,
                Background = (Brush)new BrushConverter().ConvertFromString("#181818"),
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(3),
                Margin = new Thickness(0, 0, 0, 10),
                BorderThickness = new Thickness(1),
                BorderBrush = (Brush)new BrushConverter().ConvertFromString("#000000")
            };
            copyButton.Click += (s, e) =>
            {
                Clipboard.SetText(segsFileSHA1);
                string originalContent = copyButton.Content.ToString();
                copyButton.Content = "Copied";

                var timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                timer.Tick += (sender, args) =>
                {
                    copyButton.Content = originalContent;
                    timer.Stop();
                };
                timer.Start();
            };

            var okButton = new Button
            {
                Content = "OK",
                Width = 70,
                Height = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 11,
                Foreground = Brushes.White,
                Background = (Brush)new BrushConverter().ConvertFromString("#181818"),
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(3),
                BorderThickness = new Thickness(1),
                BorderBrush = (Brush)new BrushConverter().ConvertFromString("#000000")
            };
            okButton.Click += (s, e) =>
            {
                messageBox.Close();
            };

            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(sha1TextBox);
            stackPanel.Children.Add(copyButton);
            stackPanel.Children.Add(okButton);

            messageBox.Content = stackPanel;
            messageBox.ShowDialog();
        }



        private async Task<string> ComputeFileSHA1(string filePath)
        {
            using (var sha1 = SHA1.Create())
            {
                using (var fileStream = File.OpenRead(filePath))
                {
                    var hashBytes = await sha1.ComputeHashAsync(fileStream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
                }
            }
        }

        private void BulkEditSQLButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(sqlFilePath))
            {
                MessageBox.Show("Please select a SQL file first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            BulkEditorForDB bulkEditor = new BulkEditorForDB(this, sqlFilePath);
            bulkEditor.ShowDialog();
        }


        private void XMLClearListHandler(object sender, RoutedEventArgs e)
        {
            XMLOutputTextBox.Clear();
        }


        public class Scene
        {
            public string Url { get; set; }
            public string Sha1 { get; set; }
            public XElement XElement { get; set; }  // Store the original XElement for modification
        } 

        private string Sha1ForHCDB;
        private string ContentServerURL;
        private string SceneListSHA1;



        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            string tssUrl = TSSUrlTextBox.Text;
            if (string.IsNullOrEmpty(tssUrl))
            {
                MessageBox.Show("Please enter a TSS URL.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string selectedTerritory = ((ComboBoxItem)TerritoryComboBox.SelectedItem).Content.ToString();

            // Log starting message
            XMLOutputTextBox.Text += $"Starting CDN audit with TSS URL: {tssUrl}.... ";

            // Download the TSS XML
            bool tssDownloadSuccess = await AuditDownloadFileAsync(tssUrl, "TSS.xml");
            XMLOutputTextBox.Text += tssDownloadSuccess
                ? "TSS XML downloaded successfully.\n"
                : "Failed to download TSS XML.\n";

            if (!tssDownloadSuccess)
            {
                return;
            }

            // Parse the TSS XML to find the SHA1 for the ObjectCatalogue_5_<territory>.hcdb
            string tssFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "Audit", "TSS.xml");
            Sha1ForHCDB = ParseSha1FromTssXml(tssFilePath, $"Objects/ObjectCatalogue_5_{selectedTerritory}.hcdb");

            if (string.IsNullOrEmpty(Sha1ForHCDB))
            {
                XMLOutputTextBox.Text += $"SHA1 for ObjectCatalogue_5_{selectedTerritory}.hcdb not found in TSS XML.\n";
            }
            else
            {
                XMLOutputTextBox.Text += $"SHA1 found for ObjectCatalogue_5_{selectedTerritory}.hcdb: {Sha1ForHCDB}\n";
            }

            // Parse the TSS XML to find the SHA1 for the SceneList.xml
            SceneListSHA1 = ParseSha1FromTssXml(tssFilePath, "Environments/SceneList.xml");

            if (string.IsNullOrEmpty(SceneListSHA1))
            {
                XMLOutputTextBox.Text += "SHA1 for SceneList.xml not found in TSS XML.\n";
            }
            else
            {
                XMLOutputTextBox.Text += $"SHA1 found for SceneList.xml: {SceneListSHA1}\n";
            }

            // Parse the TSS XML to find the Content Server URL
            ContentServerURL = ParseContentServerUrlFromTssXml(tssFilePath, tssUrl);

            if (string.IsNullOrEmpty(ContentServerURL))
            {
                XMLOutputTextBox.Text += "Content Server URL not found in TSS XML.\n";
            }
            else
            {
                XMLOutputTextBox.Text += $"Content Server URL Found and Set to: {ContentServerURL}\n";
            }

            // Extract the domain from the TSS URL
            Uri tssUri = new Uri(tssUrl);
            string domain = $"{tssUri.Scheme}://{tssUri.Host}";

            // Construct the URL for the ObjectCatalogue file
            string objectCatalogueUrl = $"{domain}/Objects/ObjectCatalogue_5_{selectedTerritory}.hcdb";

            // Log starting message
            XMLOutputTextBox.Text += $"Starting download of ObjectCatalogue_5_{selectedTerritory}.hcdb.... \n";

            // Download the ObjectCatalogue file
            bool catalogueDownloadSuccess = await DownloadFileWithProgressAsync(objectCatalogueUrl, $"ObjectCatalogue_5_{selectedTerritory}.hcdb");
            XMLOutputTextBox.Text += catalogueDownloadSuccess
                ? $"ObjectCatalogue_5_{selectedTerritory}.hcdb downloaded successfully.\n"
                : $"Failed to download ObjectCatalogue_5_{selectedTerritory}.hcdb.\n";

            if (catalogueDownloadSuccess)
            {
                string hcdbFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "Audit", $"ObjectCatalogue_5_{selectedTerritory}.hcdb");
                string outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "Audit");

                // Decrypt and convert the HCDB file
                bool decryptionSuccess = await DecryptAndConvertHcdbAsync(hcdbFilePath, outputDirectory);
                XMLOutputTextBox.Text += decryptionSuccess
                    ? $"ObjectCatalogue_5_{selectedTerritory}.hcdb decrypted and converted to SQL successfully.\n"
                    : $"Failed to decrypt and convert ObjectCatalogue_5_{selectedTerritory}.hcdb.\n";
            }

            // Construct the URL for the SceneList.xml file
            string sceneListUrl = $"{domain}/Environments/SceneList.xml";

            // Log starting message
            XMLOutputTextBox.Text += $"Starting download of SceneList.xml from {sceneListUrl}.... \n";

            // Download the SceneList.xml file
            bool sceneListDownloadSuccess = await DownloadFileWithProgressAsync(sceneListUrl, "SceneList.xml");
            XMLOutputTextBox.Text += sceneListDownloadSuccess
                ? "SceneList.xml downloaded successfully.\n"
                : "Failed to download SceneList.xml.\n";

            if (sceneListDownloadSuccess)
            {
                string sceneListFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "Audit", "SceneList.xml");
                string outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "Audit");

                // Decrypt the SceneList.xml file
                bool sceneListDecryptionSuccess = await DecryptSceneListAsync(sceneListFilePath, outputDirectory);
                XMLOutputTextBox.Text += sceneListDecryptionSuccess
                    ? ""
                    : "Failed to decrypt SceneList.xml.\n";
            }
        }


        private async Task<bool> DownloadFileWithProgressAsync(string url, string fileName)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(10); // Increase timeout to 10 minutes

                    var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    string downloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "Audit");
                    Directory.CreateDirectory(downloadDirectory);
                    string downloadPath = Path.Combine(downloadDirectory, fileName);

                    using (var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (var httpStream = await response.Content.ReadAsStreamAsync())
                    {
                        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                        var canReportProgress = totalBytes != -1;
                        var totalRead = 0L;
                        var buffer = new byte[8192];
                        var isMoreToRead = true;

                        while (isMoreToRead)
                        {
                            var read = await httpStream.ReadAsync(buffer, 0, buffer.Length);
                            if (read == 0)
                            {
                                isMoreToRead = false;
                            }
                            else
                            {
                                await fileStream.WriteAsync(buffer, 0, read);
                                totalRead += read;

                                if (canReportProgress)
                                {
                                    UpdateLastLine($"Downloaded {totalRead} of {totalBytes} bytes ({(double)totalRead / totalBytes:P}).... ");
                                }
                                else
                                {
                                    UpdateLastLine($"Downloaded {totalRead} bytes.... ");
                                }
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                XMLOutputTextBox.Text += $"Error downloading file from {url}: {ex.Message}\n";
                return false;
            }
        }

        private void UpdateLastLine(string text)
        {
            Dispatcher.Invoke(() =>
            {
                var lines = XMLOutputTextBox.Text.Split('\n').ToList();
                if (lines.Count > 1)
                {
                    lines[lines.Count - 1] = text;
                }
                else
                {
                    lines.Add(text);
                }
                XMLOutputTextBox.Text = string.Join("\n", lines);
                XMLOutputTextBox.ScrollToEnd();
            });
        }

        private string ParseSha1FromTssXml(string filePath, string targetFile)
        {
            try
            {
                XDocument doc = XDocument.Load(filePath);
                var sha1Element = doc.Descendants("SHA1")
                                     .FirstOrDefault(e => e.Attribute("file")?.Value == targetFile);
                return sha1Element?.Attribute("digest")?.Value;
            }
            catch (Exception ex)
            {
                XMLOutputTextBox.Text += $"Error parsing TSS XML: {ex.Message}\n";
                return null;
            }
        }

        private string ParseContentServerUrlFromTssXml(string filePath, string tssUrl)
        {
            try
            {
                XDocument doc = XDocument.Load(filePath);
                var contentServerElement = doc.Descendants("contentserver")
                                              .FirstOrDefault();
                if (contentServerElement != null)
                {
                    string contentServerUrl = contentServerElement.Value;

                    // Extract the domain from the TSS URL
                    Uri tssUri = new Uri(tssUrl);
                    string domain = $"{tssUri.Scheme}://{tssUri.Host}";

                    // Replace the domain in the content server URL
                    Uri contentServerUri = new Uri(contentServerUrl);
                    string newContentServerUrl = $"{domain}{contentServerUri.PathAndQuery}";

                    return newContentServerUrl;
                }
                return null;
            }
            catch (Exception ex)
            {
                XMLOutputTextBox.Text += $"Error parsing TSS XML for content server URL: {ex.Message}\n";
                return null;
            }
        }

        private async Task<bool> DecryptAndConvertHcdbAsync(string hcdbFilePath, string outputDirectory)
        {
            XMLOutputTextBox.Text += $"Starting decryption of HCDB file.... ";

            // Attempt decryption with SHA1
            byte[]? decryptedData = await DecryptHCDBFilesSHA1Async(hcdbFilePath, outputDirectory, Sha1ForHCDB);
            if (decryptedData == null)
            {
                XMLOutputTextBox.Text += $"SHA1 decryption failed, trying brute force for file: {hcdbFilePath}\n";
                // Attempt brute force decryption
                decryptedData = await DecryptHCDBFilesAsync(hcdbFilePath, outputDirectory);
            }

            if (decryptedData != null)
            {
                XMLOutputTextBox.Text += $"Decryption successful for HCDB file: {hcdbFilePath}\n";
                string fileName = Path.GetFileName(hcdbFilePath);
                bool processSuccess = await AuditProcessDecryptedHCDB(hcdbFilePath, decryptedData, outputDirectory, fileName);
                if (processSuccess)
                {
                    string sqlFilePath = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(fileName) + ".SQL");
                    XMLOutputTextBox.Text += $"HCDB file converted to SQL successfully: {sqlFilePath}\n";
                    return true;
                }
                else
                {
                    XMLOutputTextBox.Text += $"Failed to process decrypted HCDB file to SQL: {hcdbFilePath}\n";
                    return false;
                }
            }
            else
            {
                XMLOutputTextBox.Text += $"Failed to decrypt HCDB file: {hcdbFilePath}\n";
                return false;
            }
        }



        private async Task<bool> AuditProcessDecryptedHCDB(string filePath, byte[] decryptedData, string outputDirectory, string filename)
        {
            try
            {
                string baseFilename = Path.GetFileNameWithoutExtension(filename);
                string cleanFilename = Regex.Replace(baseFilename, "[a-fA-F0-9]{40}", ""); // Remove SHA1 hash
                cleanFilename = cleanFilename.TrimEnd('_') + ".SQL"; // Ensure it ends with ".sql"

                byte[]? processedData = HCDBUnpack(decryptedData, LogDebugInfo);
                if (processedData != null)
                {
                    string outputPath = Path.Combine(outputDirectory, cleanFilename);
                    await File.WriteAllBytesAsync(outputPath, processedData);
                    LogDebugInfo($"Processed HCDB file successfully written to {outputPath}.");

                    // Validate the processed SQL file
                    if (IsValidSqlFile(outputPath))
                    {
                        LogDebugInfo($"Validation passed for SQL file at {outputPath}.");

                        // Export to CSV
                        string csvFilePath = Path.Combine(outputDirectory, cleanFilename.Replace(".SQL", ".csv"));
                        bool csvExportSuccess = await AuditExportToCSV(outputPath, csvFilePath);
                        if (!csvExportSuccess)
                        {
                            csvExportSuccess = await AuditExportToCSV(outputPath, csvFilePath); // Retry once
                        }

                        return csvExportSuccess;
                    }
                    else
                    {
                        File.Delete(outputPath);
                        LogDebugInfo($"Invalid SQL file deleted at {outputPath}.");
                        return false;
                    }
                }
                else
                {
                    LogDebugInfo($"Failed to process decrypted data for {filename}.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error processing decrypted data for {filename}: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> AuditExportToCSV(string sqlFilePath, string csvFilePath)
        {
            try
            {
                this.csvFilePath = csvFilePath; // Save the CSV file path

                using (var writer = new StreamWriter(csvFilePath))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    string connectionString = $"Data Source={sqlFilePath};Version=3;";
                    using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        string query = @"
                SELECT 
                    o.ObjectIndex, 
                    o.ObjectId, 
                    o.Version, 
                    o.Location, 
                    o.InventoryEntryType, 
                    o.ArchiveTimeStamp, 
                    o.OdcSha1Digest, 
                    o.EntitlementIndex, 
                    o.RewardIndex, 
                    o.UserLocation, 
                    o.UserDateLastUsed, 
                    m.KeyName, 
                    m.Value
                FROM Objects o
                LEFT JOIN Metadata m ON o.ObjectIndex = m.ObjectIndex";

                        using (SQLiteCommand command = new SQLiteCommand(query, connection))
                        {
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                bool headerWritten = false;

                                while (await reader.ReadAsync())
                                {
                                    var sqliteReader = (SQLiteDataReader)reader;

                                    if (!headerWritten)
                                    {
                                        for (int i = 0; i < sqliteReader.FieldCount; i++)
                                        {
                                            csv.WriteField(sqliteReader.GetName(i));
                                        }
                                        csv.NextRecord();
                                        headerWritten = true;
                                    }

                                    for (int i = 0; i < sqliteReader.FieldCount; i++)
                                    {
                                        var value = sqliteReader.GetValue(i);

                                        if (value is DBNull)
                                        {
                                            csv.WriteField(string.Empty);
                                        }
                                        else if (value is byte[] blob)
                                        {
                                            csv.WriteField(ConvertBlobToSha1(blob));
                                        }
                                        else if (value is int && (sqliteReader.GetName(i) == "ArchiveTimeStamp" || sqliteReader.GetName(i) == "UserDateLastUsed"))
                                        {
                                            // Convert integer timestamps to hexadecimal strings
                                            csv.WriteField(ConvertTimestampToHex(value.ToString()));
                                        }
                                        else
                                        {
                                            csv.WriteField(value);
                                        }
                                    }
                                    csv.NextRecord();
                                }
                            }
                        }
                    }
                }
                XMLOutputTextBox.Text += $"Data exported to CSV successfully: {csvFilePath}\n";
                return true;
            }
            catch (Exception ex)
            {
                // Suppress error logging for the first failure
                return false;
            }
        }

        private async Task<bool> AuditDownloadFileAsync(string url, string fileName)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(5); // Increase timeout to 5 minutes

                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    string downloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "Audit");
                    Directory.CreateDirectory(downloadDirectory);
                    string downloadPath = Path.Combine(downloadDirectory, fileName);

                    using (var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                XMLOutputTextBox.Text += $"Error downloading file from {url}: {ex.Message}\n";
                return false;
            }
        }

        private List<string> failedDownloads = new List<string>();
        private List<string> sha1VerificationFails = new List<string>();

        private async Task<bool> DecryptSceneListAsync(string sceneListFilePath, string outputDirectory)
        {
            XMLOutputTextBox.Text += $"Starting decryption of SceneList.xml... ";

            try
            {
                byte[] fileContent = await File.ReadAllBytesAsync(sceneListFilePath);
                BruteforceProcess proc = new BruteforceProcess(fileContent);
                byte[] decryptedContent = proc.StartBruteForce((int)CdnMode.RETAIL);

                if (decryptedContent != null)
                {
                    string outputPath = Path.Combine(outputDirectory, "Decrypted_SceneList.xml");
                    await File.WriteAllBytesAsync(outputPath, decryptedContent);

                    // Parse the decrypted XML
                    XDocument doc = XDocument.Load(new MemoryStream(decryptedContent));
                    var scenes = doc.Descendants("SCENE").Select(scene => new Scene
                    {
                        Url = $"{ContentServerURL}Scenes/{scene.Attribute("desc").Value}",
                        Sha1 = scene.Attribute("sha1").Value,
                        XElement = scene
                    }).ToList();
                    int sceneCount = scenes.Count;

                    XMLOutputTextBox.Text += $"Decryption successful for SceneList file: {sceneListFilePath} with {sceneCount} SCENE items.\n";

                    int totalPaths = scenes.Count;
                    int downloadedCount = 0;
                    int failedCount = 0;

                    // Log total SDC paths
                    UpdateSDCDownloadStatus(totalPaths, downloadedCount, failedCount);

                    // Download SDC files concurrently
                    await DownloadFilesConcurrently(scenes, outputDirectory, 6, (scene, success) =>
                    {
                        if (success)
                        {
                            downloadedCount++;
                        }
                        else
                        {
                            failedCount++;
                            failedDownloads.Add(scene.Url);
                        }

                        UpdateSDCDownloadStatus(totalPaths, downloadedCount, failedCount);
                        // Pause for 20ms to allow status update
                        Task.Delay(20).Wait();
                    });

                    // Log the total number of download failures
                    XMLOutputTextBox.Text += $"\nTotal download failures: {failedDownloads.Count}";

                    // Write download failures to log
                    string auditFailsLogPath = Path.Combine(outputDirectory, "AuditFails.log");
                    await File.AppendAllLinesAsync(auditFailsLogPath, failedDownloads.Select(url => $"Download Failure: {url}"));

                    // Now verify the SHA1 of downloaded SDC files
                    var successfulDownloads = scenes.Where(scene => !failedDownloads.Contains(scene.Url)).ToList();
                    int sha1VerifiedCount = 0;
                    int sha1FailedCount = 0;
                    int totalFilesToVerify = successfulDownloads.Count;

                    foreach (var scene in successfulDownloads)
                    {
                        bool sha1VerificationSuccess = await DecryptAndVerifySDC(scene.Url, outputDirectory, scene.Sha1);
                        if (sha1VerificationSuccess)
                        {
                            sha1VerifiedCount++;
                        }
                        else
                        {
                            sha1FailedCount++;
                            sha1VerificationFails.Add(scene.Url);
                        }
                        UpdateSha1Status(totalFilesToVerify, sha1VerifiedCount, sha1FailedCount);
                    }

                    // Log the total number of SHA1 verification failures
                    XMLOutputTextBox.Text += $"\nTotal SHA1 verification failures: {sha1VerificationFails.Count}\n";

                    // Write SHA1 verification failures to log
                    await File.AppendAllLinesAsync(auditFailsLogPath, sha1VerificationFails.Select(url => $"SHA1 Verification Failure: {url}"));

                    // Remove failed downloads from the SceneList
                    foreach (var scene in scenes.Where(scene => failedDownloads.Contains(scene.Url)).ToList())
                    {
                        scene.XElement.Remove();
                    }

                    // Update incorrect SHA1 hashes in the SceneList
                    foreach (var scene in successfulDownloads.Where(scene => sha1VerificationFails.Contains(scene.Url)))
                    {
                        // Extract the path without the domain and protocol
                        Uri contentServerUri = new Uri(ContentServerURL);
                        string relativePath = scene.Url.Substring(contentServerUri.Scheme.Length + 3 + contentServerUri.Host.Length);

                        // Construct the full path to the decrypted SDC file with the "Decrypted_" prefix
                        string sdcFileName = "Decrypted_" + Path.GetFileName(relativePath);
                        string sdcFilePath = Path.Combine(outputDirectory, Path.GetDirectoryName(relativePath).TrimStart(Path.DirectorySeparatorChar), sdcFileName);

                        if (File.Exists(sdcFilePath))
                        {
                            byte[] decryptedSdcContent = await File.ReadAllBytesAsync(sdcFilePath);
                            using (SHA1 sha1 = SHA1.Create())
                            {
                                byte[] computedHash = sha1.ComputeHash(decryptedSdcContent);
                                string computedSha1 = BitConverter.ToString(computedHash).Replace("-", "");
                                scene.XElement.Attribute("sha1").Value = computedSha1;
                            }
                        }
                        else
                        {
                            string warningMessage = $"Warning: SDC file not found for SHA1 verification: {sdcFilePath}\n";
                            XMLOutputTextBox.Text += warningMessage;
                            await File.AppendAllLinesAsync(auditFailsLogPath, new[] { warningMessage });
                        }
                    }

                    // Save the modified SceneList as Fixed_SceneList.xml
                    string fixedSceneListPath = Path.Combine(outputDirectory, "Fixed_SceneList.xml");
                    doc.Save(fixedSceneListPath);
                    XMLOutputTextBox.Text += $"Fixed SceneList saved as: {fixedSceneListPath}\n";

                    // Call the method to count unique ObjectIndex values in the CSV file
                    await CountUniqueObjectIndexesAndGenerateUrls();

                    return true;
                }
                else
                {
                    XMLOutputTextBox.Text += $"Failed to decrypt SceneList file: {sceneListFilePath}\n";
                    return false;
                }
            }
            catch (Exception ex)
            {
                XMLOutputTextBox.Text += $"Error decrypting SceneList file: {ex.Message}\n";
                return false;
            }
        }

        private async Task CountUniqueObjectIndexesAndGenerateUrls()
        {
            if (string.IsNullOrEmpty(csvFilePath))
            {
                XMLOutputTextBox.Text += "CSV file path is not set. Cannot count unique ObjectIndexes.\n";
                return;
            }

            try
            {
                var uniqueObjectIndexes = new HashSet<string>();
                var objectIdUrls = new Dictionary<string, (string url, string sha1)>();

                using (var reader = new StreamReader(csvFilePath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<dynamic>().ToList();

                    for (int i = 1; i < records.Count; i++) // Skip header (i = 0)
                    {
                        var record = records[i];
                        string objectIndex = record.ObjectIndex;
                        string objectId = record.ObjectId;
                        string version = record.Version;
                        string sha1 = record.OdcSha1Digest;

                        if (!string.IsNullOrEmpty(objectIndex) && objectIndex != "0")
                        {
                            if (!uniqueObjectIndexes.Contains(objectIndex))
                            {
                                uniqueObjectIndexes.Add(objectIndex);

                                // Pad the version to 3 characters with leading zeros
                                string paddedVersion = version.PadLeft(3, '0');

                                // Build the new URL
                                string newUrl = $"{ContentServerURL}Objects/{objectId}/object";

                                if (paddedVersion != "000")
                                {
                                    newUrl += $"_T{paddedVersion}.odc";
                                }
                                else
                                {
                                    newUrl += ".odc";
                                }

                                objectIdUrls[objectIndex] = (newUrl, sha1);
                            }
                        }
                    }

                    XMLOutputTextBox.Text += $"Scanning Catalogue.... Total UUIDs Found: {uniqueObjectIndexes.Count}\n";
                }

                string urlsFilePath = Path.Combine(Path.GetDirectoryName(csvFilePath), "GeneratedUrls.txt");
                using (var writer = new StreamWriter(urlsFilePath))
                {
                    foreach (var urlInfo in objectIdUrls.Values)
                    {
                        await writer.WriteLineAsync($"{urlInfo.url},{urlInfo.sha1}");
                    }
                }

                XMLOutputTextBox.Text += $"Total URLs generated: {objectIdUrls.Count}. URLs saved to {urlsFilePath}\n";

                // Start downloading the URLs
                await DownloadGeneratedUrls(urlsFilePath);
            }
            catch (Exception ex)
            {
                XMLOutputTextBox.Text += $"Error counting unique ObjectIndexes in CSV file: {ex.Message}\n";
            }
        }


        private async Task DownloadGeneratedUrls(string urlsFilePath)
        {
            try
            {
                var urlLines = await File.ReadAllLinesAsync(urlsFilePath);
                int totalUrls = urlLines.Length;
                int downloadedCount = 0;
                int failedCount = 0;

                List<string> failedDownloads = new List<string>();
                List<(string url, string fileName, string expectedSha1)> successfulDownloads = new List<(string, string, string)>();
                string failedDownloadsFilePath = Path.Combine(Path.GetDirectoryName(urlsFilePath), "AuditFails.log");

                // Use a SemaphoreSlim to limit the number of concurrent downloads
                using (SemaphoreSlim concurrencySemaphore = new SemaphoreSlim(6))
                {
                    List<Task> downloadTasks = new List<Task>();

                    foreach (var line in urlLines)
                    {
                        await concurrencySemaphore.WaitAsync();

                        downloadTasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                var parts = line.Split(',');
                                var url = parts[0];
                                var expectedSha1 = parts[1];

                                // Extract path and file name
                                Uri uri = new Uri(url);
                                string path = uri.AbsolutePath.Substring(1); // Remove leading slash
                                string fileName = Path.GetFileName(path);
                                string directory = Path.GetDirectoryName(path);

                                // Create directory if it doesn't exist
                                string fullDirectoryPath = Path.Combine(Path.GetDirectoryName(urlsFilePath), directory);
                                Directory.CreateDirectory(fullDirectoryPath);

                                string decryptedFilePath = Path.Combine(fullDirectoryPath, "Decrypted_" + fileName);
                                if (File.Exists(decryptedFilePath))
                                {
                                    // Decrypted file exists, skip download and verify SHA1
                                    bool verificationSuccess = await VerifyFileSha1(decryptedFilePath, expectedSha1);
                                    if (verificationSuccess)
                                    {
                                        downloadedCount++;
                                    }
                                    else
                                    {
                                        failedCount++;
                                        failedDownloads.Add($"SHA1 Verification Failure: {url}");
                                    }
                                }
                                else
                                {
                                    bool success = await DownloadFileWithoutProgressAsync(url, Path.Combine(fullDirectoryPath, fileName));
                                    if (success)
                                    {
                                        successfulDownloads.Add((url, Path.Combine(fullDirectoryPath, fileName), expectedSha1));
                                    }
                                    else
                                    {
                                        failedCount++;
                                        failedDownloads.Add($"Download Failure: {url}");
                                    }
                                }

                                // Additional downloads for URLs containing "object"
                                if (url.Contains("object"))
                                {
                                    string baseObjectUrl = url.Replace("object", "{0}").Replace(".odc", ".png");
                                    string[] objectSizes = { "small", "large" };
                                    foreach (var size in objectSizes)
                                    {
                                        string sizeUrl = string.Format(baseObjectUrl, size);
                                        string sizeFileName = Path.GetFileName(sizeUrl);
                                        string sizeDecryptedFilePath = Path.Combine(fullDirectoryPath, "Decrypted_" + sizeFileName);

                                        if (File.Exists(sizeDecryptedFilePath))
                                        {
                                            bool verificationSuccess = await VerifyFileSha1(sizeDecryptedFilePath, expectedSha1);
                                            if (verificationSuccess)
                                            {
                                                downloadedCount++;
                                            }
                                            else
                                            {
                                                failedCount++;
                                                failedDownloads.Add($"SHA1 Verification Failure: {sizeUrl}");
                                            }
                                        }
                                        else
                                        {
                                            bool success = await DownloadFileWithoutProgressAsync(sizeUrl, Path.Combine(fullDirectoryPath, sizeFileName));
                                            if (!success)
                                            {
                                                failedCount++;
                                                failedDownloads.Add($"Download Failure: {sizeUrl}");
                                            }
                                        }
                                    }
                                }

                                UpdateODCDownloadStatus(totalUrls, downloadedCount, failedCount);
                            }
                            finally
                            {
                                concurrencySemaphore.Release();
                            }
                        }));
                    }

                    await Task.WhenAll(downloadTasks);
                }

                // Log failed downloads to AuditFails.log
                await File.AppendAllLinesAsync(failedDownloadsFilePath, failedDownloads);

                // Now verify the SHA1 of downloaded ODC files
                int sha1VerifiedCount = 0;
                int sha1FailedCount = 0;
                int totalFilesToVerify = successfulDownloads.Count;

                foreach (var download in successfulDownloads)
                {
                    bool sha1VerificationSuccess = await DecryptAndVerifyODC(download.url, download.fileName, download.expectedSha1);
                    if (sha1VerificationSuccess)
                    {
                        sha1VerifiedCount++;
                        downloadedCount++;
                    }
                    else
                    {
                        sha1FailedCount++;
                        failedDownloads.Add($"SHA1 Verification Failure: {download.url}");
                    }
                    UpdateSha1Status(totalFilesToVerify, sha1VerifiedCount, sha1FailedCount);
                }

                // Log SHA1 verification failures to AuditFails.log
                await File.AppendAllLinesAsync(failedDownloadsFilePath, failedDownloads);

                XMLOutputTextBox.Text += $"\nDownload completed. Total URLs: {totalUrls}, Successful: {downloadedCount}, Failed: {failedCount}\n";
                XMLOutputTextBox.Text += $"\nSHA1 verification completed. Total Files: {totalFilesToVerify}, Successful: {sha1VerifiedCount}, Failed: {sha1FailedCount}\n";
            }
            catch (Exception ex)
            {
                XMLOutputTextBox.Text += $"Error downloading generated URLs: {ex.Message}\n";
            }
        }



        private async Task<bool> DecryptAndVerifyODC(string url, string odcFilePath, string expectedSha1)
        {
            try
            {
                byte[] fileContent = await File.ReadAllBytesAsync(odcFilePath);
                BruteforceProcess proc = new BruteforceProcess(fileContent);
                byte[] decryptedContent = proc.StartBruteForce((int)CdnMode.RETAIL);

                if (decryptedContent != null)
                {
                    string outputDirectory = Path.GetDirectoryName(odcFilePath);
                    string outputPath = Path.Combine(outputDirectory, "OBJECT.ODC");
                    await File.WriteAllBytesAsync(outputPath, decryptedContent);

                    // Delete the original input file
                    File.Delete(odcFilePath);

                    return await VerifyFileSha1(outputPath, expectedSha1);
                }
                else
                {
                    await File.AppendAllLinesAsync("AuditFails.log", new[] { $"Decryption Failure: {url}" });
                    return false;
                }
            }
            catch (Exception ex)
            {
                await File.AppendAllLinesAsync("AuditFails.log", new[] { $"Exception: {url} - {ex.Message}" });
                return false;
            }
        }

        private async Task<bool> DecryptAndVerifySDC(string url, string outputDirectory, string expectedSha1)
        {
            try
            {
                // Extract path and file name
                Uri uri = new Uri(url);
                string path = uri.AbsolutePath.Substring(1); // Remove leading slash
                string fileName = Path.GetFileName(path);
                string directory = Path.GetDirectoryName(path);

                string sdcFilePath = Path.Combine(outputDirectory, directory, fileName);

                byte[] fileContent = await File.ReadAllBytesAsync(sdcFilePath);
                BruteforceProcess proc = new BruteforceProcess(fileContent);
                byte[] decryptedContent = proc.StartBruteForce((int)CdnMode.RETAIL);

                if (decryptedContent != null)
                {
                    string outputPath = Path.Combine(outputDirectory, directory, "Decrypted_" + fileName);
                    await File.WriteAllBytesAsync(outputPath, decryptedContent);

                    return await VerifyFileSha1(outputPath, expectedSha1);
                }
                else
                {
                    await File.AppendAllLinesAsync("AuditFails.log", new[] { $"Decryption Failure: {url}" });
                    return false;
                }
            }
            catch (Exception ex)
            {
                await File.AppendAllLinesAsync("AuditFails.log", new[] { $"Exception: {url} - {ex.Message}" });
                return false;
            }
        }



        private void UpdateODCDownloadStatus(int total, int downloaded, int failed)
        {
            Dispatcher.Invoke(() =>
            {
                var lines = XMLOutputTextBox.Text.Split('\n').ToList();
                string status = $"Total ODC paths found: {total} - Downloaded {downloaded} of {total} - Fails {failed}";
                if (lines.Count > 1 && lines.Last().StartsWith("Total ODC paths found:"))
                {
                    lines[lines.Count - 1] = status;
                }
                else
                {
                    lines.Add(status);
                }
                XMLOutputTextBox.Text = string.Join("\n", lines);
                XMLOutputTextBox.ScrollToEnd();
            });
        }

        private void UpdateSDCDownloadStatus(int total, int downloaded, int failed)
        {
            Dispatcher.Invoke(() =>
            {
                var lines = XMLOutputTextBox.Text.Split('\n').ToList();
                string status = $"Total SDC paths found: {total} - Downloaded {downloaded} of {total} - Fails {failed}";
                if (lines.Count > 1 && lines.Last().StartsWith("Total SDC paths found:"))
                {
                    lines[lines.Count - 1] = status;
                }
                else
                {
                    lines.Add(status);
                }
                XMLOutputTextBox.Text = string.Join("\n", lines);
                XMLOutputTextBox.ScrollToEnd();
            });
        }

        private async Task<bool> VerifyFileSha1(string filePath, string expectedSha1)
        {
            try
            {
                byte[] fileContent = await File.ReadAllBytesAsync(filePath);
                using (SHA1 sha1 = SHA1.Create())
                {
                    byte[] computedHash = sha1.ComputeHash(fileContent);
                    string computedSha1 = BitConverter.ToString(computedHash).Replace("-", "");

                    return computedSha1.Equals(expectedSha1, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }


        private void UpdateSha1Status(int total, int verified, int failed)
        {
            Dispatcher.Invoke(() =>
            {
                var lines = XMLOutputTextBox.Text.Split('\n').ToList();
                string status = $"Total SHA1 checks: {total} - Verified {verified} of {total} - Fails {failed}";
                if (lines.Count > 1 && lines.Last().StartsWith("Total SHA1 checks:"))
                {
                    lines[lines.Count - 1] = status;
                }
                else
                {
                    lines.Add(status);
                }
                XMLOutputTextBox.Text = string.Join("\n", lines);
                XMLOutputTextBox.ScrollToEnd();
            });
        }

        private async Task<bool> DownloadFileWithoutProgressAsync(string url, string fileName)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(10); // Increase timeout to 10 minutes

                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        private async Task DownloadFilesConcurrently(List<Scene> sceneList, string outputDirectory, int maxConcurrentDownloads, Action<Scene, bool> onDownloadComplete)
        {
            using (SemaphoreSlim concurrencySemaphore = new SemaphoreSlim(maxConcurrentDownloads))
            {
                List<Task> downloadTasks = new List<Task>();

                foreach (var scene in sceneList)
                {
                    await concurrencySemaphore.WaitAsync();

                    downloadTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            // Extract path and file name
                            Uri uri = new Uri(scene.Url);
                            string path = uri.AbsolutePath.Substring(1); // Remove leading slash
                            string fileName = Path.GetFileName(path);
                            string directory = Path.GetDirectoryName(path);

                            // Create directory if it doesn't exist
                            string fullDirectoryPath = Path.Combine(outputDirectory, directory);
                            Directory.CreateDirectory(fullDirectoryPath);

                            string decryptedFilePath = Path.Combine(fullDirectoryPath, "Decrypted_" + fileName);
                            if (File.Exists(decryptedFilePath))
                            {
                                // Decrypted file exists, skip download and verify SHA1
                                bool verificationSuccess = await VerifyFileSha1(decryptedFilePath, scene.Sha1);
                                onDownloadComplete(scene, verificationSuccess);
                            }
                            else
                            {
                                // Download file
                                bool downloadSuccess = await DownloadSDCFileAsync(scene.Url, Path.Combine(fullDirectoryPath, fileName));
                                onDownloadComplete(scene, downloadSuccess);
                            }
                        }
                        catch
                        {
                            onDownloadComplete(scene, false);
                        }
                        finally
                        {
                            concurrencySemaphore.Release();
                        }
                    }));
                }

                await Task.WhenAll(downloadTasks);
            }
        }

        private async Task<bool> DownloadSDCFileAsync(string url, string fileName)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(10); // Increase timeout to 10 minutes

                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void txtObjectId_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            txtObjectId.Text = GenerateUUID();
        }


        private async void LoadSceneListXMLv2intoEditorButton_Click(object sender, RoutedEventArgs e)
        {
            Scenes.Clear();
            string xmlPath = SceneListPathURLtextbox2.Text;
            scenesList.ItemsSource = Scenes; // Bind the observable collection to the ItemsControl

            try
            {
                int count = 0;
                foreach (var scene in ParseXmlToSceneList(xmlPath))
                {
                    Scenes.Add(scene);
                    count++;
                    if (count % 2 == 0)
                    {
                        await Task.Delay(1); // Give UI a chance to update and stay responsive
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load XML: " + ex.Message);
            }
        }


        public class SceneListEditor : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyPropertyChanged(string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            private string id;
            public string ID
            {
                get { return id; }
                set { if (id != value) { id = value; NotifyPropertyChanged(nameof(ID)); } }
            }

            private string type;
            public string Type
            {
                get { return type; }
                set { if (type != value) { type = value; NotifyPropertyChanged(nameof(Type)); } }
            }

            private string description;
            public string Description
            {
                get { return description; }
                set { if (description != value) { description = value; NotifyPropertyChanged(nameof(Description)); } }
            }

            private string config;
            public string Config
            {
                get { return config; }
                set { if (config != value) { config = value; NotifyPropertyChanged(nameof(Config)); } }
            }

            private string sceneID;
            public string SceneID
            {
                get { return sceneID; }
                set { if (sceneID != value) { sceneID = value; NotifyPropertyChanged(nameof(SceneID)); } }
            }

            private string version;
            public string Version
            {
                get { return version; }
                set { if (version != value) { version = value; NotifyPropertyChanged(nameof(Version)); } }
            }

            private string sha1;
            public string SHA1
            {
                get { return sha1; }
                set { if (sha1 != value) { sha1 = value; NotifyPropertyChanged(nameof(SHA1)); } }
            }

            private string flags;
            public string Flags
            {
                get { return flags; }
                set { if (flags != value) { flags = value; NotifyPropertyChanged(nameof(Flags)); } }
            }

            private string softcap;
            public string Softcap
            {
                get { return softcap; }
                set { if (softcap != value) { softcap = value; NotifyPropertyChanged(nameof(Softcap)); } }
            }

            private string capacity;
            public string Capacity
            {
                get { return capacity; }
                set { if (capacity != value) { capacity = value; NotifyPropertyChanged(nameof(Capacity)); } }
            }

            private string host;
            public string Host
            {
                get { return host; }
                set { if (host != value) { host = value; NotifyPropertyChanged(nameof(Host)); } }
            }

            private string homeUID;
            public string HomeUID
            {
                get { return homeUID; }
                set { if (homeUID != value) { homeUID = value; NotifyPropertyChanged(nameof(HomeUID)); } }
            }
        }

        public static IEnumerable<SceneListEditor> ParseXmlToSceneList(string xmlPath)
        {
            using (XmlReader reader = XmlReader.Create(xmlPath))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "SCENE")
                    {
                        yield return new SceneListEditor
                        {
                            ID = reader.GetAttribute("ID") ?? reader.GetAttribute("Name"),
                            Type = reader.GetAttribute("Type"),
                            Description = reader.GetAttribute("desc"),
                            Config = reader.GetAttribute("config"),
                            SceneID = reader.GetAttribute("SceneID") ?? reader.GetAttribute("ChannelID"),
                            Version = reader.GetAttribute("version"),
                            SHA1 = reader.GetAttribute("sha1"),
                            Flags = reader.GetAttribute("flags"),
                            Softcap = reader.GetAttribute("softcap"),
                            Capacity = reader.GetAttribute("capacity"),
                            Host = reader.GetAttribute("host"),
                            HomeUID = reader.GetAttribute("HOMEUID")
                        };
                    }
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                textBox.Foreground = Brushes.Yellow;

                var unsavedChangesTextBlock = this.FindName("UnsavedSceneListChanges") as TextBlock;
                if (unsavedChangesTextBlock != null)
                {
                    unsavedChangesTextBlock.Visibility = Visibility.Visible;
                }
            }
        }

        private void AddNewScenelistItem_Button_Click(object sender, RoutedEventArgs e)
        {
            var newScene = new SceneListEditor
            {
                ID = txtSdcName.Text,
                SceneID = txtChannelID.Text,
                Type = (txtSceneType.SelectedItem as ComboBoxItem)?.Content.ToString(),
                Host = (txtdHost.SelectedItem as ComboBoxItem)?.Content.ToString(),
                Config = txtsceneconfig.Text,
                Description = txtSdcPath.Text,
                SHA1 = txtSdcsha1Digest.Text,
                Version = txtSDCversion.Text,
                Flags = txtflags.Text,
                HomeUID = txthomeuuid.Text
            };

            Scenes.Insert(0, newScene); // Add the new scene at the beginning of the ObservableCollection

            var unsavedChangesTextBlock = this.FindName("UnsavedSceneListChanges") as TextBlock;
            if (unsavedChangesTextBlock != null)
            {
                unsavedChangesTextBlock.Visibility = Visibility.Visible;
            }
        }

        private async void SaveSceneListXmlButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset the text color of all TextBox elements to white
            ResetTextBoxColors();

            DeploySceneListFlag = false;

            // Define the order for sorting and group names for comments
            var typeOrder = new List<string>
    {
        "PublicLobby", "GlobalSpace", "ArcadeSpace", "Game16Space", "Game32Space", "Game60Space",
        "ClubHouse", "PublicNoVideo", "Home", "PrivateNoVideo"
    };

            // Sort the Scenes collection according to the specified order
            var sortedScenes = Scenes
                .OrderBy(scene =>
                {
                    // Find the index of the scene type in the typeOrder list
                    // If not found, give it a high index to sort it later
                    int index = typeOrder.FindIndex(t => string.Equals(t, scene.Type, StringComparison.OrdinalIgnoreCase));
                    if (index == -1)
                    {
                        // Special handling for types containing "Home" but not exactly "Home"
                        if (scene.Type.Contains("Home", StringComparison.OrdinalIgnoreCase) && !string.Equals(scene.Type, "Home", StringComparison.OrdinalIgnoreCase))
                            return typeOrder.IndexOf("Home");
                        return int.MaxValue; // Put unknown types at the end
                    }
                    return index;
                })
                .ThenBy(scene => scene.ID) // Additional sorting by ID if needed
                .ToList();

            // Create a new XElement for the SCENELIST
            var sceneListElement = new XElement("SCENELIST");

            // Track the current group for inserting comments
            string currentGroup = null;

            foreach (var scene in sortedScenes)
            {
                // Determine the group name (type) in a case-insensitive manner
                string sceneType = typeOrder.FirstOrDefault(t => string.Equals(t, scene.Type, StringComparison.OrdinalIgnoreCase)) ?? "Other";

                // If the group changes, add a comment
                if (sceneType != currentGroup)
                {
                    currentGroup = sceneType;
                    sceneListElement.Add(new XComment($" {currentGroup} "));
                }

                // Add the scene element
                sceneListElement.Add(new XElement("SCENE",
                    string.IsNullOrEmpty(scene.ID) ? null : new XAttribute("ID", scene.ID),
                    string.IsNullOrEmpty(scene.Type) ? null : new XAttribute("Type", scene.Type),
                    string.IsNullOrEmpty(scene.Description) ? null : new XAttribute("desc", scene.Description),
                    string.IsNullOrEmpty(scene.Config) ? null : new XAttribute("config", scene.Config),
                    string.IsNullOrEmpty(scene.SceneID) ? null : new XAttribute("SceneID", scene.SceneID),
                    string.IsNullOrEmpty(scene.Version) ? null : new XAttribute("version", scene.Version),
                    string.IsNullOrEmpty(scene.SHA1) ? null : new XAttribute("sha1", scene.SHA1),
                    string.IsNullOrEmpty(scene.Flags) ? null : new XAttribute("flags", scene.Flags),
                    string.IsNullOrEmpty(scene.Softcap) ? null : new XAttribute("softcap", scene.Softcap),
                    string.IsNullOrEmpty(scene.Capacity) ? null : new XAttribute("capacity", scene.Capacity),
                    string.IsNullOrEmpty(scene.Host) ? null : new XAttribute("host", scene.Host),
                    string.IsNullOrEmpty(scene.HomeUID) ? null : new XAttribute("HOMEUID", scene.HomeUID)
                ));
            }

            // Create the new XML document with the SCENELIST element
            var doc = new XDocument(sceneListElement);

            try
            {
                // Get the full path from SceneListPathURLtextbox2
                string fullPath = SceneListPathURLtextbox2.Text;

                // Extract the filename from the full path
                string filename = Path.GetFileName(fullPath);
                if (string.IsNullOrEmpty(filename))
                {
                    MessageBox.Show("Failed to extract the filename from the path specified in SceneListPathURLtextbox2.");
                    return;
                }

                // Get the save directory path from SceneListSavePathtextbox
                string saveDirectory = SceneListSavePathtextbox.Text.Trim();

                // Ensure the save directory is valid
                if (string.IsNullOrWhiteSpace(saveDirectory))
                {
                    MessageBox.Show("Please specify a valid directory path to save the SceneList XML.");
                    return;
                }

                // Combine the directory path and the filename correctly
                string savePath = Path.Combine(saveDirectory.TrimEnd('\\', '/'), filename);

                // Save the plain XML document to the constructed path
                doc.Save(savePath);

                // Compute the SHA1 hash of the XML content
                string sha1Hash = ComputeSHA1(savePath);

                // Display the SHA1 hash in the textbox
                LatestSceneListSHA1textbox.Text = sha1Hash;

                // Change the visibility of the panel to visible
                LatestSceneListSHA1panel.Visibility = Visibility.Visible;

                // Define the output file path for encryption
                string exeLocation = System.AppDomain.CurrentDomain.BaseDirectory;
                string outputFilePath = System.IO.Path.Combine(exeLocation, "Output", "ToDeploy", filename);

                // Check if encryption is enabled
                if (CheckBoxEncryptSceneList.IsChecked == true)
                {
                    // Encrypt the saved XML file
                    bool encryptionSuccess = await EncryptAndSaveFileAsync(savePath, outputFilePath);

                    if (encryptionSuccess)
                    {
                        var unsavedChangesTextBlock = this.FindName("UnsavedSceneListChanges") as TextBlock;
                        if (unsavedChangesTextBlock != null)
                        {
                            unsavedChangesTextBlock.Visibility = Visibility.Collapsed;
                        }
                        MessageBox.Show("XML saved and encrypted successfully.");
                        // Call the method to save TSS XML
                        LoadTSSXMLButton_Click(sender, e);
                    }
                    else
                    {
                        MessageBox.Show("Failed to encrypt XML");
                    }
                }
                else
                {
                    // Copy the file without encryption
                    var unsavedChangesTextBlock = this.FindName("UnsavedSceneListChanges") as TextBlock;
                    if (unsavedChangesTextBlock != null)
                    {
                        unsavedChangesTextBlock.Visibility = Visibility.Collapsed;
                    }
                    File.Copy(savePath, outputFilePath, overwrite: true);
                    MessageBox.Show("XML saved successfully without encryption.");
                    // Call the method to save TSS XML
                    LoadTSSXMLButton_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save XML: " + ex.Message);
            }
        }


        private void ResetTextBoxColors()
        {
            foreach (var item in scenesList.Items)
            {
                var container = scenesList.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                if (container != null)
                {
                    var textBoxes = FindVisualChildren<TextBox>(container);
                    foreach (var textBox in textBoxes)
                    {
                        textBox.Foreground = Brushes.White;
                    }
                }
            }
        }

        private IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }


        public async Task<bool> EncryptAndSaveFileAsync(string filePath, string outputFilePath)
        {
            bool isProcessed = true;
            string filename = Path.GetFileName(filePath);
            try
            {
                byte[] fileContent = await File.ReadAllBytesAsync(filePath);
                byte[] encryptedContent = null;
                string inputSHA1 = "";

                // Generate SHA1 hash from file content before encryption
                using (SHA1 sha1 = SHA1.Create())
                {
                    byte[] SHA1Data = sha1.ComputeHash(fileContent);
                    inputSHA1 = BitConverter.ToString(SHA1Data).Replace("-", "").ToLowerInvariant();
                    LogDebugInfo($"Input SHA1 for {filename}: {inputSHA1}");

                    // Encrypt the file content
                    string computedSha1 = inputSHA1.Substring(0, 16);
                    encryptedContent = CDSProcess.CDSEncrypt_Decrypt(fileContent, computedSha1, (int)CdnMode.RETAIL);
                }

             
                if (encryptedContent != null)
                {
                    // Ensure the output directory exists
                    string outputDirectory = Path.GetDirectoryName(outputFilePath);
                    if (!Directory.Exists(outputDirectory))
                    {
                        Directory.CreateDirectory(outputDirectory);
                        LogDebugInfo($"Output directory {outputDirectory} created.");
                    }
                    await File.WriteAllBytesAsync(outputFilePath, encryptedContent);
                    LogDebugInfo($"File {filename} encrypted and written to {outputFilePath}.");
                }
                else
                {
                    isProcessed = false;
                    LogDebugInfo($"Encryption failed for {filename}, no data written.");
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error encrypting {filename}: {ex.Message}");
                isProcessed = false;
            }
            return isProcessed;
        }


        private string ComputeSHA1(string filePath)
        {
            using (var sha1 = System.Security.Cryptography.SHA1.Create())
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    byte[] hash = sha1.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
                }
            }
        }

        private void DeleteSceneListXMLLineButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            SceneListEditor itemToRemove = button.DataContext as SceneListEditor;
            if (itemToRemove != null)
            {
                Scenes.Remove(itemToRemove); // Removes the item from the ObservableCollection
                var unsavedChangesTextBlock = this.FindName("UnsavedSceneListChanges") as TextBlock;
                if (unsavedChangesTextBlock != null)
                {
                    unsavedChangesTextBlock.Visibility = Visibility.Visible;
                }
            }

        }

        private async void LoadTSSXMLButton_Click(object sender, RoutedEventArgs e)
        {
            string pathOrUrl = TSSURLtextbox.Text;  // Retrieve the path or URL from the textbox
            string xmlString = null;

            try
            {
                if (Uri.TryCreate(pathOrUrl, UriKind.Absolute, out var uriResult) &&
                    (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                {
                    using (var client = new HttpClient())
                    {
                        var response = await client.GetAsync(pathOrUrl);
                        if (response.IsSuccessStatusCode)
                        {
                            xmlString = await response.Content.ReadAsStringAsync();
                        }
                        else
                        {
                            MessageBox.Show("Failed to load XML from URL: " + response.StatusCode);
                            return;
                        }
                    }
                }
                else
                {
                    if (File.Exists(pathOrUrl))
                    {
                        xmlString = File.ReadAllText(pathOrUrl);
                    }
                    else
                    {
                        MessageBox.Show("File does not exist: " + pathOrUrl);
                        return;
                    }
                }

                if (!string.IsNullOrEmpty(xmlString))
                {
                    loadedXmlData = XElement.Parse(xmlString);
                    GenerateXmlControls(loadedXmlData);

                    string exeLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    string savePath = Path.Combine(exeLocation, "TemporaryTSS.xml");
                    File.WriteAllText(savePath, xmlString);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error processing XML: " + ex.Message);
            }
        }

        private void SaveNewTSSXMLButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (loadedXmlData == null)
                {
                    MessageBox.Show("No TSS loaded, cannot save.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                // Get the directory path from the textbox and append "tss" subfolder
                string originalDirectoryPath = TSSeditorSavePathtextbox.Text;
                string directoryPath = Path.Combine(originalDirectoryPath, "tss");

                if (string.IsNullOrWhiteSpace(directoryPath))
                {
                    MessageBox.Show("Please specify a valid directory path.");
                    return;
                }

                // Ensure the directory exists
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string defaultFileName = "coreHztFmpQrx0002_en-US.xml";
                string baseFileName = Path.GetFileNameWithoutExtension(defaultFileName);
                string fileExtension = Path.GetExtension(defaultFileName);
                string basePath = Path.Combine(directoryPath, defaultFileName);

                // Update the VERSION field with the current date and time
                var versionElement = loadedXmlData.Descendants("VERSION").FirstOrDefault();
                if (versionElement != null)
                {
                    versionElement.Value = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt").ToUpper();
                }

                if (CheckBoxTSSAllregions.IsChecked == true)
                {
                    string[] regionCodes = {
                // List of region codes
                "en-GB", "fr-FR", "it-IT", "de-DE", "es-ES", "ja-JP", "ko-KR", "zh-TW", "zh-HK", "en-SG", "en-ID", "en-MY", "en-TH",
"af-ZA", "ar-AE", "ar-BH", "ar-DZ", "ar-EG", "ar-IQ", "ar-JO", "ar-KW", "ar-LB", "ar-LY", "ar-MA", "ar-OM", "ar-QA",
"ar-SA", "ar-SY", "ar-TN", "ar-YE", "az-AZ", "be-BY", "bg-BG", "bs-BA", "ca-ES", "cs-CZ", "cy-GB", "da-DK", "de-AT",
"de-CH", "de-LI", "de-LU", "dv-MV", "el-GR", "en-AU", "en-BZ", "en-CA", "en-CB", "en-IE", "en-JM", "en-NZ", "en-PH",
"en-TT", "en-ZA", "en-ZW", "es-AR", "es-BO", "es-CL", "es-CO", "es-CR", "es-DO", "es-EC", "es-GT", "es-HN", "es-MX",
"es-NI", "es-PA", "es-PE", "es-PR", "es-PY", "es-SV", "es-UY", "es-VE", "et-EE", "eu-ES", "fa-IR", "fi-FI", "fo-FO",
"fr-BE", "fr-CA", "fr-CH", "fr-LU", "fr-MC", "gl-ES", "gu-IN", "he-IL", "hi-IN", "hr-BA", "hr-HR", "hu-HU", "hy-AM",
"id-ID", "is-IS", "it-CH", "ka-GE", "kk-KZ", "kn-IN", "ky-KG", "lt-LT", "lv-LV", "mi-NZ", "mk-MK", "mn-MN", "mr-IN",
"ms-BN", "ms-MY", "mt-MT", "nb-NO", "nl-BE", "nl-NL", "nn-NO", "ns-ZA", "pa-IN", "pl-PL", "ps-AR", "pt-BR", "pt-PT",
"qu-BO", "qu-EC", "qu-PE", "ro-RO", "ru-RU", "sa-IN", "se-FI", "se-NO", "se-SE", "sk-SK", "sl-SI", "sq-AL", "sr-BA",
"sr-SP", "sv-FI", "sv-SE", "sw-KE", "ta-IN", "te-IN", "th-TH", "tl-PH", "tn-ZA", "tr-TR", "tt-RU", "uk-UA", "ur-PK",
"uz-UZ", "vi-VN", "xh-ZA", "zh-CN", "zh-MO", "zh-SG", "en-DK", "en-FI", "en-NO", "no-NO", "en-SE", "en-AE", "en-CZ",
"en-SA", "en-PL", "en-GR", "en-HK", "en-TW", "en-TR", "en-RO", "en-QA", "en-HU", "en-IS", "en-BG", "en-HR", "en-US",
"en-DK", "en-FI", "en-NO", "no-NO", "en-SE", "en-AE", "en-CZ", "en-SA", "en-PL", "en-GR", "en-HK", "en-TW", "en-TR",
"en-RO", "en-QA", "en-HU", "en-IS", "en-BG", "en-HR", "en-DZ", "en-AF", "en-AL", "en-AD", "en-AO", "en-AG", "en-AR",
"en-AM", "en-AT", "en-AZ", "en-BD", "en-BB", "en-BJ", "en-BT", "en-CV", "en-CF", "en-TD", "en-CG", "en-CD", "en-DJ",
"en-DM", "en-GQ", "en-ER", "en-GA", "en-GM", "en-GN", "en-GW", "en-GY", "en-HT", "en-KI", "en-LS", "en-LR", "en-MG",
"en-MW", "en-MV", "en-ML", "en-MR", "en-MU", "en-MC", "en-ME", "en-MZ", "en-MM", "en-NA", "en-NR", "en-NP", "en-NE",
"en-PW", "en-PA", "en-PG", "en-ST", "en-SM", "en-SN", "en-RS", "en-SC", "en-SL", "en-SB", "en-LK", "en-SD", "en-SR",
"en-SZ", "en-SY", "en-TJ", "en-TZ", "en-TL", "en-TG", "en-TO", "en-TM", "en-TV", "en-UG", "en-VU"

            };

                    baseFileName = baseFileName.Replace("_en-US", "");

                    foreach (var code in regionCodes)
                    {
                        string regionalFileName = $"{baseFileName}_{code}{fileExtension}";
                        string fullRegionalPath = Path.Combine(directoryPath, regionalFileName);
                        loadedXmlData.Save(fullRegionalPath);
                    }

                    // Set the visibility of the warning text block to collapsed
                    TextBlock tssSaveWarningTextblock = (TextBlock)FindName("TSSsaveWarningTextblock");
                    if (tssSaveWarningTextblock != null)
                    {
                        tssSaveWarningTextblock.Visibility = Visibility.Collapsed;
                    }

                    LoadTSSXMLButton_Click(sender, e);
                }
                else
                {
                    loadedXmlData.Save(basePath);

                    // Set the visibility of the warning text block to collapsed
                    TextBlock tssSaveWarningTextblock = (TextBlock)FindName("TSSsaveWarningTextblock");
                    if (tssSaveWarningTextblock != null)
                    {
                        tssSaveWarningTextblock.Visibility = Visibility.Collapsed;
                    }

                    LoadTSSXMLButton_Click(sender, e);
                }

                // Extra stuff if DeployHCDBFlag is true
                if (DeployHCDBFlag)
                {
                    string hcdbOutputDirectory = HcdbOutputDirectoryTextBox.Text;
                    string databaseEditsPath = Path.Combine(hcdbOutputDirectory, "Database_Edits");
                    string[] filesToMove = {
                "ObjectCatalogue_5_SCEAsia.hcdb",
                "ObjectCatalogue_5_SCEJ.hcdb",
                "ObjectCatalogue_5_SCEA.hcdb",
                "ObjectCatalogue_5_SCEE.hcdb"
            };

                    string targetDirectoryPath = Path.Combine(TSSeditorSavePathtextbox.Text, "Objects");

                    // Ensure the target directory exists
                    if (!Directory.Exists(targetDirectoryPath))
                    {
                        Directory.CreateDirectory(targetDirectoryPath);
                    }

                    foreach (var fileName in filesToMove)
                    {
                        string sourceFilePath = Path.Combine(databaseEditsPath, fileName);
                        string targetFilePath = Path.Combine(targetDirectoryPath, fileName);

                        if (File.Exists(sourceFilePath))
                        {
                            File.Copy(sourceFilePath, targetFilePath, true);
                            File.Delete(sourceFilePath);
                        }
                    }

                    // Reset the flag
                    DeployHCDBFlag = false;

                    // UI updates using Dispatcher.Invoke
                    Dispatcher.Invoke(() =>
                    {
                        // Find the LatestHCDBSHA1panel and set its visibility to Collapsed
                        var latestHCDBSHA1panel = (StackPanel)FindName("LatestHCDBSHA1panel");
                        if (latestHCDBSHA1panel != null)
                        {
                            latestHCDBSHA1panel.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            MessageBox.Show("LatestHCDBSHA1panel not found!");
                        }

                        // Clear the LatestHCDBSHA1textbox
                        var latestHCDBSHA1textbox = (TextBox)FindName("LatestHCDBSHA1textbox");
                        if (latestHCDBSHA1textbox != null)
                        {
                            latestHCDBSHA1textbox.Clear();
                        }
                        else
                        {
                            MessageBox.Show("LatestHCDBSHA1textbox not found!");
                        }

                       
                    });
                }

                // Extra stuff if DeploySceneListFlag is true
                if (DeploySceneListFlag)
                {
                    string sourceSceneListPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Output", "ToDeploy", "SceneList.xml");
                    string targetSceneListPath = Path.Combine(TSSeditorSavePathtextbox.Text, "Environments", "SceneList.xml");

                    // Ensure the target directory exists
                    string targetSceneListDirectory = Path.GetDirectoryName(targetSceneListPath);
                    if (!Directory.Exists(targetSceneListDirectory))
                    {
                        Directory.CreateDirectory(targetSceneListDirectory);
                    }

                    if (File.Exists(sourceSceneListPath))
                    {
                        File.Move(sourceSceneListPath, targetSceneListPath, true);
                        
                    }
                    // Find the LatestSceneListSHA1panel and set its visibility to Collapsed
                    var latestSceneListSHA1panel = (StackPanel)FindName("LatestSceneListSHA1panel");
                    if (latestSceneListSHA1panel != null)
                    {
                        latestSceneListSHA1panel.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        MessageBox.Show("LatestSceneListSHA1panel not found!");
                    }

                    // Clear the LatestSceneListSHA1textbox
                    var latestSceneListSHA1textbox = (TextBox)FindName("LatestSceneListSHA1textbox");
                    if (latestSceneListSHA1textbox != null)
                    {
                        latestSceneListSHA1textbox.Clear();
                    }
                    else
                    {
                        MessageBox.Show("LatestSceneListSHA1textbox not found!");
                    }
                    DeploySceneListFlag = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save XML data: " + ex.Message);
            }
        }




        private void GenerateXmlControls(XElement xmlElement)
        {
            TSSXMLControlsPanel.Children.Clear();

            // Handle specific tags
            var versionElement = xmlElement.Element("VERSION");
            if (versionElement != null) { GenerateVersionPanel(versionElement); }
            GenerateSHA1Section(xmlElement);
            var connectionElement = xmlElement.Element("connection");
            if (connectionElement != null) { GenerateConnectionPanel(connectionElement); }

            GenerateSecureRootUrlsPanel(xmlElement);

            var dnsOverrides = xmlElement.Elements("DNSOverride");
            if (dnsOverrides.Any()) { GenerateDNSOverridePanel(xmlElement); }

            // New elements handling
            GenerateAdminObjectIdPanel(xmlElement);
            GenerateMaxServiceIdsPanel(xmlElement);
            GenerateDisableBarPanel(xmlElement);
            GenerateSecureCommercePointsPanel(xmlElement);
            GenerateUseRegionalServiceIdsPanel(xmlElement);
            GenerateHttpCompressionControl(xmlElement);

            var objectsElement = xmlElement.Element("objects");
            if (objectsElement != null) { GenerateObjectsPanel(objectsElement); }

           

            var ssfwElement = xmlElement.Element("ssfw");
            if (ssfwElement != null) { GenerateSSFWPanel(ssfwElement); }

            var profanityFilterElement = xmlElement.Element("profanityfilter");
            if (profanityFilterElement != null) { GenerateProfanityFilterPanel(profanityFilterElement); }

            var dataCaptureElement = xmlElement.Element("datacapture");
            if (dataCaptureElement != null) { GenerateDataCapturePanel(dataCaptureElement); }

            GenerateSceneRedirectPanel(xmlElement);

            var ageRestrictionsElement = xmlElement.Element("agerestrictions");
            if (ageRestrictionsElement != null) { GenerateAgeRestrictionsPanel(ageRestrictionsElement); }

            var globalElement = xmlElement.Element("global");
            if (globalElement != null) { GenerateGlobalPanel(globalElement); }

            // Handle REGIONINFO and its sub-elements
            var regionInfoElement = xmlElement.Element("REGIONINFO");
            if (regionInfoElement != null)
            {
                GenerateInstanceTypesPanel(regionInfoElement.Element("INSTANCE_TYPES"));
                GenerateRegionTypesPanel(regionInfoElement.Element("REGION_TYPES"));
                GenerateRegionMapPanel(regionInfoElement.Element("REGION_MAP"));
                GenerateLocalisationsPanel(regionInfoElement.Element("LOCALISATIONS"));
            }

            // Define a set of element names to exclude from normal XML element processing
            var excludeElements = new HashSet<string> {
        "SHA1", "datacapture", "VERSION", "objects", "profanityfilter", "SecureContentRoot",
        "ScreenContentRoot", "secure_lua_object_resources_root", "secure_lua_scene_resources_root",
        "SceneRedirect", "ssfw", "disablebar", "useregionalserviceids", "http_compression", "secure_commerce_points", "commerce", "connection", "REGIONINFO", "DNSOverride", "global", "agerestrictions",
        "AdminObjectId", "maxserviceids"
    };

            // Process other elements
            GenerateXmlElementControls(xmlElement, TSSXMLControlsPanel, excludeElements);
        }

        private void GenerateHttpCompressionControl(XElement xmlElement)
        {
            // Create or retrieve the http_compression element
            var httpCompressionElement = xmlElement.Element("http_compression");

            // Main vertical panel to hold each setting on a new line
            StackPanel mainPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 5, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            // Panel for HTTP Compression and its radio buttons
            StackPanel compressionPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center
            };

            compressionPanel.Children.Add(new TextBlock
            {
                Text = "Enable HTTP Compression: ",
                FontSize = 14,
                Width = 180,
                Margin = new Thickness(20, 0, 5, 0),
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center
            });

            // Add the "True" label and radio button
            compressionPanel.Children.Add(new TextBlock
            {
                Text = "True:",
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center
            });

            RadioButton trueButton = new RadioButton
            {
                IsChecked = (httpCompressionElement?.Attribute("encodings")?.Value == "gzip"),
                Margin = new Thickness(5, 0, 20, 0),
                GroupName = "http_compression",
                Style = (Style)FindResource("ModernRadioButtonStyle"),
                Tag = httpCompressionElement
            };
            trueButton.Checked += (sender, args) =>
            {
                if (trueButton.Tag is XElement element)
                {
                    // Remove existing http_compression element if it exists
                    var existingElement = xmlElement.Element("http_compression");
                    if (existingElement != null)
                    {
                        existingElement.Remove();
                    }

                    // Add a new http_compression element
                    XElement newElement = new XElement("http_compression");
                    newElement.SetAttributeValue("encodings", "gzip");
                    xmlElement.Add(newElement);
                }
            };
            compressionPanel.Children.Add(trueButton);

            // Add the "False" label and radio button
            compressionPanel.Children.Add(new TextBlock
            {
                Text = "False:",
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center
            });

            RadioButton falseButton = new RadioButton
            {
                IsChecked = (httpCompressionElement == null || httpCompressionElement.Attribute("encodings") == null),
                Margin = new Thickness(5, 0, 0, 0),
                GroupName = "http_compression",
                Style = (Style)FindResource("ModernRadioButtonStyle"),
                Tag = httpCompressionElement
            };
            falseButton.Checked += (sender, args) =>
            {
                if (falseButton.Tag is XElement element)
                {
                    // Remove the http_compression element if it exists
                    var existingElement = xmlElement.Element("http_compression");
                    if (existingElement != null)
                    {
                        existingElement.Remove();
                    }
                }
            };
            compressionPanel.Children.Add(falseButton);

            // Add the compression panel to the main panel
            mainPanel.Children.Add(compressionPanel);

            // Add the main panel to the controls panel
            TSSXMLControlsPanel.Children.Add(mainPanel);
        }

        private void GenerateAdminObjectIdPanel(XElement xmlElement)
        {
            var adminObjectIdElement = xmlElement.Element("AdminObjectId");
            if (adminObjectIdElement != null)
            {
                // Main vertical panel to hold each setting on a new line
                StackPanel mainPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(0, 15, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                // Panel for "Other Settings" label
                StackPanel otherSettingsPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center
                };
                otherSettingsPanel.Children.Add(new TextBlock
                {
                    Text = "Other Settings",
                    FontSize = 16,
                    Foreground = new SolidColorBrush(Colors.White),
                    Margin = new Thickness(0, 0, 5, 7),
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center
                });

                // Adding the "Other Settings" panel to the main panel
                mainPanel.Children.Add(otherSettingsPanel);

                // Panel for "Admin IGA UUID" and its textbox
                StackPanel adminIdPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                adminIdPanel.Children.Add(new TextBlock
                {
                    Text = "In-Game-Admin UUID: ",
                    FontSize = 14,
                    Width = 180,
                    Margin = new Thickness(20, 0, 0, 7),
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Colors.White),
                    VerticalAlignment = VerticalAlignment.Center
                });

                var adminObjectIdTextBox = new TextBox
                {
                    Text = adminObjectIdElement.Value,
                    FontSize = 14,
                    Width = 300,
                    Style = (Style)FindResource("SmallTextBoxStyle"),
                    Margin = new Thickness(0, 0, 10, 7),
                    VerticalAlignment = VerticalAlignment.Center,
                    Tag = adminObjectIdElement
                };
                adminObjectIdTextBox.TextChanged += TSSTextBox_TextChanged;
                adminIdPanel.Children.Add(adminObjectIdTextBox);

                // Adding the Admin ID panel to the main panel
                mainPanel.Children.Add(adminIdPanel);

                // Add the main panel to the controls panel
                TSSXMLControlsPanel.Children.Add(mainPanel);
            }
        }

        private void GenerateMaxServiceIdsPanel(XElement xmlElement)
        {
            var maxServiceIdsElement = xmlElement.Element("maxserviceids");
            if (maxServiceIdsElement != null)
            {
                StackPanel maxServiceIdsPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                maxServiceIdsPanel.Children.Add(new TextBlock
                {
                    Text = "Max Service IDs: ",
                    FontSize = 14,
                    Width = 180,
                    Margin = new Thickness(20, 0, 0, 0),
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Colors.White),
                    VerticalAlignment = VerticalAlignment.Center
                });

                var maxServiceIdsTextBox = new TextBox
                {
                    Text = maxServiceIdsElement.Value,
                    FontSize = 14,
                    Width = 40,
                    Style = (Style)FindResource("SmallTextBoxStyle"),
                    Margin = new Thickness(0, 0, 10, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Tag = maxServiceIdsElement
                };
                maxServiceIdsTextBox.TextChanged += TSSTextBox_TextChanged;
                maxServiceIdsPanel.Children.Add(maxServiceIdsTextBox);

                TSSXMLControlsPanel.Children.Add(maxServiceIdsPanel);
            }
        }




        private void GenerateRegionMapPanel(XElement xmlElement)
        {
            // Main vertical panel to hold the header and items
            StackPanel mainPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 7, 0, 7),
                VerticalAlignment = VerticalAlignment.Center
            };

            // Header for the region map
            TextBlock header = new TextBlock
            {
                Text = "Region Map",
                FontSize = 16,
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 7)
            };
            mainPanel.Children.Add(header);

            // WrapPanel for map items
            WrapPanel regionMapPanel = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                Width = 1340, // Set the maximum width for the WrapPanel (adjust as needed)
                Margin = new Thickness(0)
            };

            var mapElements = xmlElement.Elements("MAP");
            foreach (var map in mapElements)
            {
                // Panel for each map item
                StackPanel mapPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(2),
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 435 // Set a fixed width for each item to fit three items per row
                };

                // Code label and textbox
                mapPanel.Children.Add(new TextBlock
                {
                    Text = "Code: ",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Colors.White),
                    Margin = new Thickness(20, 0, 5, 0),
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center
                });
                var codeTextBox = new TextBox
                {
                    Text = map.Attribute("code")?.Value ?? "Not found",
                    FontSize = 14,
                    Width = 70,
                    Style = (Style)FindResource("SmallTextBoxStyle"),
                    Margin = new Thickness(0, 0, 10, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Tag = map.Attribute("code")
                };
                codeTextBox.TextChanged += TSSTextBox_TextChanged;
                mapPanel.Children.Add(codeTextBox);

                // Location label and textbox
                mapPanel.Children.Add(new TextBlock
                {
                    Text = "Location: ",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Colors.White),
                    Margin = new Thickness(0, 0, 5, 0),
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center
                });
                var locTextBox = new TextBox
                {
                    Text = map.Attribute("loc")?.Value ?? "Not found",
                    FontSize = 14,
                    Width = 30,
                    Style = (Style)FindResource("SmallTextBoxStyle"),
                    Margin = new Thickness(0, 0, 10, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Tag = map.Attribute("loc")
                };
                locTextBox.TextChanged += TSSTextBox_TextChanged;
                mapPanel.Children.Add(locTextBox);

                // Region label and textbox
                mapPanel.Children.Add(new TextBlock
                {
                    Text = "Map: ",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Colors.White),
                    Margin = new Thickness(0, 0, 5, 0),
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center
                });
                var mapTextBox = new TextBox
                {
                    Text = map.Value,
                    FontSize = 14,
                    Width = 120,
                    Style = (Style)FindResource("SmallTextBoxStyle"),
                    VerticalAlignment = VerticalAlignment.Center,
                    Tag = map
                };
                mapTextBox.TextChanged += TSSTextBox_TextChanged;
                mapPanel.Children.Add(mapTextBox);

                // Add the mapPanel to the WrapPanel
                regionMapPanel.Children.Add(mapPanel);
            }

            // Add the WrapPanel to the main panel
            mainPanel.Children.Add(regionMapPanel);

            // Add the main panel to the controls panel
            TSSXMLControlsPanel.Children.Add(mainPanel);
        }

        // Event handler for all TextBox changes
        private void TSSTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            if (textBox.Tag is XAttribute attribute)
            {
                attribute.Value = textBox.Text;
            }
            else if (textBox.Tag is XElement element)
            {
                element.Value = textBox.Text;
            }
        }


        private void GenerateLocalisationsPanel(XElement xmlElement)
        {
            // Main vertical panel to hold header and items
            StackPanel mainPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 7, 0, 7),
                VerticalAlignment = VerticalAlignment.Center
            };

            // Header for the localisations
            TextBlock header = new TextBlock
            {
                Text = "Localisations",
                FontSize = 16,
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 7)
            };
            mainPanel.Children.Add(header);

            // WrapPanel for localisations items
            WrapPanel localisationsPanel = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                Width = 1300, // Set the maximum width for the WrapPanel
                Margin = new Thickness(0)
            };

            var refElements = xmlElement.Elements("REF");
            foreach (var refEl in refElements)
            {
                // Panel for each reference item
                StackPanel refPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(2),
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 320 // Set a fixed width for each item to fit four items per row
                };

                // Language label and textbox
                refPanel.Children.Add(new TextBlock
                {
                    Text = "Language: ",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Colors.White),
                    Margin = new Thickness(10, 0, 5, 0),
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center
                });
                var languageTextBox = new TextBox
                {
                    Text = refEl.Attribute("language")?.Value ?? "Not found",
                    FontSize = 14,
                    Width = 60,
                    Style = (Style)FindResource("SmallTextBoxStyle"),
                    Margin = new Thickness(0, 0, 10, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Tag = refEl.Attribute("language")
                };
                languageTextBox.TextChanged += TSSTextBox_TextChanged;
                refPanel.Children.Add(languageTextBox);

                // Reference label and textbox
                refPanel.Children.Add(new TextBlock
                {
                    Text = "References: ",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Colors.White),
                    Margin = new Thickness(0, 0, 5, 0),
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center
                });
                var refTextBox = new TextBox
                {
                    Text = refEl.Value,
                    FontSize = 14,
                    Width = 60,
                    Style = (Style)FindResource("SmallTextBoxStyle"),
                    VerticalAlignment = VerticalAlignment.Center,
                    Tag = refEl
                };
                refTextBox.TextChanged += TSSTextBox_TextChanged;
                refPanel.Children.Add(refTextBox);

                // Add the refPanel to the WrapPanel
                localisationsPanel.Children.Add(refPanel);
            }

            // Add the WrapPanel to the main panel
            mainPanel.Children.Add(localisationsPanel);

            // Add the main panel to the controls panel
            TSSXMLControlsPanel.Children.Add(mainPanel);
        }


        private void GenerateInstanceTypesPanel(XElement xmlElement)
        {
            // Main vertical panel to hold the header and items
            StackPanel mainPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 7, 0, 7),
                VerticalAlignment = VerticalAlignment.Center
            };

            // Header for the instance types
            TextBlock header = new TextBlock
            {
                Text = "Instance Types",
                FontSize = 16,
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 7)
            };
            mainPanel.Children.Add(header);

            // WrapPanel for instance type items
            WrapPanel instancePanel = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center,
                MaxWidth = 1300 // Adjust as needed to fit the layout
            };

            foreach (var type in xmlElement.Elements("TYPE"))
            {
                StackPanel typePanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(2),
                    VerticalAlignment = VerticalAlignment.Center
                };

                // Name label and textbox
                typePanel.Children.Add(new TextBlock
                {
                    Text = "Name: ",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Colors.White),
                    Margin = new Thickness(20, 0, 5, 0),
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center
                });
                var nameTextBox = new TextBox
                {
                    Text = type.Attribute("name")?.Value ?? "Not found",
                    FontSize = 14,
                    Width = 60,
                    Style = (Style)FindResource("SmallTextBoxStyle"),
                    Margin = new Thickness(0, 0, 10, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Tag = type.Attribute("name")
                };
                nameTextBox.TextChanged += TSSTextBox_TextChanged;
                typePanel.Children.Add(nameTextBox);

                instancePanel.Children.Add(typePanel);
            }

            // Add the WrapPanel to the main panel
            mainPanel.Children.Add(instancePanel);

            // Add the main panel to the controls panel
            TSSXMLControlsPanel.Children.Add(mainPanel);
        }




        private void GenerateRegionTypesPanel(XElement xmlElement)
        {
            // Main vertical panel to hold the header and items
            StackPanel mainPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 10, 0, 10),
                VerticalAlignment = VerticalAlignment.Center
            };

            // Header for the region types panel
            TextBlock header = new TextBlock
            {
                Text = "Region Types",
                FontSize = 16,
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 7)
            };
            mainPanel.Children.Add(header);

            // WrapPanel for region type items
            WrapPanel regionTypePanel = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center,
                MaxWidth = 1330 // Set max width to control wrapping
            };

            foreach (var type in xmlElement.Elements("TYPE"))
            {
                StackPanel typePanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(2),
                    VerticalAlignment = VerticalAlignment.Center
                };

                // Name label and textbox
                typePanel.Children.Add(new TextBlock
                {
                    Text = "Name: ",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Colors.White),
                    Margin = new Thickness(15, 0, 5, 0),
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center
                });
                var nameTextBox = new TextBox
                {
                    Text = type.Attribute("name")?.Value ?? "Not found",
                    FontSize = 14,
                    Width = 120,
                    Style = (Style)FindResource("SmallTextBoxStyle"),
                    Margin = new Thickness(0, 0, 10, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Tag = type.Attribute("name")
                };
                nameTextBox.TextChanged += TSSTextBox_TextChanged;
                typePanel.Children.Add(nameTextBox);

                // Territory ComboBox
                typePanel.Children.Add(new TextBlock
                {
                    Text = "Territory: ",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Colors.White),
                    Margin = new Thickness(0, 0, 5, 0),
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center
                });
                var territoryComboBox = new ComboBox
                {
                    Width = 80,
                    Height = 22,
                    Style = (Style)FindResource("DarkModeComboBoxStyle"),
                    Margin = new Thickness(0, 0, 10, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    ItemsSource = new[] { "SCEA", "SCEE", "SCEJ", "SCEAsia" }
                };
                territoryComboBox.SelectedIndex = 0; // Default to first item
                typePanel.Children.Add(territoryComboBox);

                // Instance ComboBox
                typePanel.Children.Add(new TextBlock
                {
                    Text = "Instance: ",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Colors.White),
                    Margin = new Thickness(0, 0, 5, 0),
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center
                });
                var instanceComboBox = new ComboBox
                {
                    Width = 80,
                    Height = 22,
                    Style = (Style)FindResource("DarkModeComboBoxStyle"),
                    Margin = new Thickness(0, 0, 10, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    ItemsSource = new[] { "EU", "US", "Japan", "Asia" }
                };
                instanceComboBox.SelectedIndex = 0; // Default to first item
                typePanel.Children.Add(instanceComboBox);

                // Type label and textbox
                typePanel.Children.Add(new TextBlock
                {
                    Text = "Type: ",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Colors.White),
                    Margin = new Thickness(0, 0, 5, 0),
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center
                });
                var typeTextBox = new TextBox
                {
                    Text = type.Value,
                    FontSize = 14,
                    Width = 50,
                    Margin = new Thickness(0, 0, 50, 0),
                    Style = (Style)FindResource("SmallTextBoxStyle"),
                    VerticalAlignment = VerticalAlignment.Center,
                    Tag = type
                };
                typeTextBox.TextChanged += TSSTextBox_TextChanged;
                typePanel.Children.Add(typeTextBox);

                // Add the panel for this type to the WrapPanel
                regionTypePanel.Children.Add(typePanel);
            }

            // Add the WrapPanel to the main panel
            mainPanel.Children.Add(regionTypePanel);

            // Add the main panel to the controls panel
            TSSXMLControlsPanel.Children.Add(mainPanel);
        }


        private void GenerateAgeRestrictionsPanel(XElement ageRestrictionsElement)
        {
            // Main vertical panel to hold the header and items
            StackPanel mainPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center
            };

            // Header for the age restrictions
            TextBlock header = new TextBlock
            {
                Text = "Age Restrictions",
                FontSize = 16,
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 7)
            };
            mainPanel.Children.Add(header);

            // WrapPanel for age restriction items
            WrapPanel ageRestrictionsPanel = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center,
                MaxWidth = 1300 // Adjust as needed to fit the layout
            };

            foreach (var ageElement in ageRestrictionsElement.Elements("age"))
            {
                StackPanel agePanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(2),
                    VerticalAlignment = VerticalAlignment.Center
                };

                TextBlock regionLabel = new TextBlock
                {
                    Text = "Region: " + ageElement.Attribute("region")?.Value + " ",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Colors.White),
                    Margin = new Thickness(20, 0, 5, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                TextBox ageTextBox = new TextBox
                {
                    Text = ageElement.Value,
                    Width = 30,
                    Style = (Style)FindResource("SmallTextBoxStyle"),
                    Margin = new Thickness(0, 0, 10, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Tag = ageElement  // Linking TextBox to the XElement
                };
                ageTextBox.TextChanged += TSSTextBox_TextChanged; // Event handler to update XML
                agePanel.Children.Add(regionLabel);
                agePanel.Children.Add(ageTextBox);

                ageRestrictionsPanel.Children.Add(agePanel);
            }

            // Add the WrapPanel to the main panel
            mainPanel.Children.Add(ageRestrictionsPanel);

            // Add the main panel to the controls panel
            TSSXMLControlsPanel.Children.Add(mainPanel);
        }


        private void GenerateGlobalPanel(XElement globalElement)
        {
            // Main vertical panel to hold the header and items
            StackPanel mainPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center
            };

            // Header for the global panel
            TextBlock header = new TextBlock
            {
                Text = "Global",
                FontSize = 16,
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            mainPanel.Children.Add(header);

            // WrapPanel for mode items
            WrapPanel globalPanel = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center,
                MaxWidth = 1330 // Adjust as needed for layout
            };

            foreach (var modeElement in globalElement.Elements("mode"))
            {
                StackPanel modePanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(2),
                    VerticalAlignment = VerticalAlignment.Center
                };

                // Mode value label and text
                TextBlock modeLabel = new TextBlock
                {
                    Text = "Mode: " + modeElement.Value,
                    FontSize = 14,
                    Width = 70,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(35, 0, 5, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                modePanel.Children.Add(modeLabel);

                // Attributes (regions)
                foreach (var attribute in modeElement.Attributes())
                {
                    TextBlock regionLabel = new TextBlock
                    {
                        Text = attribute.Name.LocalName + ": ",
                        FontSize = 14,
                        Foreground = new SolidColorBrush(Colors.White),
                        Margin = new Thickness(0, 0, 5, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    TextBox regionTextBox = new TextBox
                    {
                        Text = attribute.Value,
                        Width = 25,
                        Style = (Style)FindResource("SmallTextBoxStyle"),
                        Margin = new Thickness(0, 0, 10, 0),
                        Tag = attribute  // Linking TextBox to the XAttribute
                    };
                    regionTextBox.TextChanged += TSSTextBox_TextChanged; // Event handler to update XML
                    modePanel.Children.Add(regionLabel);
                    modePanel.Children.Add(regionTextBox);
                }

                globalPanel.Children.Add(modePanel);
            }

            // Add the WrapPanel to the main panel
            mainPanel.Children.Add(globalPanel);

            // Add the main panel to the controls panel
            TSSXMLControlsPanel.Children.Add(mainPanel);
        }


        private void GenerateDNSOverridePanel(XElement xmlElement)
        {
            StackPanel dnsOverridePanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 10, 0, 10),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock header = new TextBlock
            {
                Text = "DNS Overrides",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(0, 0, 0, 10)
            };
            dnsOverridePanel.Children.Add(header);

            StackPanel entriesPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center
            };

            foreach (var dnsOverride in xmlElement.Elements("DNSOverride"))
            {
                entriesPanel.Children.Add(CreateDNSOverrideEntry(dnsOverride, entriesPanel));
            }

            dnsOverridePanel.Children.Add(entriesPanel);

            Button addButton = new Button
            {
                Content = "Add",
                Width = 40,
                FontSize = 10,
                Foreground = new SolidColorBrush(Colors.Lime),
                FontWeight = FontWeights.Bold,
                Style = (Style)FindResource("DarkModeButtonStyle2"),
                Height = 21,
                Margin = new Thickness(25, 10, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            addButton.Click += (sender, args) => AddNewDNSOverride(xmlElement, entriesPanel);
            dnsOverridePanel.Children.Add(addButton);

            TSSXMLControlsPanel.Children.Add(dnsOverridePanel);
        }

        private StackPanel CreateDNSOverrideEntry(XElement dnsOverride, Panel entriesPanel)
        {
            StackPanel entryPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center
            };

            // DNS IP textbox
            var dnsIpTextBox = new TextBox
            {
                Text = dnsOverride.Value ?? "Enter DNS IP",
                FontSize = 14,
                Width = 110,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Style = (Style)FindResource("SmallTextBoxStyle")
            };
            dnsIpTextBox.TextChanged += (sender, args) =>
            {
                dnsOverride.Value = dnsIpTextBox.Text;  // Update the XML when the textbox text changes
            };
            entryPanel.Children.Add(new TextBlock
            {
                Text = "Primary DNS IP: ",
                FontSize = 14,
                Margin = new Thickness(20, 0, 0, 0),
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center
            });
            entryPanel.Children.Add(dnsIpTextBox);

            AddLabelAndComboBox(entryPanel, "Action:", new List<string> { "allow", "deny" }, dnsOverride.Attribute("action")?.Value, dnsOverride, "action");
            AddLabelAndComboBox(entryPanel, "Report:", new List<string> { "on", "off" }, dnsOverride.Attribute("report")?.Value, dnsOverride, "report");

            // Delete button
            Button deleteButton = new Button
            {
                Content = "Delete",
                Width = 50,
                Height = 22,
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 10,
                Foreground = new SolidColorBrush(Colors.Red),
                FontWeight = FontWeights.Bold,
                Style = (Style)FindResource("DarkModeButtonStyle2")
            };
            deleteButton.Click += (sender, args) =>
            {
                // Remove the element completely from the XML
                dnsOverride.Remove();

                // Remove from GUI
                entriesPanel.Children.Remove(entryPanel);
            };

            entryPanel.Children.Add(deleteButton);

            return entryPanel;
        }



        private void AddNewDNSOverride(XElement parentElement, Panel entriesPanel)
        {
            XElement newDnsOverride = new XElement("DNSOverride",
                new XAttribute("action", "allow"),
                new XAttribute("report", "on"),
                "");

            // Find the last DNSOverride in the XML
            var lastDnsOverride = parentElement.Elements("DNSOverride").LastOrDefault();

            if (lastDnsOverride != null)
            {
                // Add the new element directly after the last existing DNSOverride element
                lastDnsOverride.AddAfterSelf(newDnsOverride);
            }
            else
            {
                // If no DNSOverride elements exist, just add to the parent element
                parentElement.Add(newDnsOverride);
            }

            // Add to GUI
            var dnsOverrideEntry = CreateDNSOverrideEntry(newDnsOverride, entriesPanel);
            entriesPanel.Children.Add(dnsOverrideEntry);  // Add new entry to the GUI
        }

        private void AddLabelAndComboBox(StackPanel panel, string labelText, List<string> options, string currentValue, XElement element, string attributeName)
        {
            panel.Children.Add(new TextBlock
            {
                Text = labelText,
                FontSize = 14,
                Margin = new Thickness(0, 0, 5, 0),
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center
            });

            ComboBox comboBox = new ComboBox
            {
                Width = 60,
                Height = 24,
                Style = (Style)FindResource("DarkModeComboBoxStyle"),
                FontSize = 12,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            // Add ComboBoxItems with uppercase labels for display and store the actual value as lowercase in the Tag
            foreach (string option in options)
            {
                ComboBoxItem item = new ComboBoxItem
                {
                    Content = option.ToUpper(),  // Uppercase display
                    Tag = option.ToLower()       // Lowercase value for XML update
                };
                comboBox.Items.Add(item);
                // Set selected item based on current value, ensuring comparison is case-insensitive
                if (!string.IsNullOrEmpty(currentValue) && currentValue.ToLower() == option.ToLower())
                {
                    comboBox.SelectedItem = item;
                }
            }

            if (comboBox.SelectedItem == null && comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0; // Default to first item if no match found
            }

            comboBox.SelectionChanged += (sender, args) =>
            {
                if (comboBox.SelectedItem is ComboBoxItem selected)
                {
                    // Update the XML attribute to the lowercase value stored in Tag
                    element.SetAttributeValue(attributeName, selected.Tag.ToString());
                }
            };

            panel.Children.Add(comboBox);
        }

        private void GenerateConnectionPanel(XElement xmlElement)
        {
            StackPanel connectionPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 10, 0, 10),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock header = new TextBlock
            {
                Text = "Connection",
                FontSize = 16,
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            connectionPanel.Children.Add(header);

            // Process the connection details
            var muisElement = xmlElement.Element("muis");
            var contentServerElement = xmlElement.Element("contentserver");

            // MUIS IP
            AddLabelAndTextbox(connectionPanel, "MUIS IP:", muisElement?.Value ?? "Not found", 165, 120, muisElement);

            // Content Decryption Key
            // Correct use for Content Decryption Key
            AddLabelAndTextbox(connectionPanel, "Content Decryption Key:", contentServerElement?.Attribute("key")?.Value ?? "Not found", 165, 400, contentServerElement.Attribute("key"));


            // Content Server URL
            AddLabelAndTextbox(connectionPanel, "Content Server URL:", contentServerElement?.Value ?? "Not found", 165, 400, contentServerElement);

            TSSXMLControlsPanel.Children.Add(connectionPanel);
        }


        private void GenerateSecureRootUrlsPanel(XElement xmlElement)
        {
            StackPanel secureUrlsPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 10, 0, 10),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock header = new TextBlock
            {
                Text = "Secure Root URLs",
                FontSize = 16,
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            secureUrlsPanel.Children.Add(header);

            // List of URL elements to process
            var urlElements = new List<string>
    {
        "SecureContentRoot",
        "ScreenContentRoot",
        "secure_lua_object_resources_root",
        "secure_lua_scene_resources_root"
    };

            // Process each element and create a label and textbox
            foreach (var elementName in urlElements)
            {
                StackPanel urlPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                TextBlock label = new TextBlock
                {
                    Text = $"{elementName}:",
                    FontSize = 14,
                    Margin = new Thickness(20, 0, 5, 0),
                    Width = 225,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center
                };
                urlPanel.Children.Add(label);

                var urlElement = xmlElement.Element(elementName);
                TextBox textBox = new TextBox
                {
                    Width = 400,
                    Style = (Style)FindResource("SmallTextBoxStyle"),
                    Margin = new Thickness(2),
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = urlElement != null ? urlElement.Value : "Not found",
                    Tag = urlElement  // Linking TextBox to the XElement
                };
                textBox.TextChanged += TSSTextBox_TextChanged;  // Shared event handler to update XML
                urlPanel.Children.Add(textBox);
                secureUrlsPanel.Children.Add(urlPanel);
            }

            TSSXMLControlsPanel.Children.Add(secureUrlsPanel);
        }


        private void GenerateVersionPanel(XElement versionElement)
        {
            StackPanel versionPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0,12,0,0),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock label = new TextBlock
            {
                Text = "TSS Date:",
                FontSize = 16,
                Width = 80,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBox textBox = new TextBox
            {
                Text = versionElement.Value,
                Margin = new Thickness(2),
                FontSize = 15,
                Style = (Style)FindResource("SmallTextBoxStyle"),
                Width = 200,
                Tag = versionElement  // Linking TextBox to the XElement
            };

            textBox.TextChanged += TSSTextBox_TextChanged;  // Using a shared event handler

            versionPanel.Children.Add(label);
            versionPanel.Children.Add(textBox);
            TSSXMLControlsPanel.Children.Add(versionPanel);
        }


        private void GenerateSHA1Section(XElement xmlElement)
        {
            StackPanel sha1Panel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(2)
            };

            TextBlock sha1Header = new TextBlock
            {
                Text = "SHA1 Encrypted Files",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 15, 0, 10)
            };
            sha1Panel.Children.Add(sha1Header);

            StackPanel entriesPanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            var sha1Elements = xmlElement.Descendants("SHA1").ToList();
            foreach (var sha1Element in sha1Elements)
            {
                entriesPanel.Children.Add(CreateSHA1Panel(sha1Element));
            }

            sha1Panel.Children.Add(entriesPanel);

            Button addButton = new Button
            {
                Content = "Add",
                Width = 40,
                Height = 21,
                FontSize = 10,
                Foreground = new SolidColorBrush(Colors.Lime),
                Style = (Style)FindResource("DarkModeButtonStyle2"),
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(20, 10, 0, 2),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            addButton.Click += (sender, args) => AddNewSHA1Entry(xmlElement, entriesPanel);
            sha1Panel.Children.Add(addButton);

            TSSXMLControlsPanel.Children.Add(sha1Panel);
        }

        private void AddNewSHA1Entry(XElement parentElement, Panel entriesPanel)
        {
            XElement newSha1 = new XElement("SHA1",
                new XAttribute("file", ""),
                new XAttribute("digest", ""));

            // Find the last SHA1 element in the parent XML element
            var lastSha1Element = parentElement.Elements("SHA1").LastOrDefault();

            if (lastSha1Element != null)
            {
                // If there is at least one SHA1 element, add the new one after the last one
                lastSha1Element.AddAfterSelf(newSha1);
            }
            else
            {
                // If there are no SHA1 elements, just add the new one to the parent
                parentElement.Add(newSha1);
            }

            var newSha1Panel = CreateSHA1Panel(newSha1);
            entriesPanel.Children.Add(newSha1Panel);
        }


        private StackPanel CreateSHA1Panel(XElement sha1Element)
        {
            StackPanel panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0)
            };

            // Reuse the existing GenerateSHA1Panel logic here
            GenerateSHA1Panel(sha1Element, panel);

            // Add a Delete button
            Button deleteButton = new Button
            {
                Content = "Delete",
                Width = 50,
                FontSize = 10,
                Foreground = new SolidColorBrush(Colors.Red),
                Style = (Style)FindResource("DarkModeButtonStyle2"),
                Height = 22,
                Margin = new Thickness(5, 0, 0, 0)
            };
            deleteButton.Click += (sender, args) =>
            {
                // Remove the element completely from the XML
                sha1Element.Remove();
                // Remove from GUI
                ((Panel)panel.Parent).Children.Remove(panel);
            };
            panel.Children.Add(deleteButton);

            return panel;
        }


        private void GenerateSHA1Panel(XElement sha1Element, StackPanel panel)
        {
            TextBlock fileLabel = new TextBlock
            {
                Text = "File Path:",
                FontSize = 14,
                Width = 60,
                Margin = new Thickness(20, 0, 5, 0),
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBox fileTextBox = new TextBox
            {
                Text = sha1Element.Attribute("file").Value,
                Margin = new Thickness(2),
                Style = (Style)FindResource("SmallTextBoxStyle"),
                Width = 300,
                Tag = sha1Element.Attribute("file")  // Linking TextBox to the file attribute
            };
            fileTextBox.TextChanged += TextBox_TSSAttributeChanged;  // Generalized event handler

            Button downloadButton = new Button
            {
                Content = "Download",
                Width = 70,
                FontSize = 10,
                Style = (Style)FindResource("DarkModeButtonStyle2"),
                Height = 22,
                Margin = new Thickness(5, 0, 0, 0)
            };
            downloadButton.Click += (sender, args) => DownloadFileAction(fileTextBox.Text);

            TextBlock digestLabel = new TextBlock
            {
                Text = "SHA1 Digest:",
                FontSize = 14,
                Width = 90,
                Margin = new Thickness(20, 0, 5, 0),
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBox digestTextBox = new TextBox
            {
                Text = sha1Element.Attribute("digest").Value,
                Margin = new Thickness(0),
                Style = (Style)FindResource("SmallTextBoxStyle"),
                Width = 400,
                Tag = sha1Element.Attribute("digest")  // Linking TextBox to the digest attribute
            };
            digestTextBox.TextChanged += TextBox_TSSAttributeChanged;  // Generalized event handler

            Button copyButton = new Button
            {
                Content = "Copy SHA1",
                Width = 75,
                FontSize = 10,
                Style = (Style)FindResource("DarkModeButtonStyle2"),
                Height = 22,
                Margin = new Thickness(10, 0, 0, 0)
            };
            copyButton.Click += (sender, args) =>
            {
                Clipboard.SetText(digestTextBox.Text);
                MessageBox.Show("SHA1 Digest copied to clipboard!");
            };

            panel.Children.Add(fileLabel);
            panel.Children.Add(fileTextBox);
            panel.Children.Add(downloadButton);
            panel.Children.Add(digestLabel);
            panel.Children.Add(digestTextBox);
            panel.Children.Add(copyButton);
        }

        // Generalized event handler for attribute changes
        // Generalized event handler for attribute changes
        private void TextBox_TSSAttributeChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                // Change the text color to yellow
                textBox.Foreground = new SolidColorBrush(Colors.Yellow);

                // Update the attribute value if the TextBox's Tag is an XAttribute
                if (textBox.Tag is XAttribute attribute)
                {
                    attribute.Value = textBox.Text;
                }

                // Set the visibility of the warning text block to visible
                TextBlock tssSaveWarningTextblock = (TextBlock)FindName("TSSsaveWarningTextblock");
                if (tssSaveWarningTextblock != null)
                {
                    tssSaveWarningTextblock.Visibility = Visibility.Visible;
                }
            }
        }



            private async void DownloadFileAction(string filePath)
        {
            TextBox tssUrlTextBox = (TextBox)FindName("TSSURLtextbox");
            if (tssUrlTextBox != null)
            {
                Uri baseUri = new Uri(tssUrlTextBox.Text);
                string baseUrl = baseUri.GetLeftPart(UriPartial.Authority); // Gets the base URL up to the first '/'
                string fullUrl = baseUrl + "/" + filePath.TrimStart('/');

                // Download the file
                try
                {
                    using (var client = new HttpClient())
                    {
                        var response = await client.GetAsync(fullUrl);
                        if (response.IsSuccessStatusCode)
                        {
                            var data = await response.Content.ReadAsByteArrayAsync();

                            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                            saveFileDialog.FileName = Path.GetFileName(filePath); // Default file name
                            saveFileDialog.DefaultExt = Path.GetExtension(filePath); // Default file extension
                            saveFileDialog.Filter = "All files (*.*)|*.*"; // Filter files by extension

                            if (saveFileDialog.ShowDialog() == true)
                            {
                                File.WriteAllBytes(saveFileDialog.FileName, data);
                                MessageBox.Show("File downloaded successfully!");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Failed to download file: " + response.StatusCode);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error downloading file: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("TSS URL Text Box not found");
            }
        }

        private void GenerateObjectsPanel(XElement objectsElement)
        {
            StackPanel objectsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 15),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock label = new TextBlock
            {
                Text = "Object Catalogue Format:",
                FontSize = 14,
                Width = 180,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(20, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            objectsPanel.Children.Add(label);

            // Dictionary to map the GUI strings to XML tags
            Dictionary<string, string> options = new Dictionary<string, string>
    {
        {"HCDB", "prepared_database"},
        {"XML", "hierarchical_layout"}
    };

            foreach (var option in options)
            {
                StackPanel radioButtonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 0, 0, 0)
                };

                TextBlock radioButtonLabel = new TextBlock
                {
                    Text = option.Key + ":",
                    FontSize = 12,
                    Width = 50,
                    Margin = new Thickness(0, 0, 0, 0),
                    Foreground = new SolidColorBrush(Colors.White),
                    VerticalAlignment = VerticalAlignment.Center
                };
                radioButtonPanel.Children.Add(radioButtonLabel);

                RadioButton radioButton = new RadioButton
                {
                    GroupName = "ObjectFormat",
                    Width = 48,
                    Margin = new Thickness(-25, 0, 25, 0),
                    Style = (Style)FindResource("ModernRadioButtonStyle"),
                    VerticalAlignment = VerticalAlignment.Center
                };
                radioButtonPanel.Children.Add(radioButton);

                // Set the current selection based on the XML content
                if (objectsElement.Elements(option.Value).Any())
                {
                    radioButton.IsChecked = true;
                }

                radioButton.Checked += (sender, args) =>
                {
                    objectsElement.RemoveAll(); // Clear existing elements
                    objectsElement.Add(new XElement(option.Value)); // Add the new element based on the selected radio button
                };

                objectsPanel.Children.Add(radioButtonPanel);
            }

            TSSXMLControlsPanel.Children.Add(objectsPanel);
        }



        private void GenerateDisableBarPanel(XElement xmlElement)
        {
            StackPanel disableBarPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            disableBarPanel.Children.Add(new TextBlock
            {
                Text = "Home Archive Support:",
                FontSize = 14,
                Width = 180,
                Margin = new Thickness(20, 0, 5, 0),
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center
            });

            // Radio button for "Sharc Only"
            StackPanel sharcOnlyPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 0)
            };

            TextBlock sharcOnlyLabel = new TextBlock
            {
                Text = "Sharc Only:",
                FontSize = 12,
                Width = 60,
                Margin = new Thickness(0, 0, 3, 0),
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center
            };
            sharcOnlyPanel.Children.Add(sharcOnlyLabel);

            RadioButton sharcOnlyButton = new RadioButton
            {
                GroupName = "ArchiveSecurity",
                Width = 48,
                Margin = new Thickness(-15, 0, 5, 0),
                Style = (Style)FindResource("ModernRadioButtonStyle"),
                VerticalAlignment = VerticalAlignment.Center
            };
            sharcOnlyButton.IsChecked = xmlElement.Elements("disablebar").Any() && !xmlElement.Nodes().OfType<XComment>().Any(c => c.Value.Contains("disablebar"));
            sharcOnlyPanel.Children.Add(sharcOnlyButton);

            // Radio button for "Allow BAR/Non-Sharc SDAT"
            StackPanel allowBarPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(15, 0, 0, 0)
            };

            TextBlock allowBarLabel = new TextBlock
            {
                Text = "All Types:",
                FontSize = 12,
                Width = 60,
                Margin = new Thickness(-15, 0, 0, 0),
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center
            };
            allowBarPanel.Children.Add(allowBarLabel);

            RadioButton allowBarButton = new RadioButton
            {
                GroupName = "ArchiveSecurity",
                Width = 50,
                Margin = new Thickness(-15, 0, 20, 0),
                Style = (Style)FindResource("ModernRadioButtonStyle"),
                VerticalAlignment = VerticalAlignment.Center
            };
            allowBarButton.IsChecked = !sharcOnlyButton.IsChecked.Value;
            allowBarPanel.Children.Add(allowBarButton);

            // Event handling for radio button changes
            sharcOnlyButton.Checked += (sender, args) =>
            {
                var connectionElement = xmlElement.Element("connection");
                var disableBar = connectionElement.ElementsAfterSelf("disablebar").FirstOrDefault();
                var disableBarComment = connectionElement.NodesAfterSelf().OfType<XComment>().FirstOrDefault(c => c.Value.Contains("<disablebar />"));

                if (disableBar == null && disableBarComment == null)
                {
                    connectionElement.AddAfterSelf(new XElement("disablebar"));
                }
                else if (disableBarComment != null)
                {
                    disableBarComment.ReplaceWith(new XElement("disablebar"));
                }
            };

            allowBarButton.Checked += (sender, args) =>
            {
                var disableBar = xmlElement.Elements("disablebar").FirstOrDefault();
                if (disableBar != null)
                {
                    disableBar.ReplaceWith(new XComment(disableBar.ToString()));
                }
            };

            // Add panels to the main panel

            disableBarPanel.Children.Add(allowBarPanel);
            disableBarPanel.Children.Add(sharcOnlyPanel);

            TSSXMLControlsPanel.Children.Add(disableBarPanel);
        }

        private void GenerateSecureCommercePointsPanel(XElement xmlElement)
        {
            var commerceElement = xmlElement.Element("commerce");
            if (commerceElement == null)
            {
                commerceElement = new XElement("commerce");
                xmlElement.Add(commerceElement);
            }

            StackPanel commercePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            commercePanel.Children.Add(new TextBlock
            {
                Text = "Secure Commerce Points:",
                FontSize = 14,
                Width = 180,
                Margin = new Thickness(20, 0, 5, 0),
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center
            });

            // Radio button for "True"
            StackPanel truePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 0)
            };

            TextBlock trueLabel = new TextBlock
            {
                Text = "True:",
                FontSize = 12,
                Width = 50,
                Margin = new Thickness(0, 0, 0, 0),

                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center
            };
            truePanel.Children.Add(trueLabel);

            RadioButton trueButton = new RadioButton
            {
                GroupName = "SecureCommerce",
                Width = 48,
                Margin = new Thickness(-25, 0, 15, 0),
                Style = (Style)FindResource("ModernRadioButtonStyle"),
                VerticalAlignment = VerticalAlignment.Center
            };
            truePanel.Children.Add(trueButton);

            // Radio button for "False"
            StackPanel falsePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(15, 0, 0, 0)
            };

            TextBlock falseLabel = new TextBlock
            {
                Text = "False:",
                FontSize = 12,
                Width = 40,
                Margin = new Thickness(-5, 0, 0, 0),

                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center
            };
            falsePanel.Children.Add(falseLabel);

            RadioButton falseButton = new RadioButton
            {
                GroupName = "SecureCommerce",
                Width = 50,
                Margin = new Thickness(-15, 0, 15, 0),
                Style = (Style)FindResource("ModernRadioButtonStyle"),
                VerticalAlignment = VerticalAlignment.Center
            };
            falsePanel.Children.Add(falseButton);

            // Initial states for radio buttons
            var securePoints = commerceElement.Element("secure_commerce_points");
            var securePointsComment = commerceElement.Nodes().OfType<XComment>().FirstOrDefault(c => c.Value.Contains("<secure_commerce_points />"));

            trueButton.IsChecked = (securePoints != null);
            falseButton.IsChecked = !trueButton.IsChecked.Value;

            // Event handling for radio button changes
            trueButton.Checked += (sender, args) =>
            {
                var currentElement = commerceElement.Element("secure_commerce_points");
                var currentComment = commerceElement.Nodes().OfType<XComment>().FirstOrDefault(c => c.Value.Trim() == "<secure_commerce_points />");

                if (currentElement == null && currentComment != null)
                {
                    currentComment.ReplaceWith(new XElement("secure_commerce_points"));
                }
                else if (currentElement == null)
                {
                    commerceElement.Add(new XElement("secure_commerce_points"));
                }
            };


            falseButton.Checked += (sender, args) =>
            {
                var currentElement = commerceElement.Element("secure_commerce_points");
                if (currentElement != null)
                {
                    currentElement.ReplaceWith(new XComment(currentElement.ToString()));
                }
            };

            // Add panels to the main panel
            commercePanel.Children.Add(truePanel);
            commercePanel.Children.Add(falsePanel);

            TSSXMLControlsPanel.Children.Add(commercePanel);
        }


        private void GenerateUseRegionalServiceIdsPanel(XElement xmlElement)
        {
            StackPanel serviceIdsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            serviceIdsPanel.Children.Add(new TextBlock
            {
                Text = "Use Regional Service IDs:",
                FontSize = 14,
                Width = 180,
                Margin = new Thickness(20, 0, 5, 0),
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center
            });

            // Radio button for "True"
            StackPanel truePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 0)
            };

            TextBlock trueLabel = new TextBlock
            {
                Text = "True:",
                FontSize = 12,
                Width = 50,
                Margin = new Thickness(0, 0, 0, 0),
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center
            };
            truePanel.Children.Add(trueLabel);

            RadioButton trueButton = new RadioButton
            {
                GroupName = "RegionalService",
                Width = 48,
                Margin = new Thickness(-25, 0, 15, 0),
                Style = (Style)FindResource("ModernRadioButtonStyle"),
                VerticalAlignment = VerticalAlignment.Center
            };
            truePanel.Children.Add(trueButton);

            // Radio button for "False"
            StackPanel falsePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(15, 0, 0, 0)
            };

            TextBlock falseLabel = new TextBlock
            {
                Text = "False:",
                FontSize = 12,
                Width = 40,
                Margin = new Thickness(-5, 0, 0, 0),
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center
            };
            falsePanel.Children.Add(falseLabel);

            RadioButton falseButton = new RadioButton
            {
                GroupName = "RegionalService",
                Width = 50,
                Margin = new Thickness(-15, 0, 15, 0),
                Style = (Style)FindResource("ModernRadioButtonStyle"),
                VerticalAlignment = VerticalAlignment.Center
            };
            falsePanel.Children.Add(falseButton);

            // Initial state setup and event handling
            var regionalServiceNode = xmlElement.Element("useregionalserviceids");
            var regionalServiceComment = xmlElement.Nodes().OfType<XComment>().FirstOrDefault(c => c.Value.Contains("<useregionalserviceids />"));

            trueButton.IsChecked = (regionalServiceNode != null);
            falseButton.IsChecked = !trueButton.IsChecked.Value;

            trueButton.Checked += (sender, args) =>
            {
                var currentElement = xmlElement.Element("useregionalserviceids");
                var currentComment = xmlElement.Nodes().OfType<XComment>().FirstOrDefault(c => c.Value.Trim() == "<useregionalserviceids />");

                if (currentElement == null && currentComment != null)
                {
                    currentComment.ReplaceWith(new XElement("useregionalserviceids"));
                }
                else if (currentElement == null)
                {
                    xmlElement.Add(new XElement("useregionalserviceids"));
                }
            };


            falseButton.Checked += (sender, args) =>
            {
                var currentElement = xmlElement.Element("useregionalserviceids");
                if (currentElement != null)
                {
                    currentElement.ReplaceWith(new XComment(currentElement.ToString()));
                }
            };

            // Add panels to the main panel
            serviceIdsPanel.Children.Add(truePanel);
            serviceIdsPanel.Children.Add(falsePanel);

            TSSXMLControlsPanel.Children.Add(serviceIdsPanel);
        }

        private void GenerateXmlElementControls(XElement element, Panel parentPanel, HashSet<string> excludeElements)
        {
            foreach (var subElement in element.Elements().Where(x => !excludeElements.Contains(x.Name.LocalName)))
            {
                StackPanel panel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(2),
                    VerticalAlignment = VerticalAlignment.Center
                };

                TextBlock label = new TextBlock
                {
                    Text = $"{subElement.Name.LocalName}:",
                    FontSize = 14,
                    Width = 150,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Colors.White),
                    VerticalAlignment = VerticalAlignment.Center
                };
                panel.Children.Add(label);

                TextBox textBox = new TextBox
                {
                    Text = subElement.Value,
                    Margin = new Thickness(2),
                    Width = 300,
                    Tag = subElement // Store the reference to the XElement
                };

                textBox.TextChanged += (sender, args) =>
                {
                    var tb = sender as TextBox;
                    if (tb != null)
                    {
                        XElement linkedElement = tb.Tag as XElement;
                        if (linkedElement != null)
                        {
                            linkedElement.Value = tb.Text;
                        }
                    }
                };

                panel.Children.Add(textBox);
                parentPanel.Children.Add(panel);
            }
        }


        private void GenerateSceneRedirectPanel(XElement xmlElement)
        {
            StackPanel sceneRedirectPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 10, 0, 10),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock header = new TextBlock
            {
                Text = "Scene Redirects",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(0, 0, 0, 10)
            };
            sceneRedirectPanel.Children.Add(header);

            StackPanel entriesPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center
            };

            foreach (var redirect in xmlElement.Elements("SceneRedirect"))
            {
                entriesPanel.Children.Add(CreateSceneRedirectEntry(redirect, entriesPanel, xmlElement));
            }

            sceneRedirectPanel.Children.Add(entriesPanel);

            // Add button to create new scene redirects
            Button addButton = new Button
            {
                Content = "Add",
                Width = 35,
                FontSize = 9,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.Lime),
                Style = (Style)FindResource("DarkModeButtonStyle2"),
                Height = 20,
                Margin = new Thickness(20, 10, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            addButton.Click += (sender, args) => AddNewSceneRedirect(xmlElement, entriesPanel);
            sceneRedirectPanel.Children.Add(addButton);

            TSSXMLControlsPanel.Children.Add(sceneRedirectPanel);
        }

        private StackPanel CreateSceneRedirectEntry(XElement redirect, Panel entriesPanel, XElement xmlElement)
        {
            StackPanel redirectPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center
            };

            AddLabelAndTextbox(redirectPanel, "Original Destination: ", redirect.Attribute("src")?.Value ?? "", 140, 250, redirect.Attribute("src"));
            AddLabelAndTextbox(redirectPanel, "Redirect To: ", redirect.Attribute("dest")?.Value ?? "", 80, 250, redirect.Attribute("dest"));

            // Region as a ComboBox instead of a textbox
            AddLabelAndSceneRedirectComboBox(redirectPanel, "Region: ", new List<string> { "SCEA", "SCEE", "SCEJ", "SCEAsia" }, redirect.Attribute("region")?.Value, redirect, "region");

            // Delete button to remove scene redirects
            Button deleteButton = new Button
            {
                Content = "Delete",
                Width = 50,
                Height = 21,
                Foreground = new SolidColorBrush(Colors.Red),
                Margin = new Thickness(0),
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Style = (Style)FindResource("DarkModeButtonStyle2")
            };
            deleteButton.Click += (sender, args) =>
            {
                // Remove element from XML and GUI
                redirect.Remove();
                entriesPanel.Children.Remove(redirectPanel);
            };
            redirectPanel.Children.Add(deleteButton);

            return redirectPanel;
        }

        private void AddLabelAndSceneRedirectComboBox(StackPanel panel, string labelText, List<string> options, string currentValue, XElement element, string attributeName)
        {
            panel.Children.Add(new TextBlock
            {
                Text = labelText,
                FontSize = 14,
                Margin = new Thickness(0, 0, 5, 0),
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center
            });

            ComboBox comboBox = new ComboBox
            {
                Width = 75,
                Height = 24,
                Style = (Style)FindResource("DarkModeComboBoxStyle"),
                FontSize = 11,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            options.ForEach(option => comboBox.Items.Add(option));
            comboBox.SelectedItem = currentValue ?? options.First();
            comboBox.SelectionChanged += (sender, args) =>
            {
                if (comboBox.SelectedItem != null)
                {
                    element.SetAttributeValue(attributeName, comboBox.SelectedItem.ToString());
                }
            };

            panel.Children.Add(comboBox);
        }

        private void AddNewSceneRedirect(XElement parentElement, Panel entriesPanel)
        {
            XElement newRedirect = new XElement("SceneRedirect",
                new XAttribute("src", ""),
                new XAttribute("dest", ""),
                new XAttribute("region", ""));

            // Find the last SceneRedirect element in the parent XML element
            var lastRedirectElement = parentElement.Elements("SceneRedirect").LastOrDefault();

            if (lastRedirectElement != null)
            {
                // If there is at least one SceneRedirect element, add the new one after the last one
                lastRedirectElement.AddAfterSelf(newRedirect);
            }
            else
            {
                // If there are no SceneRedirect elements, just add the new one to the parent
                parentElement.Add(newRedirect);
            }

            var redirectEntry = CreateSceneRedirectEntry(newRedirect, entriesPanel, parentElement);
            entriesPanel.Children.Add(redirectEntry);  // Add new entry to the GUI
        }



        private void AddLabelAndTextbox(StackPanel parentPanel, string labelText, string textBoxText, int labelWidth, int textBoxWidth, XObject xObject = null)
        {
            StackPanel linePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock label = new TextBlock
            {
                Text = labelText,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Width = labelWidth,
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 5, 0)
            };
            linePanel.Children.Add(label);

            TextBox textBox = new TextBox
            {
                Text = textBoxText,
                Style = (Style)FindResource("SmallTextBoxStyle"),
                Width = textBoxWidth,
                Margin = new Thickness(5, 0, 5, 0),
                Tag = xObject  // Can be null
            };

            // Only attach event handler if xObject is not null
            if (xObject != null)
            {
                textBox.TextChanged += (sender, args) =>
                {
                    var tb = sender as TextBox;
                    if (tb != null && tb.Tag is XAttribute attribute)
                    {
                        attribute.Value = tb.Text;
                    }
                    else if (tb != null && tb.Tag is XElement element)
                    {
                        element.Value = tb.Text;
                    }
                };
            }

            linePanel.Children.Add(textBox);
            parentPanel.Children.Add(linePanel);
        }





        private void GenerateProfanityFilterPanel(XElement profanityFilterElement)
        {
            StackPanel profanityFilterPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 10, 0, 10),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock header = new TextBlock
            {
                Text = "Profanity Filter",
                FontSize = 16,
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };

            profanityFilterPanel.Children.Add(header);

            // Generate controls for each attribute
            GenerateAttributeControl(profanityFilterElement, "apiKey", "API Key:", profanityFilterPanel);
            GenerateBooleanAttributeControl(profanityFilterElement, "forceOffline", "Force Offline:", profanityFilterPanel);
            GenerateAttributeControl(profanityFilterElement, "privateKey", "Private Key:", profanityFilterPanel);
            GenerateAttributeControl(profanityFilterElement, "updaterOverrideUrl", "Updater Override URL:", profanityFilterPanel);

            TSSXMLControlsPanel.Children.Add(profanityFilterPanel);
        }

        private void GenerateAttributeControl(XElement element, string attributeName, string labelContent, Panel parentPanel)
        {
            StackPanel attributePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock label = new TextBlock
            {
                Text = labelContent,
                FontSize = 14,
                Margin = new Thickness(20, 0, 5, 0),
                Width = 160,
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Retrieve the attribute and use it directly in the TextBox
            XAttribute attribute = element.Attribute(attributeName);
            TextBox textBox = new TextBox
            {
                Text = attribute?.Value ?? "Attribute not found",
                Style = (Style)FindResource("SmallTextBoxStyle"),
                Width = 320,
                Margin = new Thickness(0),
                Tag = attribute  // Store the reference to the XAttribute
            };

            textBox.TextChanged += (sender, args) =>
            {
                var tb = sender as TextBox;
                if (tb != null && tb.Tag is XAttribute attr)
                {
                    attr.Value = tb.Text;  // Update the XML attribute directly
                }
            };

            attributePanel.Children.Add(label);
            attributePanel.Children.Add(textBox);
            parentPanel.Children.Add(attributePanel);
        }

        private void GenerateBooleanAttributeControl(XElement element, string attributeName, string labelContent, Panel parentPanel)
        {
            // Create a horizontal stack panel to contain the label and radio buttons
            StackPanel booleanPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center
            };

            // Create and configure the label for the whole control
            TextBlock mainLabel = new TextBlock
            {
                Text = labelContent,
                FontSize = 14,
                Margin = new Thickness(20, 0, 5, 0),
                Width = 160,
                Foreground = new SolidColorBrush(Colors.White), // Ensure the main label's text color is white
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Add the main label to the panel first
            booleanPanel.Children.Add(mainLabel);

            // Retrieve the current attribute value from the element
            XAttribute attribute = element.Attribute(attributeName);

            // Create labels for each radio button
            TextBlock trueLabel = new TextBlock
            {
                Text = "True:",
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center
            };

            // Create and configure the "True" radio button
            RadioButton trueButton = new RadioButton
            {
                IsChecked = (attribute?.Value.ToLower() == "true"),
                Margin = new Thickness(5, -3, 20, -3),
                GroupName = attributeName,
                Style = (Style)FindResource("ModernRadioButtonStyle"),
                Tag = attribute
            };
            trueButton.Checked += (sender, args) =>
            {
                if (trueButton.Tag is XAttribute attr)
                {
                    attr.Value = "true";
                }
            };

            // Create a label for the "False" radio button
            TextBlock falseLabel = new TextBlock
            {
                Text = "False:",
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center
            };

            // Create and configure the "False" radio button
            RadioButton falseButton = new RadioButton
            {
                IsChecked = (attribute?.Value.ToLower() == "false"),
                Margin = new Thickness(5, -3, 0, -3),
                Style = (Style)FindResource("ModernRadioButtonStyle"),
                GroupName = attributeName,
                Tag = attribute
            };
            falseButton.Checked += (sender, args) =>
            {
                if (falseButton.Tag is XAttribute attr)
                {
                    attr.Value = "false";
                }
            };

            // Add the true label and button to the panel
            booleanPanel.Children.Add(trueLabel);
            booleanPanel.Children.Add(trueButton);

            // Add the false label and button to the panel
            booleanPanel.Children.Add(falseLabel);
            booleanPanel.Children.Add(falseButton);

            // Add the complete boolean control panel to the parent panel
            parentPanel.Children.Add(booleanPanel);
        }



        private void GenerateDataCapturePanel(XElement dataCaptureElement)
        {
            // Check if the dataCaptureElement contains the url element
            var urlElement = dataCaptureElement.Element("url");
            if (urlElement == null) return; // Exit if no url element is found

            StackPanel dataCapturePanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 10, 0, 10),
                VerticalAlignment = VerticalAlignment.Center
            };

            // Add header
            TextBlock header = new TextBlock
            {
                Text = "Data Capture",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(0, 0, 0, 10)
            };
            dataCapturePanel.Children.Add(header);

            // Add Mode label and textbox
            StackPanel modePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock modeLabel = new TextBlock
            {
                Text = "Data Capture Mode:",
                FontSize = 14,
                Width = 140,
                Margin = new Thickness(20, 0, 0, 0),
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center
            };
            modePanel.Children.Add(modeLabel);

            XAttribute modeAttribute = urlElement.Attribute("mode");
            TextBox modeTextBox = new TextBox
            {
                Text = modeAttribute?.Value ?? "Not found",
                Width = 30,
                Margin = new Thickness(0, 0, 0, 0),
                Style = (Style)FindResource("SmallTextBoxStyle"),
                Tag = modeAttribute  // Store the reference to the XAttribute
            };

            modeTextBox.TextChanged += (sender, args) =>
            {
                var tb = sender as TextBox;
                if (tb != null && tb.Tag is XAttribute attr)
                {
                    attr.Value = tb.Text;  // Update mode attribute directly
                }
            };
            modePanel.Children.Add(modeTextBox);
            dataCapturePanel.Children.Add(modePanel);

            // Add URL label and textbox
            StackPanel urlPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock urlLabel = new TextBlock
            {
                Text = "Data Capture URL:",
                FontSize = 14,
                Width = 140,
                Margin = new Thickness(20, 0, 0, 0),
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center
            };
            urlPanel.Children.Add(urlLabel);

            TextBox urlTextBox = new TextBox
            {
                Text = urlElement.Value,
                Width = 600,
                Style = (Style)FindResource("SmallTextBoxStyle"),
                Margin = new Thickness(0),
                Tag = urlElement  // Store the reference to the XElement
            };

            urlTextBox.TextChanged += (sender, args) =>
            {
                var tb = sender as TextBox;
                if (tb != null && tb.Tag is XElement el)
                {
                    el.Value = tb.Text;  // Update URL text directly
                }
            };
            urlPanel.Children.Add(urlTextBox);
            dataCapturePanel.Children.Add(urlPanel);

            // Add the complete panel to the main GUI container
            TSSXMLControlsPanel.Children.Add(dataCapturePanel);
        }


        private void GenerateSSFWPanel(XElement ssfwElement)
        {
            StackPanel ssfwPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 10, 0, 10),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock header = new TextBlock
            {
                Text = "SSFW",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(0, 0, 0, 10)
            };
            ssfwPanel.Children.Add(header);

            // Special handling for identity element with multiple attributes
            var identityElement = ssfwElement.Element("identity");
            if (identityElement != null)
            {
                AddLabelAndTextbox(ssfwPanel, "Identity TTL: ", identityElement.Attribute("ttl")?.Value ?? "Not found", 130, 60, identityElement.Attribute("ttl"));
                AddLabelAndTextbox(ssfwPanel, "Identity Secret: ", identityElement.Attribute("secret")?.Value ?? "Not found", 130, 150, identityElement.Attribute("secret"));
                AddLabelAndTextbox(ssfwPanel, "Identity URL: ", identityElement.Value, 130, 700, identityElement);
            }

            // Process all other elements in ssfw, except identity
            foreach (var element in ssfwElement.Elements().Where(e => e.Name != "identity"))
            {
                string label = "SSFW " + element.Name.LocalName + ":";
                AddLabelAndTextbox(ssfwPanel, label, element.Value, 130, 700, element);
            }

            TSSXMLControlsPanel.Children.Add(ssfwPanel);
        }
        private void ApplyNewHCDBSHA1Button_Click(object sender, RoutedEventArgs e)
        {
            // Get the SHA1 value from LatestHCDBSHA1textbox
            var latestHCDBSHA1textbox = (TextBox)FindName("LatestHCDBSHA1textbox");
            if (latestHCDBSHA1textbox == null)
            {
                MessageBox.Show("LatestHCDBSHA1textbox not found!");
                return;
            }
            string newSha1 = latestHCDBSHA1textbox.Text;

            // Get the entriesPanel that contains all SHA1 entries
            var sha1Panel = TSSXMLControlsPanel.Children.OfType<StackPanel>()
                              .FirstOrDefault(p => p.Children.OfType<TextBlock>()
                              .Any(tb => tb.Text == "SHA1 Encrypted Files"));
            if (sha1Panel == null)
            {
                MessageBox.Show("SHA1 Panel not found!");
                return;
            }

            var entriesPanel = sha1Panel.Children.OfType<StackPanel>()
                               .FirstOrDefault(p => p.Orientation == Orientation.Vertical);

            if (entriesPanel == null)
            {
                MessageBox.Show("Entries Panel not found!");
                return;
            }

            // Iterate through all SHA1 entries and update the ones that match the condition
            foreach (var entry in entriesPanel.Children.OfType<StackPanel>())
            {
                var fileTextBox = entry.Children.OfType<TextBox>()
                    .FirstOrDefault(tb => tb.Tag is XAttribute attribute && attribute.Name == "file");
                var digestTextBox = entry.Children.OfType<TextBox>()
                    .FirstOrDefault(tb => tb.Tag is XAttribute attribute && attribute.Name == "digest");

                if (fileTextBox != null && digestTextBox != null && ((XAttribute)fileTextBox.Tag).Value.StartsWith("Objects/ObjectCatalogue_5_SCE"))
                {
                    digestTextBox.Text = newSha1;
                }
            }

            // Set the global flag to true
            DeployHCDBFlag = true;
        }

        private void ApplyNewSceneListSHA1Button_Click(object sender, RoutedEventArgs e)
        {
            // Get the SHA1 value from LatestSceneListSHA1textbox
            var latestSceneListSHA1textbox = (TextBox)FindName("LatestSceneListSHA1textbox");
            if (latestSceneListSHA1textbox == null)
            {
                MessageBox.Show("LatestSceneListSHA1textbox not found!");
                return;
            }
            string newSha1 = latestSceneListSHA1textbox.Text;

            // Get the entriesPanel that contains all SHA1 entries
            var sha1Panel = TSSXMLControlsPanel.Children.OfType<StackPanel>()
                              .FirstOrDefault(p => p.Children.OfType<TextBlock>()
                              .Any(tb => tb.Text == "SHA1 Encrypted Files"));
            if (sha1Panel == null)
            {
                MessageBox.Show("SHA1 Panel not found!");
                return;
            }

            var entriesPanel = sha1Panel.Children.OfType<StackPanel>()
                               .FirstOrDefault(p => p.Orientation == Orientation.Vertical);

            if (entriesPanel == null)
            {
                MessageBox.Show("Entries Panel not found!");
                return;
            }

            // Iterate through all SHA1 entries and update the ones that match the condition
            foreach (var entry in entriesPanel.Children.OfType<StackPanel>())
            {
                var fileTextBox = entry.Children.OfType<TextBox>()
                    .FirstOrDefault(tb => tb.Tag is XAttribute attribute && attribute.Name == "file");
                var digestTextBox = entry.Children.OfType<TextBox>()
                    .FirstOrDefault(tb => tb.Tag is XAttribute attribute && attribute.Name == "digest");

                if (fileTextBox != null && digestTextBox != null && ((XAttribute)fileTextBox.Tag).Value.StartsWith("Environments/SceneList"))
                {
                    digestTextBox.Text = newSha1;
                }
            }

            DeploySceneListFlag = true;
        }

        private void Border_DragSDCEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    string fileExtension = Path.GetExtension(files[0]).ToLower();
                    if (fileExtension == ".xml" || fileExtension == ".sdc")
                    {
                        e.Effects = DragDropEffects.Copy;
                        e.Handled = true;
                    }
                }
            }
        }

        private void Border_SDCDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    string filePath = files[0];
                    string fileExtension = Path.GetExtension(filePath).ToLower();
                    if (fileExtension == ".xml" || fileExtension == ".sdc")
                    {
                        ProcessDroppedSDCFile(filePath);
                    }
                }
            }
        }

        private void Border_SDCMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Create an OpenFileDialog to allow the user to select an .sdc or .xml file
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "SDC files (*.sdc)|*.sdc|XML files (*.xml)|*.xml|All files (*.*)|*.*",
                Title = "Select an SDC or XML file"
            };

            // Show the dialog and check if the user selected a file
            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;
                string fileExtension = Path.GetExtension(selectedFilePath).ToLower();

                // Ensure the selected file is either .sdc or .xml
                if (fileExtension == ".sdc" || fileExtension == ".xml")
                {
                    ProcessDroppedSDCFile(selectedFilePath);
                }
                else
                {
                    MessageBox.Show("Only SDC or XML files are supported.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private async void ProcessDroppedSDCFile(string filePath)
        {
            LogDebugInfo($"Processing dropped SDC file: {filePath}");

            try
            {
                if (!TryProcessSDCFile(filePath))
                {
                    LogDebugInfo($"Failed to process file normally, attempting decryption: {filePath}");
                    string baseOutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "work");
                    bool decrypted = await DecryptSDCFileAsync(filePath, baseOutputDirectory);
                    if (decrypted)
                    {
                        string decryptedFilePath = Path.Combine(baseOutputDirectory, Path.GetFileName(filePath));
                        if (!TryProcessSDCFile(decryptedFilePath))
                        {
                            LogDebugInfo("Failed to process the decrypted file.");
                        }
                        else
                        {
                            SetSceneType(decryptedFilePath);
                        }
                    }
                    else
                    {
                        LogDebugInfo("Failed to decrypt the file.");
                    }
                }
                else
                {
                    SetSceneType(filePath);
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"An error occurred while processing the file: {ex.Message}");
            }
        }

        private bool TryProcessSDCFile(string filePath)
        {
            try
            {
                LogDebugInfo($"Trying to process SDC file: {filePath}");
                string fileContent = File.ReadAllText(filePath);

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(fileContent);

                var archiveNode = xmlDoc.SelectSingleNode("//ARCHIVES/ARCHIVE");
                if (archiveNode != null)
                {
                    string archivePath = archiveNode.InnerText.Replace("[CONTENT_SERVER_ROOT]", "").TrimStart('/');
                    string folderName = Path.GetFileName(Path.GetDirectoryName(archivePath));
                    string fileName = Path.GetFileName(archivePath);

                    string sdcPath = $"{folderName}/{fileName}".Replace(".sdat", ".sdc", StringComparison.OrdinalIgnoreCase)
                                                             .Replace(".BAR", ".sdc", StringComparison.OrdinalIgnoreCase);

                    txtSdcPath.Text = sdcPath;
                    txtSdcName.Text = folderName;
                    txtsceneconfig.Text = "ADD SCENE FILE PATH HERE";

                    var match = Regex.Match(fileName, @"_T(\d{3})", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        string version = match.Groups[1].Value.TrimStart('0'); // Trim leading zeros
                        txtSDCversion.Text = version;
                    }

                    EncryptAndSetChannelID();

                    // Calculate SHA1 checksum after successfully processing the file
                    string sha1Digest = CalculateSHA1Checksum(filePath);
                    txtSdcsha1Digest.Text = sha1Digest;
                    LogDebugInfo($"SHA1 checksum: {sha1Digest}");

                    LogDebugInfo("Successfully processed the SDC file.");
                    return true;
                }
                else
                {
                    LogDebugInfo("ARCHIVE node not found.");
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error processing file: {ex.Message}");
            }

            return false;
        }


        private string CalculateSHA1Checksum(string filePath)
        {
            using (var sha1 = System.Security.Cryptography.SHA1.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = sha1.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
                }
            }
        }


        private async Task<bool> DecryptSDCFileAsync(string filePath, string baseOutputDirectory)
        {
            bool allFilesProcessed = true;
            string filename = Path.GetFileName(filePath);

            try
            {
                LogDebugInfo($"Decrypting file: {filePath}");
                byte[] fileContent = await File.ReadAllBytesAsync(filePath);

                BruteforceProcess proc = new BruteforceProcess(fileContent);
                byte[] decryptedContent = proc.StartBruteForce((int)CdnMode.RETAIL);

                if (decryptedContent != null)
                {
                    string outputDirectory = baseOutputDirectory;
                    if (!Directory.Exists(outputDirectory))
                    {
                        Directory.CreateDirectory(outputDirectory);
                        LogDebugInfo($"Created output directory: {outputDirectory}");
                    }
                    string outputPath = Path.Combine(outputDirectory, filename);
                    await File.WriteAllBytesAsync(outputPath, decryptedContent);
                    LogDebugInfo($"Decrypted content written to: {outputPath}");

                    if (!IsValidDecryptedSDCFile(outputPath))
                    {
                        File.Delete(outputPath);
                        LogDebugInfo($"Invalid decrypted SDC file, deleted: {outputPath}");
                        allFilesProcessed = false;
                    }
                }
                else
                {
                    allFilesProcessed = false;
                    LogDebugInfo("Decryption failed.");
                }
            }
            catch (Exception ex)
            {
                allFilesProcessed = false;
                LogDebugInfo($"Error decrypting file: {ex.Message}");
            }

            return allFilesProcessed;
        }

        private bool IsValidDecryptedSDCFile(string filePath)
        {
            try
            {
                LogDebugInfo($"Validating decrypted SDC file: {filePath}");
                XDocument doc = XDocument.Load(filePath);
                bool isValid = doc.Root.Element("SDC_VERSION") != null;
                LogDebugInfo($"Validation result: {isValid}");
                return isValid;
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error in IsValidDecryptedSDCFile: {ex.Message}");
                return false;
            }
        }

        private void txtChannelID_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EncryptAndSetChannelID();
        }

        private void EncryptAndSetChannelID()
        {
            try
            {
                Random random = new Random();
                int randomNumber = random.Next(30303, 50505);

                ushort sceneID = (ushort)randomNumber;

                bool isLegacyMode = legacyModeCheckBox.IsChecked ?? false;
                SceneKey key = isLegacyMode ? SIDKeyGenerator.Instance.Generate(sceneID)
                                            : SIDKeyGenerator.Instance.GenerateNewerType(sceneID);

                txtChannelID.Text = key.ToString();
                LogDebugInfo($"Channel ID set: {txtChannelID.Text}");
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error during encryption: {ex.Message}");
                txtChannelID.Text = $"Error during encryption: {ex.Message}";
            }
        }

        private void ResetSDCInfo_Click(object sender, RoutedEventArgs e)
        {
            txtSdcsha1Digest.Text = string.Empty;
            txtSdcName.Text = string.Empty;
            txtSdcPath.Text = string.Empty;
            txtSDCversion.Text = string.Empty;
            txtChannelID.Text = string.Empty;
            txtsceneconfig.Text = string.Empty;
            txthomeuuid.Text = string.Empty;
            txtflags.Text = string.Empty;

            // Reset the ComboBoxes
            txtSceneType.SelectedItem = null;
            txtdHost.SelectedItem = null;

            LogDebugInfo("SDC information reset.");
        }


        private void SetSceneType(string filePath)
        {
            try
            {
                LogDebugInfo($"Setting scene type and host based on file content: {filePath}");
                string fileContent = File.ReadAllText(filePath).ToLower();

                if (fileContent.Contains("apartment") || fileContent.Contains("private"))
                {
                    txtSceneType.SelectedItem = txtSceneType.Items.OfType<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == "Home");
                    txtdHost.SelectedItem = txtdHost.Items.OfType<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == "");
                    LogDebugInfo("Scene type set to Home and host set to nothing.");
                }
                else
                {
                    txtSceneType.SelectedItem = txtSceneType.Items.OfType<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == "GlobalSpace");
                    txtdHost.SelectedItem = txtdHost.Items.OfType<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == "en-US");
                    LogDebugInfo("Scene type set to GlobalSpace and host set to en-US.");
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error setting scene type and host: {ex.Message}");
            }
        }

        private void txtSceneType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (txtSceneType.SelectedItem is ComboBoxItem selectedItem)
            {
                switch (selectedItem.Content.ToString())
                {
                    case "Home":
                        txtdHost.SelectedItem = null;
                        txtdHost.IsEnabled = false;
                        txthomeuuid.Text = string.Empty;
                        txthomeuuid.IsEnabled = true;
                        break;
                    case "PrivateNoVideo":
                        txtdHost.SelectedItem = null;
                        txtdHost.IsEnabled = false;
                        txthomeuuid.Text = string.Empty;
                        txthomeuuid.IsEnabled = true;
                        break;
                    case "GlobalSpace":
                        txtdHost.SelectedItem = txtdHost.Items.OfType<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == "en-US");
                        txtdHost.IsEnabled = true;
                        txthomeuuid.Text = string.Empty;
                        txthomeuuid.IsEnabled = false;
                        break;
                    default:
                        txtdHost.SelectedItem = null;
                        txtdHost.IsEnabled = true;
                        txthomeuuid.Text = string.Empty;
                        txthomeuuid.IsEnabled = false;
                        break;
                }
            }
        }


    }

}
       