using Domain.Models;

namespace Domain.Interfaces
{
    public interface IAccountInterface
    {
        Task<(bool success, string errorMessage)> Email(string model, string link);
    }
}
