using System;

namespace Persistence.Models.ReadModels
{
    public class ApiKeyReadModel
    {
        public Guid Id { get; set; }

        public string ApiKey { get; set; }

        public Guid UserId { get; set; }

        public bool IsActive { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime ExpirationDate { get; set; }
    }
}