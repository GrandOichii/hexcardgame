
namespace ManagerBack.Validators;


[Serializable]
public class InvalidMatchConfigCreationParametersException : Exception
{
    public InvalidMatchConfigCreationParametersException() { }
    public InvalidMatchConfigCreationParametersException(string message) : base(message) { }
}

public class MatchConfigValidator : IValidator<MatchConfig>
{
    public Task Validate(MatchConfig value)
    {
        if (string.IsNullOrEmpty(value.SetupScript))
            throw new InvalidMatchConfigCreationParametersException("setup script is required");

        return Task.CompletedTask;
    }
}
