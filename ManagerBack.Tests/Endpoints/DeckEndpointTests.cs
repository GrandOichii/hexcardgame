using System.Net.Http.Json;
using FluentAssertions;
using ManagerBack.Tests.Mocks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ManagerBack.Tests.Endpoints;

public class DeckEndpointTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DeckEndpointTests(WebApplicationFactory<Program> factory) {
        _factory = factory.WithWebHostBuilder(builder => {
            builder.ConfigureServices(services => {
                services.AddSingleton<ICardRepository, MockCardRepository>();
                services.AddSingleton<IUserRepository, MockUserRepository>();
                services.AddSingleton<IDeckRepository, MockDeckRepository>();
            });
        });
    }

    // [Fact]
    public async Task ShouldRegister() {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var result = await client.PostAsync("/api/v1/auth/register", JsonContent.Create(new PostUserDto{
            Username = "user",
            Password = "password"
        }));

        // Assert
        result.Should().BeSuccessful();
    }
    
}