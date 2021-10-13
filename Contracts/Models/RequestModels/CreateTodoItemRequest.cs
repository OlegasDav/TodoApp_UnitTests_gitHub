using System.ComponentModel.DataAnnotations;
using Contracts.Enums;

namespace Contracts.Models.RequestModels
{
    public class CreateTodoItemRequest
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public Difficulty Difficulty { get; set; }
    }
}