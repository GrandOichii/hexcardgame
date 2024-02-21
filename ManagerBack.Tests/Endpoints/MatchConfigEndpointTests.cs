using System.Net.Http.Json;
using FluentAssertions;
using HexCore.GameMatch;
using IdentityModel.Client;
using ManagerBack.Tests.Mocks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ManagerBack.Tests.Endpoints;

public class MatchConfigEndpointTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public MatchConfigEndpointTests(WebApplicationFactory<Program> factory) {
        _factory = factory.WithWebHostBuilder(builder => {
            builder.ConfigureServices(services => {
                services.AddSingleton<ICardRepository, MockCardRepository>();
                services.AddSingleton<IUserRepository, MockUserRepository>();
                services.AddSingleton<IDeckRepository, MockDeckRepository>();
                services.AddSingleton<IMatchConfigRepository, MockMatchConfigRepository>();
            });
        });
    }

    private static async Task<string> GetJwtToken(HttpClient client, string username, string password) {
        var user = new PostUserDto {
            Username = username,
            Password = password
        };
        var reg = await client.PostAsync("/api/v1/auth/register", JsonContent.Create(user));
        reg.Should().BeSuccessful();
        var result = await client.PostAsync("/api/v1/auth/login", JsonContent.Create(user));
        result.Should().BeSuccessful();
        return await result.Content.ReadAsStringAsync();
    }

    private static async Task Login(HttpClient client, string email = "mymail@email.com", string password = "password") {
        var token = await GetJwtToken(client, email, password);
        client.SetBearerToken(token);
    }


    [Fact]
    public async Task ShouldFetchAll() {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync("/api/v1/config");

        // Assert
        result.Should().BeSuccessful();
    }

    [Fact]
    public async Task ShouldCreate() {
        // Arrange
        var client = _factory.CreateClient();
        await Login(client, "admin", "password");

        // Act
        var result = await client.PostAsync("/api/v1/config", JsonContent.Create(new MatchConfig {
            // TODO add more fields
            SetupScript = "print('setup')"
        }));

        // Assert
        result.Should().BeSuccessful();
    }

    [Fact]
    public async Task ShouldNotCreate() {
        // Arrange
        var client = _factory.CreateClient();
        await Login(client, "admin", "password");

        // Act
        var result = await client.PostAsync("/api/v1/config", JsonContent.Create(new MatchConfig {
            SetupScript = ""
        }));

        // Assert
        result.Should().HaveClientError();
    }

    [Fact]
    public async Task ShouldFetchById() {
// Arrange
        var client = _factory.CreateClient();
        await Login(client, "admin", "password");
        var response = await client.PostAsync("/api/v1/config", JsonContent.Create(new MatchConfig {
            // TODO add more fields
            SetupScript = "script"
        }));
        var id = (await response.Content.ReadFromJsonAsync<MatchConfigModel>())!.Id;

        // Act
        var result = await client.GetAsync($"/api/v1/config/{id}");

        // Assert
        result.Should().BeSuccessful();

    }

    [Fact]
    public async Task ShouldNotFetchById() {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync("/api/v1/config/invalid-id");

        // Assert
        result.Should().HaveClientError();
    }
}