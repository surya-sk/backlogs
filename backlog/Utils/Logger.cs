using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace backlog.Utils
{
    public static class Logger
    {
        static StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        static string fileName = "logs.txt";

        public static async Task WriteLogAsync(string log)
        {
            string logMessage = $"[{DateTime.Now}] - {log}\n\n";
            var file = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
            await FileIO.AppendTextAsync(file, logMessage);
        }

        public static async Task<String> ReadLogAsync()
        {
            var file = await localFolder.GetFileAsync(fileName);
            string logs = await FileIO.ReadTextAsync(file);
            return logs;
        }
    }
}
