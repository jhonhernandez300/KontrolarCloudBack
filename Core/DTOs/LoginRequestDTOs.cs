namespace Core.DTOs
{
    public class LoginRequestDTOs
    {
        public string IdentificationNumber { get; set; }
        public string Company { get; set; }
        public string AccessKey { get; set; }
        public bool IsKontrolarCloud { get; set; }
    }
}
