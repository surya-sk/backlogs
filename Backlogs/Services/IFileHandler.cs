using Backlogs.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace Backlogs.Services
{
    /// <summary>
    /// Handles reading from and writing to files
    /// </summary>
    public interface IFileHandler
    {
        /// <summary>
        /// Reads the backlogs stored in a json file, from OneDrive if sync is true
        /// </summary>
        /// <param name="sync"></param>
        /// <returns>the json in string format</returns>
        Task<string> ReadBacklogsAsync(bool sync);

        /// <summary>
        /// Writes a list of backlogs to a file in json format, to OneDrive if sync is true
        /// </summary>
        /// <param name="backlogs"></param>
        /// <param name="sync"></param>
        /// <returns></returns>
        Task WriteBacklogsAsync(ObservableCollection<Backlog> backlogs, bool sync);

        /// <summary>
        /// Downloads the json from the user's OneDrive
        /// </summary>
        /// <returns>the json in string form</returns>
        Task<string> DownloadBacklogsJsonAsync();

        /// <summary>
        /// Deletes the local backlogs file
        /// </summary>
        /// <returns></returns>
        Task DeleteLocalFilesAsync();

        /// <summary>
        /// Writes a bitmap image from a stream to the file
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task WriteBitmapAsync(Stream stream, string fileName);

        /// <summary>
        /// Writes text to the fileName
        /// </summary>
        /// <param name="text"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task WriteTextAsync(string text, string fileName);

        /// <summary>
        /// Reads text from the provided file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>the text contained in the file</returns>
        Task<string> ReadTextAsync(string fileName);

        /// <summary>
        /// Reads an image from the file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>the path to the image</returns>
        Task<string> ReadImageAsync(string fileName);

        /// <summary>
        /// Reads the backlog json present locally
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>the json representing Backlog</returns>
        Task<string> ReadBacklogJsonAsync(string fileName);

        /// <summary>
        /// Writes a log message to the Logs file
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        Task WriteLogsAsync(string message, Exception ex = null);

        /// <summary>
        /// Reads logs written to the log folder
        /// </summary>
        /// <returns>a list of logs </returns>
        Task<List<string>> ReadLogsAync();
    }
}
