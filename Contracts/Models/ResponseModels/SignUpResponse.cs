using System;

namespace Contracts.Models.ResponseModels
{
    public class SignUpResponse
    {
        public Guid Id { get; set; }
        
        public string Username { get; set; }
    }
}