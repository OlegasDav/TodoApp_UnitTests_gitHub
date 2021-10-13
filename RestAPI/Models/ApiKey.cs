using System;

namespace RestAPI.Models
{
    public class ApiKey
    {
        public Guid Id { get; set; }

        public string Key { get; set; }

        public Guid UserId { get; set; }

        public bool IsActive { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime ExpirationDate { get; set; }
    }
}