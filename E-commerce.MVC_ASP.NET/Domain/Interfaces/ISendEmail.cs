using E_commerce.MVC_ASP.NET.Models;

namespace E_commerce.MVC_ASP.NET.Domain.Interfaces
{
    public interface ISendEmail
    {
        Task<bool> SendEmailAsync(SendEmailModel model);
    }
}

