using AutoMapper;
using CRA.FactorsListener.Domain.Entities;
using CRA.FactorsListener.Cdc.Models.Persons;

namespace CRA.FactorsListener.Cdc.Profiles
{
    public class CrmPersonProfile : Profile
    {
        public CrmPersonProfile()
        {
            CreateMap<CrmPerson, PersonAddress>()
                .ForMember(dst => dst.AddressId, opt => opt.MapFrom(src => src.AddressId.ToString()))
                .ForMember(dst => dst.AddressType, opt => opt.MapFrom(src => src.AddressTypeId.ToString()));
        }
    }
}