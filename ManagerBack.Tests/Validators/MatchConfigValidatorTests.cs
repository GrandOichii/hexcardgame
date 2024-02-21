using FluentAssertions;
using HexCore.GameMatch;

namespace ManagerBack.Tests.Validators;

public class MatchConfigValidatorTests {
    private readonly MatchConfigValidator _validator;

    public MatchConfigValidatorTests()
    {
        _validator = new();
    }

    public static IEnumerable<object[]> GoodConfigList {
        // TODO add more
        get {
            yield return new object[] { new MatchConfig {
                SetupScript = "print('config')"
            } };
        }
    }

    [Theory]
    [MemberData(nameof(GoodConfigList))]
    public async Task ShouldValidate(MatchConfig config) {
        // Act
        var act = () => _validator.Validate(config);

        // Assert
        await act.Should().NotThrowAsync();
    }

   public static IEnumerable<object[]> BadConfigList {
        // TODO add more
        get {
            yield return new object[] { new MatchConfig {
                SetupScript = ""
            } };
        }
    }
    [Theory]
    [MemberData(nameof(BadConfigList))]
    public async Task ShouldNotValidate(MatchConfig config) {
        // Act
        var act = () => _validator.Validate(config);

        // Assert
        await act.Should().ThrowAsync<InvalidMatchConfigCreationParametersException>();
    }
}