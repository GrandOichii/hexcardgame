namespace ManagerBack.Validators;

[Serializable]
public class InvalidCardCreationParametersException : Exception
{
    public InvalidCardCreationParametersException() { }
    public InvalidCardCreationParametersException(string message) : base(message) { }
}

public class ExpansionCardValidator : IValidator<ExpansionCard>
{

    public Task Validate(ExpansionCard card)
    {
        // TODO add more checks
        if (string.IsNullOrEmpty(card.Name))
            throw new InvalidCardCreationParametersException("card name can't be empty");

        return Task.CompletedTask;
    }
}
