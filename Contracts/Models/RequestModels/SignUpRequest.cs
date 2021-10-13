using System.ComponentModel.DataAnnotations;

namespace Contracts.Models.RequestModels
{
    public class SignUpRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }
    }
}