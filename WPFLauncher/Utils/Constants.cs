namespace WPFLauncher
{
    internal class Constants
    {
        public const string LauncherVersion = "1.1.0";
        
        public const bool DisableDiscordCheck = false;

        #region Patcher

        public const string LauncherUpdaterName = "OpenDAoCLauncher_Update.exe";
        public const string AppData = @"%AppData%\";
        public const string UserPath = "\\Electronic Arts\\Dark Age of Camelot\\OpenDAoC\\user.dat";
        
        public const string
            RemoteVersionUrl = "https://patch.opendaoc.com/version.txt";
        public const string
            RemoteFileList = "https://patch.opendaoc.com/patchlist.txt"; 
        
        public static string RemoteFilePath;
        
        #endregion

        #region gameserver

        public const string LiveIP = "play.opendaoc.com:10300";
        public const string TitanIP = "play.opendaoc.com:10350";

        #endregion

        #region Player Urls

        public const string RegisterUrl = "https://account.opendaoc.com/";
        public const string LinkUrl = "https://account.opendaoc.com/";

        #endregion
        
        #region Messages

        public const string MessageDownloadError = "Error downloading files. Please try again later.";
        public const string MessageNoCredentials = "Please enter your account and password.";
        
        public const string DiscordMessage = "Linking the account to Discord is now required to play on OpenDAoC. Would you like to do this now?";
        public const string DiscordCaption = "Game account not linked to Discord";
        public const string DiscordError = "You won't be able to play on OpenDAoC without linking your account to Discord";

        public const string MessageReviewInstallation =
            "There was an error launching the game, please review your installation.";

        #endregion
    }
}