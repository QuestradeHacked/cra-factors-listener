using AutoMapper;
using cdc.customer_risk_assessment.crm.dbo.InternationalAddress;
using CRA.FactorsListener.Cdc.Models;
using CRA.FactorsListener.Cdc.Utils;

namespace CRA.FactorsListener.Cdc.Profiles
{
    public class InternationalEnvelopeProfile : Profile
    {
        public InternationalEnvelopeProfile()
        {
            CreateMap<Value, InternationalAddress>()
                .ForMember(dst => dst.CountryId, opt => opt.MapFrom(src => src.CountryID.ToString()))
                .ForMember(dst => dst.InternationalAddressId, opt => opt.MapFrom(src => src.InternationalAddressID.ToString()));
            
            CreateMap<Envelope, CdcEvent<InternationalAddress>>()
                .ForMember(dst => dst.Before, opt => opt.MapFrom(src => src.before))
                .ForMember(dst => dst.After, opt => opt.MapFrom(src => src.after))
                .ForMember(dst => dst.Source, opt => opt.MapFrom(src => src.source))
                .ForMember(dst => dst.TransactionInfo, opt => opt.MapFrom(src => src.transaction))
                .ForMember(dst => dst.TransactionTimestamp, opt => opt.MapFrom(src => src.ts_ms))
                .ForMember(dst => dst.Operation, opt => opt.MapFrom(src => OperationTypeUtils.Create(src.op)));
        }
    }
}