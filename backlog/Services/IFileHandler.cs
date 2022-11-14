using Backlogs.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Backlogs.Services
{
    public interface IFileHandler
    {
        Task<string> ReadBacklogsAsync(Stream stream, bool sync);
        Task WriteBacklogsAsync(ObservableCollection<Backlog> backlogs, bool sync);
        Task<string> DownloadBacklogsJsonAsync();
        Task DeleteLocalFilesAsync();
        Task WriteBitmapAsync(Stream stream, string fileName);
        Task WriteTextAsync(string text, string fileName);
        Task<string> ReadTextAsync(string fileName);
    }
}
