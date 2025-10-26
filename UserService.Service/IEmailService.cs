using System.Threading.Tasks;

namespace UserService.Service
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string htmlBody);
    }
}