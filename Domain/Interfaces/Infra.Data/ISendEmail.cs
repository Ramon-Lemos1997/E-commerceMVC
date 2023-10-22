using Domain.Models;

namespace Domain.Interfaces.Infra.Data
{
    public interface ISendEmail
    {
        Task<bool> SendEmailAsync(SendEmailModel model);
    }
}

