using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO
{
    public class Company_UserCompanyDTO
    {
        [Key]
        public int IdCompany_UserCompany { get; set; }
        public int IdCompany { get; set; }
        public required string CompanyName { get; set; }
        public required string DB { get; set; }
        public required DateTime LicenseValidDate { get; set; }
        public required int NumberSimiltaneousConnection { get; set; }

        public long Id { get; set; }
        public required string Correo { get; set; }
        public required int IdUser { get; set; }
        public required bool IsEnabled { get; set; }
        public required string Password { get; set; }
    }
}
