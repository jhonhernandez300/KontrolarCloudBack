using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO
{
    public class OptionDTO
    {
        [Key]
        public int IdOption { get; set; }
        public string? Icon { get; set; }
        public string? NameOption { get; set; }
        public string? Description { get; set; }
        public string? Controler { get; set; }
        public string? Action { get; set; }
        public int OrderBy { get; set; }
        public string? UserAssigned { get; set; }
    }
}
