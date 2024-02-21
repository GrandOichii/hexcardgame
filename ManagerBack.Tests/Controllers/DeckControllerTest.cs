using System.Security.Claims;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ManagerBack.Tests.Controllers;

public class DeckControllerTests {

    private readonly IDeckService _deckService;
    private readonly DeckController _deckController;

    public DeckControllerTests() {
        _deckService = A.Fake<IDeckService>();

        _deckController = new(_deckService);
    }

    private void AddUser(string id, string username, bool isAdmin) {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
            new(ClaimTypes.NameIdentifier, id),
            new(ClaimTypes.Role, isAdmin ? "Admin" : "User"),
        }));

        _deckController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task ShouldFetchAll() {
        // Arrange
        var userId = "1";
        AddUser(userId, "user", false);
        A.CallTo(() => _deckService.All(userId)).Returns(A.Fake<IEnumerable<DeckModel>>());

        // Act
        var result = await _deckController.All();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ShouldCreate() {
        // Arrange
        var userId = "1";
        var newDeck = A.Fake<PostDeckDto>();
        AddUser(userId, "user", false);
        A.CallTo(() => _deckService.Create(userId, newDeck))
            .Returns(A.Fake<DeckModel>())
        ;

        // Act
        var result = await _deckController.Create(newDeck);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    // TODO split off CardNotFoundException
    public static IEnumerable<object[]> DeckCreateExceptions {
        get {
            yield return new object[] { new InvalidDeckException() };
            yield return new object[] { new InvalidCIDException("") };
            yield return new object[] { new CardNotFoundException("") };
        }
    }


    [Theory]
    [MemberData(nameof(DeckCreateExceptions))]
    public async Task ShouldNotCreate(Exception e) {
        // Arrange
        var userId = "1";
        var newDeck = A.Fake<PostDeckDto>();
        AddUser(userId, "user", false);
        A.CallTo(() => _deckService.Create(userId, newDeck))
            .Throws(e)
        ;

        // Act
        var result = await _deckController.Create(newDeck);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ShouldDelete() {
        // Arrange
        var userId = "u1";
        var deckId = "d1";
        AddUser(userId, "user", false);
        A.CallTo(() => _deckService.Delete(userId, deckId)).DoesNothing();

        // Act
        var result = await _deckController.Delete(deckId);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    public static IEnumerable<object[]> DeckDeleteExceptions {
        get {
            yield return new object[] { new DeckNotFoundException() };
            yield return new object[] { new UnmatchedUserIdException() };
        }
    }


    [Theory]
    [MemberData(nameof(DeckDeleteExceptions))]
    public async Task ShouldNotDelete(Exception e) {
        // Arrange
        var userId = "u1";
        var deckId = "d1";
        AddUser(userId, "user", false);
        A.CallTo(() => _deckService.Delete(userId, deckId))
            .Throws(e)
        ;

        // Act
        var result = await _deckController.Delete(deckId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ShouldUpdate() {
        // Arrange
        var userId = "1";
        var newDeck = A.Fake<PostDeckDto>();
        AddUser(userId, "user", false);
        A.CallTo(() => _deckService.Create(userId, newDeck))
            .Returns(A.Fake<DeckModel>())
        ;

        // Act
        var result = await _deckController.Update(userId, newDeck);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    public static IEnumerable<object[]> DeckUpdateExceptions {
        get {
            yield return new object[] { new DeckNotFoundException() };
            yield return new object[] { new UnmatchedUserIdException() };
        }
    }

    [Theory]
    [MemberData(nameof(DeckUpdateExceptions))]
    public async Task ShouldNotUpdate(Exception e) {
        // Arrange
        var userId = "u1";
        var deckId = "d1";
        var newDeck = A.Fake<PostDeckDto>();
        AddUser(userId, "user", false);
        A.CallTo(() => _deckService.Update(userId, deckId, newDeck))
            .Throws(e)
        ;

        // Act
        var result = await _deckController.Update(deckId, newDeck);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}