namespace SomeWebApi.Mappings;

using AutoMapper;
using SomeWebApi.Model;

public class ServiceMappings : Profile
{
    public ServiceMappings() => CreateMap<UpsertUser, User>().ReverseMap();

}