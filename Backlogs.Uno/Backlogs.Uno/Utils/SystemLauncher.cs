using Backlogs.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backlogs.Utils.Uno
{
    public class SystemLauncher : ISystemLauncher
    {
        public async Task LaunchUriAsync(Uri uri)
        {
            await Windows.System.Launcher.LaunchUriAsync(uri);
        }
    }
}
