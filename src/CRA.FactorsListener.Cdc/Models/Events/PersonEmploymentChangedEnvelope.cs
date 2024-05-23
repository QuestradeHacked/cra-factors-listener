using CRA.FactorsListener.Cdc.Models.Enums;

namespace CRA.FactorsListener.Cdc.Models.Events
{
    public class PersonEmploymentChangedEnvelope : FactorChangedBase
    {
        public PersonEmploymentChangedEnvelope(string customerId, long? timestamp) :base(customerId, timestamp)
        {
        }

        public string Occupation { get; set; }

        public bool IsVerified { get; set; }

        public EmploymentType EmploymentType { get; set; }
    }
}