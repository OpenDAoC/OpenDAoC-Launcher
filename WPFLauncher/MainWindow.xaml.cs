using System.ComponentModel;
using System.Threading;
using System.Windows;

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

        }
        private void InitializeWorkers()
        {
            _updateChecker = new BackgroundWorker();
            _updateChecker.DoWork += UpdateChecker_DoWork;
            _updateChecker.RunWorkerCompleted += UpdateChecker_RunWorkerCompleted;
            _updateChecker.WorkerReportsProgress = true;
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            _updateChecker.RunWorkerAsync(2000);
        }
        
        private void UpdateChecker_DoWork(object sender, DoWorkEventArgs e)
        {
            _updateAvailable = Updater.CheckForNewVersion();
            Dispatcher.Invoke(() =>
            {
                PlayButton.Content = _updateAvailable ? "Updating.." : "Play";
            });

            if (!_updateAvailable) return;
            Thread.Sleep((int) e.Argument);
            Dispatcher.Invoke(() =>
            {
                Updater.DownloadUpdates();
            });
        }
        
        
        private void UpdateChecker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        { 
            PlayButton.Content = "Play";
        }
    }
}