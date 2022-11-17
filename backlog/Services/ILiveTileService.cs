using Backlogs.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backlogs.Services
{
    public interface ILiveTileService
    {
        void EnableLiveTileQueue();
        void ShowLiveTiles(string tileSetting, string tileStyle, List<Backlog> backlogs);
    }
}
