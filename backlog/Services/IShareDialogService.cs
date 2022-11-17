using Backlogs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backlogs.Services
{
    public interface IShareDialogService
    {
        Task ShowSearchDialog(Backlog backlog);
        void ShareAppLink(string link);
    }
}
