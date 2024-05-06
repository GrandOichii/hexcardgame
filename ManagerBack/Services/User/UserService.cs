using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using ManagerBack.Dtos;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace ManagerBack.Services;

/// <summary>
/// Exception that is thrown when attempting to create a new user with a taken username
/// </summary>
[Serializable]
public class UsernameTakenException : Exception
{
    public UsernameTakenException(string email) : base("email " + email + " is taken") { }
}

/// <summary>
/// Exception that is thrown when failing to login
/// </summary>
[Serializable]
public class InvalidLoginCredentialsException : Exception
{
    public InvalidLoginCredentialsException() : base("invalid login credentials") { }
    public InvalidLoginCredentialsException(string message) : base(message) { }
}

/// <summary>
/// Exception that is thrown when attempting to fetch an unknown user
/// </summary>
[Serializable]
public class UserNotFoundException : Exception
{
    public UserNotFoundException(string id) : base("user with id " + id + " not found") { }
}

/// <summary>
/// Implementation of the IUserService interface, uses an IUserRepository injected object
/// </summary>
public class UserService : IUserService
{
    /// <summary>
    /// User respository
    /// </summary>
    private readonly IUserRepository _userRepo;

    /// <summary>
    /// Object mapper
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    /// Application configuration
    /// </summary>
    private readonly IConfiguration _configuration;

    /// <summary>
    /// User data validation
    /// </summary>
    private readonly IValidator<PostUserDto> _userValidator;

    public UserService(IMapper mapper, IConfiguration configuration, IUserRepository userRepo, IValidator<PostUserDto> userValidator)
    {
        _mapper = mapper;
        _configuration = configuration;
        _userRepo = userRepo;
        _userValidator = userValidator;
    }

    public async Task<GetUserDto> Register(PostUserDto user)
    {
        if (await _userRepo.ByUsername(user.Username) is not null)
            throw new UsernameTakenException(user.Username);

        await _userValidator.Validate(user);

        var result = _mapper.Map<User>(user);
        await _userRepo.Add(result);

        return _mapper.Map<GetUserDto>(result);
    }

    public async Task<LoginResult> Login(PostUserDto user)
    {
        var existing = await _userRepo.ByUsername(user.Username)
            ?? throw new InvalidLoginCredentialsException();

        if (!BCrypt.Net.BCrypt.Verify(user.Password, existing.PasswordHash)) throw new InvalidLoginCredentialsException();

        return new LoginResult {
            Token = CreateToken(existing),
            IsAdmin = existing.IsAdmin
        };
    }


    /// <summary>
    /// Creates a JWT token for the specified user
    /// </summary>
    /// <param name="user">User data</param>
    /// <returns>JWT token</returns>
    private string CreateToken(User user) {
        var claims = new List<Claim>(){
            new(ClaimTypes.NameIdentifier, user.Id!),
            new(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value!));
        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: cred
        );
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }

}