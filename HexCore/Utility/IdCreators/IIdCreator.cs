namespace Utility.IdCreators;

/// <summary>
/// Abstract class for creating unique ids
/// </summary>
public interface IIdCreator {
    /// <summary>
    /// Generates a new unique ID
    /// </summary>
    /// <returns>New ID</returns>
    public string Next();
}

