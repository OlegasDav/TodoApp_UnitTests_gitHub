using AutoFixture.Xunit2;
using Contracts.Models.RequestModels;
using Contracts.Models.ResponseModels;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RestAPI.Controllers;
using RestAPI.Models;
using RestAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestHelper.Attributes;
using Xunit;

namespace RestAPI.UnitTests.Controllers
{
    public class ApiKeyController_Should
    {
        private readonly Mock<IApikeyService> _apikeyServiceMock = new Mock<IApikeyService>();

        private readonly ApiKeysController _sut;

        public ApiKeyController_Should()
        {
            _sut = new ApiKeysController(_apikeyServiceMock.Object);
        }

        [Theory, AutoData]
        public async Task Create_ReturnsApiKeyResponse_When_AllParametersArePassed(
            ApiKeyRequest request,
            ApiKey apiKey)
        {
            // Arrange
            _apikeyServiceMock
                .Setup(apikeyService => apikeyService.CreateApiKey(request.Username, request.Password))
                .ReturnsAsync(apiKey);

            // Act
            var result = await _sut.Create(request);

            // Assert
            result.Should().BeOfType<ActionResult<ApiKeyResponse>>()
                .Which.Value.Should().BeEquivalentTo(apiKey, option => option
                .ExcludingMissingMembers());

            result.Should().BeOfType<ActionResult<ApiKeyResponse>>()
                .Which.Value.ApiKey.Should().BeEquivalentTo(apiKey.Key);

            _apikeyServiceMock
                .Verify(apikeyService => apikeyService.CreateApiKey(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Theory, AutoData]
        public async Task Create_ReturnsNotFound_When_UsernameDoesNotExist(
            ApiKeyRequest request)
        {
            // Arrange
            _apikeyServiceMock
                .Setup(apikeyService => apikeyService.CreateApiKey(request.Username, request.Password))
                .ThrowsAsync(new BadHttpRequestException($"User with Username: '{request.Username}' does not exist!", 404));

            // Act
            var result = await _sut.Create(request);

            // Assert
            result.Should().BeOfType<ActionResult<ApiKeyResponse>>()
                .Which.Result.Should().BeOfType<NotFoundObjectResult>()
                .Which.Value.Should().BeEquivalentTo($"User with Username: '{request.Username}' does not exist!");

            _apikeyServiceMock
                .Verify(apikeyService => apikeyService.CreateApiKey(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Theory, AutoData]
        public async Task Create_ReturnsBadRequest_When_PasswordIsWrong(
            ApiKeyRequest request)
        {
            // Arrange
            _apikeyServiceMock
                .Setup(apikeyService => apikeyService.CreateApiKey(request.Username, request.Password))
                .ThrowsAsync(new BadHttpRequestException($"Wrong password for user: '{request.Username}'", 400));

            // Act
            var result = await _sut.Create(request);

            // Assert
            result.Should().BeOfType<ActionResult<ApiKeyResponse>>()
                .Which.Result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().BeEquivalentTo($"Wrong password for user: '{request.Username}'");

            _apikeyServiceMock
                .Verify(apikeyService => apikeyService.CreateApiKey(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Theory, AutoData]
        public async Task GetAllKeys_ReturnsIEnumerableApiKeyResponse_When_AllParametersArePassed(
            string username,
            string password,
            List<ApiKey> apiKeys)
        {
            // Arrange
            _apikeyServiceMock
                .Setup(apikeyService => apikeyService.GetAllApiKeys(username, password))
                .ReturnsAsync(apiKeys);

            // Act
            var result = await _sut.GetAllKeys(username, password);

            // Assert
            result.Should().BeOfType<ActionResult<List<ApiKeyResponse>>>()
                .Which.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(apiKeys, option => option
                .ExcludingMissingMembers());

            for (var i = 0; i < apiKeys.Count; i++)
            {
                result.Should().BeOfType<ActionResult<List<ApiKeyResponse>>>()
                .Which.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeOfType<List<ApiKeyResponse>>()
                .Which[i].ApiKey.Should().BeEquivalentTo(apiKeys[i].Key);
            }

            _apikeyServiceMock
                .Verify(apikeyService => apikeyService.GetAllApiKeys(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Theory, AutoData]
        public async Task GetAllKeys_ReturnsNotFound_When_UserDoesNotExist(
            string username,
            string password)
        {
            // Arrange
            _apikeyServiceMock
                .Setup(apikeyService => apikeyService.GetAllApiKeys(username, password))
                .ThrowsAsync(new BadHttpRequestException($"User with Username: '{username}' does not exists!", 404));

            // Act
            var result = await _sut.GetAllKeys(username, password);

            // Assert
            result.Should().BeOfType<ActionResult<List<ApiKeyResponse>>>()
                .Which.Result.Should().BeOfType<NotFoundObjectResult>()
                .Which.Value.Should().BeEquivalentTo($"User with Username: '{username}' does not exists!");

            _apikeyServiceMock
                .Verify(apikeyService => apikeyService.GetAllApiKeys(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Theory, AutoData]
        public async Task GetAllKeys_ReturnsBadRequest_When_PasswordIsWrong(
            string username,
            string password)
        {
            // Arrange
            _apikeyServiceMock
                .Setup(apikeyService => apikeyService.GetAllApiKeys(username, password))
                .ThrowsAsync(new BadHttpRequestException($"Wrong password for user: '{username}'", 400));

            // Act
            var result = await _sut.GetAllKeys(username, password);

            // Assert
            result.Should().BeOfType<ActionResult<List<ApiKeyResponse>>>()
                .Which.Result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().BeEquivalentTo($"Wrong password for user: '{username}'");

            _apikeyServiceMock
                .Verify(apikeyService => apikeyService.GetAllApiKeys(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Theory, AutoData]
        public async Task UpdateKeyState_ReturnsApiKeyResponse_When_AllParametersArePassed(
            Guid id,
            UpdateKeyStateRequest request,
            ApiKey apiKey)
        {
            // Arrange
            apiKey.IsActive = request.IsActive;

            _apikeyServiceMock
                .Setup(apikeyService => apikeyService.UpdateApiKeyState(id, request.IsActive))
                .ReturnsAsync(apiKey);

            // Act
            var result = await _sut.UpdateKeyState(id, request);

            // Assert
            result.Should().BeOfType<ActionResult<ApiKeyResponse>>()
                .Which.Value.Should().BeEquivalentTo(apiKey, option => option
                .ExcludingMissingMembers());

            result.Should().BeOfType<ActionResult<ApiKeyResponse>>()
                .Which.Value.ApiKey.Should().BeEquivalentTo(apiKey.Key);

            _apikeyServiceMock
                .Verify(apikeyService => apikeyService.UpdateApiKeyState(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Once);
        }

        [Theory, AutoData]
        public async Task UpdateKeyState_ReturnsNotFound_When_ApiKeyDoesNotExist(
            Guid id,
            UpdateKeyStateRequest request)
        {
            // Arrange
            _apikeyServiceMock
                .Setup(apikeyService => apikeyService.UpdateApiKeyState(id, request.IsActive))
                .ThrowsAsync(new BadHttpRequestException($"Api key with Id: '{id}' does not exists", 404));

            // Act
            var result = await _sut.UpdateKeyState(id, request);

            // Assert
            result.Should().BeOfType<ActionResult<ApiKeyResponse>>()
                .Which.Result.Should().BeOfType<NotFoundObjectResult>()
                .Which.Value.Should().BeEquivalentTo($"Api key with Id: '{id}' does not exists");

            _apikeyServiceMock
                .Verify(apikeyService => apikeyService.UpdateApiKeyState(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Once);
        }
    }
}
