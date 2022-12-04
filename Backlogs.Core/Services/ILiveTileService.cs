using Backlogs.Models;
using System.Collections.Generic;

namespace Backlogs.Services
{
    public interface ILiveTileService
    {
        void EnableLiveTileQueue();
        void ShowLiveTiles(string tileSetting, string tileStyle, List<Backlog> backlogs);
    }
}
