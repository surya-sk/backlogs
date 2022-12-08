using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Backlogs.Services
{
    public interface ISystemLauncher
    {
        Task LaunchUriAsync(Uri uri);
    }
}
