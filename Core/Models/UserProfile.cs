using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    [Table("MT_UsersProfile")]
    public class UserProfile
    {
        [Key]
        public int Id { get; set; }
        public required int IdUser { get; set; }
        public required int IdProfile { get; set; }

        public User? User { get; set; }
        public Profile? Profile { get; set; }
    }
}
