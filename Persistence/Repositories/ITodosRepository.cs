using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Persistence.Models.ReadModels;

namespace Persistence.Repositories
{
    public interface ITodosRepository
    {
        Task<IEnumerable<TodoItemReadModel>> GetAllAsync(Guid userId);

        Task<TodoItemReadModel> GetAsync(Guid id, Guid userId);
        
        Task<int> SaveOrUpdateAsync(TodoItemReadModel model);

        Task<int> DeleteAsync(Guid id);
    }
}