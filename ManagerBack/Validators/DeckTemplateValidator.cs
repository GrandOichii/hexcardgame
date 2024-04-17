namespace ManagerBack.Validators;

[Serializable]
public class InvalidDeckException : Exception
{
    public InvalidDeckException() { }
    public InvalidDeckException(string message) : base(message) { }
}
public class DeckTemplateValidator : IValidator<DeckTemplate>
{
    private static readonly int MIN_DECK_NAME_SIZE = 3;
    private static readonly int MAX_DECK_NAME_SIZE = 20;

    private readonly ICardRepository _cardRepo;
    private readonly IValidator<string> _cidValidator;

    public DeckTemplateValidator(ICardRepository cardRepo, IValidator<string> cidValidator)
    {
        _cardRepo = cardRepo;
        _cidValidator = cidValidator;
    }

    public async Task Validate(DeckTemplate deck)
    {
        var name = deck.GetDescriptor("name");
        if (string.IsNullOrEmpty(name))
            throw new InvalidDeckException("deck name can't be empty");
        if (name.Length < MIN_DECK_NAME_SIZE)
            throw new InvalidDeckException($"deck name too short (min: {MIN_DECK_NAME_SIZE})");
        if (name.Length > MAX_DECK_NAME_SIZE)
            throw new InvalidDeckException($"deck name too long (min: {MAX_DECK_NAME_SIZE})");
            
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