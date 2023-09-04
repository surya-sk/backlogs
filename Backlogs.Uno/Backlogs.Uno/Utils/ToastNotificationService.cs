using Backlogs.Models;
using Backlogs.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace Backlogs.Utils.Uno
{
    public class ToastNotificationService : IToastNotificationService
    {
        public void CreateToastNotification(Backlog backlog)
        {
            //var notifTime = DateTimeOffset.Parse(backlog.TargetDate, CultureInfo.InvariantCulture).Add(backlog.NotifTime);
            //var builder = new ToastContentBuilder()
            //    .AddText($"It's {backlog.Name} time!")
            //    .AddText($"You wanted to check out {backlog.Name} by {backlog.Director} today. Get to it!")
            //    .AddHeroImage(new Uri(backlog.ImageURL));
            //ScheduledToastNotification toastNotification = new ScheduledToastNotification(builder.GetXml(), notifTime);
            //ToastNotificationManager.CreateToastNotifier().AddToSchedule(toastNotification);
        }
    }
}
