using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
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
        public MainWindow()
        {
            InitializeComponent();
            InitializeWorkers();
            CheckVersion();
            GetQuickCharacters();
            InitializeSettings();
        }

        private void InitializeSettings()
        {
            LauncherWindow.Title = "Atlas Launcher v" + Constants.LauncherVersion;
            if (Settings.Default.Username != null)
            {
                UsernameBox.Text = Settings.Default.Username;
            }
            if (Settings.Default.Password != null)
            {
                PasswordBox.Password = Settings.Default.Password;
            }
            if (Settings.Default.QuickCarachter != null)
            {
                QuickloginCombo.Text = Settings.Default.QuickCarachter;
            }

        }
        
        private void InitializeWorkers()
        {
            _updateChecker = new BackgroundWorker();
            _updateChecker.DoWork += UpdateChecker_DoWork;
            _updateChecker.RunWorkerCompleted += UpdateChecker_RunWorkerCompleted;
            _updateChecker.WorkerReportsProgress = true;
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
                PlayButton.Content = _updateAvailable ? "Updating.." : "Play";
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

        private void UpdateChecker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep((int) e.Argument);
            Dispatcher.Invoke(() =>
            {
                Updater.DownloadUpdates();
            });
        }

        private void UpdateChecker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        { 
            PlayButton.Content = "Play";
            PlayButton.IsEnabled = true;
            SaveLocalVersion(Updater.GetVersion());
        }

        private static void SaveLocalVersion(int version)
        {
            Settings.Default.localVersion = version;
            Settings.Default.Save();
        }

        private void Play()
        {
            if (!CheckUserPass()) return;
            BuildBat();
            StartAtlas();
        }
        
        private bool CheckUserPass()
        {
            if (UsernameBox.Text == "" || PasswordBox.Password == "")
            {
                MessageBox.Show(Constants.MessageNoCredentials);
                return false;
            }

            if (!Settings.Default.SaveAccount) return true;
            Settings.Default.Username = UsernameBox.Text;
            Settings.Default.Password = PasswordBox.Password;
            Settings.Default.QuickCarachter = QuickloginCombo.Text;
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

                if (quickRealmID == "Alb")
                    quickRealm = "1";
                else if (quickRealmID == "Mid")
                    quickRealm = "2";
                else
                    quickRealm = "3";

                quickSelection = quickToon + " " + quickRealm;
            }

            serverIP = Settings.Default.PTR ? Constants.PtrIP : Constants.LiveIP;

            TextWriter run = new StreamWriter("atlaslauncher.bat");
            run.WriteLine("connect.exe game1127.dll " + serverIP + " " + UsernameBox.Text + " " + PasswordBox.Password + " " + quickSelection);
            run.Close();
        }

        private static void StartAtlas()
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "atlaslauncher.bat",
                    UseShellExecute = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
            if (process.ExitCode == 0)
            {
            }
            else
            {
                MessageBox.Show(Constants.MessageReviewInstallation, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            if (!Settings.Default.KeepOpen) Environment.Exit(0);
        }
        private void GetQuickCharacters()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                       "\\Electronic Arts\\Dark Age of Camelot\\Atlas\\user.dat";
            if (!File.Exists(path)) return;

            var userDat = File.ReadAllLines(path);
            var quickCharacters = new List<string>();

            foreach (var line in userDat)
            {
                if (line.Contains("setentry=")) continue;

                if (line.Contains("entry"))
                {
                    var entry = line.Split('=');
                    var entryData = entry[1].Split(',');
                    if (entryData[0] == "") continue;

                    var realm = "";

                    var entryRealm = entryData[4].Trim();

                    if (entryRealm == "1")
                        realm = "Alb";
                    else if (entryRealm == "2")
                        realm = "Mid";
                    else
                        realm = "Hib";

                    quickCharacters.Add(entryData[0] + " - " + realm);
                }
            }

            QuickloginCombo.ItemsSource = quickCharacters;
        }
        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            new OptionsWindow().ShowDialog();
        }
    }
}