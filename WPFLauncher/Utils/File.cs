namespace WPFLauncher
{
    /// <summary>
    ///     The class used to model the info of a file.
    /// </summary>
    public class FileData : IFile
    {
        /// <summary>
        ///     Creates a new instance of the class File.
        /// </summary>
        public FileData(string filename, string hash)
        {
            FileName = filename;
            Hash = hash;
        }

        /// <summary>
        ///     The hash of the file.
        /// </summary>
        public string Hash { get; }

        /// <summary>
        ///     The file name of the file.
        /// </summary>
        public string FileName { get; }
    }
}
