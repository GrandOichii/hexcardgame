using System.ComponentModel.DataAnnotations;
using FakeItEasy;
using FluentAssertions;
using HexCore.Decks;
using HexCore.GameMatch;

namespace ManagerBack.Tests.Validators;

public class DeckTemplateValidatorTests {
    private readonly DeckTemplateValidator _validator;
    private readonly ICardRepository _cardRepo;

    public DeckTemplateValidatorTests()
    {
        _cardRepo = A.Fake<ICardRepository>();
        var cidValidator = A.Fake<IValidator<string>>();
        A.CallTo(() => cidValidator.Validate(A<string>._)).DoesNothing();
        _validator = new(_cardRepo, cidValidator);
    }

    public static IEnumerable<object[]> GoodDeckList {
        // TODO add more
        get {
            yield return new object[] { new DeckTemplate {
                Descriptors = new() {
                    { "name", "deck name" },
                    { "description",  "deck description" },
                },
                Index = new()
            } };
        }
    }

    [Theory]
    [MemberData(nameof(GoodDeckList))]
    public async Task ShouldValidate(DeckTemplate deck) {
        // Act
        var act = () => _validator.Validate(deck);

        // Assert
        await act.Should().NotThrowAsync();
    }

   public static IEnumerable<object[]> BadDeckList {
        // TODO add more
        get {
            yield return new object[] { new DeckTemplate {
                Descriptors = new() {
                    { "name", "" },
                    { "description",  "This is the deck's description" },
                },
                Index = new()
            } };
            yield return new object[] { new DeckTemplate {
                Descriptors = new() {
                    { "name", "Deck1" },
                    { "description",  "This is the deck's description" },
                },
                Index = new() {
                    {"dev::InexistantCard", 1}
                }
            } };
            yield return new object[] { new DeckTemplate {
                Descriptors = new() {
                    { "name", "Deck1" },
                    { "description",  "This is the deck's description" },
                },

                Index = new() {
                    {"dev::Dub", 0}
                }
            } };
            yield return new object[] { new DeckTemplate {
                Descriptors = new() {
                    { "name", "Deck1" },
                    { "description",  "This is the deck's description" },
                },
                Index = new() {
                    {"dev:Dub", 0}
                }
            } };
        }
    }
    [Theory]
    [MemberData(nameof(BadDeckList))]
    public async Task ShouldNotValidate(DeckTemplate deck) {
        // Arrange
        A.CallTo(() => _cardRepo.ByCID(A<string>._)).Returns(A.Fake<CardModel>());
        CardModel? nullCard = null;
        A.CallTo(() => _cardRepo.ByCID("dev::InexistantCard")).Returns(nullCard);

        // Act
        var act = () => _validator.Validate(deck);

        // Assert
        // TODO doesn't differentiate between different types of exceptions - InvalidDeckException and CardNotFoundException
        await act.Should().ThrowAsync<Exception>();
    }
}