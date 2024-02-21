using System.Security.Claims;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ManagerBack.Tests.Controllers;

public class CardControllerTests {
    private readonly CardController _cardController;
    private readonly ICardService _cardService;


    public CardControllerTests() {
        _cardService = A.Fake<ICardService>();

        _cardController = new(_cardService);
    }

    private void AddUser(string id, string username, bool isAdmin) {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
            new(ClaimTypes.NameIdentifier, id),
            new(ClaimTypes.Role, isAdmin ? "Admin" : "User"),
        }));

        _cardController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task ShouldFetchByCID() {
        // Arange
        var cid = "expansion::card";
        A.CallTo(() => _cardService.ByCID(cid)).Returns(A.Fake<ExpansionCard>());

        // Act
        var result = await _cardController.ByCID(cid);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ShouldNotFetchByCIDCardNotFound() {
        // Arange
        var cid = "expansion::card";
        A.CallTo(() => _cardService.ByCID(cid)).Throws(new CardNotFoundException(""));

        // Act
        var result = await _cardController.ByCID(cid);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ShouldNotFetchByCIDInvalidCID() {
        // Arange
        var cid = "expansion:card";
        A.CallTo(() => _cardService.ByCID(cid)).Throws(new InvalidCIDException(""));

        // Act
        var result = await _cardController.ByCID(cid);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]    
    public async Task ShouldFetchByExpansion() {
        // Arrange
        var expansion = "expansion";
        A.CallTo(() => _cardService.ByExpansion(expansion)).Returns(A.Fake<IEnumerable<ExpansionCard>>());

        // Act
        var result = await _cardController.FromExpansion(expansion);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ShouldCreate() {
        // Arrange
        var card = A.Fake<ExpansionCard>();
        A.CallTo(() => _cardService.Create(card)).Returns(card);

        // Act
        var result = await _cardController.Create(card);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    public static IEnumerable<object[]> CardCreateExceptions {
        get {
            yield return new object[] { new CIDTakenException("") };
            yield return new object[] { new InvalidCardCreationParametersException("") };
        }
    }

    [Theory]
    [MemberData(nameof(CardCreateExceptions))]
    public async Task ShouldNotCreate(Exception e) {
        // Arrange
        var card = A.Fake<ExpansionCard>();
        A.CallTo(() => _cardService.Create(card))
            .Throws(e)
        ;

        // Act
        var result = await _cardController.Create(card);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    
    [Fact]
    public async Task ShouldUpdate() {
        // Arrange
        var card = A.Fake<ExpansionCard>();
        A.CallTo(() => _cardService.Update(card)).DoesNothing();

        // Act
        var result = await _cardController.Update(card);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    // TODO split into different tests - CardNotFoundException should return a NotFoundObjectResult
    public static IEnumerable<object[]> CardUpdateExceptions {
        get {
            yield return new object[] { new CardNotFoundException("") };
            yield return new object[] { new InvalidCardCreationParametersException("") };
        }
    }

    [Theory]
    [MemberData(nameof(CardUpdateExceptions))]
    public async Task ShouldNotUpdate(Exception e) {
        // Arrange
        var card = A.Fake<ExpansionCard>();
        A.CallTo(() => _cardService.Update(card))
            .Throws(e)
        ;

        // Act
        var result = await _cardController.Update(card);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ShouldDelete() {
        // Arrange
        var cid = "expansion::card";
        A.CallTo(() => _cardService.Delete(cid)).DoesNothing();

        // Act
        var result = await _cardController.Delete(cid);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    public static IEnumerable<object[]> CardDeleteExceptions {
        get {
            yield return new object[] { new CardNotFoundException("") };
        }
    }

    [Theory]
    [MemberData(nameof(CardDeleteExceptions))]
    public async Task ShouldNotDelete(Exception e) {
        // Arrange
        var cid = "expansion::card";
        A.CallTo(() => _cardService.Delete(cid))
            .Throws(e)
        ;

        // Act
        var result = await _cardController.Delete(cid);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }


}