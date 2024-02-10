using FluentAssertions;
using ManagerBack.Tests.Mocks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ManagerBack.Tests.Endpoints;

public class CardEndpointTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CardEndpointTests(WebApplicationFactory<Program> factory) {
        _factory = factory.WithWebHostBuilder(builder => {
            builder.ConfigureServices(services => {
                services.AddSingleton<ICardRepository, MockCardRepository>();
        //         services.AddSingleton<IPollService, MockPollService>();
        //         services.AddSingleton<IUserService, MockUserService>();
            });
        });
    }

    // TODO add authorized endpoint testing

    [Fact]
    public async Task ShouldFetchAll() {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync("/api/v1/card/all");

        // Assert
        result.Should().BeSuccessful();
    }

    // [Fact]
    public async Task ShouldFetchByCID() {
        // Arrange
        var name = "card1";
        var expansion = "expansion1";
        var client = _factory.CreateClient();

        // TODO authorize and add the card

        // Act
        var result = await client.GetAsync($"/api/v1/card/{expansion}::{name}");

        // Assert
        result.Should().BeSuccessful();
    }

    [Fact]
    public async Task ShouldNotFetchByCIDInvalidCID() {
        // Arrange
        var name = "card1";
        var expansion = "expansion1";
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync($"/api/v1/card/{expansion}:{name}");

        // Assert
        result.Should().HaveClientError();
    }

    [Fact]
    public async Task ShouldNotFetchByCIDCardNotFound() {
        // Arrange
        var name = "card1";
        var expansion = "expansion1";
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync($"/api/v1/card/{expansion}::{name}");

        // Assert
        result.Should().HaveClientError();
    }

    [Fact]
    public async Task ShouldFetchFromExpansionEmpty() {
        // Arrange
        var expansion = "expansion1";
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync($"/api/v1/card/fromexpansion/{expansion}");

        // Assert
        result.Should().BeSuccessful();

    }

    
}