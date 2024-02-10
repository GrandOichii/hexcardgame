using FluentAssertions;
using ManagerBack.Tests.Mocks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ManagerBack.Tests.Endpoints;

public class ExpansionEndpointTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ExpansionEndpointTests(WebApplicationFactory<Program> factory) {
        _factory = factory.WithWebHostBuilder(builder => {
            builder.ConfigureServices(services => {
                services.AddSingleton<ICardRepository, MockCardRepository>();
        //         services.AddSingleton<IPollService, MockPollService>();
        //         services.AddSingleton<IUserService, MockUserService>();
            });
        });
    }


    [Fact]
    public async Task ShouldFetchAll() {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync($"/api/v1/expansion");

        // Assert
        result.Should().BeSuccessful();

    }

    // [Fact]
    public async Task ShouldFetchByName() {
        // Arrange
        var expansion = "expansion1";
        var client = _factory.CreateClient();

        // TODO authorize and create cards

        // Act
        var result = await client.GetAsync($"/api/v1/expansion/{expansion}");

        // Assert
        result.Should().BeSuccessful();
    }

    [Fact]
    public async Task ShouldFailFetchByName() {
        // Arrange
        var expansion = "expansion1";
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync($"/api/v1/expansion/{expansion}");

        // Assert
        result.Should().HaveClientError();
    }

    
}