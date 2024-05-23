using AutoMapper;
using cdc.customer_risk_assessment.crm.dbo.UserAccount;
using CRA.FactorsListener.Cdc.Models;
using CRA.FactorsListener.Cdc.Utils;
using CRA.FactorsListener.Domain.Entities;

namespace CRA.FactorsListener.Cdc.Profiles
{
    public class UserAccountEnvelopeProfile : Profile
    {
        public UserAccountEnvelopeProfile()
        {
            CreateMap<Value, UserAccount>()
                .ForMember(dst => dst.UserId, opt => opt.MapFrom(src => src.UserID.ToString()))
                .ForMember(dst => dst.AccountId, opt => opt.MapFrom(src => src.AccountID.ToString()));

            CreateMap<Envelope, CdcEvent<UserAccount>>()
                .ForMember(dst => dst.Before, opt => opt.MapFrom(src => src.before))
                .ForMember(dst => dst.After, opt => opt.MapFrom(src => src.after))
                .ForMember(dst => dst.Source, opt => opt.MapFrom(src => src.source))
                .ForMember(dst => dst.TransactionInfo, opt => opt.MapFrom(src => src.transaction))
                .ForMember(dst => dst.TransactionTimestamp, opt => opt.MapFrom(src => src.ts_ms))
                .ForMember(dst => dst.Operation, opt => opt.MapFrom(src => OperationTypeUtils.Create(src.op)));
        }
    }
}