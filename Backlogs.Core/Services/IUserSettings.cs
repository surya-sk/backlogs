using System;

namespace Backlogs.Services
{
    public interface IUserSettings
    {
        event EventHandler<string> UserSettingsChanged;
        void Set<T>(string key, T value);
        T Get <T>(string key);
    }
}
