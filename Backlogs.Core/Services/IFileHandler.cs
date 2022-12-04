using Backlogs.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace Backlogs.Services
{
    public interface IFileHandler
    {
        Task<string> ReadBacklogsAsync(bool sync);
        Task WriteBacklogsAsync(ObservableCollection<Backlog> backlogs, bool sync);
        Task<string> DownloadBacklogsJsonAsync();
        Task DeleteLocalFilesAsync();
        Task WriteBitmapAsync(Stream stream, string fileName);
        Task WriteTextAsync(string text, string fileName);
        Task<string> ReadTextAsync(string fileName);
        Task<string> ReadImageAsync(string fileName);
        Task<string> ReadBacklogJsonAsync(string fileName);
    }
}
