using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backlogs.Constants
{
    public static class SettingsConstants
    {
        public const string IsFirstRun = nameof(IsFirstRun);
        public const string ShowWhatsNew = nameof(ShowWhatsNew);
        public const string Version = nameof(Version);
        public const string IsSignedIn = nameof(IsSignedIn);
        public const string UserName = nameof(UserName);
        public const string ShowLiveTile = nameof(ShowLiveTile);
        public const string SortOrder = nameof(SortOrder);
        public const string CompletedSortOrder = nameof(CompletedSortOrder);
        public const string TileStyle = nameof(TileStyle);
        public const string TileContent = nameof(TileContent);
        public const string AutoplayVideos = nameof(AutoplayVideos);
        public const string AppTheme = nameof(AppTheme);

        public static IReadOnlyDictionary<string, object> Defaults { get; } = new Dictionary<string, object>()
        {
            {IsFirstRun, true},
            {ShowWhatsNew, false },
            {Version, null },
            {IsSignedIn, false },
            {UserName, null },
            {ShowLiveTile, true },
            {SortOrder, "Name" },
            {CompletedSortOrder, "Name" },
            {TileStyle, "Peeking" },
            {TileContent, "Recently Created" },
            {AutoplayVideos, false },
            {AppTheme, "System" }
        };
    }
}
