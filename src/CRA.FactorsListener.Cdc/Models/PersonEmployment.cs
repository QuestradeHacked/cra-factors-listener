using System;
using CRA.FactorsListener.Cdc.Models.Enums;

namespace CRA.FactorsListener.Cdc.Models
{
    public class PersonEmployment : ICdcMessage
    {
        public string PersonEmploymentId { get; set; }

        public string PersonId { get; set; }

        public string JobTitle { get; set; }

        public bool IsJobTitleVerified { get; set; }

        public EmploymentType EmploymentType { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((PersonEmployment)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PersonEmploymentId, PersonId, JobTitle, IsJobTitleVerified, EmploymentType);
        }

        private bool Equals(PersonEmployment other)
        {
            return PersonEmploymentId == other.PersonEmploymentId &&
                   PersonId == other.PersonId &&
                   JobTitle == other.JobTitle &&
                   IsJobTitleVerified == other.IsJobTitleVerified &&
                   EmploymentType == other.EmploymentType;
        }
    }
}
