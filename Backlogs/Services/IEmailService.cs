using System.Threading.Tasks;

namespace Backlogs.Services
{
    /// <summary>
    /// A service to help the user send an email to the developer
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Opens the default email client to send an email to the developer
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        Task SendEmailAsync(string subject, string body);
    }
}
