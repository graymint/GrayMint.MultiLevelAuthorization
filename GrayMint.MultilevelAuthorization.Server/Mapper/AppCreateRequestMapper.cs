using AutoMapper;
using MultiLevelAuthorization.DTOs;
using MultiLevelAuthorization.Server.DTOs;

namespace MultiLevelAuthorization.Server.Mapper;

public class AppCreateRequestMapper : Profile
{
    public AppCreateRequestMapper()
    {
        CreateMap<AppCreateRequest, AppCreateRequestHandler>().ReverseMap();
    }
}