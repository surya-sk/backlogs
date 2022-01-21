using Windows.Storage;

namespace backlog.Utils
{
    public static class Settings
    {
        private static ApplicationDataContainer _settings = ApplicationData.Current.LocalSettings;

        public static bool IsFirstRun
        {
            get
            {
                if (_settings.Values.TryGetValue(nameof(IsFirstRun), out var isFirstRun))
                {
                    return (bool)isFirstRun;
                }
                return true;
            }
            set => _settings.Values[nameof(IsFirstRun)] = value;
        }

        public static bool IsSignedIn
        {
            get
            {
                if (_settings.Values.TryGetValue(nameof(IsSignedIn), out var isSignedIn))
                {
                    return (bool)isSignedIn;
                }
                return false;
            }
            set => _settings.Values[nameof(IsSignedIn)] = value;
        }

        public static string UserName
        {
            get
            {
                if(_settings.Values.TryGetValue(nameof(UserName), out var userName))
                {
                    return (string)userName;
                }
                return null;
            }
            set => _settings.Values[nameof(UserName)] = value;
        }

        public static string GetNotifTime(string id)
        {
            if(_settings.Values.TryGetValue("NotifTime", out var notifTime))
            {
                var notifTimes = (ApplicationDataCompositeValue)notifTime;
                return notifTimes[id].ToString();
            }
            return "";
        }

        public static void SetNotifTime(string id, string notifTime)
        {
            var notifTimes = (ApplicationDataCompositeValue)_settings.Values["NotifTime"];
            if(notifTimes == null)
            {
                notifTimes = new ApplicationDataCompositeValue();
            }
            notifTimes[id] = notifTime;
        }
    }
}
