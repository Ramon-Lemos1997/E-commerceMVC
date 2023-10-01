using Domain.Models;

namespace Domain.Interfaces
{
    public interface ISendEmail
    {
        Task<bool> SendEmailAsync(SendEmailModel model);
    }
}

