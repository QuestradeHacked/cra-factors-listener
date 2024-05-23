using AutoMapper;
using cdc.customer_risk_assessment.crm.dbo.PoliticallyExposedPerson;
using CRA.FactorsListener.Cdc.Models;
using CRA.FactorsListener.Cdc.Utils;

namespace CRA.FactorsListener.Cdc.Profiles
{
    public class PoliticallyExposedPersonEnvelopeProfile : Profile
    {
        public PoliticallyExposedPersonEnvelopeProfile()
        {
            CreateMap<Value, PoliticallyExposedPerson>()
                .ForMember(dst => dst.PersonId, opt => opt.MapFrom(src => src.PersonID.ToString()))
                .ForMember(dst => dst.PoliticallyExposedPersonId, opt => opt.MapFrom(src => src.PoliticallyExposedPersonID.ToString()))
                .ForMember(dst => dst.IsPoliticallyExposed, opt => opt.MapFrom(src => src.PEP_IsPoliticallyExposed));
            
            CreateMap<Envelope, CdcEvent<PoliticallyExposedPerson>>()
                .ForMember(dst => dst.Before, opt => opt.MapFrom(src => src.before))
                .ForMember(dst => dst.After, opt => opt.MapFrom(src => src.after))
                .ForMember(dst => dst.Source, opt => opt.MapFrom(src => src.source))
                .ForMember(dst => dst.TransactionInfo, opt => opt.MapFrom(src => src.transaction))
                .ForMember(dst => dst.TransactionTimestamp, opt => opt.MapFrom(src => src.ts_ms))
                .ForMember(dst => dst.Operation, opt => opt.MapFrom(src => OperationTypeUtils.Create(src.op)));
        }
    }
}