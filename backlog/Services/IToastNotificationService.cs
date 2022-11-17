using Backlogs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backlogs.Services
{
    public interface IToastNotificationService
    {
        void CreateToastNotification(Backlog backlog);
    }
}
