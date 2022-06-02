using WPFLauncher;
using System.Windows;


namespace WPFLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (Utils.CheckForNewVersion())
            {
                Utils.DownloadUpdates();
            }
        }
    }
}
