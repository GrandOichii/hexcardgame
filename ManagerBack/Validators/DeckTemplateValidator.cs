namespace ManagerBack.Validators;


public class DeckTemplateValidator : IValidator<DeckTemplate>
{
    private readonly ICardRepository _cardRepo;
    private readonly IValidator<string> _cidValidator;

    public DeckTemplateValidator(ICardRepository cardRepo, IValidator<string> cidValidator)
    {
        _cardRepo = cardRepo;
        _cidValidator = cidValidator;
    }

    public async Task Validate(DeckTemplate deck)
    {
        // TODO repeated code 
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