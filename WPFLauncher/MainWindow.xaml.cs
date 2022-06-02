using System.Windows;

namespace WPFLauncher
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _updateAvailable;
        public MainWindow()
        {
            InitializeComponent();
            _updateAvailable = Updater.CheckForNewVersion();
            PlayButton.Content = _updateAvailable ? "Update" : "Play";
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (_updateAvailable)
                Updater.DownloadUpdates();
        }
    }
}