using Microsoft.Extensions.Options;

namespace ManagerBack.Validators;


/// <summary>
/// Exception that is thrown when attempting to create/use an invalid deck
/// </summary>
[Serializable]
public class InvalidDeckException : Exception
{
    public InvalidDeckException() { }
    public InvalidDeckException(string message) : base(message) { }
}

/// <summary>
/// Deck template validator
/// </summary>
public class DeckTemplateValidator : IValidator<DeckTemplate>
{
    /// <summary>
    /// Deck restrictions
    /// </summary>
    private readonly IOptions<DeckRestrictionSettings> _restrictions;

    /// <summary>
    /// Card repository
    /// </summary>
    private readonly ICardRepository _cardRepo;

    /// <summary>
    /// Card ID validator
    /// </summary>
    private readonly IValidator<string> _cidValidator;

    public DeckTemplateValidator(ICardRepository cardRepo, IValidator<string> cidValidator, IOptions<DeckRestrictionSettings> restrictions)
    {
        _cardRepo = cardRepo;
        _cidValidator = cidValidator;
        _restrictions = restrictions;
    }

    public async Task Validate(DeckTemplate deck)
    {
        var name = deck.GetDescriptor("name");
        if (string.IsNullOrEmpty(name))
            throw new InvalidDeckException("deck name can't be empty");
        if (name.Length < _restrictions.Value.Name.MinLength)
            throw new InvalidDeckException($"deck name too short (min: {_restrictions.Value.Name.MinLength})");
        if (name.Length > _restrictions.Value.Name.MaxLength)
            throw new InvalidDeckException($"deck name too long (min: {_restrictions.Value.Name.MaxLength})");
            
        foreach (var pair in deck.Index) {
            var cid = pair.Key;
            var amount = pair.Value;

            await _cidValidator.Validate(cid);
            _ = await _cardRepo.ByCID(cid) ?? throw new CardNotFoundException($"card with cid {cid} not found");
            if (amount <= 0)
                throw new InvalidDeckException($"amount for card {cid} can't be {amount}");
            
        }
    }
}