using AutoMapper;
using cdc.customer_risk_assessment.crm.dbo.PersonEmployment;
using CRA.FactorsListener.Cdc.Models;
using CRA.FactorsListener.Cdc.Models.Enums;
using CRA.FactorsListener.Cdc.Utils;
using Value = cdc.customer_risk_assessment.crm.dbo.PersonEmployment.Value;

namespace CRA.FactorsListener.Cdc.Profiles
{
    public class PersonEmploymentEnvelopeProfile : Profile
    {
        public PersonEmploymentEnvelopeProfile()
        {
            CreateMap<Value, PersonEmployment>()
                .ForMember(dst => dst.PersonId, opt => opt.MapFrom(src => src.PersonID.ToString()))
                .ForMember(dst => dst.PersonEmploymentId, opt => opt.MapFrom(src => src.PersonEmploymentID.ToString()))
                .ForMember(dst => dst.JobTitle, opt => opt.MapFrom(src => src.PE_JobTitle))
                .ForMember(dst => dst.IsJobTitleVerified, opt => opt.MapFrom(src => src.PE_IsJobTitleVerified))
                .ForMember(dst => dst.EmploymentType, opt => opt.MapFrom(src => (EmploymentType) src.EmploymentTypeID));

            CreateMap<Envelope, CdcEvent<PersonEmployment>>()
                .ForMember(dst => dst.Before, opt => opt.MapFrom(src => src.before))
                .ForMember(dst => dst.After, opt => opt.MapFrom(src => src.after))
                .ForMember(dst => dst.Source, opt => opt.MapFrom(src => src.source))
                .ForMember(dst => dst.TransactionInfo, opt => opt.MapFrom(src => src.transaction))
                .ForMember(dst => dst.TransactionTimestamp, opt => opt.MapFrom(src => src.ts_ms))
                .ForMember(dst => dst.Operation, opt => opt.MapFrom(src => OperationTypeUtils.Create(src.op)));
        }
    }
}