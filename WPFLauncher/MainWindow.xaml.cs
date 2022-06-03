using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace WPFLauncher
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _updateAvailable;

        private BackgroundWorker _updateChecker;
        private int BreadClicks = 0;

        public MainWindow()
        {
            InitializeComponent();
            InitializeWorkers();
            CheckVersion();
            GetQuickCharacters();
            InitializeSettings();
            Activated += RefreshCount;
        }

        private void InitializeSettings()
        {
            LauncherWindow.Title = "Atlas Launcher v" + Constants.LauncherVersion;
            if (Settings.Default.Username != "") UsernameBox.Text = Settings.Default.Username;
            if (Settings.Default.Password != "") PasswordBox.Password = Settings.Default.Password;
            if (Settings.Default.QuickCharacter != "") QuickloginCombo.Text = Settings.Default.QuickCharacter;
        }

        private void InitializeWorkers()
        {
            _updateChecker = new BackgroundWorker();
            _updateChecker.DoWork += UpdateChecker_DoWork;
            _updateChecker.RunWorkerCompleted += UpdateChecker_RunWorkerCompleted;
            _updateChecker.ProgressChanged += UpdateChecker_ProgressChanged;
            _updateChecker.WorkerReportsProgress = true;
        }

        private void UpdateChecker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            PlayButton.Content = $"{e.ProgressPercentage}%";
        }

        private void CheckVersion()
        {
            _updateAvailable = Updater.CheckForNewVersion();
            PlayButton.Content = _updateAvailable ? "Update" : "Play";
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            _updateAvailable = Updater.CheckForNewVersion();
            if (_updateAvailable)
            {
                PlayButton.Content = _updateAvailable ? "Checking.." : "Play";
                PlayButton.IsEnabled = !_updateAvailable;
                _updateChecker.RunWorkerAsync(2000);
            }
            else
            {
                Play();
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Constants.RegisterUrl);
        }

        private void RefreshCount(object sender, EventArgs e)
        {
            GetPlayerCount();
            CheckVersion();
        }

        private void UpdateChecker_DoWork(object sender, DoWorkEventArgs e)
        {
            var patchlist = Updater.GetPatchlist();
            var filesToDownload = Updater.GetFilesToDownload(patchlist);
            HttpClient _httpClient;
            _httpClient = new HttpClient();
            try
            {
                var updateSelf = false;
                var totalFiles = filesToDownload.Count;
                foreach (var file in filesToDownload)
                {
                    var data = _httpClient.GetByteArrayAsync(Constants.RemoteFilePath + file.FileName).Result;
                    if (file.FileName.Contains("AtlasLauncher")) updateSelf = true;
                    new FileInfo(file.FileName).Directory?.Create();
                    File.WriteAllBytes(file.FileName, data);
                    _updateChecker.ReportProgress(100 * (filesToDownload.IndexOf(file) + 1) / totalFiles);
                }

                Updater.SaveLocalVersion(Updater.GetVersion());
                if (updateSelf) //handle self update https://andreasrohner.at/posts/Programming/C%23/A-platform-independent-way-for-a-C%23-program-to-update-itself/
                    UpdateLauncher();
            }
            catch (Exception exception)
            {
                MessageBox.Show(Constants.MessageDownloadError);
            }
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
                batFile.WriteLine("DEL \"%~f0\" & START \"\" /B \"{0}\"", self);
            }

            var startInfo = new ProcessStartInfo("AtlasLauncherUpdate.bat");
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            Process.Start(startInfo);

            Environment.Exit(0);
        }

        private void UpdateChecker_RunWorkerCompleted(object sender,
            RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            PlayButton.Content = !Updater.CheckForNewVersion() ? "Play" : "Update";
            PlayButton.IsEnabled = true;
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

            serverIP = Settings.Default.PTR ? Constants.PtrIP : Constants.LiveIP;

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

            QuickloginCombo.ItemsSource = quickCharacters.Distinct();
        }

        private void GetPlayerCount()
        {
            try
            {
                var webRequest = WebRequest.Create("https://api.atlasfreeshard.com/stats") as HttpWebRequest;

                if (webRequest == null) return;
                webRequest.ContentType = "application/json";
                webRequest.UserAgent = "Nothing";

                var response = webRequest.GetResponse();

                using (var s = response.GetResponseStream())
                {
                    using (var sr = new StreamReader(s))
                    {
                        var jsonStats = sr.ReadToEnd();
                        var stats = JsonConvert.DeserializeObject<Stats>(jsonStats);
                        if (stats != null && stats.Albion < 10)
                            AlbLabel.Content = "0" + stats.Albion;
                        else
                            AlbLabel.Content = stats?.Albion.ToString();
                        if (stats != null && stats.Midgard < 10)
                            MidLabel.Content = "0" + stats.Midgard;
                        else
                            MidLabel.Content = stats?.Midgard.ToString();
                        if (stats != null && stats.Hibernia < 10)
                            HibLabel.Content = "0" + stats.Hibernia;
                        else
                            HibLabel.Content = stats?.Hibernia.ToString();
                    }
                }
            }
            catch (Exception)
            {
                AlbLabel.Content = "N/A";
                MidLabel.Content = "N/A";
                HibLabel.Content = "N/A";
            }
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
    }

    internal class Stats
    {
        public int Albion { get; set; }
        public int Midgard { get; set; }
        public int Hibernia { get; set; }
    }
}