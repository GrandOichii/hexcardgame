using AutoMapper;
using ManagerBack.Dtos;

namespace ManagerBack;

public class AutoMapperProfile : Profile {
    public AutoMapperProfile()
    {
        CreateMap<ExpansionCard, CardModel>();

        CreateMap<User, GetUserDto>();
        CreateMap<PostUserDto, User>()
            .ForMember(u => u.PasswordHash, o => o.MapFrom(u => BCrypt.Net.BCrypt.HashPassword(u.Password)));
    }
}