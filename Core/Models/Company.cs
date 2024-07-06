using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Company
    {
        public int IdCompany { get; set; }
        public required string CompanyName { get; set; }
        public required string DB { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public required DateTime LicenseValidDate { get; set; }
        public required int NumberSimiltaneousConnection { get; set; }
    }
}
