using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    [Table("MT_Options")]
    public class Option
    {
        [Key]
        public int IdOption { get; set; }
        public required string NameOption { get; set; }
        public required string Description { get; set; }
        public required string Icon { get; set; }
        public required string Controler { get; set; }
        public required string Action { get; set; }
        public required int IdModule { get; set; }
        public required int OrderBy { get; set; }

        public ICollection<OptionProfile> OptionsProfiles { get; set; } = new List<OptionProfile>();
        public Module? Module { get; set; }
    }
}
