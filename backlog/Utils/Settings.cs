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

        public static bool ShowWhatsNew
        {
            get
            {
                if(_settings.Values.TryGetValue(nameof(ShowWhatsNew), out var showWhatsNew))
                {
                    return (bool)showWhatsNew;
                }
                return false;
            }
            set => _settings.Values[nameof(ShowWhatsNew)] = value;
        }

        public static string Version
        {
            get
            {
                if(_settings.Values.TryGetValue(nameof(Version), out var version))
                {
                    return (string)version;
                }
                return null;
            }
            set => _settings.Values[nameof(Version)] = value;
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
                if (_settings.Values.TryGetValue(nameof(UserName), out var userName))
                {
                    return (string)userName;
                }
                return null;
            }
            set => _settings.Values[nameof(UserName)] = value;
        }

        public static bool ShowLiveTile
        {
            get
            {
                if (_settings.Values.TryGetValue(nameof(ShowLiveTile), out var showLiveTile))
                {
                    return (bool)showLiveTile;
                }
                return true;
            }
            set => _settings.Values[nameof(ShowLiveTile)] = value;
        }

        public static string SortOrder
        {
            get
            {
                if(_settings.Values.TryGetValue(nameof(SortOrder), out var sortOrder))
                {
                    return (string)sortOrder;
                }
                return "Name";
            }
            set => _settings.Values[nameof(SortOrder)] = value;
        }

        public static string CompletedSortOrder
        {
            get
            {
                if(_settings.Values.TryGetValue(nameof(CompletedSortOrder), out var completedSortOrder))
                {
                    return (string)completedSortOrder;
                }
                return "Name";
            }
            set => _settings.Values[nameof(CompletedSortOrder)] = value;
        }

        public static string TileStyle
        {
            get
            {
                if(_settings.Values.TryGetValue(nameof(TileStyle), out var tileStyle))
                {
                    return (string)tileStyle;
                }
                return "Peeking";
            }
            set => _settings.Values[nameof(TileStyle)] = value;
        }
        
        public static string TileContent
        {
            get
            {
                if(_settings.Values.TryGetValue(nameof(TileContent), out var tileContent))
                {
                    return (string)tileContent;
                }
                return "Recently Created";
            }
            set => _settings.Values[nameof(TileContent)] = value;
        }

        public static int AutoplayVideos
        {
            get
            {
                if(_settings.Values.TryGetValue(nameof(AutoplayVideos), out var autoplayVideos))
                {
                    return (int)autoplayVideos;
                }
                return 0;
            }
            set => _settings.Values[nameof(AutoplayVideos)] = value;
        }
    }
}
