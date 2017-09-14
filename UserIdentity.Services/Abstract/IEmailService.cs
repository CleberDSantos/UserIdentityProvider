using UserIdentity.Services.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UserIdentity.Services.Abstract
{
    public interface IEmailService
    {
        Task SendMailAsync(IEnumerable<EmailRecipient> recipients, string subject, string body);
    }
}