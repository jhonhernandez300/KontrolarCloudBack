using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    [Table("MT_Profiles")]
    public class Profile
    {
        [Key]
        public int IdProfile { get; set; }
        public required string CodProfile { get; set; }
        public required string NameProfile { get; set; }
        public required string Description { get; set; }
        public bool IsDisabled { get; set; }

        public ICollection<UserProfile> UsersProfiles { get; set; } = new List<UserProfile>();
        public ICollection<OptionProfile> OptionsProfiles { get; set; } = new List<OptionProfile>();
    }
}
