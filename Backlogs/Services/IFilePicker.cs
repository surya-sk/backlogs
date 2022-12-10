using System.Threading.Tasks;

namespace Backlogs.Services
{
    /// <summary>
    /// Handles the system file picker functions
    /// </summary>
    public interface IFilePicker
    {
        /// <summary>
        /// Launches the system file picker
        /// </summary>
        /// <returns>the name of the file selected</returns>
        Task<string> LaunchFilePickerAsync();
    }
}
