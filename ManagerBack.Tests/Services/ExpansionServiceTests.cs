using AutoMapper;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace ManagerBack.Tests.Services;

public class ExpansionServiceTests {
    private readonly ExpansionService _expansionService;
    private readonly ICardRepository _cardRepo;

    public ExpansionServiceTests() {
        _cardRepo = A.Fake<ICardRepository>();

        _expansionService = new(_cardRepo);
    }

    [Fact]
    public async Task ShouldFetchAllEmpty() {
        // Arrange
        A.CallTo(() => _cardRepo.All()).Returns(new List<CardModel>());

        // Act
        var result = await _expansionService.All();

        // Assert
        result.Count().Should().Be(0);
    }

    [Fact]
    public async Task ShouldFetchAll() {
        // Arrange
        A.CallTo(() => _cardRepo.All()).Returns(new List<CardModel>() { new() {
            Power = -1,
            Life = -1,
            DeckUsable = true,
            Name = "Card",
            Cost = 2,
            Type = "Spell",
            Text = "",
            Script = "",
            Expansion = "expansion"
        } });

        // Act
        var result = await _expansionService.All();

        // Assert
        result.Count().Should().Be(1);
    }

    [Fact]
    public async Task ShouldFetchByName() {
        var expansion = "expansion";
        A.CallTo(() => _cardRepo.All()).Returns(new List<CardModel>() { 
            new() {
                Power = -1,
                Life = -1,
                DeckUsable = true,
                Name = "Card",
                Cost = 2,
                Type = "Spell",
                Text = "",
                Script = "",
                Expansion = expansion
            },
            new() {
                Power = -1,
                Life = -1,
                DeckUsable = true,
                Name = "Card",
                Cost = 2,
                Type = "Spell",
                Text = "",
                Script = "",

                Expansion = "expansion2"
            },
            new() {
                Power = -1,
                Life = -1,
                DeckUsable = true,
                Name = "Card",
                Cost = 2,
                Type = "Spell",
                Text = "",
                Script = "",

                Expansion = expansion
            },
        });


        // Act
        var result = await _expansionService.ByName(expansion);

        // Assert
        result.Name.Should().Be(expansion);
        result.CardCount.Should().Be(2);
    }

    [Fact]
    public async Task ShouldFailFetchByName() {
        // Arrange
        A.CallTo(() => _cardRepo.All()).Returns(new List<CardModel>() { 
            new() {
                Power = -1,
                Life = -1,
                DeckUsable = true,
                Name = "Card",
                Cost = 2,
                Type = "Spell",
                Text = "",
                Script = "",
                Expansion = "expansion1"
            },
            new() {
                Power = -1,
                Life = -1,
                DeckUsable = true,
                Name = "Card",
                Cost = 2,
                Type = "Spell",
                Text = "",
                Script = "",
                Expansion = "expansion2"
            },
        });

        // Act
        var act = () => _expansionService.ByName("expansion3");

        // Assert
        await act.Should().ThrowAsync<ExpansionNotFoundException>();
    }

}