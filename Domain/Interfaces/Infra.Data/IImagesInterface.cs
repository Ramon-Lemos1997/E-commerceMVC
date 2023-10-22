using Domain.Models;
using Microsoft.AspNetCore.Http;

namespace Domain.Interfaces.Infra.Data
{
    public interface IImagesInterface
    {
        Task<(OperationResultModel, string)> UploadImageAsync(IFormFile image);
    }
}
