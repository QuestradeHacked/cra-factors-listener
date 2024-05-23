using AutoMapper;
using CRA.FactorsListener.Domain.Entities;
using CRA.FactorsListener.Cdc.Models.Users;

namespace CRA.FactorsListener.Cdc.Profiles
{
    public class CrmUserPersonProfile : Profile
    {
        public CrmUserPersonProfile()
        {
            CreateMap<CrmUserPerson, User>()
                .ForMember(dst => dst.PersonId, opt => opt.MapFrom(src => src.PersonId.ToString()));
        }
    }
}