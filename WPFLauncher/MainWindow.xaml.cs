using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;
using WPFLauncher.Properties;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Globalization;
using Serilog;

namespace WPFLauncher
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _updateAvailable;
        private int BreadClicks = 0;
        private Updater updater = new Updater();
        private bool updating = false;

        private Serilog.Core.Logger log;

        public MainWindow()
        {
            InitializeComponent();
            InitializeLogger();

            try
            {
                CheckVersion();
                SetButtonGradients(0);
                GetQuickCharacters();
                InitializeSettings();
                Activated += RefreshCount;
                StateChanged += Window_StateChanged;
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error initializing app");
            }
            
        }

        private void InitializeSettings()
        {
            log.Information("Initializing settings");

            ShowInTaskbar = true;
            LauncherWindow.Title = "Atlas Launcher v" + Constants.LauncherVersion;
            if (Settings.Default.Username != "") UsernameBox.Text = Settings.Default.Username;
            if (Settings.Default.Password != "") PasswordBox.Password = Settings.Default.Password;
            if (Settings.Default.QuickCharacter != "") QuickloginCombo.Text = Settings.Default.QuickCharacter;

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        private void InitializeLogger()
        {
            log = new LoggerConfiguration().WriteTo.File("logs/AtlasLauncher-.txt", rollingInterval: RollingInterval.Day).CreateLogger();
        }

        private async Task CheckVersion()
        {
            if (!updating)
            {
                _updateAvailable = await updater.CheckForNewVersionAsync();
                PlayButton.Content = _updateAvailable ? "Update" : "Play";
            }
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _updateAvailable = await updater.CheckForNewVersionAsync();
                if (_updateAvailable)
                {
                    PlayButton.Content = _updateAvailable ? "Checking.." : "Play";
                    PlayButton.IsEnabled = !_updateAvailable;
                    updating = true;

                    await Task.Run(async () =>
                    {
                        await UpdateFiles();
                    });

                    updating = false;

                    PlayButton.Content = !(await updater.CheckForNewVersionAsync()) ? "Play" : "Update";
                    PlayButton.IsEnabled = true;
                    SetButtonGradients(0.0);
                    PatchProgressLabel.Content = "";
                }
                else
                {
                    if (await getDiscordStatus(UsernameBox.Text) || !Constants.ForceDiscord)
                    {
                        Play();
                    }
                    else
                    {
                        promptDiscord();
                    }

                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error starting DAOC");
            }            
        }
        
        private async Task<bool> getDiscordStatus(string accountName)
        {
            
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync($"https://api.atlasfreeshard.com/utils/discordstatus/{accountName}");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }

                return false;
            }

        }
        private bool isDiscordRequired()
        {
            try
            {
                var webRequest = WebRequest.Create("https://api.atlasfreeshard.com/utils/discordrequired") as HttpWebRequest;

                webRequest.ContentType = "application/json";
                webRequest.UserAgent = "Nothing";

                var response = webRequest.GetResponse();

                using (var s = response.GetResponseStream())
                {
                    using (var sr = new StreamReader(s))
                    {
                        var discordRequired = sr.ReadToEnd();
                        return discordRequired == "true";
                    }
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }
        
        private void promptDiscord()
        {
            var result = MessageBox.Show(Constants.DiscordMessage, Constants.DiscordCaption, MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                Process.Start(Constants.LinkUrl);
            }
            else
            {
                MessageBox.Show(Constants.DiscordError, Constants.DiscordCaption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Constants.RegisterUrl);
        }

        private async void RefreshCount(object sender, EventArgs e)
        {
            await CheckVersion();
        }

        private async Task UpdateFiles()
        {
            //TestProgress();
            //return;

            this.Dispatcher.Invoke(() => {
                PatchProgressLabel.Content = "Checking existing files";
            });

            var patchlist = await updater.GetPatchlistAsync();
            var filesToDownload = updater.GetFilesToDownload(patchlist);

            HttpClient _httpClient;
            _httpClient = new HttpClient();
            this.Dispatcher.Invoke(() => {
                PlayButton.Content = "0.0%";
                SetButtonGradients(0.0);
            });
            
            try
            {
                var updateSelf = false;
                var totalFiles = filesToDownload.Count;
                var currentFile = 0;
                foreach (var file in filesToDownload)
                {
                    currentFile++;

                    if (file == null)
                        continue;

                    try
                    {
                        var data = await _httpClient.GetByteArrayAsync(Constants.RemoteFilePath + file.FileName);
                        if (file.FileName.Contains("AtlasLauncher")) updateSelf = true;
                        new FileInfo(file.FileName).Directory?.Create();
                        File.WriteAllBytes(file.FileName, data);

                        var percent = Math.Round(100 * ((double)currentFile / totalFiles), 2);
                        this.Dispatcher.Invoke(() => {
                            PlayButton.Content = $"{percent}%";
                            PatchProgressLabel.Content = $"{currentFile} / {totalFiles}";
                            SetButtonGradients(((double)currentFile / totalFiles));
                        });
                    }
                    catch
                    {
                        //skip to next file
                    }
                    
                    
                }

                updater.SaveLocalVersion(await updater.GetVersionAsync());
                if (updateSelf) //handle self update https://andreasrohner.at/posts/Programming/C%23/A-platform-independent-way-for-a-C%23-program-to-update-itself/
                    UpdateLauncher();
            }
            catch (Exception exception)
            {
                MessageBox.Show(Constants.MessageDownloadError);
            }
        }

        private void TestProgress()
        {
            int maxCnt = 1000;
            for (int i = 0; i < maxCnt; i++)
            {
                this.Dispatcher.Invoke(() =>
                {
                    var ratio = i / (double)maxCnt;
                    PlayButton.Content = $"{ratio * 100}%";
                    SetButtonGradients(ratio);
                    PatchProgressLabel.Content = $"{i} / {maxCnt}";
                });

                System.Threading.Thread.Sleep(20);
            }
        }

        private void SetButtonGradients(double ratio)
        {
            var buttonWidth = 130;
            var textWidth = GetTextWidth();

            var background = PlayButton.Background as LinearGradientBrush;
            var startColor = Color.FromArgb(255, 255, 222, 0);
            var endColor = Color.FromArgb(0, 255, 255, 255);

            background.GradientStops.Clear();
            background.GradientStops.Add(new GradientStop(startColor, 0));
            background.GradientStops.Add(new GradientStop(startColor, ratio));
            background.GradientStops.Add(new GradientStop(endColor, ratio));
            background.GradientStops.Add(new GradientStop(endColor, 1.0));

            var textStartPosition = (buttonWidth / 2) - (textWidth / 2);
            var textRatio = 0.0d;
            if (buttonWidth * ratio > textStartPosition)
            {
                textRatio = ((buttonWidth * ratio) - textStartPosition) / textWidth;

                if (textRatio > 1) textRatio = 1;
            }            

            var foreground = PlayButton.Foreground as LinearGradientBrush;

            startColor = Color.FromArgb(255, 0, 0, 0);
            endColor = Color.FromArgb(255, 255, 222, 0);
            foreground.GradientStops.Clear();
            foreground.GradientStops.Add(new GradientStop(startColor, 0));
            foreground.GradientStops.Add(new GradientStop(startColor, textRatio));
            foreground.GradientStops.Add(new GradientStop(endColor, textRatio));
            foreground.GradientStops.Add(new GradientStop(endColor, 1.0));
        }

        private double GetTextWidth()
        {
            var btn = PlayButton;

            var formatted = new FormattedText(
                          btn.Content.ToString(), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                          new Typeface(btn.FontFamily, btn.FontStyle, btn.FontWeight, btn.FontStretch),
                          btn.FontSize, btn.Foreground, VisualTreeHelper.GetDpi(this).PixelsPerDip);

            return formatted.Width;
        }

        private static void UpdateLauncher()
        {
            var self = Assembly.GetExecutingAssembly().Location;

            using (var batFile = new StreamWriter(File.Create("AtlasLauncherUpdate.bat")))
            {
                batFile.WriteLine("@ECHO OFF");
                batFile.WriteLine("TIMEOUT /t 1 /nobreak > NUL");
                batFile.WriteLine("TASKKILL /IM \"{0}\" > NUL", "AtlasLauncher.exe");
                batFile.WriteLine("MOVE \"{0}\" \"{1}\"", Constants.LauncherUpdaterName, "AtlasLauncher.exe");
                batFile.WriteLine("DEL \"{0}\"", Constants.LauncherUpdaterName);
                batFile.WriteLine("DEL \"%~f0\" & START \"\" /B \"{0}\"", self);
            }

            var startInfo = new ProcessStartInfo("AtlasLauncherUpdate.bat");
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            Process.Start(startInfo);

            Environment.Exit(0);
        }

        private void Play()
        {
            if (!CheckUserPass()) return;
            BuildBat();
        }

        private bool CheckUserPass()
        {
            if (UsernameBox.Text == "" || PasswordBox.Password == "")
            {
                MessageBox.Show(Constants.MessageNoCredentials);
                return false;
            }

            if (Settings.Default.SaveAccount)
            {
                Settings.Default.Username = UsernameBox.Text;
                Settings.Default.Password = PasswordBox.Password;
                Settings.Default.QuickCharacter = QuickloginCombo.Text;
            }
            else
            {
                Settings.Default.Username = "";
                Settings.Default.Password = "";
                Settings.Default.QuickCharacter = "";
            }

            Settings.Default.Save();
            return true;
        }

        private void BuildBat()
        {
            var quick = QuickloginCombo.Text;
            var quickSelection = "";
            var serverIP = "";

            if (quick != "")
            {
                var quickSplit = quick.Split('-');
                var quickToon = quickSplit[0].Trim();
                var quickRealmID = quickSplit[1].Trim();
                var quickRealm = "";

                switch (quickRealmID)
                {
                    case "Alb":
                        quickRealm = "1";
                        break;
                    case "Mid":
                        quickRealm = "2";
                        break;
                    default:
                        quickRealm = "3";
                        break;
                }

                quickSelection = quickToon + " " + quickRealm;
            }

            serverIP = Settings.Default.Titan ? Constants.TitanIP : Constants.LiveIP;

            var command = "connect.exe game1127.dll " + serverIP + " " + UsernameBox.Text + " " + PasswordBox.Password +
                          " " + quickSelection;
            StartAtlas(command);
        }

        private static void StartAtlas(string command)
        {
            ProcessStartInfo ProcessInfo;
            Process AtlasProcess;
            ProcessInfo = new ProcessStartInfo("cmd.exe", "/K " + command);
            ProcessInfo.WindowStyle = ProcessWindowStyle.Hidden;
            ProcessInfo.UseShellExecute = true;
            AtlasProcess = Process.Start(ProcessInfo);

            if (!Settings.Default.KeepOpen) Environment.Exit(0);
        }

        private void GetQuickCharacters()
        {
            var path = Environment.ExpandEnvironmentVariables(Constants.AppData) + Constants.UserPath;
            if (!File.Exists(path)) return;

            var userDat = File.ReadAllLines(path);
            var quickCharacters = new List<string>();

            foreach (var line in userDat)
            {
                if (line.Contains("setentry=")) continue;

                if (!line.Contains("entry")) continue;
                var entry = line.Split('=');
                var entryData = entry[1].Split(',');
                if (entryData[0] == "") continue;

                var realm = "";

                var entryRealm = entryData[4].Trim();

                switch (entryRealm)
                {
                    case "1":
                        realm = "Alb";
                        break;
                    case "2":
                        realm = "Mid";
                        break;
                    default:
                        realm = "Hib";
                        break;
                }

                quickCharacters.Add(entryData[0] + " - " + realm);
            }

            quickCharacters.Add("");

            QuickloginCombo.ItemsSource = quickCharacters.Distinct();
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            new OptionsWindow().ShowDialog();
        }

        private void PatchButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(Constants.PatchNotesUrl);
        }

        private void GridBread_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BreadClicks++;
            if (BreadClicks != 10) return;
            new BreadWindow().ShowDialog();
            BreadClicks = 0;
        }

        private void TaskbarIcon_OnTrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            if (!IsVisible)
            {
                Show();
                if (WindowState == WindowState.Minimized)
                {
                    WindowState = WindowState.Normal;
                }

                Activate();
                Topmost = true;  // important
                Topmost = false; // important
                Focus();         // important
            }
            else
            {
                MinimizeLauncher();
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState != WindowState.Minimized) return;
            if (Settings.Default.MinimizeToTray)
            {
                Hide();
            }
        }

        private void MinimizeLauncher()
        {
            WindowState = WindowState.Minimized;
            Hide();
        }

        private void MaximizeLauncher()
        {
            WindowState = WindowState.Normal;
            Show();
        }
    }

    internal class Stats
    {
        public int Albion { get; set; }
        public int Midgard { get; set; }
        public int Hibernia { get; set; }
    }
}