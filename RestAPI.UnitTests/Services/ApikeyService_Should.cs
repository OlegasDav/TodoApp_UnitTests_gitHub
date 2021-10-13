using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Persistence.Models.ReadModels;
using Persistence.Repositories;
using RestAPI.Models;
using RestAPI.Options;
using RestAPI.Services;
using TestHelper.Attributes;
using Xunit;

namespace RestAPI.UnitTests.Services
{
    
    public class ApikeyService_Should
    {
        [Theory, AutoMoqData]
        public async Task CreateApiKey_ReturnsBadHttpException_When_UserIsNull(
            string username,
            string password,
            [Frozen] Mock<IUserRepository> userRepositoryMock,
            ApikeyService sut)
        {
            // Arrange
            userRepositoryMock
                .Setup(userRepository => userRepository.GetAsync(It.IsAny<string>()))
                .ReturnsAsync((UserReadModel) null);

            // Act & Assert
            var result = await sut.Invoking(sut => sut.CreateApiKey(username, password))
                .Should().ThrowAsync<BadHttpRequestException>()
                .WithMessage($"User with Username: '{username}' does not exist!");

            result.Which.StatusCode.Should().Be(404);

            userRepositoryMock
                .Verify(userRepository => userRepository.GetAsync(username), Times.Once);

        }

        [Theory, AutoMoqData]
        public async Task CreateApiKey_ReturnsBadHttpException_When_WrongPassword(
            string username,
            string password,
            UserReadModel user,
            [Frozen] Mock<IUserRepository> userRepositoryMock,
            ApikeyService sut)
        {
            // Arrange
            userRepositoryMock
                .Setup(userRepository => userRepository.GetAsync(It.Is<string>(user => user.Equals(username))))
                .ReturnsAsync(user);

            // Act & Assert
            var result = await sut.Invoking(sut => sut.CreateApiKey(username, password))
                .Should().ThrowAsync<BadHttpRequestException>()
                .WithMessage($"Wrong password for user: '{user.Username}'");

            result.Which.StatusCode.Should().Be(400);

            userRepositoryMock
                .Verify(userRepository => userRepository.GetAsync(It.IsAny<string>()), Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task CreateApiKey_ReturnsBadHttpException_When_LimitIsReached(
            string username,
            string password,
            UserReadModel user,
            [Frozen] Mock<IUserRepository> userRepositoryMock,
            [Frozen] Mock<IApiKeysRepository> apiKeysRepositoryMock,
            [Frozen] Mock<IOptions<ApiKeySettings>> apiKeySettingsMock,
            IEnumerable<ApiKeyReadModel> allKeys,
            ApiKeySettings apiKeySettings,
            ApikeyService sut
            )
        {
            // Arrange
            user.Username = username;
            user.Password = password;

            userRepositoryMock
                .Setup(userRepository => userRepository.GetAsync(It.Is<string>(user => user.Equals(username))))
                .ReturnsAsync(user);

            apiKeysRepositoryMock
                .Setup(apiKeysRepository => apiKeysRepository.GetByUserIdAsync(It.Is<Guid>(id => id.Equals(user.Id))))
                .ReturnsAsync(allKeys);

            apiKeySettings.ApiKeyLimit = allKeys.Count();

            apiKeySettingsMock
                .SetupGet(apiKeySettings => apiKeySettings.Value)
                .Returns(apiKeySettings);

            // Act & Assert
            var result = await sut.Invoking(sut => sut.CreateApiKey(username, password))
                .Should().ThrowAsync<BadHttpRequestException>()
                .WithMessage($"Api key limit is reached");

            result.Which.StatusCode.Should().Be(400);

            userRepositoryMock
                .Verify(userRepository => userRepository.GetAsync(It.IsAny<string>()), Times.Once);

            apiKeysRepositoryMock
                .Verify(apiKeysRepository => apiKeysRepository.GetByUserIdAsync(It.IsAny<Guid>()), Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task CreateApiKey_ReturnsApiKey_When_ParametersArePassed(
            string username,
            string password,
            UserReadModel user,
            [Frozen] Mock<IUserRepository> userRepositoryMock,
            [Frozen] Mock<IApiKeysRepository> apiKeysRepositoryMock,
            [Frozen] Mock<IOptions<ApiKeySettings>> apiKeySettingsMock,
            IEnumerable<ApiKeyReadModel> allKeys,
            ApiKeySettings apiKeySettings,
            ApikeyService sut
            )
        {
            // Arrange
            user.Username = username;
            user.Password = password;

            userRepositoryMock
                .Setup(userRepository => userRepository.GetAsync(It.Is<string>(user => user.Equals(username))))
                .ReturnsAsync(user);

            apiKeysRepositoryMock
                .Setup(apiKeysRepository => apiKeysRepository.GetByUserIdAsync(It.Is<Guid>(id => id.Equals(user.Id))))
                .ReturnsAsync(allKeys);

            apiKeySettings.ApiKeyLimit = allKeys.Count() + 1;

            apiKeySettingsMock
                .SetupGet(apiKeySettings => apiKeySettings.Value)
                .Returns(apiKeySettings);

            // Act
            var result = await sut.CreateApiKey(username, password);

            // Assert
            result.UserId.Should().Be(user.Id);
            result.IsActive.Should().BeTrue();
            
            userRepositoryMock
                .Verify(userRepository => userRepository.GetAsync(It.IsAny<string>()), Times.Once);

            apiKeysRepositoryMock
                .Verify(apiKeysRepository => apiKeysRepository.GetByUserIdAsync(It.IsAny<Guid>()), Times.Once);

            apiKeysRepositoryMock
                .Verify(apiKeysRepository => apiKeysRepository.SaveAsync(It.Is<ApiKeyReadModel>(model => model.UserId.Equals(user.Id) && model.IsActive.Equals(true))), Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task GetAllApiKeys_ReturnsBadHttpException_When_UserIsNull(
            string username,
            string password,
            [Frozen] Mock<IUserRepository> userRepositoryMock,
            ApikeyService sut)
        {
            // Arrange
            userRepositoryMock
                .Setup(userRepository => userRepository.GetAsync(It.IsAny<string>()))
                .ReturnsAsync((UserReadModel)null);

            // Act
            var result = await sut.Invoking(sut => sut.GetAllApiKeys(username, password))
                .Should().ThrowAsync<BadHttpRequestException>()
                .WithMessage($"User with Username: '{username}' does not exists!");

            // Assert
            result.Which.StatusCode.Should().Be(404);
            userRepositoryMock
                .Verify(userRepository => userRepository.GetAsync(username), Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task GetAllApiKeys_ReturnsBadHttpException_When_WrongPassword(
            string username,
            string password,
            UserReadModel user,
            [Frozen] Mock<IUserRepository> userRepositoryMock,
            ApikeyService sut)
        {
            // Arrange
            userRepositoryMock
                .Setup(userRepository => userRepository.GetAsync(It.Is<string>(user => user.Equals(username))))
                .ReturnsAsync(user);

            user.Username = username;

            // Act
            var result = await sut.Invoking(sut => sut.GetAllApiKeys(username, password))
                .Should().ThrowAsync<BadHttpRequestException>()
                .WithMessage($"Wrong password for user: '{user.Username}'");

            // Assert
            result.Which.StatusCode.Should().Be(400);
            userRepositoryMock
                .Verify(userRepository => userRepository.GetAsync(It.IsAny<string>()), Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task GetAllApiKeys_ReturnsApiKey_When_ParametersArePassed(
            string username,
            string password,
            UserReadModel user,
            [Frozen] Mock<IUserRepository> userRepositoryMock,
            [Frozen] Mock<IApiKeysRepository> apiKeysRepositoryMock,
            List<ApiKeyReadModel> allKeys,
            ApikeyService sut
            )
        {
            // Arrange
            userRepositoryMock
                .Setup(userRepository => userRepository.GetAsync(It.Is<string>(user => user.Equals(username))))
                .ReturnsAsync(user);

            user.Username = username;
            user.Password = password;

            apiKeysRepositoryMock
                .Setup(apiKeysRepository => apiKeysRepository.GetByUserIdAsync(user.Id))
                .ReturnsAsync(allKeys);

            // Act
            var result = (await sut.GetAllApiKeys(username, password)).ToList();

            // Assert
            result.Should().BeEquivalentTo(allKeys, option => option.ExcludingMissingMembers());

            for (var i = 0; i < result.Count(); i++)
            {
                result[i].Key.Should().BeEquivalentTo(allKeys[i].ApiKey);
            }

            userRepositoryMock
                .Verify(userRepository => userRepository.GetAsync(It.IsAny<string>()), Times.Once);

            apiKeysRepositoryMock
                .Verify(apiKeysRepository => apiKeysRepository.GetByUserIdAsync(It.IsAny<Guid>()), Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task UpdateApiKeyState_ReturnsBadHttpException_When_ApiKeyIsNull(
            Guid id,
            bool newState,
            [Frozen] Mock<IApiKeysRepository> apiKeysRepositoryMock,
            ApikeyService sut)
        {
            // Arrange
            apiKeysRepositoryMock
                .Setup(apiKeysRepository => apiKeysRepository.GetByApiKeyIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ApiKeyReadModel)null);

            // Act & assert
            var result = await sut.Invoking(sut => sut.UpdateApiKeyState(id, newState))
                .Should().ThrowAsync<BadHttpRequestException>()
                .WithMessage($"Api key with Id: '{id}' does not exists");

            result.Which.StatusCode.Should().Be(404);

            apiKeysRepositoryMock
                .Verify(apiKeysRepository => apiKeysRepository.GetByApiKeyIdAsync(id), Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task UpdateApiKeyState_ReturnsApi_When_ApiKeyIsNotNull(
            Guid id,
            bool newState,
            ApiKeyReadModel apiKey,
            [Frozen] Mock<IApiKeysRepository> apiKeysRepositoryMock,
            ApikeyService sut)
        {
            // Arrange
            apiKeysRepositoryMock
                .Setup(apiKeysRepository => apiKeysRepository.GetByApiKeyIdAsync(id))
                .ReturnsAsync(apiKey);

            apiKey.IsActive = newState;

            // Act
            var result = await sut.UpdateApiKeyState(id, newState);

            // Assert
            result.Should().BeEquivalentTo(apiKey, option => option.ExcludingMissingMembers());
            result.Key.Should().BeEquivalentTo(apiKey.ApiKey);

            apiKeysRepositoryMock
                .Verify(apiKeysRepository => apiKeysRepository.GetByApiKeyIdAsync(It.IsAny<Guid>()), Times.Once);

            apiKeysRepositoryMock
                .Verify(apiKeysRepository => apiKeysRepository.UpdateIsActive(id, newState), Times.Once);
        }
    }
}