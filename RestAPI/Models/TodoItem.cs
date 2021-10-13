using System;
using Contracts.Enums;

namespace RestAPI.Models
{
    public class TodoItem
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public Difficulty Difficulty { get; set; }

        public bool IsDone { get; set; }

        public DateTime DateCreated { get; set; }
    }
}