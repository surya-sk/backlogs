using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using backlog.Models;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using backlog.Utils;
using backlog.Auth;

namespace backlog.Saving
{
    class SaveData
    {
        private static SaveData instance = new SaveData();
        private ObservableCollection<Backlog> Backogs = null;
        StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        string fileName = "backlogs.txt";
        private static GraphServiceClient graphServiceClient = null;

        private SaveData()
        {
        }

        public static SaveData GetInstance()
        {
            return instance;
        }



        private async Task DeleteLocalFiles()
        {
            var file = await localFolder.GetFileAsync(fileName);
            await file.DeleteAsync(StorageDeleteOption.Default);
        }


        /// <summary>
        /// Write the backlog list in JSON format
        /// </summary>
        /// <returns></returns>
        public async Task WriteDataAsync(bool sync = false)
        {
            string json = JsonConvert.SerializeObject(Backogs);
            StorageFile storageFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(storageFile, json);
            graphServiceClient = await MSAL.GetGraphServiceClient();
            if(sync)
            {
                if (graphServiceClient is null)
                {
                    return;
                }
                using (var stream = await storageFile.OpenStreamForWriteAsync())
                {
                    await graphServiceClient.Me.Drive.Root.ItemWithPath(fileName).Content.Request().PutAsync<DriveItem>(stream);
                }
            }
        }

        public void SaveSettings(ObservableCollection<Backlog> backlogs)
        {
            Backogs = backlogs;
        }

        /// <summary>
        /// Read the backlog list in JSON and deserialze it 
        /// </summary>
        /// <returns></returns>
        public async Task ReadDataAsync(bool sync = false)
        {
            try
            {
                if(sync)
                {
                    string jsonDownload = await DownloadBacklogsJsonAsync();
                    if(jsonDownload != null)
                    {
                        StorageFile file = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                        await FileIO.WriteTextAsync(file, jsonDownload);
                    }
                }
                StorageFile storageFile = await localFolder.GetFileAsync(fileName);
                string json = await FileIO.ReadTextAsync(storageFile);
                Backogs = JsonConvert.DeserializeObject<ObservableCollection<Backlog>>(json);
            }
            catch
            {
                Backogs = new ObservableCollection<Backlog>();
            }
        }

        /// <summary>
        /// Download backlog json
        /// </summary>
        /// <returns></returns>
        private async Task<string> DownloadBacklogsJsonAsync()
        {
            graphServiceClient = await MSAL.GetGraphServiceClient();
            var search = await graphServiceClient.Me.Drive.Root.Search(fileName).Request().GetAsync();
            if (search.Count == 0)
            {
                return null;
            }
            using (Stream stream = await graphServiceClient.Me.Drive.Root.ItemWithPath(fileName).Content.Request().GetAsync())
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    string json = sr.ReadToEnd();
                    return json;
                }
            }
        }

        /// <summary>
        /// Delete the local backlogs
        /// </summary>
        /// <returns></returns>
        public async Task DeleteLocalFileAsync()
        {
            try
            {
                StorageFile file = await localFolder.GetFileAsync(fileName);
                await file.DeleteAsync(StorageDeleteOption.Default);
            }
            catch(FileNotFoundException ex)
            {
                await Logging.Logger.Error("Error deleting local backlogs", ex);
            }
        }

        public ObservableCollection<Backlog> GetBacklogs()
        {
            return Backogs;
        }
    }
}
