using FluentAssertions;
using ManagerBack.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace ManagerBack.Tests.Validators;

public class PostUserDtoValidatorTests {
    private readonly PostUserDtoValidator _validator;

    public PostUserDtoValidatorTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var conf = configuration.GetSection("UserValidation").Get<UserValidationSettings>()!;
        _validator = new(
            Options.Create(
                conf
            )
        );
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
            yield return new object[] { 
                new PostUserDto {
                    Username = "",
                    Password = "password"
                },
            };
            yield return new object[] { 
                new PostUserDto {
                    Username = "1username",
                    Password = "password"
                },
            };
            yield return new object[] { 
                new PostUserDto {
                    Username = "username.",
                    Password = "password"
                },
            };
            yield return new object[] { 
                new PostUserDto {
                    Username = "u",
                    Password = "password"
                },
            };
            
            yield return new object[] { 
                new PostUserDto {
                    Username = "username",
                    Password = "p"
                },
            };
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