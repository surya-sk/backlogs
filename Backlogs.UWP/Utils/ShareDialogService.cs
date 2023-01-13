using Backlogs.Models;
using Backlogs.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using WinFileIO = Windows.Storage.FileIO;

namespace Backlogs.Utils.UWP
{
    public class ShareDialogService : IShareDialogService
    {
        StorageFolder m_tempFolder = ApplicationData.Current.TemporaryFolder;
        Backlog m_backlog;
        string m_appLink;

        public void ShareAppLink(string link)
        {
            m_appLink = link;
            DataTransferManager _dataTransferManager = DataTransferManager.GetForCurrentView();
            _dataTransferManager.DataRequested += AppLinkDataTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        private void AppLinkDataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest _request = args.Request;
            _request.Data.SetText(m_appLink);
            _request.Data.Properties.Title = m_appLink;
            _request.Data.Properties.Description = "Share this app with your contacts";
        }

        public async Task ShowShareBacklogDialogAsync(Backlog backlog)
        {
            m_backlog = backlog;
            StorageFile backlogFile = await m_tempFolder.CreateFileAsync($"{m_backlog.Name}.bklg", CreationCollisionOption.ReplaceExisting);
            string json = JsonConvert.SerializeObject(m_backlog);
            await WinFileIO.WriteTextAsync(backlogFile, json);
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        private async void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest dataRequest = args.Request;
            dataRequest.Data.Properties.Title = $"Share {m_backlog.Name} backlog";
            dataRequest.Data.Properties.Description = "Your contacts with the Backlogs app installed can open this file and add it to their backlog";
            var fileToShare = await m_tempFolder.GetFileAsync($"{m_backlog.Name}.bklg");
            List<IStorageItem> list = new List<IStorageItem>();
            list.Add(fileToShare);
            dataRequest.Data.SetStorageItems(list);
        }
    }
}
