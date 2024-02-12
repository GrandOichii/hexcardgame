using System.Net.Http.Json;
using FluentAssertions;
using IdentityModel.Client;
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
                services.AddSingleton<IUserRepository, MockUserRepository>();
                services.AddSingleton<IDeckRepository, MockDeckRepository>();
                services.AddSingleton<ICardRepository, MockCardRepository>();
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
        var result = await client.GetAsync($"/api/v1/expansion");

        // Assert
        result.Should().BeSuccessful();

    }

    [Fact]
    public async Task ShouldFetchByName() {
        // Arrange
        var expansion = "dev";
        var client = _factory.CreateClient();

        await Login(client, "admin", "password");

        // Act
        await client.PostAsync("/api/v1/card", JsonContent.Create(new ExpansionCard {
            Power = -1,
            Life = -1,
            DeckUsable = true,
            Name = "Dub",
            Cost = 2,
            Type = "Spell",
            Expansion = expansion,
            Text = "Caster becomes a Warrior. (Keeps all other types)",
            Script = "function _Create(props)\n" +
            "    local result = CardCreation:Spell(props)\n" +
            "    result.DamageValues.damage = 2\n" +
            "    result.EffectP:AddLayer(function(playerID, caster)\n" +
            "        caster.type = caster.type..\" Warrior\"\n" +
            "        caster:AddSubtype(\"Warrior\")\n" +
            "        return nil, true\n" +
            "    end)\n" +
            "    return result\n" +
            "end"
        }));
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