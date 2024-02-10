using ManagerBack.Dtos;

namespace ManagerBack.Services;

public interface IUserService {
    public Task<GetUserDto> Register(PostUserDto user);
    public Task<string> Login(PostUserDto user);
}