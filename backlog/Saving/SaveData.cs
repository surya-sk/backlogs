﻿using System;
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
        private ObservableCollection<Backlog> IncompleteBacklogs = null;
        private ObservableCollection<Backlog> RecentlyAddedBacklogs = null;
        private ObservableCollection<Backlog> RecentlyCompletedBacklogs = null;
        private ObservableCollection<Backlog> InProgressBacklogs = null;
        private ObservableCollection<Backlog> UpcomingBacklogs = null;
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
            ResetHelperBacklogs();
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

        public ObservableCollection<Backlog> GetCompletedBacklogs()
        {
            return CompletedBacklogs;
        }

        public void ResetHelperBacklogs()
        {
            CompletedBacklogs = new ObservableCollection<Backlog>(Backlogs.Where(b => b.IsComplete));
            IncompleteBacklogs = new ObservableCollection<Backlog>(Backlogs.Where(b => b.IsComplete == false));
            InProgressBacklogs = new ObservableCollection<Backlog>(IncompleteBacklogs.Where(b => b.Progress > 0));
            var _sortedCompletedBacklogs = new ObservableCollection<Backlog>();
            var _sortedIncompleteBacklogs = new ObservableCollection<Backlog>();
            foreach(var backlog in IncompleteBacklogs)
            {
                try
                {
                    if (DateTime.Parse(backlog.TargetDate) >= DateTime.Today)
                    {
                        UpcomingBacklogs.Add(backlog);
                    }
                }
                catch
                {
                    continue;
                }
                if (backlog.CreatedDate == "None" || backlog.CreatedDate == null)
                {
                    backlog.CreatedDate = DateTimeOffset.MinValue.ToString("d", CultureInfo.InvariantCulture);
                }
                _sortedIncompleteBacklogs.Add(backlog);
            }
            foreach(var backlog in CompletedBacklogs)
            {
                try
                {
                    backlog.CompletedDate = DateTimeOffset.MinValue.ToString("d", CultureInfo.InvariantCulture);
                }
                catch
                {
                    continue;
                }
                _sortedCompletedBacklogs.Add(backlog);
            }
            foreach (var backlog in _sortedIncompleteBacklogs.OrderByDescending(b => DateTime.Parse(b.CreatedDate, CultureInfo.InvariantCulture)).Skip(0).Take(6))
            {
                RecentlyAddedBacklogs.Add(backlog);
            }
            foreach (var backlog in _sortedCompletedBacklogs.OrderByDescending(b => DateTime.Parse(b.CompletedDate, CultureInfo.InvariantCulture)).Skip(0).Take(6))
            {
                RecentlyCompletedBacklogs.Add(backlog);
            }
        }

        public ObservableCollection<Backlog> GetIncompleteBacklogs()
        {
            return IncompleteBacklogs;
        }

        public ObservableCollection<Backlog> GetRecentlyAddedBacklogs()
        {
            return RecentlyAddedBacklogs;
        }

        public ObservableCollection<Backlog> GetRecentlyCompletedBacklogs()
        {
            return RecentlyCompletedBacklogs;
        }

        public ObservableCollection<Backlog> GetInProgressBacklogs()
        {
            return InProgressBacklogs;
        }

        public ObservableCollection<Backlog> GetUpcomingBacklogs()
        {
            return UpcomingBacklogs;
        }
    }
}
