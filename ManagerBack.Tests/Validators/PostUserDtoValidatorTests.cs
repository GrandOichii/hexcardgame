using FluentAssertions;
using HexCore.GameMatch;

namespace ManagerBack.Tests.Validators;

public class PostUserDtoValidatorTests {
    private readonly PostUserDtoValidator _validator;

    public PostUserDtoValidatorTests()
    {
        _validator = new();
    }

    public static IEnumerable<object[]> GoodUserList {
        // TODO add more
        get {
            yield return new object[] { new PostUserDto {
                Username = "user1",
                Password = "password"
            } };
        }
    }

    [Theory]
    [MemberData(nameof(GoodUserList))]
    public async Task ShouldValidate(PostUserDto user) {
        // Act
        var act = () => _validator.Validate(user);

        // Assert
        await act.Should().NotThrowAsync();
    }

   public static IEnumerable<object[]> BadUserList {
        // TODO add more
        get {
            yield return new object[] { new PostUserDto {
                Username = "",
                Password = "password"
            } };
        }
    }
    [Theory]
    [MemberData(nameof(BadUserList))]
    public async Task ShouldNotValidate(PostUserDto user) {
        // Act
        var act = () => _validator.Validate(user);

        // Assert
        await act.Should().ThrowAsync<InvalidRegisterCredentialsException>();
    }
}