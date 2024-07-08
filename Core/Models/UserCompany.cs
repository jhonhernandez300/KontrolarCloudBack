using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    [Table("MT_UserCompanies")]
    public class UserCompany
    {
        [Key]
        public int IdUserCompany { get; set; }
        public required int IdUser { get; set; }
        public required int IdCompany { get; set; }

        public User User { get; set; }
        public Company Company { get; set; }
    }
}
