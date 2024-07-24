using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO
{
    public class ModuleOptionDTO
    {
        [Key]
        public int IdModuleOptionDTO { get; set; }

        public int IdModule { get; set; }
        public required string NameModule { get; set; }
        public required string IconModule { get; set; }
        public required string colorModule { get; set; }

        public int IdOption { get; set; }
        public required string IconOption { get; set; }
        public required string NameOption { get; set; }
        public required string Description { get; set; }

        public required string Controler { get; set; }
        public required string Action { get; set; }
        public required int OrderBy { get; set; }
        public required string UserAssigned { get; set; }
    }

}
