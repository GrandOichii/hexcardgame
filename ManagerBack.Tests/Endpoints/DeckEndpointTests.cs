using System.Net.Http.Json;
using FluentAssertions;
using IdentityModel.Client;
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
        await Login(client, "user", "password");

        // Act
        var result = await client.GetAsync("/api/v1/deck");

        // Assert
        result.Should().BeSuccessful();
    }

    [Fact]
    public async Task ShouldCreateEmpty() {
        // Arrange
        var client = _factory.CreateClient();
        await Login(client, "user", "password");

        // Act
        var result = await client.PostAsync("/api/v1/deck", JsonContent.Create(new PostDeckDto{
            Name = "deck1",
            Description = "This is the deck's description."
        }));

        // Assert
        result.Should().BeSuccessful();
        (await result.Content.ReadFromJsonAsync<DeckModel>()).Should().NotBeNull();
    }

    [Fact]
    public async Task ShouldCreate() {
        // Arrange
        var client = _factory.CreateClient();
        var c1 = new ExpansionCard {
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
        };
        var c2 = new ExpansionCard {
            Power = 3,
            Life = 3,
            DeckUsable = true,
            Name = "Elven Outcast",
            Cost = 4,
            Type = "Unit - Rogue",
            Expansion = "dev",
            Text = "When [CARDNAME] enters play, it deals 1 damage to itself.",
            Script = "function _Create(props)\n" +
            "    local result = CardCreation:Unit(props)\n" +
            "    result:AddSubtype(\"Rogue\")\n" +
            "\n" +
            "    result.OnEnterP:AddLayer(function (playerID, tile)\n" +
            "        DealDamage(result.id, {tile.iPos, tile.jPos}, 1)\n" +
            "        return nil, true\n" +
            "    end)\n" +
            "\n" +
            "    return result\n" +
            "end"
        };
        await Login(client, "admin", "password");

        // Act
        await client.PostAsync("/api/v1/card", JsonContent.Create(c1));
        await client.PostAsync("/api/v1/card", JsonContent.Create(c2));
        var result = await client.PostAsync("/api/v1/deck", JsonContent.Create(new PostDeckDto{
            Name = "deck1",
            Description = "This is the deck's description.",
            Index = new() {
                {c1.Expansion + "::" + c1.Name, 1},
                {c2.Expansion + "::" + c2.Name, 3}
            }
        }));

        // Assert
        result.Should().BeSuccessful();

    }

    [Fact]
    public async Task ShouldNotCreate() {
        // Arrange
        var deck = new PostDeckDto {
            Name = "",
            Description = "This is the deck's description"
        };
        var client = _factory.CreateClient();
        await Login(client, "user", "password");
        var c1 = new ExpansionCard {
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
        };
        var c2 = new ExpansionCard {
            Power = 3,
            Life = 3,
            DeckUsable = true,
            Name = "Elven Outcast",
            Cost = 4,
            Type = "Unit - Rogue",
            Expansion = "dev",
            Text = "When [CARDNAME] enters play, it deals 1 damage to itself.",
            Script = "function _Create(props)\n" +
            "    local result = CardCreation:Unit(props)\n" +
            "    result:AddSubtype(\"Rogue\")\n" +
            "\n" +
            "    result.OnEnterP:AddLayer(function (playerID, tile)\n" +
            "        DealDamage(result.id, {tile.iPos, tile.jPos}, 1)\n" +
            "        return nil, true\n" +
            "    end)\n" +
            "\n" +
            "    return result\n" +
            "end"
        };
        var c3 = new ExpansionCard {
            Power = -1,
            Life = -1,
            DeckUsable = true,
            Name = "Knowledge Tower",
            Cost = 6,
            Type = "Structure",
            Expansion = "starters",
            Text = "At the start of yout turn, draw a card.",
            Script = "function _Create(props)\n" +
            "    local result = CardCreation:Structure(props)\n" +
            "    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()\n" +
            "        :Check(Common:IsOwnersTurn(result))\n" +
            "        :Cost(Common:NoCost())\n" +
            "        :IsSilent(false)\n" +
            "        :On(TRIGGERS.TURN_START)\n" +
            "        :Zone(ZONES.PLACED)\n" +
            "        :Effect(function (playerID, args)\n" +
            "            DrawCards(playerID, 1)\n" +
            "        end)\n" +
            "        :Build()\n" +
            "    return result\n" +
            "end"
        };

        // Act
        await client.PostAsync("/api/v1/card", JsonContent.Create(c1));
        await client.PostAsync("/api/v1/card", JsonContent.Create(c2));
        await client.PostAsync("/api/v1/card", JsonContent.Create(c3));
        var result = await client.PostAsync("/api/v1/deck", JsonContent.Create(deck));

        // Assert
        result.Should().HaveClientError();
    }

    [Fact]
    public async Task ShouldDelete() {
        // Arrange
        var client = _factory.CreateClient();
        await Login(client, "user", "password");

        // Act
        var create = await client.PostAsync("/api/v1/deck", JsonContent.Create(new PostDeckDto {
            Name = "deck1",
            Description = "This is the deck's description."
        }));

        var deckId = (await create.Content.ReadFromJsonAsync<DeckModel>())!.Id;
        var result = await client.DeleteAsync($"/api/v1/deck/{deckId}");

        // Assert
        result.Should().BeSuccessful();
    }

    [Fact]
    public async Task ShouldNotDelete() {
        // Arrange
        var client = _factory.CreateClient();
        await Login(client, "user", "password");

        // Act
        var result = await client.DeleteAsync("/api/v1/deck/deck_id_here");

        // Assert
        result.Should().HaveClientError();
    }

    [Fact]
    public async Task ShouldUpdate() {
        // Arrange
        var client = _factory.CreateClient();
        await Login(client, "admin", "password");
        const string newName = "Deck";
        const string newDescription = "Upadted description";
        var c1 = new ExpansionCard {
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
        };
        var c2 = new ExpansionCard {
            Power = 3,
            Life = 3,
            DeckUsable = true,
            Name = "Elven Outcast",
            Cost = 4,
            Type = "Unit - Rogue",
            Expansion = "dev",
            Text = "When [CARDNAME] enters play, it deals 1 damage to itself.",
            Script = "function _Create(props)\n" +
            "    local result = CardCreation:Unit(props)\n" +
            "    result:AddSubtype(\"Rogue\")\n" +
            "\n" +
            "    result.OnEnterP:AddLayer(function (playerID, tile)\n" +
            "        DealDamage(result.id, {tile.iPos, tile.jPos}, 1)\n" +
            "        return nil, true\n" +
            "    end)\n" +
            "\n" +
            "    return result\n" +
            "end"
        };
        await client.PostAsync("/api/v1/card", JsonContent.Create(c1));
        await client.PostAsync("/api/v1/card", JsonContent.Create(c2));
        var create = await client.PostAsync("/api/v1/deck", JsonContent.Create(new PostDeckDto
        {
            Name = "deck1",
            Description = "This is the deck's description.",
            Index = new() {
                {c1.Expansion + "::" + c1.Name, 2},
                {c2.Expansion + "::" + c2.Name, 4},
            }
        }));
        var deckId = (await create.Content.ReadFromJsonAsync<DeckModel>())!.Id;

        // Act
        var result = await client.PutAsync($"/api/v1/deck/{deckId}", JsonContent.Create(new PostDeckDto {
            Name = newName,
            Description = newDescription,
            Index = new() {
                {c1.Expansion + "::" + c1.Name, 10},
            }
        }));

        // Assert
        result.Should().BeSuccessful();
        var updatedDeck = await result.Content.ReadFromJsonAsync<DeckModel>();
        updatedDeck.Should().NotBeNull();
        updatedDeck!.Index.Count.Should().Be(1);
    }

    [Fact]
    public async Task ShouldUpdateEmpty() {
        // Arrange
        var client = _factory.CreateClient();
        await Login(client, "user", "password");
        const string newName = "Deck";
        const string newDescription = "Upadted description";
        var create = await client.PostAsync("/api/v1/deck", JsonContent.Create(new PostDeckDto
        {
            Name = "deck1",
            Description = "This is the deck's description."
        }));
        var deckId = (await create.Content.ReadFromJsonAsync<DeckModel>())!.Id;

        // Act
        var result = await client.PutAsync($"/api/v1/deck/{deckId}", JsonContent.Create(new PostDeckDto {
            Name = newName,
            Description = newDescription
        }));

        // Assert
        result.Should().BeSuccessful();
        var updatedDeck = await result.Content.ReadFromJsonAsync<DeckModel>();
        updatedDeck.Should().NotBeNull();
        updatedDeck!.Name.Should().Be(newName);
        updatedDeck.Description.Should().Be(newDescription);
    }

    [Fact]
    public async Task ShouldNotUpdate() {
        // Arrange
        var client = _factory.CreateClient();
        await Login(client, "user", "password");
        // Act
        var result = await client.PutAsync($"/api/v1/deck/deck_id_here", JsonContent.Create(new PostDeckDto {
            Name = "New name",
            Description = "New description"
        }));

        // Assert
        result.Should().HaveClientError();
    }
}