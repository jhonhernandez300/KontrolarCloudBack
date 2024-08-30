namespace Core.DTOs
{
    public class OptionProfileDTO
    {
        public int IdModule { get; set; }
        public int IdOption { get; set; }
        public string IconOption { get; set; } = string.Empty;
        public string NameOption { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public int OrderBy { get; set; }
        public bool UserAssigned { get; set; }
    }
}
