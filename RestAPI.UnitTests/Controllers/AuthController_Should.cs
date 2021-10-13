using AutoFixture.Xunit2;
using Contracts.Models.RequestModels;
using Contracts.Models.ResponseModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Persistence.Models.ReadModels;
using Persistence.Repositories;
using RestAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestHelper.Attributes;
using Xunit;

namespace RestAPI.UnitTests.Controllers
{
    public class AuthController_Should
    {
        private readonly Mock<IUserRepository> _usersRepositoryMock = new Mock<IUserRepository>();

        private readonly AuthController _sut;

        public AuthController_Should()
        {
            _sut = new AuthController(_usersRepositoryMock.Object);
        }

        [Theory, AutoData]
        public async Task SignUp_ReturnsNotFoundObjectResult_When_UserReadModelIsNull(
            SignUpRequest request,
            UserReadModel userReadModel)
        {
            // Arrange
            _usersRepositoryMock
                .Setup(usersRepository => usersRepository.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(userReadModel);

            // Act
            var result = await _sut.SignUp(request);

            // Assert
            result.Should().BeOfType<ActionResult<SignUpResponse>>()
                .Which.Result.Should().BeOfType<ConflictObjectResult>();

            _usersRepositoryMock
                .Verify(usersRepository => usersRepository.GetAsync(request.Username), Times.Once);
        }

        [Theory, AutoData]
        public async Task SignUp_ReturnsSignUpResponse_When_UserReadModelIsNull(
            SignUpRequest request,
            UserReadModel userReadModel)
        {
            // Arrange
            userReadModel.Username = request.Username;
            userReadModel.Password = request.Password;

            _usersRepositoryMock
                .Setup(usersRepository => usersRepository.GetAsync(request.Username))
                .ReturnsAsync((UserReadModel) null);

            // Act
            var result = await _sut.SignUp(request);

            // Assert
            result.Should().BeOfType<ActionResult<SignUpResponse>>()
                .Which.Value.Should().BeEquivalentTo(request, options => options
                .Excluding(model => model.Password));

            _usersRepositoryMock
                .Verify(usersRepository => usersRepository.GetAsync(It.IsAny<string>()), Times.Once);

            _usersRepositoryMock
                .Verify(usersRepository => usersRepository.SaveAsync(It.Is<UserReadModel>(model => model.Username.Equals(userReadModel.Username) 
                && model.Password.Equals(userReadModel.Password))), Times.Once);
        }
    }
}
