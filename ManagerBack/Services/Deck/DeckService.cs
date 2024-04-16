using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;
using ManagerBack.Dtos;
using ManagerBack.Validators;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace ManagerBack.Services;

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
public class DeckAmountLimitException : Exception
{
    public DeckAmountLimitException() { }
    public DeckAmountLimitException(string message) : base(message) { }
}

public partial class DeckService : IDeckService
{
    // TODO subject to change
    private static readonly int MAX_DECK_COUNT = 20;

    private readonly IMapper _mapper;
    private readonly IDeckRepository _deckRepo;
    private readonly ICardRepository _cardRepo;
    private readonly IValidator<DeckTemplate> _deckValidator;
    private readonly IUserRepository _userRepository;

    public DeckService(IDeckRepository deckRepo, IMapper mapper, ICardRepository cardRepo, IValidator<DeckTemplate> deckValidator, IUserRepository userRepository)
    {
        _deckRepo = deckRepo;
        _mapper = mapper;
        _cardRepo = cardRepo;
        _deckValidator = deckValidator;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<DeckModel>> All(string userId)
    {
        var decks = await _deckRepo.Filter(d => d.OwnerId == userId);
        return decks;
    }

    public async Task<DeckModel> Create(string userId, PostDeckDto deck)
    {
        var idValid = await _userRepository.CheckId(userId);
        if (!idValid)
            throw new UserNotFoundException($"no user with id {userId}");
        
        var deckCount = await GetDeckCount(userId);
        if (deckCount >= MAX_DECK_COUNT) {
            throw new DeckAmountLimitException($"can't go over the deck limit ({MAX_DECK_COUNT})");
        }
        var newDeck = _mapper.Map<DeckModel>(deck);
        newDeck.OwnerId = userId;
        
        await _deckValidator.Validate(deck.ToDeckTemplate());

        await _deckRepo.Add(newDeck);
        return newDeck;
    }

    public async Task Delete(string userId, string deckId)
    {
        var idValid = await _userRepository.CheckId(userId);
        if (!idValid)
            throw new UserNotFoundException($"no user with id {userId}");

        var deck = await _deckRepo.ById(deckId)
            ?? throw new DeckNotFoundException($"no deck with id {deckId}");

        if (deck.OwnerId != userId)
            throw new UnmatchedUserIdException();

        var count = await _deckRepo.Delete(deckId);
        if (count == 0)
            // * shouldn't happen
            throw new DeckDeletionException($"failed to delete deck with id {deckId}");
    }

    public async Task<DeckModel> Update(string userId, string deckId, PostDeckDto deck)
    {
        var idValid = await _userRepository.CheckId(userId);
        if (!idValid)
            throw new UserNotFoundException($"no user with id {userId}");

        var existing = await _deckRepo.ById(deckId)
            ?? throw new DeckNotFoundException($"no deck with id {deckId}");
        
        if (existing.OwnerId != userId)
            throw new UnmatchedUserIdException();

        var newDeck = _mapper.Map<DeckModel>(deck);
        newDeck.Id = deckId;
        newDeck.OwnerId = userId;
        var count = await _deckRepo.Update(deckId, newDeck);
        if (count == 0)
            // * shouldn't happen
            throw new DeckUpdateException($"failed to update deck with id {deckId}");

        return newDeck;
    }

    private async Task<int> GetDeckCount(string userId) {
        var decks = await _deckRepo.Filter(d => d.OwnerId == userId);
        return decks.Count();
    }
}