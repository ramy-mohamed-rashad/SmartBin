using System.ComponentModel.DataAnnotations;

namespace SmartBin.ModelViews
{
    public class RegisterMV
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        [Required]
        public string FullName { get; set; }
    }

    public class LoginMV
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class AuthResponseMV
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string UserId { get; set; }
        public string Role { get; set; }
    }
}