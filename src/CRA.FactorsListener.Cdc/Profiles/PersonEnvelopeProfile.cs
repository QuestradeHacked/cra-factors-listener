using AutoMapper;
using cdc.customer_risk_assessment.crm.dbo.Persons;
using CRA.FactorsListener.Cdc.Models;
using CRA.FactorsListener.Cdc.Utils;
using CRA.FactorsListener.Domain.Entities;

namespace CRA.FactorsListener.Cdc.Profiles
{
    public class PersonEnvelopeProfile : Profile
    {
        public PersonEnvelopeProfile()
        {
            CreateMap<Value, Person>()
                .ForMember(dst => dst.PersonId, opt => opt.MapFrom(src => src.PersonID.ToString()))
                .ForMember(dst => dst.CustomerId, opt => opt.MapFrom(src => src.CustomerUUID));

            CreateMap<Envelope, CdcEvent<Person>>()
                .ForMember(dst => dst.Before, opt => opt.MapFrom(src => src.before))
                .ForMember(dst => dst.After, opt => opt.MapFrom(src => src.after))
                .ForMember(dst => dst.Source, opt => opt.MapFrom(src => src.source))
                .ForMember(dst => dst.TransactionInfo, opt => opt.MapFrom(src => src.transaction))
                .ForMember(dst => dst.TransactionTimestamp, opt => opt.MapFrom(src => src.ts_ms))
                .ForMember(dst => dst.Operation, opt => opt.MapFrom(src => OperationTypeUtils.Create(src.op)));
        }
    }
}