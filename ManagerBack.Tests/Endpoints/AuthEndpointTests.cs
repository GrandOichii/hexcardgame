using System.Net.Http.Json;
using FluentAssertions;
using ManagerBack.Tests.Mocks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace ManagerBack.Tests.Endpoints;

public class AuthEndpointTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthEndpointTests(WebApplicationFactory<Program> factory) {
        _factory = factory.WithWebHostBuilder(builder => {
            builder.ConfigureServices(services => {
                services.AddSingleton<ICardRepository, FakeCardRepository>();
                services.AddSingleton<IUserRepository, FakeUserRepository>();
                services.AddSingleton<IDeckRepository, FakeDeckRepository>();
                services.AddSingleton<IMatchConfigRepository, FakeMatchConfigRepository>();
                services.AddSingleton<ILoggerFactory, NullLoggerFactory>();

            });
        });
    }

    [Fact]
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

    [Fact]
    public async Task ShouldNotRegister() {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var result = await client.PostAsync("/api/v1/auth/register", JsonContent.Create(new PostUserDto{
            Username = "",
            Password = "password"
        }));

        // Assert
        result.Should().HaveClientError();
    }

    [Fact]
    public async Task ShouldLogin() {
        // Arrange
        var client = _factory.CreateClient();
        var login = new PostUserDto {
            Username = "user",
            Password = "password"
        };
        // Act
        await client.PostAsync("/api/v1/auth/register", JsonContent.Create(login));
        var result = await client.PostAsync("/api/v1/auth/login", JsonContent.Create(login));

        // Assert
        result.Should().BeSuccessful();
    }

    [Theory]
    [InlineData("", ""), InlineData("user", ""), InlineData("user", "wrong-password"), InlineData("another", "password")]
    public async Task ShouldNotLogin(string username, string password) {
        // Arrange
        var client = _factory.CreateClient();
        var user = new PostUserDto {
            Username = "user",
            Password = "password"
        };

        // Act
        await client.PostAsync("/api/v1/auth/register", JsonContent.Create(user));
        var result = await client.PostAsync("/api/v1/auth/login", JsonContent.Create(new PostUserDto {
            Username = username,
            Password = password
        }));

        // Assert
        result.Should().HaveClientError();
    }
    
}