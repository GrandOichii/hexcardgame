using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;
using ManagerBack.Dtos;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace ManagerBack.Services;

// TODO limit the amount of decks a player can have
// TODO? does the userId need to be checked?

[Serializable]
public class DeckNotFoundException : Exception
{
    public DeckNotFoundException() { }
    public DeckNotFoundException(string message) : base(message) { }
}

[Serializable]
public class UnmatchedUserIdException : Exception
{
    public UnmatchedUserIdException() { }
}

[Serializable]
public class DeckDeletionException : Exception
{
    public DeckDeletionException() { }
    public DeckDeletionException(string message) : base(message) { }
}

[Serializable]
public class DeckUpdateException : Exception
{
    public DeckUpdateException() { }
    public DeckUpdateException(string message) : base(message) { }
}

[Serializable]
public class InvalidDeckException : Exception
{
    public InvalidDeckException() { }
    public InvalidDeckException(string message) : base(message) { }
}

public partial class DeckService : IDeckService
{
    private readonly IMapper _mapper;
    private readonly IDeckRepository _deckRepo;
    private readonly ICardRepository _cardRepo;


    public DeckService(IDeckRepository deckRepo, IMapper mapper, ICardRepository cardRepo)
    {
        _deckRepo = deckRepo;
        _mapper = mapper;
        _cardRepo = cardRepo;
    }

    [GeneratedRegex("^.+::.+$")] private static partial Regex CIDPattern();
    public async Task Validate(PostDeckDto deck) {
        // TODO add more checks
        if (string.IsNullOrEmpty(deck.Name))
            throw new InvalidDeckException("deck name is empty");
        foreach (var pair in deck.Index) {
            var cid = pair.Key;
            var amount = pair.Value;

            if (!CIDPattern().IsMatch(cid)) throw new InvalidCIDException(cid);
            var _ = await _cardRepo.ByCID(cid) ?? throw new CardNotFoundException($"card with cid {cid} not found");
            if (amount <= 0)
                throw new InvalidDeckException($"amount for card {cid} can't be {amount}");
            
        }
    }

    public async Task<IEnumerable<DeckModel>> All(string userId)
    {
        var decks = await _deckRepo.Filter(d => d.OwnerId == userId);
        return decks;
    }

    public async Task<DeckModel> Create(string userId, PostDeckDto deck)
    {
        var newDeck = _mapper.Map<DeckModel>(deck);
        newDeck.OwnerId = userId;
        
        await Validate(deck);

        await _deckRepo.Add(newDeck);
        return newDeck;
    }

    public async Task Delete(string userId, string deckId)
    {
        var deck = await _deckRepo.ById(deckId)
            ?? throw new DeckNotFoundException($"no deck with id {deckId}");

        // TODO? should admins be able to remove another user's deck
        if (deck.OwnerId != userId)
            throw new UnmatchedUserIdException();

        var count = await _deckRepo.Delete(deckId);
        if (count == 0)
            // * shouldn't happen
            throw new DeckDeletionException($"failed to delete deck with id {deckId}");
    }

    public async Task<DeckModel> Update(string userId, string deckId, PostDeckDto deck)
    {
        var existing = await _deckRepo.ById(deckId)
            ?? throw new DeckNotFoundException($"no deck with id {deckId}");
        
        if (existing.OwnerId != userId)
            throw new UnmatchedUserIdException();

        var newDeck = _mapper.Map<DeckModel>(deck);;
        var count = await _deckRepo.Update(deckId, newDeck);
        if (count == 0)
            // * shouldn't happen
            throw new DeckUpdateException($"failed to update deck with id {deckId}");

        return newDeck;
    }
}