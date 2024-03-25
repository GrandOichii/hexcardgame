
using AutoMapper;

namespace ManagerBack.Services;

[System.Serializable]
public class MatchConfigNotFoundException : System.Exception
{
    public MatchConfigNotFoundException() { }
    public MatchConfigNotFoundException(string message) : base(message) { }
}

[System.Serializable]
public class NoBasicMatchConfigException : System.Exception
{
    public NoBasicMatchConfigException() : base("basic match config not found") { }
}

public class MatchConfigService : IMatchConfigService
{
    private readonly IMatchConfigRepository _configRepo;
    private readonly IMapper _mapper;
    private readonly IValidator<MatchConfig> _configValidator;

    public MatchConfigService(IMatchConfigRepository configRepo, IMapper mapper, IValidator<MatchConfig> configValidator)
    {
        _configRepo = configRepo;
        _mapper = mapper;
        _configValidator = configValidator;
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

    public async Task<MatchConfigModel> Create(PostMatchConfigDto config)
    {
        await _configValidator.Validate(config);
        
        var newConfig = _mapper.Map<MatchConfigModel>(config);
        await _configRepo.Add(newConfig);
        return newConfig;   
    }
}