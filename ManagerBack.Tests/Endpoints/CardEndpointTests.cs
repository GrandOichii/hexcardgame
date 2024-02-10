using System.Net.Http.Json;
using FluentAssertions;
using IdentityModel.Client;
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
                services.AddSingleton<IUserRepository, MockUserRepository>();
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

    [Fact]
    public async Task ShouldCreate() {
        // Arrange
        var client = _factory.CreateClient();
        await Login(client, "admin", "password");

        // Act
        var result = await client.PostAsync("/api/v1/card/create", JsonContent.Create(new ExpansionCard {
            Power = -1,
            Life = -1,
            DeckUsable = true,
            Name = "Dub",
            Cost = 2,
            Type = "Spell",
            Expansion = "dev",
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

        // Assert
        result.Should().BeSuccessful();
    }

    // TODO add more test cases
    public static IEnumerable<object[]> BadCardList
    {
        get
        {
            yield return new object[] { new ExpansionCard {
                Power = -1,
                Life = -1,
                DeckUsable = true,
                Name = "",
                Cost = 2,
                Type = "Spell",
                Expansion = "dev",
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
            } };
        }
    }

    [Theory]
    [MemberData(nameof(BadCardList))]
    public async Task ShouldNotCreate(ExpansionCard card) {
        // Arrange
        var client = _factory.CreateClient();
        await Login(client, "admin", "password");

        // Act
        var result = await client.PostAsync("/api/v1/card/create", JsonContent.Create(card));

        // Assert
        result.Should().HaveClientError();
    }

    [Theory]
    [InlineData(false), InlineData(true)]
    public async Task ShouldNotCreateUnauthorized(bool login) {
        // Arrange
        var client = _factory.CreateClient();
        if (login)
            await Login(client, "user", "password");

        // Act
        var result = await client.PostAsync("/api/v1/card/create", JsonContent.Create(new ExpansionCard {
            Power = -1,
            Life = -1,
            DeckUsable = true,
            Name = "Dub",
            Cost = 2,
            Type = "Spell",
            Expansion = "dev",
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

        // Assert
        result.Should().HaveClientError();
    }

    // [Fact]
    // public async Task ShouldDelete(ExpansionCard card) {

    // }

}