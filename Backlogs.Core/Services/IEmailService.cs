using System.Threading.Tasks;

namespace Backlogs.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string subject, string body);
    }
}
