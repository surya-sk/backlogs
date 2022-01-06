using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Search;

namespace backlog.Logging
{
    public class Logger
    {
        public static Logger instance = null;

        static Logger()
        {
        }

        public static Logger GetInstance()
        {
            if (instance == null)
            {
                instance = new Logger();
            }
            return instance;
        }

        /// <summary>
        /// Returns the "Logs" folder and creates it, if it does not exist.
        /// </summary>
        /// <returns>Returns the "Logs" folder.</returns>
        public static IAsyncOperation<StorageFolder> GetLogFolderAsync()
        {
            return ApplicationData.Current.LocalFolder.CreateFolderAsync("Logs", CreationCollisionOption.OpenIfExists);
        }

        private static async Task WriteLog(string message, Exception ex = null)
        {
            var _logsFolder = await GetLogFolderAsync();
            try
            {
                var logFile = await _logsFolder.GetFileAsync("backlogs.log");
                if(ex !=null)
                    await FileIO.WriteTextAsync(logFile, $"[{DateTime.Now}] - {message}\nException: {ex.Message}");
                else
                    await FileIO.WriteTextAsync(logFile, $"[{DateTime.Now}] - {message}");
            }
            catch
            {
                await _logsFolder.CreateFileAsync("backlogs.log", CreationCollisionOption.ReplaceExisting);
                await WriteLog(message, ex);
            }
        }

        public static async Task Trace(string message)
        {
              await WriteLog(message);
        }

        /// <summary>
        /// Adds a Debug message to the log
        /// </summary>
        public static async Task Debug(string message)
        {
            await WriteLog(message);
        }

        /// <summary>
        /// Adds a Info message to the log
        /// </summary>
        public static async Task Info(string message)
        {
            await WriteLog(message);
        }

        /// <summary>
        /// Adds a Warn message to the log
        /// </summary>
        public static async Task Warn(string message)
        {
            await WriteLog(message);
        }

        /// <summary>
        /// Adds a Error message to the log
        /// </summary>
        public static async Task Error(string message, Exception e)
        {
            await WriteLog(message, e);
        }

        /// <summary>
        /// Opens the log folder.
        /// </summary>
        /// <returns>An async Task.</returns>
        public static IAsyncOperation<bool> OpenLogFolderAsync()
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            return Windows.System.Launcher.LaunchFolderAsync(folder);
        }

        /// <summary>
        /// Returns the logs
        /// </summary>
        /// <returns>An async task</returns>
        public static async Task<string> GetLogsAsync()
        {
            var folder = await GetLogFolderAsync();
            var path = Path.Combine(folder.Path, "backlogs.log");
            var logFile = await StorageFile.GetFileFromPathAsync(path);
            return await FileIO.ReadTextAsync(logFile);
        }
    }
}
