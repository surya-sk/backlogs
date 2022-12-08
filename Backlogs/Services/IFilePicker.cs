using System.Threading.Tasks;

namespace Backlogs.Services
{
    public interface IFilePicker
    {
        Task<string> LaunchFilePickerAsync();
    }
}
