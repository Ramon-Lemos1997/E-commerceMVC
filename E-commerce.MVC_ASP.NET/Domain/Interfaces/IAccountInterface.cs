using E_commerce.MVC_ASP.NET.Models;

namespace E_commerce.MVC_ASP.NET.Domain.Interfaces
{
    public interface IAccountInterface
    {
        Task<(bool success, string errorMessage)> Email(string model, string link);
    }
}
