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
        ObservableCollection<Backlog> backlogs;
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            SaveData.GetInstance().ReadDataAsync();
            var _backlogs = SaveData.GetInstance().GetBacklogs();
            backlogs = new ObservableCollection<Backlog>(_backlogs.Where(b => b.NotifSent = false));
            foreach(Backlog b in backlogs)
            {
                Debug.WriteLine(b.Name);
                var builder = new ToastContentBuilder().AddText($"Remember to check out {b.Name}", hintMaxLines: 1)
                    .AddText($"You wanted to check {b.Name} by {b.Director} out today. Here's your reminder!", hintMaxLines: 2)
                    .AddHeroImage(new Uri(b.ImageURL));
                ScheduledToastNotification toastNotification = new ScheduledToastNotification(builder.GetXml(), DateTimeOffset.Now.AddSeconds(10));
                ToastNotificationManager.CreateToastNotifier().AddToSchedule(toastNotification);
                b.NotifSent = true;
            }
        }
    }
}
