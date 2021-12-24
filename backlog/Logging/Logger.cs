using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Search;

namespace backlog.Logging
{
    public static class Logger
    {
        private static readonly NLog.Logger NLOGGER;

        static Logger()
        {
            LogManager.Configuration = new XmlLoggingConfiguration(Path.Combine(Package.Current.InstalledLocation.Path, @"Logging\NLog.config"));
            LogManager.Configuration.Variables["LogPath"] = GetLogsFolderPath();
            NLOGGER = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Returns the "Logs" folder and creates it, if it does not exist.
        /// </summary>
        /// <returns>Returns the "Logs" folder.</returns>
        public static IAsyncOperation<StorageFolder> GetLogFolderAsync()
        {
            return ApplicationData.Current.LocalFolder.CreateFolderAsync("Logs", CreationCollisionOption.OpenIfExists);
        }

        private static string GetLogsFolderPath()
        {
            return Path.Combine(ApplicationData.Current.LocalFolder.Path, "Logs");
        }

        /// <summary>
        /// Calculates the size of the "Logs" folder.
        /// </summary>
        /// <returns>Returns the "Logs" folder size in KB.</returns>
        public static async Task<long> GetLogFolderSizeAsync()
        {
            StorageFolder logsFolder = await GetLogFolderAsync();
            StorageFileQueryResult result = logsFolder.CreateFileQuery();

            IEnumerable<Task<ulong>> fileSizeTasks = (await result.GetFilesAsync()).Select(async file => (await file.GetBasicPropertiesAsync()).Size);
            ulong[] sizes = await Task.WhenAll(fileSizeTasks);

            return sizes.Sum(l => (long)l) / 1024;
        }

        public static void Trace(string message)
        {
              NLOGGER.Trace(message);
        }

        /// <summary>
        /// Adds a Debug message to the log
        /// </summary>
        public static void Debug(string message)
        {
             NLOGGER.Debug(message);
        }

        /// <summary>
        /// Adds a Info message to the log
        /// </summary>
        public static void Info(string message)
        {
              NLOGGER.Info(message);
        }

        /// <summary>
        /// Adds a Warn message to the log
        /// </summary>
        public static void Warn(string message)
        {
               NLOGGER.Warn(message);
        }

        /// <summary>
        /// Adds a Error message to the log
        /// </summary>
        public static void Error(string message, Exception e)
        {
            if (e != null)
            {
                NLOGGER.Error(e, message);
            }
            else
            {
                NLOGGER.Error(message);
            }
        }

        /// <summary>
        /// Adds a Error message to the log
        /// </summary>
        public static void Error(string message)
        {
            Error(message, null);
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
            var path = Path.Combine(GetLogsFolderPath(), "backlogs.log");
            var logFile = await StorageFile.GetFileFromPathAsync(path);
            return await FileIO.ReadTextAsync(logFile);
        }
    }
}
