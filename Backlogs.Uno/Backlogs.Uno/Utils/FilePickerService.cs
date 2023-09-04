using Backlogs.Services;
using Backlogs.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Backlogs.Utils.Uno
{
    public class FilePickerService : IFilePicker
    {
        public async Task<string?> LaunchFilePickerAsync()
        {
            var _picker = new Windows.Storage.Pickers.FileOpenPicker();
            _picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            _picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            _picker.FileTypeFilter.Add(".bklg");

            StorageFile _file = await _picker.PickSingleFileAsync();
            if (_file != null)
            {
                StorageFolder _tempFolder = ApplicationData.Current.TemporaryFolder;
                await _tempFolder.CreateFileAsync(_file.Name, CreationCollisionOption.ReplaceExisting);
                string json = await Windows.Storage.FileIO.ReadTextAsync(_file);
                var _stFile = await _tempFolder.GetFileAsync(_file.Name);
                await Windows.Storage.FileIO.WriteTextAsync(_stFile, json);
                return _stFile.Name;
            }
            return null;
        }
    }
}
