using System.Threading.Tasks;

namespace Fan.Emails
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
