using Util;

namespace Core.GameMatch;



// TODO remove, complicated for no reason
/// <summary>
/// Singleton object for creating and fetching matches
/// </summary>
public class MatchMaster
{
    static public MatchMaster Instance { get; } = new MatchMaster();
    static public IIdCreator IDCreator { get; set; } = new BasicIDCreator();


    public Dictionary<string, Match> Index { get; set; }
    private MatchMaster() {
        Index = new();
    }

    /// <summary>
    /// Creates and saves a new Match object
    /// </summary>
    /// <returns>The created match</returns>
    public Match New(ICardMaster cMaster, MatchConfig config, string coreFilePath = "../core/core.lua") {
        var id = IDCreator.Next();
        var result = new Match(id, config, cMaster, coreFilePath);
        Index.Add(id, result);
        return result;
    }
}
