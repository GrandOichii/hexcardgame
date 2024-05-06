
namespace ManagerBack.Validators;

/// <summary>
/// Exception that is thrown when attempting to create an invalid match configuration
/// </summary>
[Serializable]
public class InvalidMatchConfigCreationParametersException : Exception
{
    public InvalidMatchConfigCreationParametersException() { }
    public InvalidMatchConfigCreationParametersException(string message) : base(message) { }
}

/// <summary>
/// Match configuration validator
/// </summary>
public class MatchConfigValidator : IValidator<MatchConfig>
{
    public Task Validate(MatchConfig value)
    {
        if (string.IsNullOrEmpty(value.SetupScript))
            throw new InvalidMatchConfigCreationParametersException("setup script is required");

        return Task.CompletedTask;
    }
}
