using ManagerBack.Dtos;

namespace ManagerBack.Services;

public class LoginResult {
    public required string Token { get; set; }
    public required bool IsAdmin { get; set; }
}

public interface IUserService {
    public Task<GetUserDto> Register(PostUserDto user);
    public Task<LoginResult> Login(PostUserDto user);
}