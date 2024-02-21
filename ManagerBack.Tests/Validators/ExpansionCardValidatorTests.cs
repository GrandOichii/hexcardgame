using FluentAssertions;
using HexCore.GameMatch;

namespace ManagerBack.Tests.Validators;

public class ExpansionCardValidatorTests {
    private readonly ExpansionCardValidator _validator;

    public ExpansionCardValidatorTests()
    {
        _validator = new();
    }

    public static IEnumerable<object[]> GoodCardList {
        // TODO add more
        get {
            yield return new object[] { new ExpansionCard {
                Power = -1,
                Life = -1,
                DeckUsable = true,
                Name = "Dub",
                Cost = 2,
                Type = "Spell",
                Expansion = "dev",
                Text = "Caster becomes a Warrior. (Keeps all other types)",
                Script = "function _Create(props)\n" +
                "    local result = CardCreation:Spell(props)\n" +
                "    result.DamageValues.damage = 2\n" +
                "    result.EffectP:AddLayer(function(playerID, caster)\n" +
                "        caster.type = caster.type..\" Warrior\"\n" +
                "        caster:AddSubtype(\"Warrior\")\n" +
                "        return nil, true\n" +
                "    end)\n" +
                "    return result\n" +
                "end"
            } };
        }
    }

    [Theory]
    [MemberData(nameof(GoodCardList))]
    public async Task ShouldValidate(ExpansionCard card) {
        // Act
        var act = () => _validator.Validate(card);

        // Assert
        await act.Should().NotThrowAsync();
    }

   public static IEnumerable<object[]> BadCardList {
        // TODO add more
        get {
            yield return new object[] { new ExpansionCard {
                Power = -1,
                Life = -1,
                DeckUsable = true,
                Name = "",
                Cost = 2,
                Type = "Spell",
                Expansion = "dev",
                Text = "Caster becomes a Warrior. (Keeps all other types)",
                Script = "function _Create(props)\n" +
                "    local result = CardCreation:Spell(props)\n" +
                "    result.DamageValues.damage = 2\n" +
                "    result.EffectP:AddLayer(function(playerID, caster)\n" +
                "        caster.type = caster.type..\" Warrior\"\n" +
                "        caster:AddSubtype(\"Warrior\")\n" +
                "        return nil, true\n" +
                "    end)\n" +
                "    return result\n" +
                "end"
            } };
        }
    }
    [Theory]
    [MemberData(nameof(BadCardList))]
    public async Task ShouldNotValidate(ExpansionCard card) {
        // Act
        var act = () => _validator.Validate(card);

        // Assert
        await act.Should().ThrowAsync<InvalidCardCreationParametersException>();
    }
}