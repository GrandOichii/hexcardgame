using System.Text.RegularExpressions;
using AutoMapper;

namespace ManagerBack.Services;


[Serializable]
public class CardNotFoundException : Exception
{
    public CardNotFoundException(string message) : base(message) {}
}

[Serializable]
public class CIDTakenException : Exception
{
    public CIDTakenException(string cid) : base($"cid {cid} is already taken") { }
}

public partial class CardService : ICardService
{
    private readonly IMapper _mapper;
    private readonly ICardRepository _cardRepo;
    private readonly IValidator<string> _cidValidator;
    private readonly IValidator<ExpansionCard> _cardValidator;

    public CardService(IMapper mapper, ICardRepository cardRepo, IValidator<string> cidValidator, IValidator<ExpansionCard> cardValidator)
    {
        _cardRepo = cardRepo;
        _mapper = mapper;
        _cidValidator = cidValidator;
        _cardValidator = cardValidator;
    }

    public async Task<IEnumerable<ExpansionCard>> All()
    {
        return (await _cardRepo.All()).Select(_mapper.Map<ExpansionCard>);
    }

    public async Task<ExpansionCard> ByCID(string cid)
    {
        await _cidValidator.Validate(cid);
        var result = await _cardRepo.ByCID(cid) 
            ?? throw new CardNotFoundException($"no card with CID {cid}");

        return result;
    }

    // TODO make more complex querying
    public async Task<IEnumerable<ExpansionCard>> ByExpansion(string expansion)
    {
        var cards = await _cardRepo.Filter(c => c.Expansion == expansion);
        return cards.Select(_mapper.Map<ExpansionCard>);
    }

    public async Task<ExpansionCard> Create(ExpansionCard card)
    {
        var existing = await _cardRepo.ByCID(card.GetCID());
        if (existing is not null)
            throw new CIDTakenException(card.GetCID());

        await _cardValidator.Validate(card);

        await _cardRepo.Add(_mapper.Map<CardModel>(card));
        return card;
    }

    public async Task Delete(string cid)
    {
        var deletedCount = await _cardRepo.Delete(cid);
        if (deletedCount != 1) 
            throw new CardNotFoundException(cid);
    }

    public async Task Update(ExpansionCard card)
    {
        await _cardValidator.Validate(card);

        var count = await _cardRepo.Update(_mapper.Map<CardModel>(card));
        if (count == 0) 
            throw new CardNotFoundException("no card with cid " + card.GetCID());
        
    }
}