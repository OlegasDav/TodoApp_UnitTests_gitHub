using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Models.RequestModels;
using Contracts.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Persistence.Models.ReadModels;
using Persistence.Repositories;
using RestAPI.Options;
using RestAPI.Services;

namespace RestAPI.Controllers
{
    [ApiController]
    [Route("apiKeys")]
    public class ApiKeysController : ControllerBase
    {
        private readonly IApikeyService _apikeyService;

        public ApiKeysController(IApikeyService apikeyService)
        {
            _apikeyService = apikeyService;
        }
        
        [HttpPost]
        public async Task<ActionResult<ApiKeyResponse>> Create(ApiKeyRequest request)
        {
            try
            {
                var apiKey = await _apikeyService.CreateApiKey(request.Username, request.Password);
                
                return new ApiKeyResponse
                {
                    Id = apiKey.Id,
                    ApiKey = apiKey.Key,
                    UserId = apiKey.UserId,
                    IsActive = apiKey.IsActive,
                    DateCreated = apiKey.DateCreated,
                    ExpirationDate = apiKey.ExpirationDate
                };
            }
            catch (BadHttpRequestException exception)
            {
                switch (exception.StatusCode)
                {
                    case 404:
                        return NotFound(exception.Message);
                    case 400:
                        return BadRequest(exception.Message);
                    default: throw;
                }
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<ApiKeyResponse>>> GetAllKeys(string username, string password)
        {
            try
            {
                var apiKeys = await _apikeyService.GetAllApiKeys(username, password);
            
                return Ok(apiKeys.Select(apiKey => new ApiKeyResponse
                {
                    Id = apiKey.Id,
                    ApiKey = apiKey.Key,
                    UserId = apiKey.UserId,
                    IsActive = apiKey.IsActive,
                    DateCreated = apiKey.DateCreated,
                    ExpirationDate = apiKey.ExpirationDate
                }).ToList());
            }
            catch (BadHttpRequestException exception)
            {
                switch (exception.StatusCode)
                {
                    case 404:
                        return NotFound(exception.Message);
                    case 400:
                        return BadRequest(exception.Message);
                    default: throw;
                }
            }
        }
        
        [HttpPut]
        [Route("{id}/isActive")]
        public async Task<ActionResult<ApiKeyResponse>> UpdateKeyState(Guid id, UpdateKeyStateRequest request)
        {
            try
            {
                var apiKey = await _apikeyService.UpdateApiKeyState(id, request.IsActive);
        
                return new ApiKeyResponse
                {
                    Id = apiKey.Id,
                    ApiKey = apiKey.Key,
                    UserId = apiKey.UserId,
                    IsActive = request.IsActive,
                    DateCreated = apiKey.DateCreated,
                    ExpirationDate = apiKey.ExpirationDate
                };
            }
            catch (BadHttpRequestException exception)
            {
                switch (exception.StatusCode)
                {
                    case 404:
                        return NotFound(exception.Message);
                    default: throw;
                }
            }
        }
    }
}