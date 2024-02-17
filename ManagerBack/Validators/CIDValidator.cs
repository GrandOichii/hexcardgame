
using System.Text.RegularExpressions;

namespace ManagerBack.Validators;

[Serializable]
public class InvalidCIDException : Exception
{
    public InvalidCIDException(string cid) : base($"invalid cid {cid}") { }
}

public partial class CIDValidator : IValidator<string>
{
    [GeneratedRegex("^.+::.+$")] private static partial Regex CIDPattern();

    public async Task Validate(string cid)
    {
        if (!CIDPattern().IsMatch(cid)) throw new InvalidCIDException(cid);
    }
}