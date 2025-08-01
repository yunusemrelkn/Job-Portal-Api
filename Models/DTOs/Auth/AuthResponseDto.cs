using JobPortal.Api.Models.DTOs.User;

namespace JobPortal.Api.Models.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public UserDto User { get; set; }
    }
}
