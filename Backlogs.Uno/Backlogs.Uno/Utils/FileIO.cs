﻿using Backlogs.Models;
using Backlogs.Services;
using Backlogs.Uno;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
//using Windows.UI.Xaml.Media.Imaging;

namespace Backlogs.Utils.Uno
{
    public class FileIO : IFileHandler
    {
        StorageFolder m_localFolder = ApplicationData.Current.LocalFolder;
        string m_fileName = "backlogs.txt";
        SemaphoreSlim m_semaphore = new SemaphoreSlim(1);

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

        public async Task<string?> DownloadBacklogsJsonAsync()
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
                string? jsonDownload = await DownloadBacklogsJsonAsync();
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
            return image.Path;
        }

        public async Task<List<Log>> ReadLogsAync()
        {
            var folder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("Logs", CreationCollisionOption.OpenIfExists);
            var path = Path.Combine(folder.Path, "backlogs.log");
            var logFile = await StorageFile.GetFileFromPathAsync(path);
            var lines = await Windows.Storage.FileIO.ReadLinesAsync(logFile);
            var logList = new List<Log>();
            foreach (var line in lines)
            {
                if (line == "\n" || line == "") continue;
                var currLine = line.Split("---");
                Debug.WriteLine("Printing.." + line);
                Log _ = new Log() { Date = currLine[0], Message = currLine[1] };
                logList.Add(_);
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
            Debug.WriteLine("HI");
            string json = JsonConvert.SerializeObject(backlogs);
            Debug.WriteLine("StorageFile:" +json);
            StorageFile storageFile = await m_localFolder.CreateFileAsync(m_fileName, CreationCollisionOption.ReplaceExisting); 
            await Windows.Storage.FileIO.WriteTextAsync(storageFile, json);
            var graphServiceClient = await App.Services.GetRequiredService<IMsal>().GetGraphServiceClient();
            Debug.WriteLine(sync);
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
            //using (var randomAccessStream = stream.AsRandomAccessStream())
            //{
            //    BitmapImage image = new BitmapImage();
            //    randomAccessStream.Seek(0);
            //    await image.SetSourceAsync(randomAccessStream);

            //    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
            //    SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();
            //    var storageFile = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            //    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, await storageFile.OpenAsync(FileAccessMode.ReadWrite));
            //    encoder.SetSoftwareBitmap(softwareBitmap);
            //    await encoder.FlushAsync();
            //}
            throw new NotImplementedException();
        }

        public async Task WriteLogsAsync(string message, Exception? ex = null)
        {
            StorageFolder logsFolder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("Logs", CreationCollisionOption.OpenIfExists);
            await m_semaphore.WaitAsync();
            try
            {
                var logFile = await logsFolder.CreateFileAsync("backlogs.log", CreationCollisionOption.ReplaceExisting);
                Log log = new Log();
                log.Date = $"{DateTime.Now}";
                if (ex != null)
                    log.Message = $"{message}. Exception: {ex.Message}";
                else
                    log.Message = message;

                await Windows.Storage.FileIO.AppendTextAsync(logFile, log.ToString());
            }
            catch
            {
                Debug.WriteLine("File busy!!!!");
            }
            finally
            {
                m_semaphore.Release();
            }



        }

        public async Task WriteTextAsync(string text, string fileName)
        {
            var storageFile = await m_localFolder.GetFileAsync(fileName);
            await m_semaphore.WaitAsync();
            try
            {
                await Windows.Storage.FileIO.WriteTextAsync(storageFile, text);
            }
            finally
            {
                m_semaphore.Release();
            }
        }
    }
}
