namespace Contracts.Interfaces
{
    public interface IAccountInterface
    {
        Task<(bool success, string errorMessage)> Email(string model);
    }
}
