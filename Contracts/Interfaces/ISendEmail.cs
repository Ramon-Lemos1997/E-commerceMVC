using Contracts.Models;

namespace Contracts.Interfaces
{
    public interface ISendEmail
    {
        Task<bool> SendEmailAsync(SendEmailModel model);
    }
}

