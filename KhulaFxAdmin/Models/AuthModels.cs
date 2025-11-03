namespace KhulaFxAdmin.Models
{
    public class GoogleLoginRequest
    {
        public string Credential { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Message { get; set; }
    }

    public class AdminUser
    {
        public int Id { get; set; }
        public string GoogleEmail { get; set; } = string.Empty;
        public string GoogleId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}