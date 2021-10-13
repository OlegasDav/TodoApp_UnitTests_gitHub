using Contracts.Models.ResponseModels;
using Persistence.Models.ReadModels;

namespace RestAPI
{
    public static class TodosExtensions
    {
        public static TodosItemResponse MapToTodoItemResponse(this TodoItemReadModel model)
        {
            return new TodosItemResponse
            {
                Id = model.Id,
                UserId = model.UserId,
                Title = model.Title,
                Description = model.Description,
                Difficulty = model.Difficulty,
                IsDone = model.IsDone,
                DateCreated = model.DateCreated
            };
        }
    }
}