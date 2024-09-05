using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class ErrorDetail
    {
        public required string Code { get; set; }
        public required string Message { get; set; }
        public required object Params { get; set; }
        public required string Detail { get; set; }
    }
}
