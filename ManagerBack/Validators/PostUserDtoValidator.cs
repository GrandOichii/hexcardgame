using System.Text.RegularExpressions;

namespace ManagerBack.Validators;

/// <summary>
/// Exception that is thrown when attempting to create an invalid user
/// </summary>
[Serializable]
public class InvalidRegisterCredentialsException : Exception
{
    public InvalidRegisterCredentialsException(string message) : base(message) { }
}

/// <summary>
/// User validator
/// </summary>
public partial class PostUserDtoValidator : IValidator<PostUserDto>
{
    /// <summary>
    /// Minimum username length
    /// </summary>
    private static readonly int MIN_USERNAME_LENGTH = 4;

    /// <summary>
    /// Maximum username length
    /// </summary>
    private static readonly int MAX_USERNAME_LENGTH = 20;

    /// <summary>
    /// Minimum password length
    /// </summary>
    private static readonly int MIN_PASSWORD_LENGTH = 8;

    /// <summary>
    /// Maximum password length
    /// </summary>
    private static readonly int MAX_PASSWORD_LENGTH = 20;

    [GeneratedRegex("^[A-Za-z][A-Za-z0-9_]+$")] private static partial Regex UserNamePattern();

    public Task Validate(PostUserDto user)
    {
        // TODO better username validation
        if (string.IsNullOrEmpty(user.Username))
            throw new InvalidRegisterCredentialsException($"invalid username");
        if (user.Username.Length < MIN_USERNAME_LENGTH)
            throw new InvalidRegisterCredentialsException($"username too short: minimal length is {MIN_USERNAME_LENGTH}");
        if (user.Username.Length > MAX_USERNAME_LENGTH)
            throw new InvalidRegisterCredentialsException($"username too long: maximal length is {MAX_USERNAME_LENGTH}");
        if (!UserNamePattern().IsMatch(user.Username))
            throw new InvalidRegisterCredentialsException("invalid username");

        if (string.IsNullOrEmpty(user.Password))
            throw new InvalidRegisterCredentialsException($"invalid password");
        if (user.Password.Length < MIN_PASSWORD_LENGTH)
            throw new InvalidRegisterCredentialsException($"password too short: minimal length is {MIN_PASSWORD_LENGTH}");
        if (user.Password.Length > MAX_PASSWORD_LENGTH)
            throw new InvalidRegisterCredentialsException($"password too long: maximal length is {MAX_PASSWORD_LENGTH}");

        return Task.CompletedTask;
    }
}
