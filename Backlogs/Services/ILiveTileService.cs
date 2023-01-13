using Backlogs.Models;
using System.Collections.Generic;

namespace Backlogs.Services
{
    /// <summary>
    /// Creates and shows live tiles on Windows 10 devices
    /// </summary>
    public interface ILiveTileService
    {
        /// <summary>
        /// Enables the live tile queue to enable multiple tiles
        /// </summary>
        void EnableLiveTileQueue();

        /// <summary>
        /// Generates and shows backlogs provided in a live tile
        /// </summary>
        /// <param name="tileSetting"></param>
        /// <param name="tileStyle"></param>
        /// <param name="backlogs"></param>
        void ShowLiveTiles(string tileSetting, string tileStyle, List<Backlog> backlogs);
    }
}
