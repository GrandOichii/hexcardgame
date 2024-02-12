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
        result.Should().BeOfType<BadRequestObjectResult>();
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
        
    }
}