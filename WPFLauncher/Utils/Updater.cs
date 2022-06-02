using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using WPFLauncher.Properties;

namespace WPFLauncher
{
    public class Updater
    {
        private static int GetVersion()
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

        private static List<FileData> GetPatchlist()
        {
            var remotePatchlist = new List<FileData>();
            var client = new WebClient();
            var data = client.DownloadData(Constants.RemoteFileList);
            var patchlistByLine = Encoding.Default.GetString(data)
                .Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
            Constants.RemoteFilePath = patchlistByLine[0];

            for (var i = 1; i < patchlistByLine.Length; i += 2)
            {
                var file = new FileData(patchlistByLine[i], patchlistByLine[i + 1]);
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

        private static List<FileData> GetFilesToDownload(List<FileData> remotepatchList)
        {
            var filesToDownload = remotepatchList.Where(file => !LocalFileExists(file)).ToList();
            return filesToDownload;
        }

        public static bool DownloadUpdates()
        {
            var patchlist = GetPatchlist();
            var filesToDownload = GetFilesToDownload(patchlist);
            var i = 0; //TODO remove
            HttpClient _httpClient;
            _httpClient = new HttpClient();
            try
            {
                foreach (var file in filesToDownload)
                {
                    if (file.FileName == "AtlasLauncher.exe") continue; //TODO handle self update https://andreasrohner.at/posts/Programming/C%23/A-platform-independent-way-for-a-C%23-program-to-update-itself/
                    var data = _httpClient.GetByteArrayAsync(Constants.RemoteFilePath + file.FileName).Result;
                    File.WriteAllBytes(file.FileName, data);
                    //TODO debug stuff this needs to be removed
                    i++;
                    if (i > 10) break;
                }
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(Constants.MessageDownloadError);
                return false;
            }
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