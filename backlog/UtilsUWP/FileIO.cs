using Backlogs.Models;
using Backlogs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

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
                await Logging.Logger.Error("Error deleting local backlogs", ex);
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

        public async Task<string> ReadBacklogsAsync(bool sync = false)
        {
            if (sync)
            {
                string jsonDownload = await DownloadBacklogsJsonAsync();
                if (jsonDownload != null)
                {
                    StorageFile file = await m_localFolder.CreateFileAsync(m_fileName, CreationCollisionOption.ReplaceExisting);
                    await WriteTextAsync(m_fileName, jsonDownload);
                }
            }
            StorageFile storageFile = await m_localFolder.GetFileAsync(m_fileName);
            string json = await ReadTextAsync(storageFile.Name);
            return json;
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
            await WriteTextAsync(m_fileName, json);
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

        public Task WriteBitmapAsync(Stream stream, string fileName)
        {
            throw new NotImplementedException();
        }

        public async Task WriteTextAsync(string text, string fileName)
        {
            var storageFile = await m_localFolder.GetFileAsync(fileName);
            await Windows.Storage.FileIO.WriteTextAsync(storageFile, text);
        }
    }
}
