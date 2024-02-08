using AutoMapper;

namespace ManagerBack;

public class AutoMapperProfile : Profile {
    public AutoMapperProfile()
    {
        CreateMap<ExpansionCard, CardModel>();
    }
}