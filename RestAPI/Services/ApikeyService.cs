using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Persistence.Models.ReadModels;
using Persistence.Repositories;
using RestAPI.Models;
using RestAPI.Options;

namespace RestAPI.Services
{
    public class ApikeyService : IApikeyService
    {
        private readonly IUserRepository _userRepository;
        private readonly IApiKeysRepository _apiKeysRepository;
        private readonly IOptions<ApiKeySettings> _apiKeySettings;

        private ApiKeySettings ApiKeySettings => _apiKeySettings.Value;

        public ApikeyService(
            IUserRepository userRepository, 
            IApiKeysRepository apiKeysRepository,
            IOptions<ApiKeySettings> apiKeySettings)
        {
            _userRepository = userRepository;
            _apiKeysRepository = apiKeysRepository;
            _apiKeySettings = apiKeySettings;
        }
        
        public async Task<ApiKey> CreateApiKey(string username, string password)
        {
            var user = await _userRepository.GetAsync(username);

            if (user is null)
            {
                throw new BadHttpRequestException($"User with Username: '{username}' does not exist!", 404);
            }

            if (!user.Password.Equals(password))
            {
                throw new BadHttpRequestException($"Wrong password for user: '{user.Username}'", 400);
            }

            var allKeys = await _apiKeysRepository.GetByUserIdAsync(user.Id);

            if (ApiKeySettings.ApiKeyLimit < allKeys.Count() + 1)
            {
                throw new BadHttpRequestException($"Api key limit is reached", 400);
            }

            var apiKey = new ApiKeyReadModel
            {
                Id = Guid.NewGuid(),
                ApiKey = Guid.NewGuid().ToString("N"),
                UserId = user.Id,
                IsActive = true,
                DateCreated = DateTime.Now,
                ExpirationDate = DateTime.Now.AddMinutes(ApiKeySettings.ExpirationTimeInMinutes)
            };

            await _apiKeysRepository.SaveAsync(apiKey);

            return new ApiKey
            {
                Id = apiKey.Id,
                Key = apiKey.ApiKey,
                UserId = apiKey.UserId,
                IsActive = apiKey.IsActive,
                DateCreated = apiKey.DateCreated,
                ExpirationDate = apiKey.ExpirationDate
            };
        }

        public async Task<IEnumerable<ApiKey>> GetAllApiKeys(string username, string password)
        {
            var user = await _userRepository.GetAsync(username);
        
            if (user is null)
            {
                throw new BadHttpRequestException($"User with Username: '{username}' does not exists!", 404);
            }
        
            if (!user.Password.Equals(password))
            {
                throw new BadHttpRequestException($"Wrong password for user: '{user.Username}'", 400);
            }
        
            var apiKeys = await _apiKeysRepository.GetByUserIdAsync(user.Id);

            return apiKeys.Select(apiKey => new ApiKey
            {
                Id = apiKey.Id,
                Key = apiKey.ApiKey,
                UserId = apiKey.UserId,
                IsActive = apiKey.IsActive,
                DateCreated = apiKey.DateCreated,
                ExpirationDate = apiKey.ExpirationDate
            });
        }

        public async Task<ApiKey> UpdateApiKeyState(Guid id, bool newState)
        {
            var apiKey = await _apiKeysRepository.GetByApiKeyIdAsync(id);
        
            if (apiKey is null)
            {
                throw new BadHttpRequestException($"Api key with Id: '{id}' does not exists", 404);
            }
        
            await _apiKeysRepository.UpdateIsActive(id, newState);

            return new ApiKey
            {
                Id = apiKey.Id,
                Key = apiKey.ApiKey,
                UserId = apiKey.UserId,
                IsActive = newState,
                DateCreated = apiKey.DateCreated,
                ExpirationDate = apiKey.ExpirationDate
            };;
        }
    }
}