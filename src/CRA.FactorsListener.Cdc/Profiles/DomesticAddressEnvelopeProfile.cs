using AutoMapper;
using cdc.customer_risk_assessment.crm.dbo.DomesticAddress;
using CRA.FactorsListener.Cdc.Models;
using CRA.FactorsListener.Cdc.Utils;

namespace CRA.FactorsListener.Cdc.Profiles
{
    public class DomesticEnvelopeProfile : Profile
    {
        public DomesticEnvelopeProfile()
        {
            CreateMap<Value, DomesticAddress>()
                .ForMember(dst => dst.DomesticAddressId, opt => opt.MapFrom(src => src.DomesticAddressID.ToString()));
            
            CreateMap<Envelope, CdcEvent<DomesticAddress>>()
                .ForMember(dst => dst.Before, opt => opt.MapFrom(src => src.before))
                .ForMember(dst => dst.After, opt => opt.MapFrom(src => src.after))
                .ForMember(dst => dst.Source, opt => opt.MapFrom(src => src.source))
                .ForMember(dst => dst.TransactionInfo, opt => opt.MapFrom(src => src.transaction))
                .ForMember(dst => dst.TransactionTimestamp, opt => opt.MapFrom(src => src.ts_ms))
                .ForMember(dst => dst.Operation, opt => opt.MapFrom(src => OperationTypeUtils.Create(src.op)));
        }
    }
}