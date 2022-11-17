using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backlogs.Services
{
    public interface IUserSettings
    {
        event EventHandler<string> UserSettingsChanged;
        void Set<T>(string key, T value);
        T Get <T>(string key);
    }
}
