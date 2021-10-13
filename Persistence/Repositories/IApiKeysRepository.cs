using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Persistence.Models.ReadModels;

namespace Persistence.Repositories
{
    public interface IApiKeysRepository
    {
        Task<IEnumerable<ApiKeyReadModel>> GetByUserIdAsync(Guid userId);
        
        Task<ApiKeyReadModel> GetByApiKeyIdAsync(Guid apiKey);
        
        Task<ApiKeyReadModel> GetByApiKeyAsync(string apiKey);

        Task<int> SaveAsync(ApiKeyReadModel model);

        Task<int> UpdateIsActive(Guid id, bool isActive);
    }
}