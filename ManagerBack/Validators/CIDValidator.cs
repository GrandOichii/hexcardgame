
using System.Text.RegularExpressions;

namespace ManagerBack.Validators;

/// <summary>
/// Exception that represents an invalid card ID
/// </summary>
[Serializable]
public class InvalidCIDException : Exception
{
    public InvalidCIDException(string cid) : base($"invalid cid {cid}") { }
}

/// <summary>
/// Validator class for validating card IDs
/// </summary>
public partial class CIDValidator : IValidator<string>
{
    [GeneratedRegex("^.+::.+$")] private static partial Regex CIDPattern();

    public Task Validate(string cid)
    {
        if (!CIDPattern().IsMatch(cid)) throw new InvalidCIDException(cid);
        return Task.CompletedTask;
    }
}