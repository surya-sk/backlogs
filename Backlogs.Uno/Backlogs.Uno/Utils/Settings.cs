using Backlogs.Constants;
using Backlogs.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Backlogs.Utils.Uno
{
    public class Settings : IUserSettings
    {
        private static ApplicationDataContainer m_settings = ApplicationData.Current.LocalSettings;

        public event EventHandler<string>? UserSettingsChanged;

        public T Get<T>(string key)
        {
            if(m_settings.Values.TryGetValue(key, out var v))
            {
                return (T)v;
            }
            return (T)SettingsConstants.Defaults[key];
        }

        public void Set<T>(string key, T value)
        {
            m_settings.Values[key] = value;
            UserSettingsChanged?.Invoke(this, key);
        }
    }
}
