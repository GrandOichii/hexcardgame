using AutoMapper;
using ManagerBack.Dtos;

namespace ManagerBack;

public class AutoMapperProfile : Profile {
    public AutoMapperProfile()
    {
        // AllowNullCollections = true;

        CreateMap<ExpansionCard, CardModel>();
        
        CreateMap<MatchConfig, MatchConfigModel>();

        CreateMap<User, GetUserDto>();
        CreateMap<PostUserDto, User>()
            .ForMember(u => u.PasswordHash, o => o.MapFrom(u => BCrypt.Net.BCrypt.HashPassword(u.Password)));

        CreateMap<PostDeckDto, DeckModel>();

        CreateMap<PostMatchConfigDto, MatchConfigModel>();
        CreateMap<MatchProcess, GetMatchProcessDto>();
    }
}