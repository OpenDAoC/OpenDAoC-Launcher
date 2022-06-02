using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace WPFLauncher
{
    public class Utils
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
            var currentVersion = Properties.Settings.Default.localVersion;
            return version > currentVersion;
        }

        private static List<FileData> GetPatchlist()
        {
            var remotePatchlist = new List<FileData>();
            var client = new WebClient();
            var data = client.DownloadData(Constants.RemoteFileList);
            var patchlistByLine = Encoding.Default.GetString(data).Split(new [] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
            Constants.RemoteFilePath = patchlistByLine[0];
            Console.WriteLine(Constants.RemoteFilePath);
            
            for (var i = 1; i < patchlistByLine.Length; i += 2)
            {
                var file = new FileData(patchlistByLine[i], patchlistByLine[i + 1]);
                remotePatchlist.Add(file);
            }
            
            return remotePatchlist;
        }

        private static bool LocalFileExists(FileData file)
        {
            return File.Exists(file.FileName);
        }

        private static List<FileData> GetFilesToDownload(List<FileData> remotepatchList)
        {
            var filesToDownload = remotepatchList.Where(file => !LocalFileExists(file)).ToList();
            return filesToDownload;
        }

        public static void DownloadUpdates()
        {
            if (!CheckForNewVersion()) return;
            var patchlist = GetPatchlist();
            var filesToDownload = GetFilesToDownload(patchlist);
            var i = 0;
            HttpClient _httpClient;
            _httpClient = new HttpClient();
            foreach (var file in filesToDownload)
            {
                var data = _httpClient.GetByteArrayAsync(Constants.RemoteFilePath + file.FileName).Result;
                File.WriteAllBytes(file.FileName, data);
                i++;
                if (i > 10) break;
            }
        }
    }
}
