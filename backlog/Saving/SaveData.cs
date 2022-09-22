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
using System.Globalization;

namespace backlog.Saving
{
    class SaveData
    {
        private static SaveData instance = new SaveData();
        private ObservableCollection<Backlog> Backlogs = null;
        private ObservableCollection<Backlog> CompletedBacklogs = null;
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


        /// <summary>
        /// Write the backlog list in JSON format
        /// </summary>
        /// <returns></returns>
        public async Task WriteDataAsync(bool sync = false)
        {
            string json = JsonConvert.SerializeObject(Backlogs);
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
            Backlogs = backlogs;
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
                Backlogs = JsonConvert.DeserializeObject<ObservableCollection<Backlog>>(json);
            }
            catch
            {
                Backlogs = new ObservableCollection<Backlog>();
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
        /// Read completed backlogs from the local file
        /// </summary>
        /// <returns></returns>
        public async Task ReadCompletedBacklogsAsync()
        {
            try
            {
                StorageFile storageFile = await localFolder.GetFileAsync("CompletedBacklogs.txt");
                string json = await FileIO.ReadTextAsync(storageFile);
                CompletedBacklogs = JsonConvert.DeserializeObject<ObservableCollection<Backlog>>(json);
            }
            catch
            {
                Debug.WriteLine("Creating new collection");
                CompletedBacklogs = new ObservableCollection<Backlog>(Backlogs.Where(b => b.IsComplete));
            }
        }

        /// <summary>
        /// Write completed backlogs to the local file
        /// </summary>
        /// <returns></returns>
        public async Task WriteCompletedBacklogsAsync()
        {
            string json = JsonConvert.SerializeObject(CompletedBacklogs);
            StorageFile storageFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(storageFile, json);
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
            return Backlogs;
        }

        public void SetCompletedBacklogs(ObservableCollection<Backlog> completedBacklogs)
        {
            CompletedBacklogs = completedBacklogs;
            switch (Settings.SortOrder)
            {
                case "Name":
                    CompletedBacklogs = new ObservableCollection<Backlog>(CompletedBacklogs.OrderBy(b => b.Name));
                    break;
                case "Completed Date Asc.":
                    CompletedBacklogs = new ObservableCollection<Backlog>(CompletedBacklogs.OrderBy(b => Convert.ToDateTime(b.CompletedDate, CultureInfo.InvariantCulture)));
                    break;
                case "Completed Date Dsc.":
                    CompletedBacklogs = new ObservableCollection<Backlog>(CompletedBacklogs.OrderByDescending(b => Convert.ToDateTime(b.CompletedDate, CultureInfo.InvariantCulture)));
                    break;
                case "Lowest Rating":
                    CompletedBacklogs = new ObservableCollection<Backlog>(CompletedBacklogs.OrderBy(b => b.UserRating));
                    break;
                case "Highest Rating":
                    CompletedBacklogs = new ObservableCollection<Backlog>(CompletedBacklogs.OrderByDescending(b => b.UserRating));
                    break;
            }
        }

        public ObservableCollection<Backlog> GetCompletedBacklogs()
        {
            return CompletedBacklogs;
        }

        
    }
}
