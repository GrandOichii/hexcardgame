using AutoMapper;
using MongoDB.Driver;

namespace ManagerBack.Services;

/// <summary>
/// The exception that is thrown when requesting to fetch an unknown deck 
/// </summary>
[Serializable]
public class DeckNotFoundException : Exception
{
    public DeckNotFoundException() { }
    public DeckNotFoundException(string message) : base(message) { }
}

/// <summary>
/// The exception that is thrown when requesting to fetch a deck that doesn't belong to the requester 
/// </summary>
[Serializable]
public class UnmatchedUserIdException : Exception
{
    public UnmatchedUserIdException() { }
}

/// <summary>
/// A general deck deletion exception 
/// </summary>
[Serializable]
public class DeckDeletionException : Exception
{
    public DeckDeletionException() { }
    public DeckDeletionException(string message) : base(message) { }
}

/// <summary>
/// A general deck updating exception
/// </summary>
[Serializable]
public class DeckUpdateException : Exception
{
    public DeckUpdateException() { }
    public DeckUpdateException(string message) : base(message) { }
}

/// <summary>
/// The exception that is thrown when requesting to create a deck that will go over the deck limit
/// </summary>
[Serializable]
public class DeckAmountLimitException : Exception
{
    public DeckAmountLimitException() { }
    public DeckAmountLimitException(string message) : base(message) { }
}

/// <summary>
/// Implementation of the IDeckService interface, uses the IDeckRepository injected object
/// </summary>
public partial class DeckService : IDeckService
{
    /// <summary>
    /// The limit for the amount of decks a user can have
    /// </summary>
    private static readonly int MAX_DECK_COUNT = 20;

    /// <summary>
    /// Mapper object
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    /// Deck repository
    /// </summary>
    private readonly IDeckRepository _deckRepo;

    /// <summary>
    /// Deck validator
    /// </summary>
    private readonly IValidator<DeckTemplate> _deckValidator;

    /// <summary>
    /// User repository
    /// </summary>
    private readonly IUserRepository _userRepository;

    public DeckService(IDeckRepository deckRepo, IMapper mapper, IValidator<DeckTemplate> deckValidator, IUserRepository userRepository)
    {
        _deckRepo = deckRepo;
        _mapper = mapper;
        _deckValidator = deckValidator;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<DeckModel>> All(string userId)
    {
        var decks = await _deckRepo.Filter(d => d.OwnerId == userId);
        return decks;
    }

    public async Task<DeckModel> Add(string userId, PostDeckDto deck)
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