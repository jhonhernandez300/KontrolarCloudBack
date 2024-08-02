using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO
{
    public class ModuleDTO
    {
        [Key]
        public int IdModule { get; set; }
        public string? NameModule { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public List<OptionDTO> Options { get; set; } = new List<OptionDTO>();
    }
}
