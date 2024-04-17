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
        // TODO add better checks
        if (string.IsNullOrEmpty(card.Expansion))
            throw new InvalidCardCreationParametersException("card expansion can't be empty");
        if (string.IsNullOrEmpty(card.Name))
            throw new InvalidCardCreationParametersException("card name can't be empty");
        if (string.IsNullOrEmpty(card.Type))
            throw new InvalidCardCreationParametersException("card type can't be empty");
        if (string.IsNullOrEmpty(card.Script))
            throw new InvalidCardCreationParametersException("card script can't be empty");

        return Task.CompletedTask;
    }
}
