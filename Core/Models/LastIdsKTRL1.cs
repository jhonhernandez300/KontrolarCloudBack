using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    [Table("MT_LastIdsKTRL1")]
    public class LastIdsKTRL1
    {
        [Key]
        public int IdLastIdsKTRL1 { get; set; }
        public required string TableName { get; set; }
        public required long Last { get; set; }
    }
}
