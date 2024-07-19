using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models
{
    [Table("MT_Modules")]
    public class Module
    {
        [Key]
        public int IdModule { get; set; }
        public required string NameModule { get; set; }

        public ICollection<Option> Options { get; set; } = new List<Option>();
    }
}
