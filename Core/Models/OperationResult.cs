using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class OperationResult<T>
    {
        public T Data { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}
