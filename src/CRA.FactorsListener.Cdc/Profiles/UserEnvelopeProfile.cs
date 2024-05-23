using AutoMapper;
using cdc.customer_risk_assessment.crm.dbo.Users;
using CRA.FactorsListener.Cdc.Models;
using CRA.FactorsListener.Cdc.Utils;
using CRA.FactorsListener.Domain.Entities;

namespace CRA.FactorsListener.Cdc.Profiles
{
    public class UserEnvelopeProfile : Profile
    {
        public UserEnvelopeProfile()
        {
            CreateMap<Value, User>()
                .ForMember(dst => dst.UserId, opt => opt.MapFrom(src => src.UserID.ToString()))
                .ForMember(dst => dst.PersonId, opt => opt.MapFrom(src => src.PersonID.ToString()));

            CreateMap<Envelope, CdcEvent<User>>()
                .ForMember(dst => dst.Before, opt => opt.MapFrom(src => src.before))
                .ForMember(dst => dst.After, opt => opt.MapFrom(src => src.after))
                .ForMember(dst => dst.Source, opt => opt.MapFrom(src => src.source))
                .ForMember(dst => dst.TransactionInfo, opt => opt.MapFrom(src => src.transaction))
                .ForMember(dst => dst.TransactionTimestamp, opt => opt.MapFrom(src => src.ts_ms))
                .ForMember(dst => dst.Operation, opt => opt.MapFrom(src => OperationTypeUtils.Create(src.op)));
        }
    }
}