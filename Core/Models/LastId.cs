using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    [Table("MT_LastIds")]
    public class LastId
    {
        [Key]
        public int IdLastIds { get; set; }
        public required string TableName { get; set; }
        public required int Last { get; set; }
    }
}
