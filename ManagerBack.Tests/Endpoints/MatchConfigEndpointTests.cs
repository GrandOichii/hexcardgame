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
                services.AddSingleton<ICardRepository, FakeCardRepository>();
                services.AddSingleton<IUserRepository, FakeUserRepository>();
                services.AddSingleton<IDeckRepository, FakeDeckRepository>();
                services.AddSingleton<IMatchConfigRepository, FakeMatchConfigRepository>();
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
        return (await result.Content.ReadFromJsonAsync<LoginResult>())!.Token;
    }

    private static async Task Login(HttpClient client, string email = "mymail@email.com", string password = "password") {
        var token = await GetJwtToken(client, email, password);
        client.SetBearerToken(token);
    }


    [Fact]
    public async Task ShouldFetchAll() {
        // Arrange
        var client = _factory.CreateClient();
        await Login(client, "admin", "password");

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
        var result = await client.PostAsync("/api/v1/config", JsonContent.Create(new PostMatchConfigDto {
            // TODO add more fields
            Name = "config1",
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
        var result = await client.PostAsync("/api/v1/config", JsonContent.Create(new PostMatchConfigDto {
            Name = "config1",
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
        var response = await client.PostAsync("/api/v1/config", JsonContent.Create(new PostMatchConfigDto {
            // TODO add more fields
            Name = "config1",
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

    [Fact]
    public async Task ShouldFetchBasic() {
        // Arrange
        var client = _factory.CreateClient();
        await Login(client, "admin", "password");
        await client.PostAsync("/api/v1/config", JsonContent.Create(new PostMatchConfigDto {
            // TODO add more fields
            Name = "basic",
            SetupScript = "script"
        }));

        // Act
        var result = await client.GetAsync($"/api/v1/config/basic");

        // Assert
        result.Should().BeSuccessful();
    }
}