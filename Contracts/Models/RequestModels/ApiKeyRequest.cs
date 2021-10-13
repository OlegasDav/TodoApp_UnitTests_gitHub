using System.ComponentModel.DataAnnotations;

namespace Contracts.Models.RequestModels
{
    public class ApiKeyRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}