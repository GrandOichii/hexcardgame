using System.Linq.Expressions;
using AutoMapper;
using FakeItEasy;
using FluentAssertions;
using HexCore.GameMatch;

namespace ManagerBack.Tests.Services;

public class MatchConfigServiceTests {
    private readonly MatchConfigService _configService;
    private readonly Mapper _mapper;
    private readonly IMatchConfigRepository _configRepo;
    private readonly IValidator<MatchConfig> _validator;

    public MatchConfigServiceTests() {
        var mC = new MapperConfiguration(cfg => {
            cfg.AddProfile(new AutoMapperProfile());
        });
        _mapper = new Mapper(mC);
        _configRepo = A.Fake<IMatchConfigRepository>();
        _validator = A.Fake<IValidator<MatchConfig>>();
        _configService = new(_configRepo, _mapper, _validator, A.Fake<ILogger<MatchConfigService>>());
    }

    [Fact]
    public async Task ShouldFetchAll() {
        // Arrange
        var configs = A.Fake<IEnumerable<MatchConfigModel>>();
        A.CallTo(() => _configRepo.All()).Returns(configs);
        
        // Act
        var result = await _configService.All();

        // Assert
        result.Should().BeEquivalentTo(configs);
    }

    [Fact]
    public async Task ShouldCreate() {
        // Arrange
        var config = A.Fake<PostMatchConfigDto>();
        var configModel = _mapper.Map<MatchConfigModel>(config);
        A.CallTo(() => _validator.Validate(config)).DoesNothing();
        A.CallTo(() => _configRepo.Add(configModel)).DoesNothing();

        // Act
        var result = await _configService.Add(config);

        // Assert
        result.Should().BeEquivalentTo(configModel);
    }

    [Fact]
    public async Task ShouldNotCreate() {
        // Arrange
        var config = A.Fake<PostMatchConfigDto>();
        var configModel = _mapper.Map<MatchConfigModel>(config);
        A.CallTo(() => _validator.Validate(config)).Throws<InvalidMatchConfigCreationParametersException>();

        // Act
        var act = () => _configService.Add(config);

        // Assert
        await act.Should().ThrowAsync<InvalidMatchConfigCreationParametersException>();
    }

    [Fact]
    public async Task ShouldFetchById() {
        // Arrange
        var id = "config-id";
        var config = A.Fake<MatchConfigModel>();
        A.CallTo(() => _configRepo.ById(id)).Returns(config);
        
        // Act
        var result = await _configService.ById(id);

        // Assert
        result.Should().Be(config);
    }

    [Fact]
    public async Task ShouldNotFetchBasic() {
        // Arrange
        
        // Act
        var act = () => _configService.Basic();

        // Assert
        await act.Should().ThrowAsync<NoBasicMatchConfigException>();

    }

    [Fact]
    public async Task ShouldFetchBasic() {
        // Arrange
        var config = A.Fake<MatchConfigModel>();
        A.CallTo(() => _configRepo.Filter(A<Expression<Func<MatchConfigModel, bool>>>._)).Returns(new List<MatchConfigModel> { config });
        
        // Act
        var result = await _configService.Basic();

        // Assert
        result.Should().Be(config);
    }
}