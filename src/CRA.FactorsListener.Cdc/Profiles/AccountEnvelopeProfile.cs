using AutoMapper;
using cdc.customer_risk_assessment.crm.dbo.Account;
using CRA.FactorsListener.Cdc.Models;
using CRA.FactorsListener.Cdc.Utils;
using CRA.FactorsListener.Cdc.Models.Accounts;

namespace CRA.FactorsListener.Cdc.Profiles
{
    public class AccountEnvelopeProfile : Profile
    {
        public AccountEnvelopeProfile()
        {
            CreateMap<Value, Account>()
                .ForMember(dst => dst.AccountId, opt => opt.MapFrom(src => src.AccountID.ToString()))
                .ForMember(dst => dst.AccountStatusId, opt => opt.MapFrom(src => src.AccountStatusID.ToString()));
                
            CreateMap<Envelope, CdcEvent<Account>>()
                .ForMember(dst => dst.Before, opt => opt.MapFrom(src => src.before))
                .ForMember(dst => dst.After, opt => opt.MapFrom(src => src.after))
                .ForMember(dst => dst.Source, opt => opt.MapFrom(src => src.source))
                .ForMember(dst => dst.TransactionInfo, opt => opt.MapFrom(src => src.transaction))
                .ForMember(dst => dst.TransactionTimestamp, opt => opt.MapFrom(src => src.ts_ms))
                .ForMember(dst => dst.Operation, opt => opt.MapFrom(src => OperationTypeUtils.Create(src.op)));
        }
    }
}