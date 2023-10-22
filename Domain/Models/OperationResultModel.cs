
namespace Domain.Models
{
    public class OperationResultModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public OperationResultModel(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }

}
