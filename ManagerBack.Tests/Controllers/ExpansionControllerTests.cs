using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace ManagerBack.Tests.Controllers;

public class ExpansionControllerTests {
    private readonly ExpansionController _expansionController;
    private readonly IExpansionService _expansionService;

    public ExpansionControllerTests() {
        _expansionService = A.Fake<IExpansionService>();

        _expansionController = new(_expansionService);
    }

    [Fact]
    public async Task ShouldReturnAll() {
        // Arrange
        A.CallTo(() => _expansionService.All()).Returns(A.Fake<IEnumerable<Expansion>>());

        // Act
        var result = await _expansionController.All();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ShouldReturnByName() {
        var name = "expansion";
        // Arrange
        A.CallTo(() => _expansionService.ByName(name)).Returns(A.Fake<Expansion>());

        // Act
        var result = await _expansionController.ByName(name);

        // Arrange
        result.Should().BeOfType<OkObjectResult>();
    }
}