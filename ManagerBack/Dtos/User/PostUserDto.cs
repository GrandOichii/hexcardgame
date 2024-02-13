namespace ManagerBack.Dtos;


[Serializable]
public class InvalidRegisterCredentialsException : Exception
{
    public InvalidRegisterCredentialsException(string message) : base(message) { }
}

public class PostUserDto {
    // TODO add max limiters
    private static readonly int MIN_USERNAME_LENGTH = 4;
    private static readonly int MIN_PASSWORD_LENGTH = 8;

    public required string Username { get; set; }
    public required string Password { get; set; }

    public void Validate() {
        // TODO better username validation
        if (string.IsNullOrEmpty(Username))
            throw new InvalidRegisterCredentialsException($"invalid email");
        if (Username.Length < MIN_USERNAME_LENGTH)
            throw new InvalidRegisterCredentialsException($"username too short: minimal length is {MIN_USERNAME_LENGTH}");

        if (string.IsNullOrEmpty(Password))
            throw new InvalidRegisterCredentialsException($"invalid password");
        if (Password.Length < MIN_PASSWORD_LENGTH)
            throw new InvalidRegisterCredentialsException($"password too short: minimal length is {MIN_PASSWORD_LENGTH}");
    }
}