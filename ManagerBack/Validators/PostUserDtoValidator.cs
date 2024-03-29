namespace ManagerBack.Validators;

[Serializable]
public class InvalidRegisterCredentialsException : Exception
{
    public InvalidRegisterCredentialsException(string message) : base(message) { }
}

public class PostUserDtoValidator : IValidator<PostUserDto>
{
    private static readonly int MIN_USERNAME_LENGTH = 4;
    private static readonly int MAX_USERNAME_LENGTH = 20;
    private static readonly int MIN_PASSWORD_LENGTH = 8;
    private static readonly int MAX_PASSWORD_LENGTH = 20;

    public Task Validate(PostUserDto user)
    {
        // TODO better username validation
        if (string.IsNullOrEmpty(user.Username))
            throw new InvalidRegisterCredentialsException($"invalid email");
        if (user.Username.Length < MIN_USERNAME_LENGTH)
            throw new InvalidRegisterCredentialsException($"username too short: minimal length is {MIN_USERNAME_LENGTH}");
        if (user.Username.Length > MAX_USERNAME_LENGTH)
            throw new InvalidRegisterCredentialsException($"username too long: maximal length is {MAX_USERNAME_LENGTH}");

        if (string.IsNullOrEmpty(user.Password))
            throw new InvalidRegisterCredentialsException($"invalid password");
        if (user.Password.Length < MIN_PASSWORD_LENGTH)
            throw new InvalidRegisterCredentialsException($"password too short: minimal length is {MIN_PASSWORD_LENGTH}");
        if (user.Password.Length > MAX_PASSWORD_LENGTH)
            throw new InvalidRegisterCredentialsException($"password too long: maximal length is {MAX_PASSWORD_LENGTH}");

        return Task.CompletedTask;
    }
}
