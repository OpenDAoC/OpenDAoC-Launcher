namespace WPFLauncher
{
    internal class Constants
    {
        public const string RemoteVersionUrl = "https://patch.atlasfreeshard.com/version.txt";
        public const string RemoteFileList = "https://patch.atlasfreeshard.com/patchlist-new.txt"; //TODO change to production url

        public static string RemoteFilePath;
        
        #region Messages
        
        public const string MessageDownloadError = "Error downloading files. Please try again later.";
        
        #endregion
    }
}