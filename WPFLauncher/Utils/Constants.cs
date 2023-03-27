namespace WPFLauncher
{
    internal class Constants
    {
        public const string LauncherVersion = "1.0.9";
        
        public const bool DisableDiscordCheck = false;

        #region Patcher

        public const string LauncherUpdaterName = "OpenDAoCLauncher_Update.exe";
        public const string AppData = @"%AppData%\";
        public const string UserPath = "\\Electronic Arts\\Dark Age of Camelot\\OpenDAoC\\user.dat";
        
        public const string
            RemoteVersionUrl = "https://patch.atlasfreeshard.com/version-new.txt";
        public const string
            RemoteFileList = "https://patch.atlasfreeshard.com/patchlist-new.txt"; 
        
        public static string RemoteFilePath;
        
        #endregion

        #region gameserver

        public const string LiveIP = "play.atlasfreeshard.com:10300";
        public const string TitanIP = "play.atlasfreeshard.com:10350";

        #endregion

        #region Player Urls

        public const string RegisterUrl = "https://account.atlasfreeshard.com/";
        public const string LinkUrl = "https://account.atlasfreeshard.com/";

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