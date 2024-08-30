using System.ComponentModel.DataAnnotations;

namespace Core.DTOs
{
    public class UserDTO
    {        
        public int IdUser { get; set; }
        public required string IdentificationNumber { get; set; }
        public required string Names { get; set; }
        public required string Surnames { get; set; }
        public required bool UserMaster { get; set; }
    }
}
