namespace SharedLibrary.DTOs
{
    public class UserDto
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string? Password { get; set; } // optional, only for updates
    }
}
