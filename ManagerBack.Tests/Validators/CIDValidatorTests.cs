using FluentAssertions;
using HexCore.GameMatch;

namespace ManagerBack.Tests.Validators;

public class CIDValidatorTests {
    private readonly CIDValidator _validator;

    public CIDValidatorTests()
    {
        _validator = new();
    }

    public static IEnumerable<object[]> GoodCIDList {
        // TODO add more
        get {
            yield return new object[] { "dev::card" };
        }
    }

    [Theory]
    [MemberData(nameof(GoodCIDList))]
    public async Task ShouldValidate(string cid) {
        // Act
        var act = () => _validator.Validate(cid);

        // Assert
        await act.Should().NotThrowAsync();
    }

   public static IEnumerable<object[]> BadCCIDist {
        // TODO add more
        get {
            yield return new object[] { "card" };
            yield return new object[] { "dev" };
            yield return new object[] { "dev::" };
            yield return new object[] { "::card" };
            yield return new object[] { "dev:card" };
        }
    }
    [Theory]
    [MemberData(nameof(BadCCIDist))]
    public async Task ShouldNotValidate(string cid) {
        // Act
        var act = () => _validator.Validate(cid);

        // Assert
        await act.Should().ThrowAsync<InvalidCIDException>();
    }
}