namespace ManagerBack.Validators;

[Serializable]
public class InvalidDeckException : Exception
{
    public InvalidDeckException() { }
    public InvalidDeckException(string message) : base(message) { }
}

public class DeckValidator : IValidator<PostDeckDto>
{
    private readonly ICardRepository _cardRepo;
    private readonly IValidator<string> _cidValidator;

    public DeckValidator(ICardRepository cardRepo, IValidator<string> cidValidator)
    {
        _cardRepo = cardRepo;
        _cidValidator = cidValidator;
    }

    public async Task Validate(PostDeckDto deck)
    {
        if (string.IsNullOrEmpty(deck.Name))
            throw new InvalidDeckException("deck name is empty");
            foreach (var pair in deck.Index) {
                var cid = pair.Key;
                var amount = pair.Value;

                await _cidValidator.Validate(cid);
                var _ = await _cardRepo.ByCID(cid) ?? throw new CardNotFoundException($"card with cid {cid} not found");
                if (amount <= 0)
                    throw new InvalidDeckException($"amount for card {cid} can't be {amount}");
            
        }    
    }
}