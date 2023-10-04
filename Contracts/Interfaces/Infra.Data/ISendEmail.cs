using Contracts.Models;

namespace Contracts.Interfaces.Infra.Data
{
    public interface ISendEmail
    {
        Task<bool> SendEmailAsync(SendEmailModel model);
    }
}

