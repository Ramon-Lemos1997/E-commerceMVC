using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Models
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
