using Backlogs.Constants;
using Backlogs.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Backlogs.UtilsUWP
{
    public class Settings : IUserSettings
    {
        private static ApplicationDataContainer m_settings = ApplicationData.Current.LocalSettings;
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
        }
    }
}
