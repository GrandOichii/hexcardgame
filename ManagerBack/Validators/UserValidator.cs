namespace ManagerBack.Validators;

[Serializable]
public class InvalidRegisterCredentialsException : Exception
{
    public InvalidRegisterCredentialsException(string message) : base(message) { }
}

public class PostUserDtoValidator : IValidator<PostUserDto>
{
    // TODO add max limiters
    private static readonly int MIN_USERNAME_LENGTH = 4;
    private static readonly int MIN_PASSWORD_LENGTH = 8;

    public async Task Validate(PostUserDto user)
    {
        // TODO better username validation
        if (string.IsNullOrEmpty(user.Username))
            throw new InvalidRegisterCredentialsException($"invalid email");
        if (user.Username.Length < MIN_USERNAME_LENGTH)
            throw new InvalidRegisterCredentialsException($"username too short: minimal length is {MIN_USERNAME_LENGTH}");

        if (string.IsNullOrEmpty(user.Password))
            throw new InvalidRegisterCredentialsException($"invalid password");
        if (user.Password.Length < MIN_PASSWORD_LENGTH)
            throw new InvalidRegisterCredentialsException($"password too short: minimal length is {MIN_PASSWORD_LENGTH}");
    }
}
