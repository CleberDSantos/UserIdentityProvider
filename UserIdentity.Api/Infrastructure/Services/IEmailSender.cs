using System.Threading.Tasks;

namespace UserIdentity.WebUI.Infrastructure.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}