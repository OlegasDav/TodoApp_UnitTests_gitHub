using System.Threading.Tasks;
using Persistence.Models.ReadModels;

namespace Persistence.Repositories
{
    public interface IUserRepository
    {
        /// <summary>
        /// Get user by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>Returns UserReadModel</returns>
        Task<UserReadModel> GetAsync(string username); 
            
        /// <summary>
        /// Get user by username and password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<UserReadModel> GetAsync(string username, string password);

        Task<int> SaveAsync(UserReadModel model);
    }
}