
using Microsoft.Extensions.Options;

namespace ManagerBack.Tasks;

/// <summary>
/// Background task that periodically clears all non-active matches
/// </summary>
public class ClearMatchesTask : IHostedService
{
    /// <summary>
    /// Match clearing timer
    /// </summary>
    private Timer? _timer;

    /// <summary>
    /// Match clearing settings
    /// </summary>
    private readonly IOptions<ClearMatchesSettings> _settings;

    /// <summary>
    /// Match process service
    /// </summary>
    private readonly IMatchService _matchService;

    public ClearMatchesTask(IOptions<ClearMatchesSettings> settings, IMatchService matchService)
    {
        _settings = settings;
        _matchService = matchService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_settings.Value.Enabled) return Task.CompletedTask;

        _timer = new(ClearMatches, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(_settings.Value.Timeout));

        return Task.CompletedTask;
    }

    /// <summary>
    /// Clears the matches
    /// </summary>
    /// <param name="state">Timer state</param>
    private async void ClearMatches(object? state) {
        await _matchService.Remove(match => 
            (_settings.Value.ClearFinished && match.Status == MatchStatus.FINISHED) ||
            (_settings.Value.ClearCrashed && match.Status == MatchStatus.CRASHED)
        );
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}