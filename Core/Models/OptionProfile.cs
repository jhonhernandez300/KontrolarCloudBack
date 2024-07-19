using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    [Table("MT_OptionsProfile")]
    public class OptionProfile
    {
        [Key]
        public int Id { get; set; }
        public required int IdProfile { get; set; }
        public required int IdOption { get; set; }

        public Option? Option { get; set; }
        public Profile? Profile { get; set; }
    }
}
