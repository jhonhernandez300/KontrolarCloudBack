using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    [Table("MT_Companies")]
    public class Company
    {
        [Key]
        public int IdCompany { get; set; }
        public required string CompanyName { get; set; }
        public required string DB { get; set; }                
        public required DateTime LicenseValidDate { get; set; }
        public required int NumberSimiltaneousConnection { get; set; }

        public ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();
    }
}
