using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Contracts.Enums;
using Contracts.Models.RequestModels;
using Contracts.Models.ResponseModels;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Persistence.Models.ReadModels;
using Persistence.Repositories;
using RestAPI.Controllers;
using TestHelper.Attributes;
using Xunit;

namespace RestAPI.UnitTests.Controllers
{
    public class TodosController_Should
    {
        private readonly Mock<IUserRepository> _usersRepositoryMock = new Mock<IUserRepository>();
        private readonly Mock<ITodosRepository> _todosRepositoryMock = new Mock<ITodosRepository>();
        private readonly Mock<HttpContext> _httpContextMock = new Mock<HttpContext>();
        private readonly Random _random = new Random();

        private readonly TodosController _sut;

        public TodosController_Should()
        {
            _sut = new TodosController(_todosRepositoryMock.Object, _usersRepositoryMock.Object)
            {
                ControllerContext =
                {
                    HttpContext = _httpContextMock.Object
                }
            };
        }

        [Theory, AutoData]
        public async Task GetAllTodoItems_When_GetAllIsCalled(
            Guid userId,
            List<TodoItemReadModel> todoItemReadModels)
        {
            // Arrange
            _httpContextMock.SetupGet(httpContext => httpContext.Items["userId"]).Returns(userId);

            todoItemReadModels.ForEach(todoItem => todoItem.UserId = userId);

            _todosRepositoryMock
                .Setup(todosRepository => todosRepository.GetAllAsync(userId))
                .ReturnsAsync(todoItemReadModels);
            
            // Act
            var result = await _sut.GetAll();

            // Assert
            result.Should().BeEquivalentTo(todoItemReadModels);
            
            _todosRepositoryMock
                .Verify(todosRepository => todosRepository.GetAllAsync(It.IsAny<Guid>()), Times.Once);
        }

        [Theory, AutoData]
        public async Task Get_ReturnsNotFoundObjectResult_When_TodoItemIsNull(
            Guid id,
            Guid userId)
        {
            // Arrange
            _httpContextMock.SetupGet(httpContext => httpContext.Items["userId"]).Returns(userId);

            _todosRepositoryMock
                .Setup(todosRepository => todosRepository.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync((TodoItemReadModel) null);

            // Act
            var result = await _sut.Get(id);

            //Assert
            result.Should().BeOfType<ActionResult<TodosItemResponse>>()
                .Which.Result.Should().BeOfType<NotFoundObjectResult>()
                .Which.Value.Should().BeEquivalentTo($"Todo item with id: '{id}' does not exist");

            _todosRepositoryMock
                .Verify(todosRepository => todosRepository.GetAsync(id, userId), Times.Once);
        }

        [Theory, AutoData]
        public async Task Get_ReturnsTodoItem_When_IdParameterIsPassed(
            Guid id,
            Guid userId,
            TodoItemReadModel todoItemReadModel)
        {
            // Arrange
            _httpContextMock.SetupGet(httpContext => httpContext.Items["userId"]).Returns(userId);

            _todosRepositoryMock
                .Setup(todosRepository => todosRepository.GetAsync(id, userId))
                .ReturnsAsync(todoItemReadModel);

            // Act
            var result = await _sut.Get(id);

            //Assert
            result.Should().BeOfType<ActionResult<TodosItemResponse>>()
                .Which.Value.Should().BeOfType<TodosItemResponse>()
                .Which.Should().BeEquivalentTo(todoItemReadModel);

            _todosRepositoryMock
                .Verify(todosRepository => todosRepository.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
        }

        [Theory, AutoData]
        public async Task Create_ReturnsException_When_RowsAffectedMoreThanOne(
            Guid userId,
            int rowsAffected,
            CreateTodoItemRequest request)
        {
            // Arrange
            _httpContextMock.SetupGet(httpContext => httpContext.Items["userId"]).Returns(userId);

            rowsAffected = _random.Next(2, int.MaxValue);

            _todosRepositoryMock
                .Setup(todosRepository => todosRepository.SaveOrUpdateAsync(It.IsAny<TodoItemReadModel>()))
                .ReturnsAsync(rowsAffected);

            // Act & assert
            var result = await _sut.Invoking(sut => sut.Create(request))
                .Should().ThrowAsync<Exception>()
                .WithMessage("Something went wrong");

            _todosRepositoryMock
                .Verify(todosRepository => todosRepository
                .SaveOrUpdateAsync(It.Is<TodoItemReadModel>(model => model.UserId.Equals(userId)
                && model.Title.Equals(request.Title)
                && model.Description.Equals(request.Description)
                && model.Difficulty.Equals(request.Difficulty)
                && model.IsDone.Equals(false))), Times.Once);
        }

        [Theory, AutoData]
        public async Task Create_ReturnsTodosItemResponse_When_RowsAffectedParameterIsPassed(
            Guid userId,
            int rowsAffected,
            TodoItemReadModel todoItemReadModel,
            CreateTodoItemRequest request)
        {
            // Arrange
            _httpContextMock.SetupGet(httpContext => httpContext.Items["userId"]).Returns(userId);

            rowsAffected = 1;

            todoItemReadModel.UserId = userId;
            todoItemReadModel.Title = request.Title;
            todoItemReadModel.Description = request.Description;
            todoItemReadModel.Difficulty = request.Difficulty;
            todoItemReadModel.IsDone = false;

            _todosRepositoryMock
                .Setup(todosRepository => todosRepository.SaveOrUpdateAsync(It.IsAny<TodoItemReadModel>()))
                .ReturnsAsync(rowsAffected);

            // Act
            var result = await _sut.Create(request);

            // Assert
            result.Should().BeOfType<ActionResult<TodosItemResponse>>()
                .Which.Result.Should().BeOfType<CreatedAtActionResult>()
                .Which.Value.Should().BeOfType<TodosItemResponse>()
                .Which.Should().BeEquivalentTo(todoItemReadModel, option => option
                .Excluding(model => model.Id)
                .Excluding(model => model.DateCreated));

            _todosRepositoryMock
                .Verify(todosRepository => todosRepository
                .SaveOrUpdateAsync(It.Is<TodoItemReadModel>(model => model.UserId.Equals(todoItemReadModel.UserId)
                && model.Title.Equals(todoItemReadModel.Title)
                && model.Description.Equals(todoItemReadModel.Description)
                && model.Difficulty.Equals(todoItemReadModel.Difficulty)
                && model.IsDone.Equals(todoItemReadModel.IsDone))), Times.Once);
        }

        [Theory, AutoData]
        public async Task Update_ReturnsNotFoundObjectResult_When_TodoItemIsNull(
            Guid id,
            Guid userId,
            UpdateTodoItemRequest request)
        {
            // Arrange
            _httpContextMock.SetupGet(httpContext => httpContext.Items["userId"]).Returns(userId);

            _todosRepositoryMock
                .Setup(todosRepository => todosRepository.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync((TodoItemReadModel)null);

            // Act
            var result = await _sut.Update(id, request);

            //Assert
            result.Should().BeOfType<ActionResult<TodosItemResponse>>()
                .Which.Result.Should().BeOfType<NotFoundObjectResult>()
                .Which.Value.Should().BeEquivalentTo($"Todo item with id: '{id}' does not exist");

            _todosRepositoryMock
                .Verify(todosRepository => todosRepository.GetAsync(id, userId), Times.Once);
        }

        [Theory, AutoData]
        public async Task Update_ReturnsTodoItem_When_IdParameterIsPassed(
            Guid id,
            Guid userId,
            UpdateTodoItemRequest request,
            TodoItemReadModel todoItemReadModel)
        {
            // Arrange
            todoItemReadModel.Id = id;
            todoItemReadModel.UserId = userId;

            _httpContextMock.SetupGet(httpContext => httpContext.Items["userId"]).Returns(userId);

            _todosRepositoryMock
                .Setup(todosRepository => todosRepository.GetAsync(id, userId))
                .ReturnsAsync(todoItemReadModel);

            // Act
            var result = await _sut.Update(id, request);

            //Assert
            result.Should().BeOfType<ActionResult<TodosItemResponse>>()
                .Which.Value.Should().BeOfType<TodosItemResponse>()
                .Which.Should().BeEquivalentTo(todoItemReadModel);

            _todosRepositoryMock
                .Verify(todosRepository => todosRepository.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);

            _todosRepositoryMock
                .Verify(todosRepository => todosRepository.SaveOrUpdateAsync(todoItemReadModel), Times.Once);
        }

        [Theory, AutoData]
        public async Task UpdateStatus_ReturnsNotFoundObjectResult_When_TodoItemIsNull(
            Guid id,
            Guid userId)
        {
            // Arrange
            _httpContextMock.SetupGet(httpContext => httpContext.Items["userId"]).Returns(userId);

            _todosRepositoryMock
                .Setup(todosRepository => todosRepository.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync((TodoItemReadModel)null);

            // Act
            var result = await _sut.UpdateStatus(id);

            //Assert
            result.Should().BeOfType<ActionResult<TodosItemResponse>>()
                .Which.Result.Should().BeOfType<NotFoundObjectResult>()
                .Which.Value.Should().BeEquivalentTo($"Todo item with id: '{id}' does not exist");

            _todosRepositoryMock
                .Verify(todosRepository => todosRepository.GetAsync(id, userId), Times.Once);
        }

        [Theory, AutoData]
        public async Task UpdateStatus_ReturnsTodoItem_When_IdParameterIsPassed(
            Guid id,
            Guid userId,
            TodoItemReadModel todoItemReadModel)
        {
            // Arrange
            todoItemReadModel.Id = id;
            todoItemReadModel.UserId = userId;

            _httpContextMock.SetupGet(httpContext => httpContext.Items["userId"]).Returns(userId);

            _todosRepositoryMock
                .Setup(todosRepository => todosRepository.GetAsync(id, userId))
                .ReturnsAsync(todoItemReadModel);

            // Act
            var result = await _sut.UpdateStatus(id);

            //Assert
            result.Should().BeOfType<ActionResult<TodosItemResponse>>()
                .Which.Value.Should().BeOfType<TodosItemResponse>()
                .Which.Should().BeEquivalentTo(todoItemReadModel);

            _todosRepositoryMock
                .Verify(todosRepository => todosRepository.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);

            _todosRepositoryMock
                .Verify(todosRepository => todosRepository.SaveOrUpdateAsync(todoItemReadModel), Times.Once);
        }

        [Theory, AutoData]
        public async Task Delete_ReturnsNotFoundObjectResult_When_TodoItemIsNull(
            Guid id,
            Guid userId)
        {
            // Arrange
            _httpContextMock.SetupGet(httpContext => httpContext.Items["userId"]).Returns(userId);

            _todosRepositoryMock
                .Setup(todosRepository => todosRepository.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync((TodoItemReadModel)null);

            // Act
            var result = await _sut.Delete(id);

            //Assert
            result.Should().BeOfType<NotFoundObjectResult>()
                .Which.Value.Should().BeEquivalentTo($"Todo item with id: '{id}' does not exist");

            _todosRepositoryMock
                .Verify(todosRepository => todosRepository.GetAsync(id, userId), Times.Once);
        }

        [Theory, AutoData]
        public async Task Delete_ReturnsNoContent_When_IdParameterIsPassed(
            Guid id,
            Guid userId,
            TodoItemReadModel todoItemReadModel)
        {
            // Arrange
            _httpContextMock.SetupGet(httpContext => httpContext.Items["userId"]).Returns(userId);

            _todosRepositoryMock
                .Setup(todosRepository => todosRepository.GetAsync(id, userId))
                .ReturnsAsync(todoItemReadModel);

            // Act
            var result = await _sut.Delete(id);

            //Assert
            result.Should().BeOfType<NoContentResult>();

            _todosRepositoryMock
                .Verify(todosRepository => todosRepository.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);

            _todosRepositoryMock
                .Verify(todosRepository => todosRepository.DeleteAsync(id), Times.Once);
        }
    }
}