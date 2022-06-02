using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
            InitializeSettings();
            CheckVersion();
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
            Settings.Default.Username = UsernameBox.Text;
            Settings.Default.Password = PasswordBox.Password;
            Settings.Default.Save();
            return true;
        }

        private void BuildBat()
        {
            TextWriter run = new StreamWriter("atlaslauncher.bat");
            run.WriteLine("connect.exe game1127.dll " + Constants.LiveIP + " " + UsernameBox.Text + " " + PasswordBox.Password);
            run.Close();
        }

        private void StartAtlas()
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
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            new OptionsWindow().ShowDialog();
        }
    }
}