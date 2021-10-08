using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using backlog.Saving;
using backlog.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace backlog.Utils
{
    internal class ToastBackgroundTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            ObservableCollection<Backlog> backlogs = SaveData.GetInstance().GetBacklogs();
            foreach(Backlog b in backlogs)
            {
                if(b.RemindEveryday)
                {
                    var builder = new ToastContentBuilder()
                                    .AddText($"Have you checked out {b.Name} today?", hintMaxLines: 1)
                                    .AddText($"You wanted to check {b.Name} by {b.Director} out today. Here's your reminder!", hintMaxLines: 2)
                                    .AddHeroImage(new Uri(b.ImageURL));
                    builder.Show();
                }
            }
        }
    }
}
