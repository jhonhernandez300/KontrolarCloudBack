using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    [Table("MT_UsersCompanies")]
    public class UserCompany
    {
        [Key]
        public long Id { get; set; }
        public required int IdUser { get; set; }
        public required int IdCompany { get; set; }
        public required string Password { get; set; }
        public required string Correo { get; set; }
        public required int IsEnabled { get; set; }

        public User? User { get; set; }
        public Company? Company { get; set; }
    }
}
