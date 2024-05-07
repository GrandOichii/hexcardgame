using AutoMapper;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace ManagerBack.Tests.Services;

public class CardServiceTests {
    private readonly CardService _cardService;
    private readonly ICardRepository _cardRepo;
    private readonly IValidator<ExpansionCard> _validator;

    public CardServiceTests() {
        var mC = new MapperConfiguration(cfg => {
            cfg.AddProfile(new AutoMapperProfile());
        });
        var mapper = new Mapper(mC);
        _cardRepo = A.Fake<ICardRepository>();
        _validator = A.Fake<IValidator<ExpansionCard>>();

        _cardService = new(mapper, _cardRepo, new CIDValidator(), _validator);
    }

    [Fact]
    public async Task ShouldFetchByCID() {
        // Arrange
        var cid = "expansion::card";
        var card = A.Fake<CardModel>();
        A.CallTo(() => _cardRepo.ByCID(cid)).Returns(card);

        // Act
        var result = await _cardService.ByCID(cid);

        // Assert
        result.Should().Be(card);
    }

    [Fact]
    public async Task ShouldNotFetchByCIDCardNotFound() {
        // Arrange
        var cid = "expansion::card";
        CardModel? card = null;
        A.CallTo(() => _cardRepo.ByCID(cid)).Returns(card);

        // Act
        var act = () => _cardService.ByCID(cid);

        // Assert
        await act.Should().ThrowAsync<CardNotFoundException>();
    }

    [Fact]
    public async Task ShouldNotFetchByCIDInvalidCID() {
        // Arrange
        var cid = "expansion:card";

        // Act
        var act = () => _cardService.ByCID(cid);

        // Assert
        await act.Should().ThrowAsync<InvalidCIDException>();
    }

    [Fact]
    public async Task ShouldCreate() {
        // Arrange
        var card = A.Fake<CardModel>();
        CardModel? c = null;
        A.CallTo(() => _cardRepo.ByCID(card.Expansion + "::" + card.Name)).Returns(c);
        A.CallTo(() => _validator.Validate(card)).DoesNothing();
        A.CallTo(() => _cardRepo.Add(card)).DoesNothing();

        // Act
        var result = await _cardService.Add(card);

        // Assert
        result.Should().Be(card);
    }

    [Fact]
    public async Task ShouldNotCreate() {
        // Arrange
        var card = A.Fake<CardModel>();
        CardModel? c = null;
        A.CallTo(() => _cardRepo.ByCID(card.Expansion + "::" + card.Name)).Returns(c);
        A.CallTo(() => _validator.Validate(card)).Throws<InvalidCardCreationParametersException>();
        A.CallTo(() => _cardRepo.Add(card)).DoesNothing();

        // Act
        var act = () => _cardService.Add(card);

        // Assert
        await act.Should().ThrowAsync<InvalidCardCreationParametersException>();
    }

    [Fact]
    public async Task ShouldNotCreateCIDTaken() {
        // Arrange
        var card1 = A.Fake<CardModel>();
        var card2 = A.Fake<CardModel>();
        A.CallTo(() => _cardRepo.ByCID(card2.Expansion + "::" + card2.Name)).Returns(card1);

        // Act
        var act = () => _cardService.Add(card2);

        // Assert
        await act.Should().ThrowAsync<CIDTakenException>();
    }

    [Fact]
    public async Task ShouldUpdate() {
        // Arrange
        A.CallTo(() => _cardRepo.Update(A<CardModel>._)).Returns(1);

        // Act
        var act = () => _cardService.Update(A.Fake<CardModel>());

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ShouldNotUpdate() {
        // Arrange
        A.CallTo(() => _cardRepo.Update(A<CardModel>._)).Returns(0);

        // Act
        var act = () => _cardService.Update(A.Fake<CardModel>());

        // Assert
        await act.Should().ThrowAsync<CardNotFoundException>();
    }

    [Fact]
    public async Task ShouldDelete () {
        // Arrange
        var cid = "expansion::card";
        A.CallTo(() => _cardRepo.Delete(cid)).Returns(1);

        // Act
        var act = () => _cardService.Delete(cid);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ShouldNotDelete() {
        // Arrange
        var cid = "expansion::card";
        A.CallTo(() => _cardRepo.Delete(cid)).Returns(0);

        // Act
        var act = () => _cardService.Delete(cid);

        // Assert
        await act.Should().ThrowAsync<CardNotFoundException>();
    }

    [Fact]
    public async Task ShouldFetchByExpansion() {
        // Arrange
        var cards = A.Fake<IEnumerable<CardModel>>();
        var query = A.Fake<CardQuery>();
        A.CallTo(() => _cardRepo.Query(query)).Returns(cards);

        // Act
        var result = await _cardService.Query(query);

        // Assert
        result.Should().BeEquivalentTo(cards);
    }

    [Fact]
    public async Task ShouldFetchAllCIDs() {
        // Arrange
        var cards = A.Fake<IEnumerable<CardModel>>();
        A.CallTo(() => _cardRepo.All()).Returns(cards);
        var expected = cards.Select(c => c.GetCID());

        // Act
        var result = await _cardService.AllCIDs();

        // Assert
        result.Should().BeEquivalentTo(expected);
    }
}