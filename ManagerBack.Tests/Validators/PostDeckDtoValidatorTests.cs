using System.ComponentModel.DataAnnotations;
using FakeItEasy;
using FluentAssertions;
using HexCore.GameMatch;

namespace ManagerBack.Tests.Validators;

public class PostDeckDtoValidatorTests {
    private readonly PostDeckDtoValidator _validator;
    private readonly ICardRepository _cardRepo;

    public PostDeckDtoValidatorTests()
    {
        _cardRepo = A.Fake<ICardRepository>();
        var cidValidator = A.Fake<IValidator<string>>();
        A.CallTo(() => cidValidator.Validate(A<string>._)).DoesNothing();
        _validator = new(_cardRepo, cidValidator);
    }

    public static IEnumerable<object[]> GoodDeckList {
        // TODO add more
        get {
            yield return new object[] { new PostDeckDto {
                Name = "deck name",
                Description = "deck description"
            } };
        }
    }

    [Theory]
    [MemberData(nameof(GoodDeckList))]
    public async Task ShouldValidate(PostDeckDto deck) {
        // Act
        var act = () => _validator.Validate(deck);

        // Assert
        await act.Should().NotThrowAsync();
    }

   public static IEnumerable<object[]> BadDeckList {
        // TODO add more
        get {
            yield return new object[] { new PostDeckDto {
                Name = "",
                Description = "This is the deck's description"
            } };
            yield return new object[] { new PostDeckDto {
                Name = "Deck1",
                Description = "This is the deck's description",
                Index = new() {
                    {"dev::InexistantCard", 1}
                }
            } };
            yield return new object[] { new PostDeckDto {
                Name = "Deck1",
                Description = "This is the deck's description",
                Index = new() {
                    {"dev::Dub", 0}
                }
            } };
            yield return new object[] { new PostDeckDto {
                Name = "Deck1",
                Description = "This is the deck's description",
                Index = new() {
                    {"dev:Dub", 0}
                }
            } };

        }
    }
    [Theory]
    [MemberData(nameof(BadDeckList))]
    public async Task ShouldNotValidate(PostDeckDto deck) {
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