using System.Threading.Tasks;

namespace HaT7FptBook.Utility
{
    public interface ISendMailService
    {
        Task SendMail(MailContent mailContent);
    
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}