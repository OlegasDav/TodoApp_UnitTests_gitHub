using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestAPI.Models;

namespace RestAPI.Services
{
    public interface IApikeyService
    {
        Task<ApiKey> CreateApiKey(string username, string password);

        Task<IEnumerable<ApiKey>> GetAllApiKeys(string username, string password);

        Task<ApiKey> UpdateApiKeyState(Guid id, bool newState);
    }
}