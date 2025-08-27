using SupportLink.Controllers;
using SupportLink.Data;
using System.Threading.Tasks;

namespace SupportLink.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlBody, string? attachmentPath = null);
    }
}


