using AutoMapper;
using FakeItEasy;
using FluentAssertions;
using HexCore.Decks;
using Microsoft.AspNetCore.Mvc;

namespace ManagerBack.Tests.Services;

public class DeckServiceTests {
    private readonly DeckService _deckService;
    private readonly IMapper _mapper;
    private readonly IDeckRepository _deckRepo;
    private readonly ICardRepository _cardRepo;
    private readonly IValidator<DeckTemplate> _validator;
    private readonly IUserRepository _userRepo;

    public DeckServiceTests() {
        var mC = new MapperConfiguration(cfg => {
            cfg.AddProfile(new AutoMapperProfile());
        });
        _mapper = new Mapper(mC);
        _deckRepo = A.Fake<IDeckRepository>();
        _cardRepo = A.Fake<ICardRepository>();
        _validator = A.Fake<IValidator<DeckTemplate>>();
        _userRepo = A.Fake<IUserRepository>();

        _deckService = new(_deckRepo, _mapper, _validator, _userRepo);
    }

    [Fact]
    public async Task ShouldFetchAll() {
        // Arrange
        var userId = "u1";
        var decks = A.Fake<IEnumerable<DeckModel>>();
        A.CallTo(() => _deckRepo.Filter(d => d.OwnerId == userId)).Returns(decks);

        // Act
        var result = await _deckService.All(userId);

        // Assert
        result.Should().Equal(decks);
    }

    [Fact]
    public async Task ShouldCreate() {
        // Arrange
        var deck = new PostDeckDto {
            Name = "Deck",
            Description = ""
        };
        var deckModel = _mapper.Map<DeckModel>(deck);
        var userId = "u1";
        A.CallTo(() => _userRepo.CheckId(userId)).Returns(true);
        A.CallTo(() => _deckRepo.Add(deckModel)).DoesNothing();

        // Act
        var result = await _deckService.Add(userId, deck);

        // Assert
        result.Should().BeEquivalentTo(deckModel, opt => opt.Excluding(d => d.OwnerId));
        result.OwnerId.Should().Be(userId);
    }

    [Fact]
    public async Task ShouldNotCreateInvalidDeck() {
        // Arrange
        var deck = A.Fake<PostDeckDto>();
        var deckModel = _mapper.Map<DeckModel>(deck);
        var userId = "u1";
        // CardModel? nullCard = null; 
        A.CallTo(() => _validator.Validate(A<DeckTemplate>._)).Throws<InvalidDeckException>();
        A.CallTo(() => _userRepo.CheckId(userId)).Returns(true);
        // A.CallTo(() => _cardRepo.ByCID(A<string>._)).Returns(nullCard);
        // A.CallTo(() => _cardRepo.ByCID("dev::Dub")).Returns(A.Fake<CardModel>());

        // Act
        var act = () => _deckService.Add(userId, deck);

        // Assert
        await act.Should().ThrowAsync<InvalidDeckException>();
    }


    [Fact]
    public async Task ShouldNotCreateInvalidCID() {
        // Arrange
        var deck = A.Fake<PostDeckDto>();
        var deckModel = _mapper.Map<DeckModel>(deck);
        var userId = "u1";
        // CardModel? nullCard = null; 
        A.CallTo(() => _validator.Validate(A<DeckTemplate>._)).Throws(new InvalidCIDException(""));
        A.CallTo(() => _userRepo.CheckId(userId)).Returns(true);
        // A.CallTo(() => _cardRepo.ByCID(A<string>._)).Returns(nullCard);
        // A.CallTo(() => _cardRepo.ByCID("dev::Dub")).Returns(A.Fake<CardModel>());

        // Act
        var act = () => _deckService.Add(userId, deck);

        // Assert
        await act.Should().ThrowAsync<InvalidCIDException>();
    }

    [Fact]
    public async Task ShouldNotCreateCardNotFound() {
        // Arrange
        var deck = A.Fake<PostDeckDto>();
        var deckModel = _mapper.Map<DeckModel>(deck);
        var userId = "u1";
        // CardModel? nullCard = null; 
        A.CallTo(() => _validator.Validate(A<DeckTemplate>._)).Throws(new CardNotFoundException(""));
        A.CallTo(() => _userRepo.CheckId(userId)).Returns(true);
        // A.CallTo(() => _cardRepo.ByCID(A<string>._)).Returns(nullCard);
        // A.CallTo(() => _cardRepo.ByCID("dev::Dub")).Returns(A.Fake<CardModel>());

        // Act
        var act = () => _deckService.Add(userId, deck);

        // Assert
        await act.Should().ThrowAsync<CardNotFoundException>();
    }

    [Fact]
    public async Task ShouldNotCreateInvalidUser() {
        var deck = new PostDeckDto {
            Name = "Deck",
            Description = ""
        };
        var deckModel = _mapper.Map<DeckModel>(deck);
        var userId = "u1";
        A.CallTo(() => _userRepo.CheckId(userId)).Returns(false);
        A.CallTo(() => _deckRepo.Add(deckModel)).DoesNothing();

        // Act
        var act = () => _deckService.Add(userId, deck);

        // Assert
        await act.Should().ThrowAsync<UserNotFoundException>();
    }

    [Fact]
    public async Task ShouldDelete() {
        // Arrange
        var userId = "u1";
        var deckId = "d1";
        var deck = new DeckModel {
            Name = "Deck",
            Description = "deck description",
            OwnerId = userId
        };
        A.CallTo(() => _deckRepo.ById(deckId)).Returns(deck);
        A.CallTo(() => _deckRepo.Delete(deckId)).Returns(1);
        A.CallTo(() => _userRepo.CheckId(userId)).Returns(true);

        // Act
        var act = () => _deckService.Delete(userId, deckId);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ShouldNotDeleteInvalidUserId() {
        // Arrange
        var userId = "u1";
        var deckId = "d1";
        var deck = new DeckModel {
            Name = "Deck",
            Description = "deck description",
            OwnerId = userId
        };
        A.CallTo(() => _deckRepo.ById(deckId)).Returns(deck);
        A.CallTo(() => _deckRepo.Delete(deckId)).Returns(1);
        A.CallTo(() => _userRepo.CheckId(userId)).Returns(false);

        // Act
        var act = () => _deckService.Delete(userId, deckId);

        // Assert
        await act.Should().ThrowAsync<UserNotFoundException>();
    }

    [Fact]
    public async Task ShouldNotDeleteDeckNotFound()
    {
        // Arrange
        var userId = "u1";
        var deckId = "d1";
        DeckModel? deck = null;
        A.CallTo(() => _deckRepo.ById(deckId)).Returns(deck);
        A.CallTo(() => _deckRepo.Delete(deckId)).Returns(0);
        A.CallTo(() => _userRepo.CheckId(userId)).Returns(true);

        // Act
        var act = () => _deckService.Delete(userId, deckId);

        // Assert
        await act.Should().ThrowAsync<DeckNotFoundException>();
    }

    [Fact]
    public async Task ShouldUpdate()
    {
        // Arrange
        var userId = "u1";
        var deckId = "d1";
        var deck = new DeckModel {
            Id = deckId,
            Name = "Deck",
            Description = "deck description",
            OwnerId = userId
        };
        A.CallTo(() => _deckRepo.ById(deckId)).Returns(deck);
        A.CallTo(() => _deckRepo.Update(deckId, A<DeckModel>._)).Returns(1);
        A.CallTo(() => _userRepo.CheckId(userId)).Returns(true);

        // Act
        var act = () => _deckService.Update(userId, deckId, A.Fake<PostDeckDto>());

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ShouldNotUpdateInvalidUserId()
    {
        // Arrange
        var userId = "u1";
        var deckId = "d1";
        var deck = new DeckModel {
            Id = deckId,
            Name = "Deck",
            Description = "deck description",
            OwnerId = userId
        };
        A.CallTo(() => _deckRepo.ById(deckId)).Returns(deck);
        A.CallTo(() => _deckRepo.Update(deckId, A<DeckModel>._)).Returns(1);
        A.CallTo(() => _userRepo.CheckId(userId)).Returns(false);

        // Act
        var act = () => _deckService.Update(userId, deckId, A.Fake<PostDeckDto>());

        // Assert
        await act.Should().ThrowAsync<UserNotFoundException>();
    }

    [Fact]
    public async Task ShouldNotUpdateDeckNotFound()
    {
        // Arrange
        var userId = "u1";
        var deckId = "d1";
        DeckModel? deck = null;
        A.CallTo(() => _deckRepo.ById(deckId)).Returns(deck);
        A.CallTo(() => _deckRepo.Update(deckId, A<DeckModel>._)).Returns(0);
        A.CallTo(() => _userRepo.CheckId(userId)).Returns(true);

        // Act
        var act = () => _deckService.Update(userId, deckId, A.Fake<PostDeckDto>());

        // Assert
        await act.Should().ThrowAsync<DeckNotFoundException>();
    }

}