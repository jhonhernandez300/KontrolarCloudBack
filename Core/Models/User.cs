using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    [Table("MT_Users")]
    public class User
    {
        [Key]
        public int IdUser { get; set; }
        public required string IdentificationNumber { get; set; }
        public required string Names { get; set; }
        public required string Surnames { get; set; }
        public required bool UserMaster { get; set; }
        public required bool IsDisabled { get; set; }

        public ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();
        public ICollection<UserProfile> UsersProfiles { get; set; } = new List<UserProfile>();
    }
}
