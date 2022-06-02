using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using WPFLauncher.Properties;

namespace WPFLauncher
{
    public class Updater
    {
        public static int GetVersion()
        {
            var client = new WebClient();
            var stream = client.OpenRead(Constants.RemoteVersionUrl);
            var reader = new StreamReader(stream);
            var line = reader.ReadLine();
            var version = int.Parse(line);
            return version;
        }

        public static bool CheckForNewVersion()
        {
            var version = GetVersion();
            var currentVersion = Settings.Default.localVersion;
            return version > currentVersion;
        }

        public static List<FileData> GetPatchlist()
        {
            var remotePatchlist = new List<FileData>();
            var client = new WebClient();
            var data = client.DownloadData(Constants.RemoteFileList);
            var patchlistByLine = Encoding.Default.GetString(data)
                .Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
            Constants.RemoteFilePath = patchlistByLine[0];

            for (var i = 1; i < patchlistByLine.Length; i ++)
            {
                var line = patchlistByLine[i].Split(new[] {'\t'}, StringSplitOptions.RemoveEmptyEntries);
                var file = new FileData(line[0], line[1], line[2]);
                remotePatchlist.Add(file);
            }

            return remotePatchlist;
        }

        private static bool LocalFileExists(FileData file)
        {
            if (!File.Exists(file.FileName)) return false;
            var localHash = CalculateHash(file.FileName);
            return localHash == file.Hash;
        }

        public static List<FileData> GetFilesToDownload(List<FileData> remotepatchList)
        {
            var filesToDownload = remotepatchList.Where(file => !LocalFileExists(file)).ToList();
            return filesToDownload;
        }
        
        public static void SaveLocalVersion(int version)
        {
            Settings.Default.localVersion = version;
            Settings.Default.Save();
        }
        
        private static string CalculateHash(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return StringifyHash(md5.ComputeHash(stream));
                }
            }
        }
        private static string StringifyHash(byte[] md5)
        {
            return BitConverter.ToString(md5).Replace("-", "").ToLowerInvariant();
        }
    }
}