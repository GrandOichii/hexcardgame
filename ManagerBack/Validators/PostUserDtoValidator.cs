using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

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
    [GeneratedRegex("^[A-Za-z][A-Za-z0-9_]+$")] private static partial Regex UserNamePattern();

    /// <summary>
    /// Validation settings
    /// </summary>
    private readonly IOptions<UserValidationSettings> _settings;

    public PostUserDtoValidator(IOptions<UserValidationSettings> settings)
    {
        _settings = settings;
    }

    public Task Validate(PostUserDto user)
    {
        // TODO better username validation
        if (string.IsNullOrEmpty(user.Username))
            throw new InvalidRegisterCredentialsException($"invalid username");
        if (user.Username.Length < _settings.Value.Username.MinLength)
            throw new InvalidRegisterCredentialsException($"username too short: minimal length is {_settings.Value.Username.MinLength}");
        if (user.Username.Length > _settings.Value.Username.MaxLength)
            throw new InvalidRegisterCredentialsException($"username too long: maximal length is {_settings.Value.Username.MaxLength}");
        if (!UserNamePattern().IsMatch(user.Username))
            throw new InvalidRegisterCredentialsException("invalid username");

        if (string.IsNullOrEmpty(user.Password))
            throw new InvalidRegisterCredentialsException($"invalid password");
        if (user.Password.Length < _settings.Value.Password.MinLength)
            throw new InvalidRegisterCredentialsException($"password too short: minimal length is {_settings.Value.Password.MinLength}");
        if (user.Password.Length > _settings.Value.Password.MaxLength)
            throw new InvalidRegisterCredentialsException($"password too long: maximal length is {_settings.Value.Password.MaxLength}");

        return Task.CompletedTask;
    }
}
