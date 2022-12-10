using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Backlogs.Services
{
    /// <summary>
    /// Handles launching stuff using the default system application
    /// </summary>
    public interface ISystemLauncher
    {
        /// <summary>
        /// Opens the url in the default browser
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        Task LaunchUriAsync(Uri uri);
    }
}
