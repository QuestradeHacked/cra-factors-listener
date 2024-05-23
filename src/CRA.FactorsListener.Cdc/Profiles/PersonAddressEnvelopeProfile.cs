using AutoMapper;
using cdc.customer_risk_assessment.crm.dbo.PersonAddress;
using CRA.FactorsListener.Cdc.Models;
using CRA.FactorsListener.Cdc.Utils;
using CRA.FactorsListener.Domain.Entities;

namespace CRA.FactorsListener.Cdc.Profiles
{
    public class PersonAddressEnvelopeProfile : Profile
    {
        public PersonAddressEnvelopeProfile()
        {
            CreateMap<Value, PersonAddress>()
                .ForMember(dst => dst.AddressId, opt => opt.MapFrom(src => src.AddressID.ToString()))
                .ForMember(dst => dst.AddressType, opt => opt.MapFrom(src => src.AddressTypeID))
                .ForMember(dst => dst.PersonId, opt => opt.MapFrom(src => src.PersonID.ToString()));

            CreateMap<Envelope, CdcEvent<PersonAddress>>()
                .ForMember(dst => dst.Before, opt => opt.MapFrom(src => src.before))
                .ForMember(dst => dst.After, opt => opt.MapFrom(src => src.after))
                .ForMember(dst => dst.Source, opt => opt.MapFrom(src => src.source))
                .ForMember(dst => dst.TransactionInfo, opt => opt.MapFrom(src => src.transaction))
                .ForMember(dst => dst.TransactionTimestamp, opt => opt.MapFrom(src => src.ts_ms))
                .ForMember(dst => dst.Operation, opt => opt.MapFrom(src => OperationTypeUtils.Create(src.op)));
        }
    }
}