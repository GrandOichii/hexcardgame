using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace ManagerBack.Tests.Controllers;

public class MatchConfigControllerTests {

    private readonly IMatchConfigService _configService;
    private readonly MatchConfigController _configController;
    public MatchConfigControllerTests() {
        _configService = A.Fake<IMatchConfigService>();

        _configController = new(_configService);
    }


    [Fact]
    public async Task ShouldFetchAll() {
        // Arrange
        A.CallTo(() => _configService.All()).Returns(A.Fake<IEnumerable<MatchConfigModel>>());

        // Act
        var result = await _configController.All();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ShouldCreate() {
        // Arrange
        var config = A.Fake<MatchConfigModel>();
        A.CallTo(() => _configService.Create(config)).Returns(config);

        // Act
        var result = await _configController.Create(config);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ShouldNotCreate() {
        // Arrange
        var config = A.Fake<MatchConfigModel>();
        A.CallTo(() => _configService.Create(config)).Throws<InvalidMatchConfigCreationParametersException>();

        // Act
        var result = await _configController.Create(config);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ShouldFetchById() {
        // Arrange
        var id = "config-id";
        A.CallTo(() => _configService.ById(id)).Returns(A.Fake<MatchConfigModel>());

        // Act
        var result = await _configController.ById(id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ShouldNotFetchById() {
        // Arrange
        var id = "config-id";
        A.CallTo(() => _configService.ById(id)).Throws<MatchConfigNotFoundException>();

        // Act
        var result = await _configController.ById(id);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}