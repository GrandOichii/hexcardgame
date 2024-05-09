using System.Text.RegularExpressions;
using AutoMapper;
using ManagerBack.Controllers;
using Serilog;

namespace ManagerBack.Services;

/// <summary>
/// The exception that is thrown when requesting to fetch an unknown card 
/// </summary>
[Serializable]
public class CardNotFoundException : Exception
{
    public CardNotFoundException(string message) : base(message) {}
}

/// <summary>
/// The exception that is thrown when requesting to create a card, whose ID is already taken
/// </summary>
[Serializable]
public class CIDTakenException : Exception
{
    public CIDTakenException(string cid) : base($"cid {cid} is already taken") { }
}

/// <summary>
/// Implementation of the ICardService interface, uses the injected ICardRepository object 
/// </summary>
public partial class CardService : ICardService
{
    /// <summary>
    /// Mapper object
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    /// Card repository
    /// </summary>
    private readonly ICardRepository _cardRepo;

    /// <summary>
    /// Card ID validator
    /// </summary>
    private readonly IValidator<string> _cidValidator;

    /// <summary>
    /// Card validator
    /// </summary>
    private readonly IValidator<ExpansionCard> _cardValidator;

    /// <summary>
    /// Logger
    /// </summary>
    private readonly ILogger<CardService> _logger;

    public CardService(IMapper mapper, ICardRepository cardRepo, IValidator<string> cidValidator, IValidator<ExpansionCard> cardValidator, ILogger<CardService> logger)
    {
        _cardRepo = cardRepo;
        _mapper = mapper;
        _cidValidator = cidValidator;
        _cardValidator = cardValidator;
        _logger = logger;
    }

    public async Task<IEnumerable<ExpansionCard>> All()
    {
        return (await _cardRepo.All()).Select(_mapper.Map<ExpansionCard>);
    }

    public async Task<IEnumerable<string>> AllCIDs()
    {
        return (await _cardRepo.All()).Select(c => c.GetCID());
    }

    public async Task<ExpansionCard> ByCID(string cid)
    {
        await _cidValidator.Validate(cid);
        var result = await _cardRepo.ByCID(cid) 
            ?? throw new CardNotFoundException($"no card with cid {cid}");

        return result;
    }

    public async Task<ExpansionCard> Add(ExpansionCard card)
    {
        var existing = await _cardRepo.ByCID(card.GetCID());
        if (existing is not null)
            throw new CIDTakenException(card.GetCID());

        await _cardValidator.Validate(card);

        await _cardRepo.Add(_mapper.Map<CardModel>(card));

        _logger.LogInformation("Created new card {@cid}", card.GetCID());

        return card;
    }

    public async Task Delete(string cid)
    {
        var deletedCount = await _cardRepo.Delete(cid);
        if (deletedCount != 1) 
            throw new CardNotFoundException("no card with cid " + cid);

        _logger.LogInformation("Deleted card {@cid}", cid);
    }

    public async Task<IEnumerable<ExpansionCard>> Query(CardQuery query)
    {
        return await _cardRepo.Query(query);
    }

    public async Task Update(ExpansionCard card)
    {
        await _cardValidator.Validate(card);

        var count = await _cardRepo.Update(_mapper.Map<CardModel>(card));
        if (count == 0) 
            throw new CardNotFoundException("no card with cid " + card.GetCID());

        _logger.LogInformation("Updated card {@cid}", card.GetCID());
    }
}