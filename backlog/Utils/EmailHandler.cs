using Backlogs.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Email;

namespace Backlogs.Utils.UWP
{
    public class EmailHandler : IEmailService
    {
        public async Task SendEmailAsync(string subject, string body)
        {
            EmailMessage emailMessage = new EmailMessage();
            emailMessage.To.Add(new EmailRecipient("surya.sk05@outlook.com"));
            emailMessage.Subject = subject;
            emailMessage.Body = body;
            await EmailManager.ShowComposeNewEmailAsync(emailMessage);
        }
    }
}
