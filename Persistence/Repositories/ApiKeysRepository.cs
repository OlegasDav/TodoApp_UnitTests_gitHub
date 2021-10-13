using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Persistence.Models.ReadModels;

namespace Persistence.Repositories
{
    public class ApiKeysRepository : IApiKeysRepository
    {
        private const string TableName = "ApiKeys"; 
            
        private readonly ISqlClient _sqlClient;

        public ApiKeysRepository(ISqlClient sqlClient)
        {
            _sqlClient = sqlClient;
        }
        
        public Task<IEnumerable<ApiKeyReadModel>> GetByUserIdAsync(Guid userId)
        {
            var sql = $"SELECT * FROM {TableName} WHERE UserId = @UserId";

            return _sqlClient.QueryAsync<ApiKeyReadModel>(sql, new
            {
                UserId = userId
            });
        }

        public Task<ApiKeyReadModel> GetByApiKeyIdAsync(Guid id)
        {
            var sql = $"SELECT * FROM {TableName} WHERE Id = @Id";

            return _sqlClient.QuerySingleOrDefaultAsync<ApiKeyReadModel>(sql, new
            {
                Id = id
            });
        }

        public Task<ApiKeyReadModel> GetByApiKeyAsync(string apiKey)
        {
            var sql = $"SELECT * FROM {TableName} WHERE ApiKey = @ApiKey";

            return _sqlClient.QuerySingleOrDefaultAsync<ApiKeyReadModel>(sql, new
            {
                ApiKey = apiKey
            });
        }

        public Task<int> SaveAsync(ApiKeyReadModel model)
        {
            var sql = $"INSERT INTO {TableName} (Id, ApiKey, UserId, IsActive, DateCreated, ExpirationDate) VALUES (@Id, @ApiKey, @UserId, @IsActive, @DateCreated, @ExpirationDate)";

            return _sqlClient.ExecuteAsync(sql, model);
        }

        public Task<int> UpdateIsActive(Guid id, bool isActive)
        {
            var sql = $"UPDATE {TableName} SET IsActive = @IsActive WHERE Id = @Id";

            return _sqlClient.ExecuteAsync(sql, new
            {
                Id = id,
                IsActive = isActive
            });
        }
    }
}