using Backlogs.Models;
using Backlogs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace Backlogs.Utils.UWP
{
    public class FileIO : IFileHandler
    {
        StorageFolder m_localFolder = ApplicationData.Current.LocalFolder;
        string m_fileName = "backlogs.txt";

        public async Task DeleteLocalFilesAsync()
        {
            try
            {
                StorageFile file = await m_localFolder.GetFileAsync(m_fileName);
                await file.DeleteAsync(StorageDeleteOption.Default);
            }
            catch (FileNotFoundException ex)
            {
                await WriteLogsAsync("Error deleting local backlogs", ex);
            }
        }

        public async Task<string> DownloadBacklogsJsonAsync()
        {
            var graphServiceClient = await App.Services.GetRequiredService<IMsal>().GetGraphServiceClient();
            var search = await graphServiceClient.Me.Drive.Root.Search(m_fileName).Request().GetAsync();
            if (search.Count == 0)
            {
                return null;
            }
            using (Stream stream = await graphServiceClient.Me.Drive.Root.ItemWithPath(m_fileName).Content.Request().GetAsync())
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    string json = sr.ReadToEnd();
                    return json;
                }
            }
        }

        public async Task<string> ReadBacklogJsonAsync(string fileName)
        {
            StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;
            StorageFile file = await tempFolder.GetFileAsync(fileName);
            string json = await Windows.Storage.FileIO.ReadTextAsync(file);
            return json;
        }

        public async Task<string> ReadBacklogsAsync(bool sync = false)
        {
            if (sync)
            {
                string jsonDownload = await DownloadBacklogsJsonAsync();
                if (jsonDownload != null)
                {
                    StorageFile file = await m_localFolder.CreateFileAsync(m_fileName, CreationCollisionOption.ReplaceExisting);
                    await Windows.Storage.FileIO.WriteTextAsync(file, jsonDownload);
                }
            }
            StorageFile storageFile = await m_localFolder.GetFileAsync(m_fileName);
            string json = await ReadTextAsync(storageFile.Name);
            return json;
        }

        public async Task<string> ReadImageAsync(string fileName)
        {
            var cacheFolder = ApplicationData.Current.LocalCacheFolder;
            var image = await cacheFolder.GetFileAsync(fileName);
            return image.Name;
        }

        public async Task<List<string>> ReadLogsAync()
        {
            var folder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("Logs", CreationCollisionOption.OpenIfExists);
            var path = Path.Combine(folder.Path, "backlogs.log");
            var logFile = await StorageFile.GetFileFromPathAsync(path);
            var lines = await Windows.Storage.FileIO.ReadLinesAsync(logFile);
            var logList = new List<string>();
            foreach (var line in lines)
            {
                logList.Add(line);
            }
            return logList;
        }

        public async Task<string> ReadTextAsync(string fileName)
        {
            var storageFile = await m_localFolder.GetFileAsync(fileName);
            string text = await Windows.Storage.FileIO.ReadTextAsync(storageFile);
            return text;
        }

        public async Task WriteBacklogsAsync(ObservableCollection<Backlog> backlogs, bool sync = false)
        {
            string json = JsonConvert.SerializeObject(backlogs);
            StorageFile storageFile = await m_localFolder.CreateFileAsync(m_fileName, CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(storageFile, json);
            var graphServiceClient = await App.Services.GetRequiredService<IMsal>().GetGraphServiceClient();
            if (sync)
            {
                if (graphServiceClient is null)
                {
                    return;
                }
                using (var stream = await storageFile.OpenStreamForWriteAsync())
                {
                    await graphServiceClient.Me.Drive.Root.ItemWithPath(m_fileName).Content.Request().PutAsync<DriveItem>(stream);
                }
            }
        }

        public async Task WriteBitmapAsync(Stream stream, string fileName)
        {
            using (var randomAccessStream = stream.AsRandomAccessStream())
            {
                BitmapImage image = new BitmapImage();
                randomAccessStream.Seek(0);
                await image.SetSourceAsync(randomAccessStream);

                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
                SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                var storageFile = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, await storageFile.OpenAsync(FileAccessMode.ReadWrite));
                encoder.SetSoftwareBitmap(softwareBitmap);
                await encoder.FlushAsync();
            }
        }

        public async Task WriteLogsAsync(string message, Exception ex = null)
        {
            StorageFolder logsFolder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("Logs", CreationCollisionOption.OpenIfExists);
            try
            {
                var logFile = await logsFolder.GetFileAsync("backlogs.log");
                if (ex != null)
                    await Windows.Storage.FileIO.AppendTextAsync(logFile, $"[{DateTime.Now}] - {message}\nException: {ex.Message}\n\n");
                else
                    await Windows.Storage.FileIO.AppendTextAsync(logFile, $"[{DateTime.Now}] - {message}\n\n");
            }
            catch
            {
                await logsFolder.CreateFileAsync("backlogs.log", CreationCollisionOption.ReplaceExisting);
                await WriteLogsAsync(message, ex);
            }

        }

        public async Task WriteTextAsync(string text, string fileName)
        {
            var storageFile = await m_localFolder.GetFileAsync(fileName);
            await Windows.Storage.FileIO.WriteTextAsync(storageFile, text);
        }
    }
}
