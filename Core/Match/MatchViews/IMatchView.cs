namespace Core.GameMatch.View;

/// <summary>
/// View model of the match status
/// </summary>
public interface IMatchView
{
    /// <summary>
    /// Start the view
    /// </summary>
    public void Start();

    /// <summary>
    /// Updates the view with the new match data
    /// </summary>
    /// <param name="match">The displayed match</param>
    public void Update(Match match);

    /// <summary>
    /// Ends the view
    /// </summary>
    public void End();
}
