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
using HomeTools.Crypto;
using HomeTools.BARFramework;
using HomeTools.UnBAR;
using System.IO.Compression;
using HomeTools.ChannelID;
using CustomLogger;
using HomeTools.AFS;
using System.Collections.Concurrent;
using System.Globalization;


namespace NautilusXP2024
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private AppSettings _settings;

        private SolidColorBrush _selectedThemeColor;

        

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

        public MainWindow()
        {
            InitializeComponent();

            LoggerAccessor.SetupLogger("NautilusXP2024");

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
            UpdateTaskbarIconWithTint(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.png"), themeColor);

            AFSClass.MapperHelperFolder = Directory.GetCurrentDirectory();

            _ = new Timer(AFSClass.ScheduledUpdate, null, TimeSpan.Zero, TimeSpan.FromMinutes(1440));

            logFlushTimer = new System.Threading.Timer(_ => FlushLogBuffer(), null, Timeout.Infinite, Timeout.Infinite);

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
            CdsOutputDirectoryTextBox.Text = _settings.CdsOutputDirectory;
            BarSdatSharcOutputDirectoryTextBox.Text = _settings.BarSdatSharcOutputDirectory;
            MappedOutputDirectoryTextBox.Text = _settings.MappedOutputDirectory;
            HcdbOutputDirectoryTextBox.Text = _settings.HcdbOutputDirectory;
            sqlOutputDirectoryTextBox.Text = _settings.SqlOutputDirectory;
            TicketListOutputDirectoryTextBox.Text = _settings.TicketListOutputDirectory;
            LUACOutputDirectoryTextBox.Text = _settings.LuacOutputDirectory;
            LUAOutputDirectoryTextBox.Text = _settings.LuaOutputDirectory;
            InfToolOutputDirectoryTextBox.Text = _settings.InfToolOutputDirectory;
            CacheOutputDirectoryTextBox.Text = _settings.CacheOutputDirectory;
            cpuPercentageTextBox.Text = _settings.CpuPercentage.ToString();
            ThemeColorPicker.SelectedColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_settings.ThemeColor);
            switch (_settings.FileOverwriteBehavior)
            {
                case OverwriteBehavior.Skip:
                    RadioButtonFileSkip.IsChecked = true;
                    break;
                case OverwriteBehavior.Rename:
                    RadioButtonFileRename.IsChecked = true;
                    break;
                case OverwriteBehavior.Overwrite:
                    RadioButtonFileOverwrite.IsChecked = true;
                    break;
            };
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

            SelectLastUsedTab(_settings.LastTabUsed);
        }




        private void CdsOutputDirectoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.CdsOutputDirectory = CdsOutputDirectoryTextBox.Text;
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
            if (_settings != null && HcdbOutputDirectoryTextBox != null)
            {
                _settings.HcdbOutputDirectory = HcdbOutputDirectoryTextBox.Text;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void SqlOutputDirectoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null && sqlOutputDirectoryTextBox != null)
            {
                _settings.SqlOutputDirectory = sqlOutputDirectoryTextBox.Text;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void TicketListOutputDirectoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null && TicketListOutputDirectoryTextBox != null)
            {
                _settings.TicketListOutputDirectory = TicketListOutputDirectoryTextBox.Text;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void LuacOutputDirectoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null && LUACOutputDirectoryTextBox != null)
            {
                _settings.LuacOutputDirectory = LUACOutputDirectoryTextBox.Text;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void LuaOutputDirectoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null && LUAOutputDirectoryTextBox != null)
            {
                _settings.LuaOutputDirectory = LUAOutputDirectoryTextBox.Text;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void InfToolOutputDirectoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null && InfToolOutputDirectoryTextBox != null)
            {
                _settings.InfToolOutputDirectory = InfToolOutputDirectoryTextBox.Text;
                SettingsManager.SaveSettings(_settings);
            }
        }

        private void CacheOutputDirectoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null && CacheOutputDirectoryTextBox != null)
            {
                _settings.CacheOutputDirectory = CacheOutputDirectoryTextBox.Text;
                SettingsManager.SaveSettings(_settings);
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
        private string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.log");
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
                    logBuffer.Add($"{DateTime.Now}: {message}");

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
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.png");
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



        private void CpuPercentageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null && int.TryParse(cpuPercentageTextBox.Text, out int cpuPercentage))
            {
                _settings.CpuPercentage = cpuPercentage;
                SettingsManager.SaveSettings(_settings);
            }
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



        private void ClickToBrowseArchiveCreatorHandler(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("Archive Creation: Browsing for files Initiated");

            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "ZIP files (*.zip)|*.zip",
                Multiselect = true
            };

            string message = "No items selected.";
            int displayTime = 2000; // Default to 2 seconds for the message display.

            if (openFileDialog.ShowDialog() == true)
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
                    });
                    displayTime = 1000; // Change display time since items were added
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

            TemporaryMessageHelper.ShowTemporaryMessage(ArchiveCreatorDragAreaText, message, 2000);
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




        // Placeholder method for the archive creation process
        private Task<bool> CreateArchiveAsync(string[] itemPaths, ArchiveTypeSetting type)
        {
            // Here you would log the start of the archive creation process
            LogDebugInfo($"Archive Creation: Beginning Archive Creation for {itemPaths.Length} items with type {type}.");

            int i = 0;

            foreach (string itemPath in itemPaths)
            {
                LogDebugInfo($"Archive Creation: Processing item {i + 1}: {itemPath}");
                if (itemPath.ToLower().EndsWith(".zip"))
                {

                    string filename = Path.GetFileNameWithoutExtension(itemPath);
                    LogDebugInfo($"Archive Creation: Processing item {i + 1}: Extracting ZIP: {filename}");

                    // Combine the temporary folder path with the unique folder name
                    string temppath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                    LogDebugInfo($"Archive Creation: Processing item {i + 1}: Temporary extraction path: {temppath}");

                    UncompressFile(itemPath, temppath);

                    bool sdat = false;
                    IEnumerable<string> enumerable = Directory.EnumerateFiles(temppath, "*.*", SearchOption.AllDirectories);
                    BARArchive? bararchive = null;

                    // Declare the fileExtension variable
                    string fileExtension = "";

                    switch (_settings.ArchiveTypeSettingRem)
                    {
                        case ArchiveTypeSetting.BAR:
                            bararchive = new BARArchive($"{_settings.BarSdatSharcOutputDirectory}/{filename}.BAR", temppath, Convert.ToInt32(ArchiveCreatorTimestampTextBox.Text, 16), false, true);
                            fileExtension = ".BAR"; // Set file extension
                            break;
                        case ArchiveTypeSetting.BAR_S:
                            bararchive = new BARArchive($"{_settings.BarSdatSharcOutputDirectory}/{filename}.bar", temppath, Convert.ToInt32(ArchiveCreatorTimestampTextBox.Text, 16), true, true);
                            fileExtension = ".bar"; // Set file extension
                            break;
                        case ArchiveTypeSetting.SDAT:
                            bararchive = new BARArchive($"{_settings.BarSdatSharcOutputDirectory}/{filename}.BAR", temppath, Convert.ToInt32(ArchiveCreatorTimestampTextBox.Text, 16), false, true);
                            sdat = true;
                            fileExtension = ".sdat";  // Set file extension
                            break;
                        case ArchiveTypeSetting.CORE_SHARC:
                            bararchive = new BARArchive($"{_settings.BarSdatSharcOutputDirectory}/{filename}.SHARC", temppath, Convert.ToInt32(ArchiveCreatorTimestampTextBox.Text, 16), true, true, ToolsImpl.base64DefaultSharcKey);
                            fileExtension = ".SHARC"; // Set file extension
                            break;
                        case ArchiveTypeSetting.SDAT_SHARC:
                            sdat = true;
                            bararchive = new BARArchive($"{_settings.BarSdatSharcOutputDirectory}/{filename}.SHARC", temppath, Convert.ToInt32(ArchiveCreatorTimestampTextBox.Text, 16), true, true, ToolsImpl.base64CDNKey2);
                            fileExtension = ".sdat"; // Set file extension
                            break;
                        case ArchiveTypeSetting.CONFIG_SHARC:
                            bararchive = new BARArchive($"{_settings.BarSdatSharcOutputDirectory}/{filename}.sharc", temppath, Convert.ToInt32(ArchiveCreatorTimestampTextBox.Text, 16), true, true, ToolsImpl.base64CDNKey2);
                            fileExtension = ".sharc"; // Set file extension
                            break;
                    }

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
                            bararchive = new BARArchive($"{_settings.BarSdatSharcOutputDirectory}/{filename}.BAR", folderPath, Convert.ToInt32(ArchiveCreatorTimestampTextBox.Text, 16), false, true);
                            fileExtension = ".BAR"; // Set the file extension
                            break;
                        case ArchiveTypeSetting.BAR_S:
                            bararchive = new BARArchive($"{_settings.BarSdatSharcOutputDirectory}/{filename}.bar", folderPath, Convert.ToInt32(ArchiveCreatorTimestampTextBox.Text, 16), true, true);
                            fileExtension = ".bar"; // Set the file extension
                            break;
                        case ArchiveTypeSetting.SDAT:
                            bararchive = new BARArchive($"{_settings.BarSdatSharcOutputDirectory}/{filename}.BAR", folderPath, Convert.ToInt32(ArchiveCreatorTimestampTextBox.Text, 16), false, true);
                            sdat = true;
                            fileExtension = ".sdat"; // Set the file extension
                            break;
                        case ArchiveTypeSetting.CORE_SHARC:
                            bararchive = new BARArchive($"{_settings.BarSdatSharcOutputDirectory}/{filename}.SHARC", folderPath, Convert.ToInt32(ArchiveCreatorTimestampTextBox.Text, 16), true, true, ToolsImpl.base64DefaultSharcKey);
                            fileExtension = ".SHARC"; // Set the file extension
                            break;
                        case ArchiveTypeSetting.SDAT_SHARC:
                            sdat = true;
                            bararchive = new BARArchive($"{_settings.BarSdatSharcOutputDirectory}/{filename}.SHARC", folderPath, Convert.ToInt32(ArchiveCreatorTimestampTextBox.Text, 16), true, true, ToolsImpl.base64CDNKey2);
                            fileExtension = ".sdat"; // Set the file extension
                            break;
                        case ArchiveTypeSetting.CONFIG_SHARC:
                            bararchive = new BARArchive($"{_settings.BarSdatSharcOutputDirectory}/{filename}.sharc", folderPath, Convert.ToInt32(ArchiveCreatorTimestampTextBox.Text, 16), true, true, ToolsImpl.base64CDNKey2);
                            fileExtension = ".sharc"; // Set the file extension
                            break;
                    }


                    bararchive.AllowWhitespaceInFilenames = true;

                    foreach (string path in enumerable)
                    {
                        var fullPath = Path.Combine(folderPath, path);
                        bararchive.AddFile(fullPath);
                        LogDebugInfo($"Archive Creation: Processing item {i + 1}: Added file to archive from directory: {fullPath}");
                    }

                    // Get the name of the directory
                    string directoryName = new DirectoryInfo(folderPath).Name;
                    LogDebugInfo($"Archive Creation: Processing item { i + 1}: Processing directory into archive: {directoryName}");

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

                    LogDebugInfo("Archive Creation: Completed processing item for archive creation.");

                }

                i++;
            }

            // Log the completion and result of the archive creation process
            LogDebugInfo("Archive Creation: Process Success");

            return Task.FromResult(true); 
        }


        // TAB 1: Logic for Unpacking Archives

        private async void ArchiveUnpackerExecuteButtonClick(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("Archive Unpacking: Process Initiated");
            TemporaryMessageHelper.ShowTemporaryMessage(ArchiveUnpackerDragAreaText, "Processing....", 2000);

            Directory.CreateDirectory(_settings.MappedOutputDirectory);
            LogDebugInfo($"Archive Unpacking: Output directory created at {_settings.MappedOutputDirectory}");

            string filesToUnpack = ArchiveUnpackerTextBox.Text;
            if (!string.IsNullOrWhiteSpace(filesToUnpack))
            {
                string[] filePaths = filesToUnpack.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                LogDebugInfo($"Archive Unpacking: Starting unpacking for {filePaths.Length} files");
                bool unpackingSuccess = await UnpackFilesAsync(filePaths);

                string message = unpackingSuccess ? $"Success: {filePaths.Length} Files Unpacked" : "Unpacking Failed";
                TemporaryMessageHelper.ShowTemporaryMessage(ArchiveUnpackerDragAreaText, message, 2000);

                LogDebugInfo($"Archive Unpacking: Result - {message}");
            }
            else
            {
                LogDebugInfo("Archive Unpacking: Aborted - No files listed for Unpacking.");
                TemporaryMessageHelper.ShowTemporaryMessage(ArchiveUnpackerDragAreaText, "No files listed for Unpacking.", 2000);
            }
        }



        private void ArchiveUnpackerDragDropHandler(object sender, DragEventArgs e)
        {
            LogDebugInfo("Archive Unpacking: Drag and Drop Initiated");
            TemporaryMessageHelper.ShowTemporaryMessage(ArchiveUnpackerDragAreaText, "Processing....", 2000);

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedItems = (string[])e.Data.GetData(DataFormats.FileDrop);
                List<string> validFiles = new List<string>();

                var validExtensions = new[] { ".bar", ".sdat", ".sharc", ".dat" };
                foreach (var item in droppedItems)
                {
                    if (Directory.Exists(item))
                    {
                        var filesInDirectory = Directory.GetFiles(item, "*.*", SearchOption.AllDirectories)
                            .Where(file => validExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                            .ToList();
                        if (filesInDirectory.Count > 0)
                        {
                            validFiles.AddRange(filesInDirectory);
                        }
                    }
                    else if (File.Exists(item) && validExtensions.Contains(Path.GetExtension(item).ToLowerInvariant()))
                    {
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
                        var existingFilesSet = new HashSet<string>(ArchiveUnpackerTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        int initialCount = existingFilesSet.Count;

                        existingFilesSet.UnionWith(validFiles); // Automatically removes duplicates
                        newFilesCount = existingFilesSet.Count - initialCount;
                        duplicatesCount = validFiles.Count - newFilesCount;

                        ArchiveUnpackerTextBox.Text = string.Join(Environment.NewLine, existingFilesSet);

                        message = $"{newFilesCount} file(s) added, {duplicatesCount} duplicate(s) filtered";
                        TemporaryMessageHelper.ShowTemporaryMessage(ArchiveUnpackerDragAreaText, message, 2000);

                        LogDebugInfo($"Archive Unpacking: {newFilesCount} files added, {duplicatesCount} duplicates filtered from Drag and Drop.");
                    });
                }
                else
                {
                    LogDebugInfo("Archive Unpacking: Drag and Drop - No valid files found.");
                    message = "No valid files found.";
                    TemporaryMessageHelper.ShowTemporaryMessage(ArchiveUnpackerDragAreaText, message, 2000);
                }
            }
            else
            {
                LogDebugInfo("Archive Unpacking: Drag and Drop - No Data Present.");
                TemporaryMessageHelper.ShowTemporaryMessage(ArchiveUnpackerDragAreaText, "Drag and Drop operation failed - No Data Present.", 3000);
            }
        }



        private void ClickToBrowseArchiveUnpackerHandler(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("Archive Unpacking: Browsing for files Initiated");

            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Supported files (*.bar;*.sdat;*.sharc;*.dat)|*.bar;*.sdat;*.sharc;*.dat",
                Multiselect = true
            };

            string message = "No files selected.";
            int displayTime = 2000; // Default to 2 seconds for the message display.

            if (openFileDialog.ShowDialog() == true)
            {
                var selectedFiles = openFileDialog.FileNames;

                if (selectedFiles.Any())
                {
                    LogDebugInfo($"Archive Unpacking: {selectedFiles.Length} files selected via File Browser.");

                    Dispatcher.Invoke(() =>
                    {
                        var existingFilesSet = new HashSet<string>(ArchiveUnpackerTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        int initialCount = existingFilesSet.Count;
                        existingFilesSet.UnionWith(selectedFiles);
                        int newFilesCount = existingFilesSet.Count - initialCount;
                        int duplicatesCount = selectedFiles.Length - newFilesCount;

                        ArchiveUnpackerTextBox.Text = string.Join(Environment.NewLine, existingFilesSet);

                        string addedFilesMessage = newFilesCount > 0 ? $"{newFilesCount} files added" : "No new files added";
                        string duplicatesMessage = duplicatesCount > 0 ? $", {duplicatesCount} duplicates filtered" : "";
                        message = addedFilesMessage + duplicatesMessage;

                        TemporaryMessageHelper.ShowTemporaryMessage(ArchiveUnpackerDragAreaText, message, 2000);
                    });
                }
                else
                {
                    LogDebugInfo("Archive Unpacking: File Browser - No compatible files were selected.");
                    message = "No compatible files selected.";
                }
            }
            else
            {
                LogDebugInfo("Archive Unpacking: File Browser - Dialog Cancelled.");
            }

            TemporaryMessageHelper.ShowTemporaryMessage(ArchiveUnpackerDragAreaText, message, 2000);
        }


        // Placeholder method for the unpacking process
        private async Task<bool> UnpackFilesAsync(string[] filePaths)
        {
            // Log the start of the unpacking process
            LogDebugInfo($"Archive Unpacking: Beginning unpacking process for {filePaths.Length} files");

            string ogfilename = string.Empty;

            string Outputpath = MappedOutputDirectoryTextBox.Text;

            // Ensure the output directory exists
            Directory.CreateDirectory(Outputpath);

            foreach (string filePath in filePaths)
            {
                if (File.Exists(filePath))
                {
                    string filename = Path.GetFileName(filePath);

                    // Determine the action based on file extension
                    if (filename.ToLower().EndsWith(".bar") || filename.ToLower().EndsWith(".dat"))
                    {
                        string barfile = Outputpath + $"/{filename}";
                        // Copy the file to the output directory for processing
                        File.WriteAllBytes(barfile, File.ReadAllBytes(filePath));
                        await RunUnBAR.Run(Directory.GetCurrentDirectory(), barfile, Outputpath, false);
                        ogfilename = filename;
                        filename = filename[..^4];
                        // Delete the copied file after processing
                        if (File.Exists(barfile))
                            File.Delete(barfile);
                    }
                    else if (filename.ToLower().EndsWith(".sharc"))
                    {
                        string barfile = Outputpath + $"/{filename}";
                        File.WriteAllBytes(barfile, File.ReadAllBytes(filePath));
                        await RunUnBAR.Run(Directory.GetCurrentDirectory(), barfile, Outputpath, false);
                        ogfilename = filename;
                        filename = filename[..^6];
                        if (File.Exists(barfile))
                            File.Delete(barfile);
                    }
                    else if (filename.ToLower().EndsWith(".sdat"))
                    {
                        // Directly use the original path without copying the file
                        await RunUnBAR.Run(Directory.GetCurrentDirectory(), filePath, Outputpath, true);
                        ogfilename = filename;
                        filename = filename[..^5];
                    }
                    else if (filename.ToLower().EndsWith(".zip"))
                    {
                        string barfile = Outputpath + $"/{filename}";
                        UncompressFile(barfile, Outputpath);
                        ogfilename = filename;
                        filename = filename[..^4];
                        if (File.Exists(barfile))
                            File.Delete(barfile);
                    }
                    else
                    {
                        // Skip the file if it doesn't match any known types
                        continue;
                    }

                    // Determine if the directory exists and has files for mapping
                    string directoryToMap = Directory.Exists(Outputpath + $"/{filename}") ? Outputpath + $"/{filename}" : Outputpath;
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
                }
            }

            return true;
        }


        private async Task ExperimentalMapperAsync(string directoryPath, string ogfilename)
        {

            var uuidMappings = LoadUUIDMappings();
            DeleteDatFilesInMappedOutputDirectory();

            Application.Current.Dispatcher.Invoke(() =>
            {
                ArchiveUnpackerDragAreaText.Text = $"MAPPING {ogfilename}.. Please Wait...";
                ArchiveUnpackerTextBox.AppendText($"\n\nARCHIVE UNPACKER: Extracted {ogfilename} Successfully\n");
                ArchiveUnpackerTextBox.AppendText($"ARCHIVE UNPACKER: Mapping {ogfilename} Please Wait...");
                ArchiveUnpackerTextBox.ScrollToEnd();
            });


            var allFiles = Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.AllDirectories);

            var pathsFound = await ScanFilesAsync(allFiles);

            var pathHashes = new Dictionary<int, string>();
            foreach (var path in pathsFound.Keys)
            {
                int hash = ComputeAFSHash(path.ToLower());
                if (!pathHashes.ContainsKey(hash))
                {
                    pathHashes.Add(hash, path.ToUpper());

                }
            }

            foreach (var fullPath in allFiles)
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullPath).ToUpper(); // Convert to uppercase

                if (fileNameWithoutExtension != null)
                {
                    if (int.TryParse(fileNameWithoutExtension, System.Globalization.NumberStyles.HexNumber, null, out int fileHash))
                    {
                        if (pathHashes.TryGetValue(fileHash, out var matchingPath))
                        {
                            var newFileName = matchingPath.Replace('/', Path.DirectorySeparatorChar);
                            var newFilePath = Path.Combine(directoryPath, newFileName);

                            try
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));
                                File.Move(fullPath, newFilePath, true);

                            }
                            catch (Exception ex)
                            {
                                LogDebugInfo($"Error renaming file from {fullPath} to {newFilePath}: {ex.Message}");
                            }
                        }
                    }
                }
            }

            await RenameLeftoverFilesAsync(directoryPath);

            await CheckAndHandleUnmappedFiles(directoryPath, pathsFound.Keys.ToList(), uuidMappings);


            Application.Current.Dispatcher.Invoke(() =>
            {
                ArchiveUnpackerTextBox.AppendText($"\nARCHIVE UNPACKER: SUCCESS {ogfilename} Mapped fully!");
                ArchiveUnpackerTextBox.ScrollToEnd();
                ArchiveUnpackerDragAreaText.Text = $"SUCCESS {ogfilename} Mapped Fully!";
            });
        }


        private async Task CheckAndHandleUnmappedFiles(string directoryPath, List<string> paths, Dictionary<int, string> uuidMappings)
        {
            
            string uuidPrefixFound = string.Empty; // To track if a UUID prefix is found
            List<string> modifiedPaths = new List<string>(); // To store modified paths

            // Define the pattern for unmapped files
            string pattern = @"^[A-F0-9]{8}(\..+)?$";
            var unmappedFiles = Directory.EnumerateFiles(directoryPath, "*", SearchOption.TopDirectoryOnly)
                                          .Where(file => Regex.IsMatch(Path.GetFileNameWithoutExtension(file).ToUpper(), pattern));

            foreach (var file in unmappedFiles)
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file).ToUpper();
                if (int.TryParse(fileNameWithoutExtension, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int fileHash) && uuidMappings.TryGetValue(fileHash, out var uuidPath))
                {
                    // Log and create directory based on UUID for unmapped files
                    
                    var targetDirectoryPath = Path.Combine(directoryPath, uuidPath.TrimStart('/'));
                    Directory.CreateDirectory(targetDirectoryPath);
                    

                    // Capture the UUID prefix when found
                    uuidPrefixFound = uuidPath;
                    break; // Stop processing further unmapped files once a UUID is found
                }
                else
                {
                   
                }
            }

            // If a UUID prefix is found, prepare and log modified paths
            if (!string.IsNullOrEmpty(uuidPrefixFound))
            {
                foreach (var path in paths)
                {
                    string modifiedPath = $"{uuidPrefixFound}{path}";
                    modifiedPaths.Add(modifiedPath); // Add modified path to list
                   
                }

                // Call the function to process and rename files based on modified paths
                await ProcessAndRenameFilesWithModifiedPaths(directoryPath, modifiedPaths);

                // If a UUID prefix was found, try to rename the output directory
                if (!string.IsNullOrEmpty(uuidPrefixFound))
                {
                    // Extract UUID from the found prefix
                    var uuidMatch = Regex.Match(uuidPrefixFound, @"OBJECTS[\\\/]([A-F0-9]{8}-[A-F0-9]{8}-[A-F0-9]{8}-[A-F0-9]{8})[\\\/]?");

                    if (uuidMatch.Success)
                    {
                        string uuid = uuidMatch.Groups[1].Value;
                        RenameOutputDirectory(directoryPath, uuid);
                    }
                }
            }
        }

        private async Task ProcessAndRenameFilesWithModifiedPaths(string directoryPath, List<string> modifiedPaths)
        {
            // Dictionary to hold the hash and new path mapping
            var hashToPathMapping = new Dictionary<int, string>();

            // Compute hash for each modified path and add to dictionary
            foreach (var path in modifiedPaths)
            {
                int hash = ComputeAFSHash(path);
                if (!hashToPathMapping.ContainsKey(hash))
                {
                    hashToPathMapping[hash] = path.ToUpper();  // Convert path to uppercase before adding
                }
            }

            // Now attempt to rename files based on the hash mapping
            var allFiles = Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.AllDirectories);
            foreach (var file in allFiles)
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file).ToUpper();
                if (int.TryParse(fileNameWithoutExtension, System.Globalization.NumberStyles.HexNumber, null, out int fileHash) && hashToPathMapping.TryGetValue(fileHash, out var newPath))
                {
                    // Generate full path for the new location of this file, ensure it's uppercase
                    var newFilePath = Path.Combine(directoryPath, newPath.Replace('/', Path.DirectorySeparatorChar)).ToUpper(); // Ensure new file path is uppercase

                    try
                    {
                        // Ensure the directory for the new path exists (Note: Directories are case-insensitive in Windows but case-sensitive in UNIX-based systems)
                        Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));

                        // Rename the file to its new path (now in uppercase)
                        File.Move(file, newFilePath, overwrite: true);
                        
                    }
                    catch (Exception ex)
                    {
                        LogDebugInfo($"Error renaming file {file} based on modified path hash: {ex.Message}");
                    }
                }
            }
        }

        private void RenameOutputDirectory(string directoryPath, string uuid)
        {
            var parentDirectory = Directory.GetParent(directoryPath).FullName;
            var newDirectoryName = directoryPath.Replace("object", uuid);
            var newDirectoryPath = Path.Combine(parentDirectory, newDirectoryName);

            // Check if directory already exists to avoid exceptions
            if (!Directory.Exists(newDirectoryPath))
            {
                Directory.Move(directoryPath, newDirectoryPath);
                LogDebugInfo($"Renamed output directory: {directoryPath} to {newDirectoryPath}");
            }
            else
            {
                LogDebugInfo($"Could not rename the directory as {newDirectoryPath} already exists.");
            }
        }



        private Dictionary<int, string> LoadUUIDMappings()
        {
            string uuidHelperFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uuid_helper.txt");
            var uuidMappings = new Dictionary<int, string>();

            if (File.Exists(uuidHelperFilePath))
            {
                foreach (var line in File.ReadAllLines(uuidHelperFilePath))
                {
                    var parts = line.Split(':');
                    if (parts.Length == 2)
                    {
                        if (int.TryParse(parts[0], System.Globalization.NumberStyles.HexNumber, null, out int hash))
                        {
                            uuidMappings[hash] = $"OBJECTS/{parts[1].Trim()}/";
                        }
                    }
                }
            }
            else
            {
                LogDebugInfo("UUID helper file not found.");
            }

            return uuidMappings;
        }



       
       private void DeleteDatFilesInMappedOutputDirectory()
        {
            // Assuming _settings.MappedOutputDirectory holds the path to the target directory
            string outputDirectoryPath = _settings.MappedOutputDirectory;
            if (string.IsNullOrWhiteSpace(outputDirectoryPath))
            {
                LogDebugInfo("Mapped output directory path is not set.");
                return;
            }

           
            try
            {
                var datFiles = Directory.EnumerateFiles(outputDirectoryPath, "*.dat", SearchOption.TopDirectoryOnly);
                foreach (var file in datFiles)
                {
                    File.Delete(file);
                    LogDebugInfo($"Deleted .dat file: {file}");
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error deleting .dat files in mapped output directory: {ex.Message}");
            }

           
            try
            {
                var mapFiles = Directory.EnumerateFiles(outputDirectoryPath, "*.map", SearchOption.AllDirectories);
                foreach (var file in mapFiles)
                {
                    File.Delete(file);
                    LogDebugInfo($"Deleted .map file: {file}");
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error deleting .map files in mapped output directory: {ex.Message}");
            }
        }

        private bool CheckForUnmappedFiles(string directoryPath)
        {
            string pattern = @"^[A-F0-9]{8}(\..+)?$"; // Pattern for unmapped files
            var unmappedFiles = Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories)
                                         .Where(file => Regex.IsMatch(Path.GetFileNameWithoutExtension(file).ToUpper(), pattern));

            return unmappedFiles.Any(); // Returns true if any unmapped files exist
        }


        private async Task RenameLeftoverFilesAsync(string directoryPath)
        {
            string helperFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scene_file_mapper_helper.txt");

            if (!File.Exists(helperFilePath))
            {
                LogDebugInfo("Helper file not found.");
                return;
            }

            var fileMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in File.ReadAllLines(helperFilePath))
            {
                var parts = line.Split(new[] { ':' }, 2);
                if (parts.Length == 2)
                {
                    string hashPart = parts[0].Trim();
                    string pathPart = parts[1].Trim().Replace('/', '\\'); // Ensure using backslashes
                    fileMappings[hashPart] = pathPart;
                }
            }

            var filesInRootDirectory = Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly);

            foreach (var filePath in filesInRootDirectory)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);

                if (fileMappings.TryGetValue(fileName.ToUpper(), out var newRelativePath))
                {
                    // Ensure that newRelativePath doesn't start with a directory separator
                    newRelativePath = newRelativePath.TrimStart('\\');
                    var newFilePath = Path.Combine(directoryPath, newRelativePath);

                    // Log the new file path to check for issues
                    

                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));
                        File.Move(filePath, newFilePath);
                        
                    }
                    catch (Exception ex)
                    {
                        LogDebugInfo($"Error renaming file from {filePath} to {newFilePath}: {ex.Message}");
                    }
                }
            }
        }

        private async Task<ConcurrentDictionary<string, bool>> ScanFilesAsync(IEnumerable<string> filePaths)
        {
            var options = new ParallelOptions { MaxDegreeOfParallelism = 12 };
            var pathsFound = new ConcurrentDictionary<string, bool>();

            string[] extensions = {
        ".mdl", ".dds", ".repertoire_circuit", ".luac", ".lua", ".xml", ".efx", ".effect", ".ttf", ".hkx",
        ".probe", ".ocean", ".skn", ".ani", ".mp3", ".atmos", ".png", ".bin", ".raw", ".ini", ".enemy",
        ".ui-setup", ".cam-def", ".level-setup", ".node-def", ".spline-def", ".tmx", ".json", ".atgi", ".bank",
        ".bnk", ".agf", ".jpg", ".mp4", ".dat", ".svml", ".oel", ".wav", ".gui-setup", ".tga",
        ".rig", ".txt", ".cdata", ".atmos"
    };
            string extensionsPattern = string.Join("|", extensions.Select(Regex.Escape));

            string combinedPattern = $@"([\\/\x22\]][^\n\r:*\?""<>|\s]+?({extensionsPattern}))";

            await Task.Run(() =>
            {
                Parallel.ForEach(filePaths, options, filePath =>
                {
                    try
                    {
                        if (File.Exists(filePath))
                        {
                            var fileContent = File.ReadAllBytes(filePath);
                            var contentString = Encoding.Default.GetString(fileContent);
                            contentString = Regex.Replace(contentString, "[\x00-\x05\x1E\xCB]", "\"");



                            var matches = Regex.Matches(contentString, combinedPattern, RegexOptions.IgnoreCase);

                            foreach (Match match in matches)
                            {
                                var matchedPath = TransformPath(match.Value);
                                CheckAndAddSpecialCases(matchedPath, pathsFound);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions as needed
                    }
                });
            });

            return pathsFound;
        }



        private void CheckAndAddSpecialCases(string matchedPath, ConcurrentDictionary<string, bool> pathsFound)
        {
           

            pathsFound.TryAdd(matchedPath, true);
            string[] staticFiles = { "localisation.xml", "resources.xml", "object.xml", "editor.oxml", "catalogueentry.xml", "maker.png", "large.png", "small.png", "object.odc" };
            foreach (var staticFile in staticFiles)
            {
                pathsFound.TryAdd(staticFile, true);
            }

            if (matchedPath.EndsWith(".atmos", StringComparison.OrdinalIgnoreCase))
            {
                var cdataPath = matchedPath.Replace(".atmos", ".cdata");
                pathsFound.TryAdd(cdataPath, true);
            }
            if (matchedPath.EndsWith(".probe", StringComparison.OrdinalIgnoreCase))
            {
                string[] replacements = { ".ocean", ".scene", ".sdc" };
                foreach (var replacement in replacements)
                {
                    var newPath = matchedPath.Replace(".probe", replacement);
                    pathsFound.TryAdd(newPath, true);
                }
            }
        }


        private string TransformPath(string matchedPath)
        {
            
            char[] charsToTrim = { '\x01', '\x02', '\x03', '\x04', '\x22', '\x23', '\x5D', '\x2F', '\x5C' };
            matchedPath = matchedPath.TrimStart(charsToTrim);
            matchedPath = Regex.Replace(matchedPath, "(file:///resource_root/build/)|(file://resource_root/build/)|(resource_root/build/)", "", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            matchedPath = Regex.Replace(matchedPath, @"(?<=\b)(NATGParticles|TATGParticles|xenvironments|Tenvironments)", m => m.Value.Substring(1), RegexOptions.IgnoreCase | RegexOptions.Compiled);
            matchedPath = matchedPath.Replace("/", "\\");
            return matchedPath;
        }

        private int ComputeAFSHash(string text)
        {
            string normalizedText;
            if (text.StartsWith("0X", StringComparison.OrdinalIgnoreCase))
            {
                normalizedText = text.Substring(2);
            }
            else
            {
                normalizedText = text;
            }

            normalizedText = normalizedText
                .Replace(Path.DirectorySeparatorChar, '/')
                .ToLower();

            int hash = 0;
            foreach (char ch in normalizedText)
                hash = hash * 37 + ch;

            return hash;
        }




        // TAB 2: Logic for CDS Encryption 

        private async void CDSEncrypterExecuteButtonClick(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("CDS Encryption: Process Initiated");
            TemporaryMessageHelper.ShowTemporaryMessage(CDSEncrypterDragAreaText, "Processing....", 2000);

            // Assuming _settings.CDSEncrypterOutputDirectory is the relevant setting
            if (!Directory.Exists(_settings.CdsOutputDirectory))
            {
                Directory.CreateDirectory(_settings.CdsOutputDirectory);
                LogDebugInfo($"CDS Encryption: Output directory created at {_settings.CdsOutputDirectory}");
            }

            string filesToEncrypt = CDSEncrypterTextBox.Text;
            if (!string.IsNullOrWhiteSpace(filesToEncrypt))
            {
                string[] filePaths = filesToEncrypt.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                LogDebugInfo($"CDS Encryption: Starting encryption for {filePaths.Length} files");
                bool encryptionSuccess = await EncryptFilesAsync(filePaths);

                string message = encryptionSuccess ? $"Success: {filePaths.Length} Files Encrypted (Simulated)" : "Encryption Failed";
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




        private void ClickToBrowseCDSEncryptHandler(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("CDS Encryption: Browsing for files Initiated");

            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Supported files (*.sdc;*.odc;*.xml;*.bar)|*.sdc;*.odc;*.xml;*.bar",
                Multiselect = true
            };

            string message = "No files selected.";
            int displayTime = 2000;

            if (openFileDialog.ShowDialog() == true)
            {
                var selectedFiles = openFileDialog.FileNames;
                var validExtensions = new[] { ".sdc", ".odc", ".xml", ".bar" };

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

                        message = $"{newFilesCount} file(s) added, {duplicatesCount} duplicate(s) filtered";
                        TemporaryMessageHelper.ShowTemporaryMessage(CDSEncrypterDragAreaText, message, 2000);

                        LogDebugInfo($"CDS Encryption: {newFilesCount} files added, {duplicatesCount} duplicates filtered via File Browser.");
                    });
                }
                else
                {
                    LogDebugInfo("CDS Encryption: File Browser - No compatible files were selected.");
                    message = "No compatible files selected.";
                }
            }
            else
            {
                LogDebugInfo("CDS Encryption: File Browser - Dialog Cancelled.");
            }

            TemporaryMessageHelper.ShowTemporaryMessage(CDSEncrypterDragAreaText, message, 2000);
        }



        // Placeholder method for the encryption process
        private Task<bool> EncryptFilesAsync(string[] filePaths)
        {
            // Log the start of the encryption process
            LogDebugInfo($"CDS Encryption: Beginning encryption process for {filePaths.Length} files");

            // TODO: Implement the actual encryption logic here

            // Log the completion and result of the encryption process
            LogDebugInfo("CDS Encryption: Encryption Process Completed - Simulated Success");

            return Task.FromResult(true); // Simulate success
        }





        // TAB 2: Logic for CDS Decryption

        private async void CDSDecrypterExecuteButtonClick(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("CDS Decryption: Process Initiated");
            TemporaryMessageHelper.ShowTemporaryMessage(CDSDecrypterDragAreaText, "Processing....", 2000);

            // Assuming _settings.CDSDecrypterOutputDirectory is the relevant setting
            if (!Directory.Exists(_settings.CdsOutputDirectory))
            {
                Directory.CreateDirectory(_settings.CdsOutputDirectory);
                LogDebugInfo($"CDS Decryption: Output directory created at {_settings.CdsOutputDirectory}");
            }

            string filesToDecrypt = CDSDecrypterTextBox.Text;
            if (!string.IsNullOrWhiteSpace(filesToDecrypt))
            {
                string[] filePaths = filesToDecrypt.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                LogDebugInfo($"CDS Decryption: Starting decryption for {filePaths.Length} files");
                bool decryptionSuccess = await DecryptFilesAsync(filePaths);

                string message = decryptionSuccess ? $"Success: {filePaths.Length} Files Decrypted (Simulated)" : "Decryption Failed";
                TemporaryMessageHelper.ShowTemporaryMessage(CDSDecrypterDragAreaText, message, 2000);

                LogDebugInfo($"CDS Decryption: Result - {message}");
            }
            else
            {
                LogDebugInfo("CDS Decryption: Aborted - No files listed for Decryption.");
                TemporaryMessageHelper.ShowTemporaryMessage(CDSDecrypterDragAreaText, "No files listed for Decryption.", 2000);
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
                string message = string.Empty;
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

                        message = $"{newFilesCount} file(s) added, {duplicatesCount} duplicate(s) filtered";
                        TemporaryMessageHelper.ShowTemporaryMessage(CDSDecrypterDragAreaText, message, 2000);

                        LogDebugInfo($"CDS Decryption: {newFilesCount} files added, {duplicatesCount} duplicates filtered from Drag and Drop.");
                    });
                }
                else
                {
                    LogDebugInfo("CDS Decryption: Drag and Drop - No valid files found.");
                    message = "No valid files found.";
                    TemporaryMessageHelper.ShowTemporaryMessage(CDSDecrypterDragAreaText, message, 2000);
                }
            }
            else
            {
                LogDebugInfo("CDS Decryption: Drag and Drop - No Data Present.");
                TemporaryMessageHelper.ShowTemporaryMessage(CDSDecrypterDragAreaText, "Drag and Drop operation failed - No Data Present.", 2000);
            }
        }


        private void ClickToBrowseCDSDecryptHandler(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("CDS Decryption: Browsing for files Initiated");

            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Supported files (*.sdc;*.odc;*.xml;*.bar)|*.sdc;*.odc;*.xml;*.bar",
                Multiselect = true
            };

            string message = "No files selected.";
            int displayTime = 2000; // Default to 2 seconds for the message display.

            if (openFileDialog.ShowDialog() == true)
            {
                var selectedFiles = openFileDialog.FileNames;
                var validExtensions = new[] { ".sdc", ".odc", ".xml", ".bar" }.Select(ext => ext.ToLowerInvariant()).ToArray();

                List<string> validFiles = selectedFiles
                    .Where(file => validExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                    .ToList();

                if (validFiles.Any())
                {
                    Dispatcher.Invoke(() =>
                    {
                        var existingFilesSet = new HashSet<string>(CDSDecrypterTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        int initialCount = existingFilesSet.Count;
                        existingFilesSet.UnionWith(validFiles); // This will automatically remove duplicates
                        int newFilesCount = existingFilesSet.Count - initialCount;
                        int duplicatesCount = validFiles.Count - newFilesCount;

                        CDSDecrypterTextBox.Text = string.Join(Environment.NewLine, existingFilesSet);

                        message = $"{newFilesCount} file(s) added, {duplicatesCount} duplicate(s) filtered";
                        TemporaryMessageHelper.ShowTemporaryMessage(CDSDecrypterDragAreaText, message, 2000);

                        LogDebugInfo($"CDS Decryption: {newFilesCount} files added, {duplicatesCount} duplicates filtered via File Browser.");
                    });
                }
                else
                {
                    LogDebugInfo("CDS Decryption: File Browser - No compatible files were selected.");
                    message = "No compatible files selected.";
                }
            }
            else
            {
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

            // Assuming _settings.HCDBEncrypterOutputDirectory is the relevant setting
            if (!Directory.Exists(_settings.HcdbOutputDirectory))
            {
                Directory.CreateDirectory(_settings.HcdbOutputDirectory);
                LogDebugInfo($"HCDB Conversion: Output directory created at {_settings.HcdbOutputDirectory}");
            }

            string filesToConvert = HCDBEncrypterTextBox.Text;
            if (!string.IsNullOrWhiteSpace(filesToConvert))
            {
                string[] filePaths = filesToConvert.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                LogDebugInfo($"HCDB Conversion: Starting conversion for {filePaths.Length} files");
                bool conversionSuccess = await ConvertSqlToHcdbAsync(filePaths);

                string message = conversionSuccess ? $"Success: {filePaths.Length} Files Converted to HCDB" : "Conversion Failed";
                TemporaryMessageHelper.ShowTemporaryMessage(HCDBEncrypterDragAreaText, message, 2000);

                LogDebugInfo($"HCDB Conversion: Result - {message}");
            }
            else
            {
                LogDebugInfo("HCDB Conversion: Aborted - No SQL files listed for conversion.");
                TemporaryMessageHelper.ShowTemporaryMessage(HCDBEncrypterDragAreaText, "No SQL files listed for conversion.", 2000);
            }
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



        private void ClickToBrowseHCDBEncryptHandler(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("HCDB Conversion: Browsing for SQL files Initiated");


            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "SQL files (*.sql)|*.sql",
                Multiselect = true
            };

            string message = "No files selected.";
            int displayTime = 2000; // Default to 2 seconds for the message display.

            if (openFileDialog.ShowDialog() == true)
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
                        string duplicatesMessage = duplicatesCount > 0 ? $", {duplicatesCount} duplicates filtered" : "";
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

                TemporaryMessageHelper.ShowTemporaryMessage(HCDBEncrypterDragAreaText, message, 2000);
            }
            else
            {
                LogDebugInfo("HCDB Conversion: File Browser - Dialog Cancelled.");
            }

            TemporaryMessageHelper.ShowTemporaryMessage(HCDBEncrypterDragAreaText, message, 2000);
        }



        // Placeholder method for SQL to HCDB conversion
        private Task<bool> ConvertSqlToHcdbAsync(string[] filePaths)
        {
            LogDebugInfo($"HCDB Conversion: Starting conversion for {filePaths.Length} SQL file(s)");

            // TODO: Implement the actual conversion logic here
            bool conversionResult = true; // Simulate success for now

            if (conversionResult)
            {
                LogDebugInfo("HCDB Conversion: Conversion process completed successfully");
            }
            else
            {
                LogDebugInfo("HCDB Conversion: Conversion process failed");
            }

            return Task.FromResult(conversionResult);
        }





        // TAB 3: Logic for packing HCDB to SQL

        // HCDB Decrypter execute button click
        private async void HCDBDecrypterExecuteButtonClick(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("HCDB to SQL Conversion: Process Initiated");
            TemporaryMessageHelper.ShowTemporaryMessage(HCDBDecrypterDragAreaText, "Processing....", 1000000);

            // Assuming _settings.HCDBDecrypterOutputDirectory is the relevant setting
            if (!Directory.Exists(_settings.SqlOutputDirectory))
            {
                Directory.CreateDirectory(_settings.SqlOutputDirectory);
                LogDebugInfo($"HCDB to SQL Conversion: Output directory created at {_settings.SqlOutputDirectory}");
            }

            string filesToConvert = HCDBDecrypterTextBox.Text;
            if (!string.IsNullOrWhiteSpace(filesToConvert))
            {
                string[] filePaths = filesToConvert.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                LogDebugInfo($"HCDB to SQL Conversion: Starting conversion for {filePaths.Length} files");
                bool conversionSuccess = await ConvertHcdbToSqlAsync(filePaths);

                string message = conversionSuccess ? $"Success: {filePaths.Length} Files Converted to SQL (Simulated)" : "Conversion Failed";
                TemporaryMessageHelper.ShowTemporaryMessage(HCDBDecrypterDragAreaText, message, 2000);

                LogDebugInfo($"HCDB to SQL Conversion: Result - {message}");
            }
            else
            {
                LogDebugInfo("HCDB to SQL Conversion: Aborted - No HCDB files listed for conversion.");
                TemporaryMessageHelper.ShowTemporaryMessage(HCDBDecrypterDragAreaText, "No HCDB files listed for conversion.", 2000);
            }
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


        private void ClickToBrowseHCDBDecryptHandler(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("HCDB to SQL Conversion: Browsing for HCDB files Initiated");

            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "HCDB files (*.hcdb)|*.hcdb",
                Multiselect = true
            };

            string message = "No files selected.";
            int displayTime = 2000; // Default to 2 seconds for the message display.

            if (openFileDialog.ShowDialog() == true)
            {
                var selectedFiles = openFileDialog.FileNames;

                if (selectedFiles.Any())
                {
                    Dispatcher.Invoke(() =>
                    {
                        var existingFilesSet = new HashSet<string>(HCDBDecrypterTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        int initialCount = existingFilesSet.Count;
                        existingFilesSet.UnionWith(selectedFiles);
                        int newFilesCount = existingFilesSet.Count - initialCount;
                        int duplicatesCount = selectedFiles.Length - newFilesCount;

                        HCDBDecrypterTextBox.Text = string.Join(Environment.NewLine, existingFilesSet);

                        string addedFilesMessage = newFilesCount > 0 ? $"{newFilesCount} HCDB files added" : "No new HCDB files added";
                        string duplicatesMessage = duplicatesCount > 0 ? $", {duplicatesCount} duplicates filtered" : "";
                        message = addedFilesMessage + duplicatesMessage;

                        LogDebugInfo($"HCDB to SQL Conversion: {newFilesCount} HCDB files added, {duplicatesCount} duplicates filtered via File Browser");
                    });
                    displayTime = 1000; // Change display time since files were added
                }
                else
                {
                    message = "No HCDB files selected.";
                    LogDebugInfo("HCDB to SQL Conversion: No HCDB files selected in File Browser.");
                }

                TemporaryMessageHelper.ShowTemporaryMessage(HCDBDecrypterDragAreaText, message, 2000);
            }
            else
            {
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





        // TAB 4: Logic for  Ticket LST

        private async void TicketLSTDecrypterExecuteButtonClick(object sender, RoutedEventArgs e)
        {
            TemporaryMessageHelper.ShowTemporaryMessage(TicketLSTDecrypterDragAreaText, "Processing...", 1000000);
            string filesToDecrypt = TicketLSTDecrypterTextBox.Text;
            if (!string.IsNullOrWhiteSpace(filesToDecrypt))
            {
                string[] filePaths = filesToDecrypt.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                await Task.Delay(100); // Simulate some asynchronous operation
                bool decryptionSuccess = await DecryptLstFilesAsync(filePaths);
                string message = decryptionSuccess ? $"Success: {filePaths.Length} LST Files Decrypted (Simulated)" : "Decryption Failed";
                TemporaryMessageHelper.ShowTemporaryMessage(TicketLSTDecrypterDragAreaText, message, 2000);
            }
            else
            {
                TemporaryMessageHelper.ShowTemporaryMessage(TicketLSTDecrypterDragAreaText, "No LST files listed for decryption.", 2000);
            }
        }

        private void TicketLSTDecrypterDragDropHandler(object sender, DragEventArgs e)
        {
            LogDebugInfo("Ticket LST Encryption: Drag and Drop Initiated");
            TemporaryMessageHelper.ShowTemporaryMessage(TicketLSTDecrypterDragAreaText, "Processing...", 1000000);

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
                        var filesInDirectory = Directory.GetFiles(item, "*.lst", SearchOption.AllDirectories).ToList();
                        validFiles.AddRange(filesInDirectory);
                    }
                    else if (File.Exists(item) && Path.GetExtension(item).ToLowerInvariant() == ".lst")
                    {
                        validFiles.Add(item);
                    }
                }

                if (validFiles.Count > 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var existingFilesSet = new HashSet<string>(TicketLSTDecrypterTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        int initialCount = existingFilesSet.Count;
                        existingFilesSet.UnionWith(validFiles); // Automatically removes duplicates
                        int newFilesCount = existingFilesSet.Count - initialCount;
                        int duplicatesCount = validFiles.Count - newFilesCount;

                        TicketLSTDecrypterTextBox.Text = string.Join(Environment.NewLine, existingFilesSet);

                        string addedFilesMessage = newFilesCount > 0 ? $"{newFilesCount} LST files added" : "No new LST files added";
                        string duplicatesMessage = duplicatesCount > 0 ? $", {duplicatesCount} duplicates filtered" : "";
                        message = addedFilesMessage + duplicatesMessage;

                        LogDebugInfo($"Ticket LST Encryption: {newFilesCount} LST files added, {duplicatesCount} duplicates filtered");
                    });
                    displayTime = 1000; // Change display time since files were added
                }
                else
                {
                    message = "No LST files found.";
                    LogDebugInfo("Ticket LST Encryption: No LST files found in Drag and Drop.");
                }

                TemporaryMessageHelper.ShowTemporaryMessage(TicketLSTDecrypterDragAreaText, message, 2000);
            }
            else
            {
                LogDebugInfo("Ticket LST Encryption: Drag and Drop - No Data Present.");
                TemporaryMessageHelper.ShowTemporaryMessage(TicketLSTDecrypterDragAreaText, "Drag and Drop operation failed - No Data Present.", 2000);
            }
        }



        private void ClickToBrowseTicketLSTEncryptHandler(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("Ticket LST Decryption: Browsing for LST files Initiated");

            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "LST files (*.lst)|*.lst",
                Multiselect = true
            };

            string message = "No files selected.";
            int displayTime = 2000; // Default to 2 seconds for the message display.

            if (openFileDialog.ShowDialog() == true)
            {
                var selectedFiles = openFileDialog.FileNames;

                if (selectedFiles.Any())
                {
                    Dispatcher.Invoke(() =>
                    {
                        var existingFilesSet = new HashSet<string>(TicketLSTEncrypterTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        int initialCount = existingFilesSet.Count;
                        existingFilesSet.UnionWith(selectedFiles); // Automatically removes duplicates
                        int newFilesCount = existingFilesSet.Count - initialCount;
                        int duplicatesCount = selectedFiles.Length - newFilesCount;

                        TicketLSTEncrypterTextBox.Text = string.Join(Environment.NewLine, existingFilesSet);

                        string addedFilesMessage = newFilesCount > 0 ? $"{newFilesCount} LST files added" : "No new LST files added";
                        string duplicatesMessage = duplicatesCount > 0 ? $", {duplicatesCount} duplicates filtered" : "";
                        message = addedFilesMessage + duplicatesMessage;

                        LogDebugInfo($"Ticket LST Decryption: {newFilesCount} LST files added, {duplicatesCount} duplicates filtered via File Browser");
                    });
                    displayTime = 1000; // Change display time since files were added
                }
                else
                {
                    message = "No LST files selected.";
                    LogDebugInfo("Ticket LST Decryption: No LST files selected in File Browser.");
                }

                TemporaryMessageHelper.ShowTemporaryMessage(TicketLSTEncrypterDragAreaText, message, 2000);
            }
            else
            {
                LogDebugInfo("Ticket LST Decryption: File Browser - Dialog Cancelled.");
            }

            TemporaryMessageHelper.ShowTemporaryMessage(TicketLSTEncrypterDragAreaText, message, 2000);
        }


        // Placeholder method for LST file decryption
        private Task<bool> DecryptLstFilesAsync(string[] filePaths)
        {
            LogDebugInfo($"Ticket LST Decryption: Starting decryption for {filePaths.Length} LST file(s)");

            // TODO: Implement the actual decryption logic here
            bool decryptionResult = true; // Simulate success for now

            if (decryptionResult)
            {
                LogDebugInfo("Ticket LST Decryption: Decryption process completed successfully");
            }
            else
            {
                LogDebugInfo("Ticket LST Decryption: Decryption process failed");
            }

            return Task.FromResult(decryptionResult);
        }


        // TAB 4: Logic for Ticket LST Encryption

        private async void TicketLSTEncrypterExecuteButtonClick(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("Ticket LST Encryption: Process Initiated");
            TemporaryMessageHelper.ShowTemporaryMessage(TicketLSTEncrypterDragAreaText, "Processing...", 1000000);

            string filesToEncrypt = TicketLSTEncrypterTextBox.Text;
            if (!string.IsNullOrWhiteSpace(filesToEncrypt))
            {
                string[] filePaths = filesToEncrypt.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                LogDebugInfo($"Ticket LST Encryption: Starting encryption for {filePaths.Length} files");
                bool encryptionSuccess = await EncryptLstFilesAsync(filePaths);

                string message = encryptionSuccess ? $"Success: {filePaths.Length} LST Files Encrypted (Simulated)" : "Encryption Failed";
                TemporaryMessageHelper.ShowTemporaryMessage(TicketLSTEncrypterDragAreaText, message, 2000);

                LogDebugInfo($"Ticket LST Encryption: Result - {message}");
            }
            else
            {
                LogDebugInfo("Ticket LST Encryption: Aborted - No LST files listed for encryption.");
                TemporaryMessageHelper.ShowTemporaryMessage(TicketLSTEncrypterDragAreaText, "No LST files listed for encryption.", 2000);
            }
        }

        private void TicketLSTEncrypterDragDropHandler(object sender, DragEventArgs e)
        {
            LogDebugInfo("Ticket LST Decryption: Drag and Drop Initiated");
            TemporaryMessageHelper.ShowTemporaryMessage(TicketLSTEncrypterDragAreaText, "Processing...", 1000000);

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
                        var filesInDirectory = Directory.GetFiles(item, "*.lst", SearchOption.AllDirectories).ToList();
                        validFiles.AddRange(filesInDirectory);
                    }
                    else if (File.Exists(item) && Path.GetExtension(item).ToLowerInvariant() == ".lst")
                    {
                        validFiles.Add(item);
                    }
                }

                if (validFiles.Count > 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var existingFilesSet = new HashSet<string>(TicketLSTEncrypterTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        int initialCount = existingFilesSet.Count;
                        existingFilesSet.UnionWith(validFiles); // Automatically removes duplicates
                        int newFilesCount = existingFilesSet.Count - initialCount;
                        int duplicatesCount = validFiles.Count - newFilesCount;

                        TicketLSTEncrypterTextBox.Text = string.Join(Environment.NewLine, existingFilesSet);

                        string addedFilesMessage = newFilesCount > 0 ? $"{newFilesCount} LST files added" : "No new LST files added";
                        string duplicatesMessage = duplicatesCount > 0 ? $", {duplicatesCount} duplicates filtered" : "";
                        message = addedFilesMessage + duplicatesMessage;

                        LogDebugInfo($"Ticket LST Decryption: {newFilesCount} LST files added, {duplicatesCount} duplicates filtered");
                    });
                    displayTime = 1000; // Change display time since files were added
                }
                else
                {
                    message = "No LST files found.";
                    LogDebugInfo("Ticket LST Decryption: No LST files found in Drag and Drop.");
                }

                TemporaryMessageHelper.ShowTemporaryMessage(TicketLSTEncrypterDragAreaText, message, 2000);
            }
            else
            {
                LogDebugInfo("Ticket LST Decryption: Drag and Drop - No Data Present.");
                TemporaryMessageHelper.ShowTemporaryMessage(TicketLSTEncrypterDragAreaText, "Drag and Drop operation failed - No Data Present.", 2000);
            }
        }



        private void ClickToBrowseTicketLSTDecryptHandler(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("Ticket LST Encryption: Browsing for LST files Initiated");

            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "LST files (*.lst)|*.lst",
                Multiselect = true
            };

            string message = "No files selected.";
            int displayTime = 2000; // Default to 2 seconds for the message display.

            if (openFileDialog.ShowDialog() == true)
            {
                var selectedFiles = openFileDialog.FileNames;

                if (selectedFiles.Any())
                {
                    Dispatcher.Invoke(() =>
                    {
                        var existingFilesSet = new HashSet<string>(TicketLSTDecrypterTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        int initialCount = existingFilesSet.Count;
                        existingFilesSet.UnionWith(selectedFiles); // Automatically removes duplicates
                        int newFilesCount = existingFilesSet.Count - initialCount;
                        int duplicatesCount = selectedFiles.Length - newFilesCount;

                        TicketLSTDecrypterTextBox.Text = string.Join(Environment.NewLine, existingFilesSet);

                        string addedFilesMessage = newFilesCount > 0 ? $"{newFilesCount} LST files added" : "No new LST files added";
                        string duplicatesMessage = duplicatesCount > 0 ? $", {duplicatesCount} duplicates filtered" : "";
                        message = addedFilesMessage + duplicatesMessage;

                        LogDebugInfo($"Ticket LST Encryption: {newFilesCount} LST files added, {duplicatesCount} duplicates filtered via File Browser");
                    });
                    displayTime = 1000; // Change display time since files were added
                }
                else
                {
                    message = "No LST files selected.";
                    LogDebugInfo("Ticket LST Encryption: No LST files selected in File Browser.");
                }

                TemporaryMessageHelper.ShowTemporaryMessage(TicketLSTDecrypterDragAreaText, message, 2000);
            }
            else
            {
                LogDebugInfo("Ticket LST Encryption: File Browser - Dialog Cancelled.");
            }

            TemporaryMessageHelper.ShowTemporaryMessage(TicketLSTDecrypterDragAreaText, message, 2000);
        }



        // Placeholder method for the LST file encryption process
        private Task<bool> EncryptLstFilesAsync(string[] filePaths)
        {
            LogDebugInfo($"Ticket LST Encryption: Starting encryption for {filePaths.Length} LST file(s)");

            // TODO: Implement the actual encryption logic here
            bool encryptionResult = true; // Simulate success for now

            if (encryptionResult)
            {
                LogDebugInfo("Ticket LST Encryption: Encryption process completed successfully");
            }
            else
            {
                LogDebugInfo("Ticket LST Encryption: Encryption process failed");
            }

            return Task.FromResult(encryptionResult);
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

                string compilerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "HomeLuaC.exe");



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

            string compilerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "HomeLuaC.exe");


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
                string duplicatesMessage = duplicatesCount > 0 ? $", {duplicatesCount} duplicates filtered" : "";
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


        private void ClickToBrowseHandlerLUACompiler(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "LUA files (*.lua)|*.lua",
                Multiselect = true
            };
            string message = "No files selected.";
            int displayTime = 2000; // Default to 2 seconds for the message display.

            if (openFileDialog.ShowDialog() == true)
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
                    string duplicatesMessage = duplicatesCount > 0 ? $", {duplicatesCount} duplicates filtered" : "";
                    message = addedFilesMessage + duplicatesMessage;

                    displayTime = 2000; // Change display time since files were added
                    string logPrefix = "LUA Compilation to LUAC: Click to Browse - ";
                    LogDebugInfo(logPrefix + addedFilesMessage); // This will log "Drag and Drop Info: X LUA files added" or "Drag and Drop Info: No new LUA files added"
                    if (duplicatesCount > 0)
                    {
                        // Ensure we clean up the message by trimming any leading comma and space
                        string cleanDuplicatesMessage = duplicatesMessage.TrimStart(',', ' ').Trim();
                        LogDebugInfo(logPrefix + cleanDuplicatesMessage); // This will log "Drag and Drop Info: X duplicates filtered" if there are any duplicates
                    }
                }
                else
                {
                    LogDebugInfo("LUA Compilation to LUAC: Click to Browse -  No LUA files were selected.");
                    message = "No LUA files selected.";
                }
            }

            TemporaryMessageHelper.ShowTemporaryMessage(LUACompilerDragAreaText, message, 2000);
        }


        private void LUACompilerTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Assuming LUACOutputDirectoryTextBox is the name of your TextBox containing the output directory path
            string outputDirectory = LUACOutputDirectoryTextBox.Text;

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
        private string unluac122Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "unluac122.jar");
        private string unluac2023Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "unluac_2023_12_24.jar");
        private string unluacNETPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "unluac.exe");


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
                string duplicatesMessage = duplicatesCount > 0 ? $", {duplicatesCount} duplicates filtered" : "";
                message = addedFilesMessage + duplicatesMessage;

                // Log final message
                LogDebugInfo("LUAC Decompilation to LUA: Drag and Drop - " + message);

                TemporaryMessageHelper.ShowTemporaryMessage(LUACDecompilerDragAreaText, message, 2000);
            }
        }


        private void ClickToBrowseHandlerLUACDecompiler(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "LUAC files (*.luac)|*.luac",
                Multiselect = true
            };

            string message = "No files selected.";
            int displayTime = 2000; // Default to 2 seconds for the message display.

            if (openFileDialog.ShowDialog() == true)
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

                    message = $"{newFilesCount} LUAC files added" + (duplicatesCount > 0 ? $", {duplicatesCount} duplicates filtered" : "");
                    displayTime = 1000; // Change display time since files were added
                    string logPrefix = "LUAC Decompilation to LUA: Click to Browse - ";
                    LogDebugInfo(logPrefix + $"{newFilesCount} LUAC files added");
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

            TemporaryMessageHelper.ShowTemporaryMessage(LUACDecompilerDragAreaText, message, 2000);
        }


        private void LUACDecompilerTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Assuming LUAOutputDirectoryTextBox is the name of your TextBox containing the output directory path
            string outputDirectory = LUAOutputDirectoryTextBox.Text;

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


        // TAB 7: Logic for Decrypting INF Files
        private async void INFDecrypterExecuteButtonClick(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("INF Decryption: Process Initiated");
            TemporaryMessageHelper.ShowTemporaryMessage(INFDecrypterDragAreaText, "Processing... 0% done", 1000);

            string baseTempFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "tempINF");

            string filesToDecrypt = INFDecrypterTextBox.Text;

            if (!string.IsNullOrWhiteSpace(filesToDecrypt))
            {
                string[] filePaths = filesToDecrypt.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                int segmentSize = (int)Math.Ceiling((double)filePaths.Length / 100);

                bool saveDecryptedInfs = CheckBoxSaveDecInfs.IsChecked ?? false;

                for (int i = 0; i < 100; i++)
                {
                    int start = i * segmentSize;
                    int end = (i == 99) ? filePaths.Length : start + segmentSize;
                    string[] segment = filePaths.Skip(start).Take(end - start).ToArray();

                    string segmentTempFolderPath = Path.Combine(baseTempFolderPath, $"Segment{i + 1}");
                    string infSubfolderPath = Path.Combine(segmentTempFolderPath, "INF");
                    CreateAndCleanTempFolder(infSubfolderPath);

                    await Task.Run(() => CopyFilesToFolder(segment, infSubfolderPath));

                    string segmentOutput = await ExecuteJavaJarINFAsync(segmentTempFolderPath, saveDecryptedInfs);

                    Dispatcher.Invoke(() =>
                    {
                        INFDecrypterTextBox.AppendText(segmentOutput);
                        INFDecrypterTextBox.ScrollToEnd();
                        // Update progress message
                        double progressPercentage = (i + 1); // Each segment represents 1% of the total progress
                        TemporaryMessageHelper.ShowTemporaryMessage(INFDecrypterDragAreaText, $"Processing... {progressPercentage}% done", 1000);
                    });

                    // Handle "Decrypted INFs" folder
                    string decryptedInfsFolderPath = Path.Combine(segmentTempFolderPath, "Decrypted INFs");
                    if (Directory.Exists(decryptedInfsFolderPath))
                    {
                        string outputDirectory = InfToolOutputDirectoryTextBox.Text;
                        if (!Directory.Exists(outputDirectory))
                        {
                            Directory.CreateDirectory(outputDirectory);
                        }

                        foreach (var file in Directory.GetFiles(decryptedInfsFolderPath))
                        {
                            string destFileName = Path.Combine(outputDirectory, Path.GetFileName(file));
                            File.Move(file, destFileName, true);
                        }
                    }


                    // Clean up the segment's temp folder (optional)
                    // DeleteTempFolder(segmentTempFolderPath);
                }

                TemporaryMessageHelper.ShowTemporaryMessage(INFDecrypterDragAreaText, "All INF files processed", 2000);
                LogDebugInfo("INF Decryption: All INF files processed");
            }
            else
            {
                LogDebugInfo("INF Decryption: No INF files listed for decryption.");
                TemporaryMessageHelper.ShowTemporaryMessage(INFDecrypterDragAreaText, "No INF files listed for decryption.", 2000);
            }
        }



        private void DeleteTempFolder(string folderPath)
        {
            try
            {
                if (Directory.Exists(folderPath))
                {
                    DirectoryInfo di = new DirectoryInfo(folderPath);
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dir in di.GetDirectories())
                    {
                        dir.Delete(true); // true for recursive delete
                    }
                    Directory.Delete(folderPath);
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error deleting temporary folder {folderPath}: {ex.Message}");
            }
        }

        private void CopyFilesToFolder(string[] filePaths, string folderPath)
        {
            foreach (string filePath in filePaths)
            {
                if (File.Exists(filePath) && filePath.EndsWith("_INF"))
                {
                    string destFile = Path.Combine(folderPath, Path.GetFileName(filePath));
                    File.Copy(filePath, destFile, true);
                }
            }
        }

        private async Task<string> ExecuteJavaJarINFAsync(string workingDirectory, bool saveDecryptedInfs)
        {
            try
            {
                string jarFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "deinf.jar");
                string javaCommand = saveDecryptedInfs
                                     ? $"java -jar \"{jarFilePath}\" -s \"{Path.Combine(workingDirectory, "INF")}\""
                                     : $"java -jar \"{jarFilePath}\" \"{Path.Combine(workingDirectory, "INF")}\"";

                var processStartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = workingDirectory,
                    FileName = "cmd.exe",
                    Arguments = "/c " + javaCommand,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                StringBuilder completeOutput = new StringBuilder();

                using (Process process = Process.Start(processStartInfo))
                {
                    while (!process.StandardOutput.EndOfStream)
                    {
                        string line = await process.StandardOutput.ReadLineAsync();
                        string filteredOutput = FilterINFDecryptionOutput(line, null);
                        completeOutput.AppendLine(filteredOutput);
                    }

                    await process.WaitForExitAsync();
                }

                return completeOutput.ToString().Trim();
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error during Java JAR execution: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        private string FilterINFDecryptionOutput(string output, string filename)
        {
            var finalOutput = new StringBuilder();
            var validLinePattern = @"^\|\s\d+";
            var lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (Regex.IsMatch(line, validLinePattern))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 8)
                    {
                        string downloadURL = parts[2].Trim();
                        string urlBeforeThirdSlash = "";
                        string urlAfterThirdSlash = "";

                        int slashCount = 0;
                        for (int i = 0; i < downloadURL.Length; i++)
                        {
                            if (downloadURL[i] == '/')
                            {
                                slashCount++;
                                if (slashCount == 3)
                                {
                                    urlBeforeThirdSlash = downloadURL.Substring(0, i);
                                    urlAfterThirdSlash = downloadURL.Substring(i + 1);
                                    break;
                                }
                            }
                        }

                        finalOutput.AppendLine($"\n\n-- Input URI: {parts[1].Trim()} (_INF)\n-- Download Date: {parts[4].Trim()}\n-- File Size: {parts[6].Trim()} Bytes");
                        finalOutput.AppendLine($"-- Server: {urlBeforeThirdSlash}/");
                        finalOutput.AppendLine($"-- Path: {urlAfterThirdSlash}");
                    }
                }
            }

            return finalOutput.ToString().TrimEnd();
        }


        private void INFDecrypterDragDropHandler(object sender, DragEventArgs e)
        {
            LogDebugInfo("INF Decryption: Drag and Drop Initiated");
            TemporaryMessageHelper.ShowTemporaryMessage(INFDecrypterDragAreaText, "Processing...", 1000000);

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
                        var filesInDirectory = Directory.EnumerateFiles(item, "*_INF", SearchOption.AllDirectories).ToList();
                        validFiles.AddRange(filesInDirectory);
                    }
                    else if (File.Exists(item) && item.EndsWith("_INF"))
                    {
                        validFiles.Add(item);
                    }
                }

                if (validFiles.Count > 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var existingFilesSet = new HashSet<string>(INFDecrypterTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        int initialCount = existingFilesSet.Count;
                        existingFilesSet.UnionWith(validFiles); // This adds new files and removes duplicates
                        int newFilesCount = existingFilesSet.Count - initialCount;
                        int duplicatesCount = validFiles.Count - newFilesCount;

                        INFDecrypterTextBox.Text = string.Join(Environment.NewLine, existingFilesSet);

                        message = $"{newFilesCount} INF files added" + (duplicatesCount > 0 ? $", {duplicatesCount} duplicates filtered" : "");

                        LogDebugInfo($"INF Decryption: {newFilesCount} INF files added, {duplicatesCount} duplicates filtered");
                    });
                    displayTime = 1000; // Change display time since files were added
                }
                else
                {
                    message = "No INF files found.";
                    LogDebugInfo("INF Decryption: No INF files found in Drag and Drop.");
                }

                TemporaryMessageHelper.ShowTemporaryMessage(INFDecrypterDragAreaText, message, 2000);
            }
            else
            {
                LogDebugInfo("INF Decryption: Drag and Drop - No Data Present.");
                TemporaryMessageHelper.ShowTemporaryMessage(INFDecrypterDragAreaText, "Drag and Drop operation failed - No Data Present.", 2000);
            }
        }


        private void ClickToBrowseINFDecrypterHandler(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("INF Decryption: Browsing for INF files Initiated");

            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "INF files (*_INF)|*_INF",
                Multiselect = true,
                CheckFileExists = true
            };

            string message = "No files selected.";
            int displayTime = 2000; // Default to 2 seconds for the message display.

            if (openFileDialog.ShowDialog() == true)
            {
                var selectedFiles = openFileDialog.FileNames;

                if (selectedFiles.Any())
                {
                    Dispatcher.Invoke(() =>
                    {
                        var existingFilesSet = new HashSet<string>(INFDecrypterTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        int initialCount = existingFilesSet.Count;
                        existingFilesSet.UnionWith(selectedFiles); // This adds new files and removes duplicates
                        int newFilesCount = existingFilesSet.Count - initialCount;
                        int duplicatesCount = selectedFiles.Length - newFilesCount;

                        INFDecrypterTextBox.Text = string.Join(Environment.NewLine, existingFilesSet);

                        message = $"{newFilesCount} INF files added" + (duplicatesCount > 0 ? $", {duplicatesCount} duplicates filtered" : "");

                        LogDebugInfo($"INF Decryption: {newFilesCount} INF files added, {duplicatesCount} duplicates filtered via File Browser");
                    });
                    displayTime = 1000; // Change display time since files were added
                }
                else
                {
                    message = "No INF files selected.";
                    LogDebugInfo("INF Decryption: No INF files selected in File Browser.");
                }

                TemporaryMessageHelper.ShowTemporaryMessage(INFDecrypterDragAreaText, message, 2000);
            }
            else
            {
                LogDebugInfo("INF Decryption: File Browser - Dialog Cancelled.");
            }

            TemporaryMessageHelper.ShowTemporaryMessage(INFDecrypterDragAreaText, message, 2000);
        }




        // TAB 7: Logic for extracting mapping and logging cache zips


        private async void CACHEMapperExecuteButtonClick(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("CACHE Folder Processing: Process Initiated");
            TemporaryMessageHelper.ShowTemporaryMessage(CACHEMapperDragAreaText, "Processing...", 1000000);

            string[] baseTempFolderPaths = new string[4];
            for (int i = 0; i < baseTempFolderPaths.Length; i++)
            {
                baseTempFolderPaths[i] = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", $"tempCACHE{i + 1}", "INF");

                CreateAndCleanTempFolder(baseTempFolderPaths[i]);
            }

            string cacheFoldersToProcess = CACHEMapperTextBox.Text;
            if (!string.IsNullOrWhiteSpace(cacheFoldersToProcess))
            {
                string[] cacheFolderPaths = cacheFoldersToProcess.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                int segmentSize = (int)Math.Ceiling((double)cacheFolderPaths.Length / 4);

                List<Task> copyTasks = new List<Task>();
                List<Task<string>> processTasks = new List<Task<string>>();

                string[] subFolders = { "CLANS", "GLOBALS", "HTTP", "OBJECTDEFS", "OBJECTDYNAMIC", "OBJECTTHUMBS", "PROFILE", "SCENES", "VIDEO", "WORLDMAP" };
                for (int i = 0; i < 4; i++)
                {
                    int start = i * segmentSize;
                    int end = (i == 3) ? cacheFolderPaths.Length : start + segmentSize;
                    string[] segment = cacheFolderPaths.Skip(start).Take(end - start).ToArray();

                    copyTasks.Add(Task.Run(() => CopyCacheFilesToFolder(segment, subFolders, baseTempFolderPaths[i])));
                    processTasks.Add(ExecuteJavaJarAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", $"tempCACHE{i + 1}")));

                }

                await Task.WhenAll(copyTasks);
                var processingResults = await Task.WhenAll(processTasks);

                string combinedOutput = string.Join(Environment.NewLine, processingResults).TrimEnd();

                Dispatcher.Invoke(() =>
                {
                    CACHEMapperTextBox.AppendText(combinedOutput);
                    CACHEMapperTextBox.ScrollToEnd();
                });

                foreach (var folderPath in baseTempFolderPaths.Select(p => Path.GetDirectoryName(p)))
                {
                    DeleteTempFolder(folderPath);
                }

                TemporaryMessageHelper.ShowTemporaryMessage(CACHEMapperDragAreaText, "All CACHE folders processed", 2000);
                LogDebugInfo("CACHE Folder Processing: All CACHE folders processed");
            }
            else
            {
                LogDebugInfo("CACHE Folder Processing: No CACHE folders listed for processing.");
                TemporaryMessageHelper.ShowTemporaryMessage(CACHEMapperDragAreaText, "No CACHE folders listed for processing.", 2000);
            }
        }

        private void CopyCacheFilesToFolder(string[] cacheFolderPaths, string[] subFolders, string folderPath)
        {
            foreach (string cacheFolderPath in cacheFolderPaths)
            {
                foreach (string subFolder in subFolders)
                {
                    string fullPath = Path.Combine(cacheFolderPath, subFolder);
                    if (Directory.Exists(fullPath))
                    {
                        var infFiles = Directory.EnumerateFiles(fullPath, "*_INF");
                        foreach (var infFile in infFiles)
                        {
                            string destFile = Path.Combine(folderPath, Path.GetFileName(infFile));
                            File.Copy(infFile, destFile, true);
                        }
                    }
                }
            }
        }


        private void CreateAndCleanTempFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            else
            {
                DirectoryInfo di = new DirectoryInfo(folderPath);
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
            }
        }

        private async Task<string> ExecuteJavaJarAsync(string workingDirectory)
        {
            try
            {
                string jarFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "deinf.jar");
                string javaCommand = $"java -jar \"{jarFilePath}\" \"{Path.Combine(workingDirectory, "INF")}\"";

                var processStartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = workingDirectory,
                    FileName = "cmd.exe",
                    Arguments = "/c " + javaCommand,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                StringBuilder completeOutput = new StringBuilder();

                using (Process process = Process.Start(processStartInfo))
                {
                    while (!process.StandardOutput.EndOfStream)
                    {
                        string line = await process.StandardOutput.ReadLineAsync();
                        string filteredOutput = FilterCACHEDeinfOutput(line, null);
                        completeOutput.AppendLine(filteredOutput);
                    }

                    await process.WaitForExitAsync();
                }

                return completeOutput.ToString();
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error during Java JAR execution: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }


        private string FilterCACHEDeinfOutput(string output, string filename)
        {
            var finalOutput = new StringBuilder();
            var validLinePattern = @"^\|\s\d+";
            var lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (Regex.IsMatch(line, validLinePattern))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 8)
                    {
                        string downloadURL = parts[2].Trim();
                        string urlBeforeThirdSlash = "";
                        string urlAfterThirdSlash = "";

                        int slashCount = 0;
                        for (int i = 0; i < downloadURL.Length; i++)
                        {
                            if (downloadURL[i] == '/')
                            {
                                slashCount++;
                                if (slashCount == 3)
                                {
                                    urlBeforeThirdSlash = downloadURL.Substring(0, i);
                                    urlAfterThirdSlash = downloadURL.Substring(i + 1); // +1 to skip the slash itself
                                    break;
                                }
                            }
                        }

                        // Append formatted line to final output
                        finalOutput.AppendLine($"\n\n-- Input URI: {parts[1].Trim()} (_INF)\n-- Download Date: {parts[4].Trim()}\n-- File Size: {parts[6].Trim()} Bytes");
                        finalOutput.AppendLine($"-- Server: {urlBeforeThirdSlash}/");
                        finalOutput.AppendLine($"-- Path: {urlAfterThirdSlash}");

                    }
                }
            }

            return finalOutput.ToString().TrimEnd();
        }




        private async Task MapCacheAsync(string[] itemPaths)
        {
            var cacheFolderNames = new HashSet<string> { "CACHE", "CACHE_DH", "CACHE_HHQ", "CACHE_PSHLAB183", "CACHE_PSHLAB186" };

            foreach (string itemPath in itemPaths)
            {
                if (Directory.Exists(itemPath))
                {
                    // Check if the directory itself is one of the specified CACHE folders
                    if (cacheFolderNames.Contains(Path.GetFileName(itemPath)))
                    {
                        AddCACHEFolderItemToList(itemPath);
                    }
                    else
                    {
                        // Search for specified CACHE folders within the directory
                        var subDirectories = Directory.GetDirectories(itemPath, "*", SearchOption.AllDirectories);
                        foreach (var subDir in subDirectories)
                        {
                            if (cacheFolderNames.Contains(Path.GetFileName(subDir)))
                            {
                                AddCACHEFolderItemToList(subDir);
                            }
                        }
                    }
                }
            }
            // Optionally, update the UI or log the final message after processing all directories
            LogDebugInfo($"Finished processing directories.");
        }




        private async void CACHEMapperDragDropHandler(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedItems = (string[])e.Data.GetData(DataFormats.FileDrop);
                await MapCacheAsync(droppedItems);
            }
            else
            {
                LogDebugInfo("CACHE Mapping: Drag and Drop - No Data Present.");
            }
        }

        private void AddCACHEFolderItemToList(string itemPath)
        {
            Dispatcher.Invoke(() =>
            {
                var existingItemsSet = new HashSet<string>(CACHEMapperTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                if (existingItemsSet.Add(itemPath))
                {
                    CACHEMapperTextBox.Text += itemPath + Environment.NewLine;
                    LogDebugInfo($"Added CACHE directory to list: {itemPath}");
                }
            });
        }

        private void ClickToBrowseCACHEMapperHandler(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("CACHE Mapping: Browsing for directories Initiated");

            var folderDialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Multiselect = true,
                Title = "Select Folders Containing CACHE Subfolders"
            };

            if (folderDialog.ShowDialog() == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
            {
                var selectedFolders = folderDialog.FileNames;
                var cacheFolderNames = new HashSet<string> { "CACHE", "CACHE_DH", "CACHE_HHQ", "CACHE_PSHLAB183", "CACHE_PSHLAB186" };
                HashSet<string> existingItemsSet = new HashSet<string>(CACHEMapperTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));

                foreach (var folderPath in selectedFolders)
                {
                    if (Directory.Exists(folderPath))
                    {
                        if (cacheFolderNames.Contains(Path.GetFileName(folderPath)))
                        {
                            existingItemsSet.Add(folderPath);
                        }
                        else
                        {
                            var subDirectories = Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories);
                            foreach (var subDir in subDirectories)
                            {
                                if (cacheFolderNames.Contains(Path.GetFileName(subDir)))
                                {
                                    existingItemsSet.Add(subDir);
                                }
                            }
                        }
                    }
                }

                Dispatcher.Invoke(() =>
                {
                    CACHEMapperTextBox.Text = string.Join(Environment.NewLine, existingItemsSet);
                    string message = $"{existingItemsSet.Count} items added to the list";
                    TemporaryMessageHelper.ShowTemporaryMessage(CACHEMapperDragAreaText, message, 2000);
                    LogDebugInfo(message);
                });
            }
            else
            {
                LogDebugInfo("CACHE Mapping: Folder Browser - Dialog Cancelled.");
                TemporaryMessageHelper.ShowTemporaryMessage(CACHEMapperDragAreaText, "Folder selection was cancelled.", 2000);
            }
        }


        // TAB 8: Logic for SDC Creation

        private void CreateSdcButton_Click(object sender, RoutedEventArgs e)
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

            // Construct the thumbnail image file names
            string makerImageSuffix = string.IsNullOrEmpty(thumbnailSuffix) ? "" : $"_T{thumbnailSuffix}";
            string smallImageSuffix = string.IsNullOrEmpty(thumbnailSuffix) ? "" : $"_T{thumbnailSuffix}";
            string largeImageSuffix = string.IsNullOrEmpty(thumbnailSuffix) ? "" : $"_T{thumbnailSuffix}";

            string makerImage = $"[THUMBNAIL_ROOT]maker{makerImageSuffix}.png";
            string smallImage = $"[THUMBNAIL_ROOT]small{smallImageSuffix}.png";
            string largeImage = $"[THUMBNAIL_ROOT]large{largeImageSuffix}.png";

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
                            new XElement("ARCHIVES",
                                new XElement("ARCHIVE", new XAttribute("size", archiveSize), new XAttribute("timestamp", timestamp), $"[CONTENT_SERVER_ROOT]{archivePath}")
                            ) : null
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

                    // Placeholder languages
                    // AddLanguagePlaceholder("fr-FR"),
                    // AddLanguagePlaceholder("de-DE"),
                    // AddLanguagePlaceholder("it-IT"),
                    // AddLanguagePlaceholder("es-ES"),
                    // AddLanguagePlaceholder("ja-JP"),
                    // AddLanguagePlaceholder("ko-KR"),
                    // AddLanguagePlaceholder("zh-TW"),
                    // AddLanguagePlaceholder("zh-HK"),
                    new XElement("age_rating", new XAttribute("minimum_age", "0"), new XAttribute("parental_control_level", "1"))
                    )
            );

            // Show save file dialog
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "SDC files (*.sdc)|*.sdc",
                DefaultExt = "sdc",
                FileName = "NewSDCFile"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                // Save the XML to the selected file path
                sdcXml.Save(saveFileDialog.FileName);
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
                case RememberLastTabUsed.TickLSTTool:
                    MainTabControl.SelectedIndex = 3; // Index of TickLSTTool tab
                    break;
                case RememberLastTabUsed.SceneIDTool:
                    MainTabControl.SelectedIndex = 4; // Index of SceneIDTool tab
                    break;
                case RememberLastTabUsed.LUACTool:
                    MainTabControl.SelectedIndex = 5; // Index of LUACTool tab
                    break;
                case RememberLastTabUsed.CacheTool:
                    MainTabControl.SelectedIndex = 6; // Index of CachTool tab
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

        private void ODCCreateODC_Click(object sender, RoutedEventArgs e)
        {
            // Gather data from the form
            string name = odcNameTextBox.Text;
            string description = odcDescriptionTextBox.Text;
            string maker = odcMakerTextBox.Text;
            string hdkVersion = odcHDKVersionTextBox.Text;
            string thumbnailSuffix = odcThumbnailSuffixTextBox.Text;
            string uuid = odcUUIDTextBox.Text;
            string timestamp = odcTimestampTextBox.Text;

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
                    new XElement("timestamp", timestamp)
                )
            );

            // Show save file dialog
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {

                Filter = "ODC files (.odc)|.odc",
                DefaultExt = "odc",
                FileName = "NewODCFile"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                // Save the XML to the selected file path
                odcXml.Save(saveFileDialog.FileName);
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
            odcUUIDTextBox.Text = GenerateUUID();
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

        private void SDCCreateSDCButton_Click_4(object sender, RoutedEventArgs e)
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
                string helperFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scene_file_mapper_helper.txt");
                HashSet<string> existingPaths = new HashSet<string>();

                if (File.Exists(helperFilePath))
                {
                    // Read existing content and remove trailing new lines
                    var existingContent = File.ReadAllText(helperFilePath).TrimEnd('\r', '\n');
                    using (var writer = new StreamWriter(helperFilePath, false)) // Overwrite to remove trailing new lines
                    {
                        writer.Write(existingContent);
                        if (!string.IsNullOrEmpty(existingContent))
                        {
                            // Ensure there's a new line after existing content if it's not empty
                            writer.WriteLine();
                        }

                        // Process paths from text boxes
                        var uniquePaths = new HashSet<string>();
                        ProcessTextBoxForUniquePaths(Path2HashOutputExtraTextBox.Text, existingPaths, uniquePaths);
                        ProcessTextBoxForUniquePaths(Path2HashInputTextBox.Text, existingPaths, uniquePaths, true);

                        foreach (var path in uniquePaths)
                        {
                            if (!string.IsNullOrWhiteSpace(path))
                            {
                                int hash = BarHash(path);
                                string hashString = hash.ToString("X8").ToUpper();
                                if (hashString != "00000000")
                                {
                                    writer.WriteLine($"{hashString}:{path.ToUpper()}");
                                }
                            }
                        }
                    }
                }
                else
                {
                    // File doesn't exist, create new file and add paths
                    using (var writer = new StreamWriter(helperFilePath))
                    {
                        var uniquePaths = new HashSet<string>();
                        ProcessTextBoxForUniquePaths(Path2HashOutputExtraTextBox.Text, existingPaths, uniquePaths);
                        ProcessTextBoxForUniquePaths(Path2HashInputTextBox.Text, existingPaths, uniquePaths, true);

                        foreach (var path in uniquePaths)
                        {
                            if (string.IsNullOrWhiteSpace(path)) continue;
                            int hash = BarHash(path);
                            writer.WriteLine($"{hash.ToString("X8").ToUpper()}:{path.ToUpper()}");
                        }
                    }
                }

                // After adding paths, update the text and use a delay to revert the message.
                Path2HashDragAreaText.Text = "Paths added to scene_file_mapper_helper.txt";
                await Task.Delay(1000); // Wait for one second
                Path2HashDragAreaText.Text = "Drag a folder or TXT/XML here to hash all paths";


            }
        }



        private void ProcessTextBoxForUniquePaths(string textBoxContent, HashSet<string> existingPaths, HashSet<string> uniquePaths, bool isInput = false)
        {
            var lines = textBoxContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                string path = isInput ? line : line.Contains(":") ? line.Split(':')[1] : line;
                path = path.Trim();
                if (!existingPaths.Contains(path.ToUpper()) && !uniquePaths.Contains(path.ToUpper()) && !string.IsNullOrWhiteSpace(path))
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
        private void CdsBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var folderPath = OpenFolderDialog(CdsOutputDirectoryTextBox.Text);
            if (folderPath != null)
            {
                CdsOutputDirectoryTextBox.Text = folderPath;
            }
        }

        private void CdsOpenButton_Click(object sender, RoutedEventArgs e)
        {
            string outputDirectory = CdsOutputDirectoryTextBox.Text;

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

        private void sqlBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var folderPath = OpenFolderDialog(sqlOutputDirectoryTextBox.Text);
            if (folderPath != null)
            {
                sqlOutputDirectoryTextBox.Text = folderPath;
            }
        }

        private void sqlOpenButton_Click(object sender, RoutedEventArgs e)
        {
            string outputDirectory = sqlOutputDirectoryTextBox.Text;

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

        private void InfToolBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var folderPath = OpenFolderDialog(InfToolOutputDirectoryTextBox.Text);
            if (folderPath != null)
            {
                InfToolOutputDirectoryTextBox.Text = folderPath;
            }
        }

        private void InfToolOpenButton_Click(object sender, RoutedEventArgs e)
        {
            string outputDirectory = InfToolOutputDirectoryTextBox.Text;

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

        private void CacheBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var folderPath = OpenFolderDialog(CacheOutputDirectoryTextBox.Text);
            if (folderPath != null)
            {
                CacheOutputDirectoryTextBox.Text = folderPath;
            }
        }

        private void CacheOpenButton_Click(object sender, RoutedEventArgs e)
        {
            string outputDirectory = CacheOutputDirectoryTextBox.Text;

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

        private void LUACBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var folderPath = OpenFolderDialog(LUACOutputDirectoryTextBox.Text);
            if (folderPath != null)
            {
                LUACOutputDirectoryTextBox.Text = folderPath;
            }
        }

        private void LUACOpenButton_Click(object sender, RoutedEventArgs e)
        {
            string outputDirectory = LUACOutputDirectoryTextBox.Text;

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

        private void LUABrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var folderPath = OpenFolderDialog(LUAOutputDirectoryTextBox.Text);
            if (folderPath != null)
            {
                LUAOutputDirectoryTextBox.Text = folderPath;
            }
        }

        private void LUAOpenButton_Click(object sender, RoutedEventArgs e)
        {
            string outputDirectory = LUAOutputDirectoryTextBox.Text;

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

        private void IncreaseCpuPercentage_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(cpuPercentageTextBox.Text, out int value))
            {
                value = Math.Min(value + 1, 90); // Assuming 90 is the max value
                cpuPercentageTextBox.Text = value.ToString();
            }
        }

        private void DecreaseCpuPercentage_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(cpuPercentageTextBox.Text, out int value))
            {
                value = Math.Max(value - 1, 25); // Assuming 25 is the min value
                cpuPercentageTextBox.Text = value.ToString();
            }
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

        private void HandleSdcFileDrop(string[] files)
        {
            // Assuming only one file can be dropped at a time
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            string filePath = files.FirstOrDefault();
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                MessageBox.Show("Invalid file. Please drop a valid SDC XML file.");
                return;
            }

            try
            {
                // Load the SDC XML file
                XDocument sdcXml = XDocument.Load(filePath);

                // Assuming English is the default language for the application
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var englishLanguageElement = sdcXml.Root.Elements("LANGUAGE").FirstOrDefault(el => el.Attribute("REGION")?.Value == "en-GB");
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                if (englishLanguageElement != null)
                {
                    sdcNameTextBox.Text = englishLanguageElement.Element("NAME")?.Value ?? "";
                    sdcDescriptionTextBox.Text = englishLanguageElement.Element("DESCRIPTION")?.Value ?? "";
                    sdcMakerTextBox.Text = englishLanguageElement.Element("MAKER")?.Value ?? "";

                    var archiveElement = englishLanguageElement.Element("ARCHIVES")?.Element("ARCHIVE");
                    sdcServerArchivePathTextBox.Text = archiveElement?.Value ?? "";
                    sdcArchiveSizeTextBox.Text = archiveElement?.Attribute("size")?.Value ?? "";
                    sdcTimestampTextBox.Text = archiveElement?.Attribute("timestamp")?.Value ?? "";

                    // Extract the thumbnail suffix from the image path
                    var makerImagePath = englishLanguageElement.Element("MAKER_IMAGE")?.Value ?? "";
                    sdcThumbnailSuffixTextBox.Text = ExtractSDCThumbnailSuffix(makerImagePath);

                    // Check if offline checkbox should be checked
                    offlineSDCCheckBox.IsChecked = englishLanguageElement.Element("ARCHIVES") == null;
                }

                // Set HDK version
                sdcHDKVersionTextBox.Text = sdcXml.Root.Attribute("hdk_version")?.Value ?? "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading SDC file: {ex.Message}");
            }
        }

        private string ExtractSDCThumbnailSuffix(string imagePath)
        {
            // Assuming the thumbnail suffix is in the format "_Txxx.png"
            var match = Regex.Match(imagePath, @"_T(\d+)\.png");
            return match.Success ? match.Groups[1].Value : "";
        }


        private void HandleOdcFileDrop(string[] files)
        {
            // Assuming only one file can be dropped at a time
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            string filePath = files.FirstOrDefault();
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                MessageBox.Show("Invalid file. Please drop a valid ODC XML file.");
                return;
            }

            try
            {
                // Load the ODC XML file
                XDocument odcXml = XDocument.Load(filePath);

                // Parse and set the values to the form
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                odcNameTextBox.Text = odcXml.Root.Elements("name").FirstOrDefault()?.Value ?? "";
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                odcDescriptionTextBox.Text = odcXml.Root.Elements("description").FirstOrDefault()?.Value ?? "";
                odcMakerTextBox.Text = odcXml.Root.Elements("maker").FirstOrDefault()?.Value ?? "";
                odcHDKVersionTextBox.Text = odcXml.Root.Attribute("hdk_version")?.Value ?? "";
                odcUUIDTextBox.Text = odcXml.Root.Element("uuid")?.Value ?? "";
                odcTimestampTextBox.Text = odcXml.Root.Element("timestamp")?.Value ?? "";

                // Extract the thumbnail suffix from the image path
                var makerImagePath = odcXml.Root.Elements("maker_image").FirstOrDefault()?.Value ?? "";
                odcThumbnailSuffixTextBox.Text = ExtractODCThumbnailSuffix(makerImagePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading ODC file: {ex.Message}");
            }
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





        private void ClickToBrowseHandlerEbootPatcher(object sender, RoutedEventArgs e)
        {
            LogDebugInfo("EBOOT Patcher: Click to Browse EBOOT File selected - Checking...");
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Eboot files (*.bin;*.elf;*.self)|*.bin;*.elf;*.self",
                Multiselect = false
            };
            string message = "No file selected.";

            if (openFileDialog.ShowDialog() == true)
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

                Dispatcher.Invoke(() =>
                {
                    EbootPatcherDragAreaText.Text = message;
                });
            }
            else
            {
                message = "File selection was canceled.";
                Dispatcher.Invoke(() =>
                {
                    EbootPatcherDragAreaText.Text = message;
                });
            }
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

            string destinationFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "temp");
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
            string scetoolDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "scetool");
            string scetoolPath = Path.Combine(scetoolDirectory, "scetool.exe");
            string tempDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "temp");
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
            string tempDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "temp");
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
            string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "ebootdefs.json");
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
            string ebootElfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "temp", "EBOOT.ELF");
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
                string ebootElfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "temp", "EBOOT.ELF");


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
                                Dispatcher.Invoke(() => {
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
                                    Dispatcher.Invoke(() => {
                                        UpdateGUIElement(offsetKey, data);
                                    });
                                }
                            }
                        }
                        else
                        {
                            Dispatcher.Invoke(() => {
                                UpdateGUIElement(offsetKey, null);
                            });
                        }
                    }

                    string fullMuisVersion = secondPartOfMuis != "" ? $"{firstPartOfMuis}.{secondPartOfMuis}" : firstPartOfMuis;
                    Dispatcher.Invoke(() => {
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
                Dispatcher.Invoke(() => {
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
            string ebootElfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "temp", "EBOOT.ELF");
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
            string elfFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "temp", "EBOOT.ELF");
            string scetoolDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "scetool");
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
            string elfFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "temp", "EBOOT.ELF");

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
            Dispatcher.Invoke(() => {
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
            Dispatcher.Invoke(() => {
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
            Dispatcher.Invoke(() => {
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
            Dispatcher.Invoke(() => {
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
                            Dispatcher.Invoke(() => {
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

        private void CheckBoxTTYSpam_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBoxMLAA_Checked(object sender, RoutedEventArgs e)
        {

        }


    }
}
