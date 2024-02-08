using System.Text.RegularExpressions;
using AutoMapper;

namespace ManagerBack.Services;

[Serializable]
public class InvalidCIDException : Exception
{
    public InvalidCIDException(string cid) : base($"invalid CID {cid}") { }
}

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

    public CardService(IMapper mapper, ICardRepository cardRepo)
    {
        _cardRepo = cardRepo;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ExpansionCard>> All()
    {
        return (await _cardRepo.All()).Select(_mapper.Map<ExpansionCard>);
    }

    [GeneratedRegex("^.+::.+$")] private static partial Regex CIDPattern();
    public async Task<ExpansionCard> ByCID(string cid)
    {
        if (!CIDPattern().IsMatch(cid))
            throw new InvalidCIDException(cid);
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
        var existing = await _cardRepo.ByCID(card.CID);
        if (existing is not null)
            throw new CIDTakenException(card.CID);

        // TODO seems weird - we feed the expansion card to the repo, then we get the card model, then we turn that model back to the expansion card
        // i guess this is the right thing to do, although it seems very bulky
        await _cardRepo.Add(_mapper.Map<CardModel>(card));
        return card;
    }

    public async Task Delete(string cid)
    {
        var deletedCount = await _cardRepo.Delete(cid);
        if (deletedCount != 1) 
            throw new InvalidCIDException(cid);
    }

}