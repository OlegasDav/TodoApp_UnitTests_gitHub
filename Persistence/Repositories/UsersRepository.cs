using System.Threading.Tasks;
using Persistence.Models.ReadModels;

namespace Persistence.Repositories
{
    public class UsersRepository : IUserRepository
    {
        private const string TableName = "Users";
        private readonly ISqlClient _sqlClient;

        public UsersRepository(ISqlClient sqlClient)
        {
            _sqlClient = sqlClient;
        }
        
        public Task<UserReadModel> GetAsync(string username)
        {
            var sql = $"SELECT * FROM {TableName} WHERE Username = @Username";

            return _sqlClient.QuerySingleOrDefaultAsync<UserReadModel>(sql, new
            {
                Username = username
            });
        }

        public Task<UserReadModel> GetAsync(string username, string password)
        {
            var sql = $"SELECT * FROM {TableName} WHERE Username = @Username AND Password = @Password";

            return _sqlClient.QuerySingleOrDefaultAsync<UserReadModel>(sql, new
            {
                Username = username,
                Password = password
            });
        }

        public Task<int> SaveAsync(UserReadModel model)
        {
            var sql =
                $"INSERT INTO {TableName} (Id, Username, Password, DateCreated) VALUES (@Id, @Username, @Password, @DateCreated)";

            return _sqlClient.ExecuteAsync(sql, model);
        }
    }
}