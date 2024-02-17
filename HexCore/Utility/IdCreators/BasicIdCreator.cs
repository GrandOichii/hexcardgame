namespace Utility.IdCreators;

/// <summary>
/// Basic ID creator, gives out a string format of a number
/// </summary>
public class BasicIDCreator : IIdCreator {
    private int _count = 0;
    public string Next()
    {
        return (++_count).ToString();
    }
}
