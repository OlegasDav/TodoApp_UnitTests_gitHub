using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Models.RequestModels;
using Contracts.Models.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Persistence.Models.ReadModels;
using Persistence.Repositories;
using RestAPI.Attributes;

namespace RestAPI.Controllers
{
    [ApiKey]
    [ApiController]
    [Route("todos")]
    public class TodosController : ControllerBase
    {
        private readonly ITodosRepository _todosRepository;
        private readonly IUserRepository _userRepository;
        
        public TodosController(ITodosRepository todosRepository, IUserRepository userRepository)
        {
            _todosRepository = todosRepository;
            _userRepository = userRepository;
        }
        
        [HttpGet]
        public async Task<IEnumerable<TodosItemResponse>> GetAll()
        {
            var userId = (Guid) HttpContext.Items["userId"];
            
            var todos = await _todosRepository.GetAllAsync(userId);

            return todos.Select(todo => todo.MapToTodoItemResponse());
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<TodosItemResponse>> Get(Guid id)
        {
            var userId = (Guid) HttpContext.Items["userId"];
            
            var todoItem = await _todosRepository.GetAsync(id, userId);
            
            if (todoItem is null)
            {
                return NotFound($"Todo item with id: '{id}' does not exist");
            }

            return todoItem.MapToTodoItemResponse();
        }

        [HttpPost]
        public async Task<ActionResult<TodosItemResponse>> Create(CreateTodoItemRequest request)
        {
            var userId = (Guid) HttpContext.Items["userId"];

            var todoItemReadModel = new TodoItemReadModel
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = request.Title,
                Description = request.Description,
                Difficulty = request.Difficulty,
                IsDone = false,
                DateCreated = DateTime.Now
            };

            var rowsAffected = await _todosRepository.SaveOrUpdateAsync(todoItemReadModel);

            if (rowsAffected > 1)
            {
                throw new Exception("Something went wrong");
            }

            return CreatedAtAction(nameof(Get), new { todoItemReadModel.Id }, todoItemReadModel.MapToTodoItemResponse());
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<ActionResult<TodosItemResponse>> Update(Guid id, UpdateTodoItemRequest request)
        {
            var userId = (Guid) HttpContext.Items["userId"];

            var todoItem = await _todosRepository.GetAsync(id, userId);

            if (todoItem is null)
            {
                return NotFound($"Todo item with id: '{id}' does not exist");
            }
            
            todoItem.Title = request.Title;
            todoItem.Description = request.Description;

            await _todosRepository.SaveOrUpdateAsync(todoItem);

            return todoItem.MapToTodoItemResponse();
        }

        [HttpPatch]
        [Route("{id}/toggleStatus")]
        public async Task<ActionResult<TodosItemResponse>> UpdateStatus(Guid id)
        {
            var userId = (Guid) HttpContext.Items["userId"];

            var todoItem = await _todosRepository.GetAsync(id, userId);

            if (todoItem is null)
            {
                return NotFound($"Todo item with id: '{id}' does not exist");
            }

            todoItem.IsDone = !todoItem.IsDone;
            
            await _todosRepository.SaveOrUpdateAsync(todoItem);

            return todoItem.MapToTodoItemResponse();
        }
        

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = (Guid) HttpContext.Items["userId"];

            var todoItem = await _todosRepository.GetAsync(id, userId);

            if (todoItem is null)
            {
                return NotFound($"Todo item with id: '{id}' does not exist");
            }
            
            await _todosRepository.DeleteAsync(id);

            return NoContent();
        }
    }
}