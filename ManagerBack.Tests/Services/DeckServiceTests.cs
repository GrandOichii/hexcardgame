using AutoMapper;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace ManagerBack.Tests.Services;

public class DeckServiceTests {
    private readonly DeckService _deckService;
    private readonly IMapper _mapper;
    private readonly IDeckRepository _deckRepo;
    private readonly ICardRepository _cardRepo;

    public DeckServiceTests() {
        var mC = new MapperConfiguration(cfg => {
            cfg.AddProfile(new AutoMapperProfile());
        });
        _mapper = new Mapper(mC);
        _deckRepo = A.Fake<IDeckRepository>();
        _cardRepo = A.Fake<ICardRepository>();

        _deckService = new(_deckRepo, _mapper, _cardRepo, new DeckValidator(_cardRepo, new CIDValidator()));
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
        A.CallTo(() => _deckRepo.Add(deckModel)).DoesNothing();

        // Act
        var result = await _deckService.Create(userId, deck);

        // Assert
        result.Should().BeEquivalentTo(deckModel, opt => opt.Excluding(d => d.OwnerId));
        result.OwnerId.Should().Be(userId);
    }

    public static IEnumerable<object[]> BadDeckList {
        get {
            yield return new object[] { new PostDeckDto {
                Name = "",
                Description = "This is the deck's description"
            }, new InvalidDeckException() };
            yield return new object[] { new PostDeckDto {
                Name = "Deck1",
                Description = "This is the deck's description",
                Index = new() {
                    {"dev::InexistantCard", 1}
                }
            }, new CardNotFoundException("")};
            yield return new object[] { new PostDeckDto {
                Name = "Deck1",
                Description = "This is the deck's description",
                Index = new() {
                    {"dev::Dub", 0}
                }
            }, new InvalidDeckException()};
            yield return new object[] { new PostDeckDto {
                Name = "Deck1",
                Description = "This is the deck's description",
                Index = new() {
                    {"dev:Dub", 0}
                }
            }, new InvalidCIDException("")};
        }
    }

    [Theory]
    [MemberData(nameof(BadDeckList))]
    public async Task ShouldNotCreate(PostDeckDto deck, Exception e) {
        // Arrange
        var deckModel = _mapper.Map<DeckModel>(deck);
        var userId = "u1";
        CardModel? nullCard = null; 
        A.CallTo(() => _deckRepo.Add(deckModel)).Throws(e);
        A.CallTo(() => _cardRepo.ByCID(A<string>._)).Returns(nullCard);
        A.CallTo(() => _cardRepo.ByCID("dev::Dub")).Returns(A.Fake<CardModel>());

        // Act
        var act = () => _deckService.Create(userId, deck);

        // Assert
        await act.Should().ThrowAsync<Exception>();
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

        // Act
        var act = () => _deckService.Delete(userId, deckId);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ShouldNotDelete()
    {
        // Arrange
        var userId = "u1";
        var deckId = "d1";
        DeckModel? deck = null;
        A.CallTo(() => _deckRepo.ById(deckId)).Returns(deck);
        A.CallTo(() => _deckRepo.Delete(deckId)).Returns(0);

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

        // Act
        var act = () => _deckService.Update(userId, deckId, A.Fake<PostDeckDto>());

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ShouldNotUpdate()
    {
        // Arrange
        var userId = "u1";
        var deckId = "d1";
        DeckModel? deck = null;
        A.CallTo(() => _deckRepo.ById(deckId)).Returns(deck);
        A.CallTo(() => _deckRepo.Update(deckId, A<DeckModel>._)).Returns(0);

        // Act
        var act = () => _deckService.Update(userId, deckId, A.Fake<PostDeckDto>());

        // Assert
        await act.Should().ThrowAsync<DeckNotFoundException>();
    }

}