
using AutoMapper;

namespace ManagerBack.Services;

/// <summary>
/// The exception that is thrown when requesting to fetch an unknown match configuration 
/// </summary>
[System.Serializable]
public class MatchConfigNotFoundException : System.Exception
{
    public MatchConfigNotFoundException() { }
    public MatchConfigNotFoundException(string message) : base(message) { }
}

/// <summary>
/// The exception that is thrown when requesting to fetch a non-existant basic configuration 
/// </summary>
[System.Serializable]
public class NoBasicMatchConfigException : System.Exception
{
    public NoBasicMatchConfigException() : base("basic match config not found") { }
}

/// <summary>
/// Implementation of the IMatchConfigService interface, uses an IMatchConfigRepository injected object 
/// </summary>
public class MatchConfigService : IMatchConfigService
{
    /// <summary>
    /// Match configuration repository
    /// </summary>
    private readonly IMatchConfigRepository _configRepo;

    /// <summary>
    /// Object mapper
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    /// Match configuration validator
    /// </summary>
    private readonly IValidator<MatchConfig> _configValidator;

    /// <summary>
    /// Logger
    /// </summary>
    private readonly ILogger<MatchConfigService> _logger;

    public MatchConfigService(IMatchConfigRepository configRepo, IMapper mapper, IValidator<MatchConfig> configValidator, ILogger<MatchConfigService> logger)
    {
        _configRepo = configRepo;
        _mapper = mapper;
        _configValidator = configValidator;
        _logger = logger;
    }

    public async Task<IEnumerable<MatchConfigModel>> All()
    {
        return await _configRepo.All();
    }

    public async Task<MatchConfigModel> Basic()
    {
        var found = await _configRepo.Filter(c => c.Name == "basic");
        var result = found.FirstOrDefault()
            ?? throw new NoBasicMatchConfigException()
        ;
        return result;
    }

    public async Task<MatchConfigModel> ById(string id)
    {
        var result = await _configRepo.ById(id)
            ?? throw new MatchConfigNotFoundException()
        ;

        return result;
    }

    public async Task<MatchConfigModel> Add(PostMatchConfigDto config)
    {
        await _configValidator.Validate(config);
        
        var newConfig = _mapper.Map<MatchConfigModel>(config);
        await _configRepo.Add(newConfig);

        _logger.LogInformation("Create new match configuration {id}", newConfig.Id);

        return newConfig;   
    }

    public async Task<MatchConfigModel> Update(PostMatchConfigDto newConfig)
    {
        await _configValidator.Validate(newConfig);
        var config = _mapper.Map<MatchConfigModel>(newConfig);

        var count = await _configRepo.Update(config.Name, config);
        if (count == 0) 
            throw new MatchConfigNotFoundException("no match config with name " + newConfig.Name);

        return config;
    }
}