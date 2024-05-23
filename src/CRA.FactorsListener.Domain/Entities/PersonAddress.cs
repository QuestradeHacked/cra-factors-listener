using System;
using Google.Cloud.Firestore;

namespace CRA.FactorsListener.Domain.Entities
{
    [FirestoreData]
    public class PersonAddress : BaseEntity
    {
        [FirestoreProperty]
        public string PersonId { get; set; }

        [FirestoreProperty]
        public string AddressId { get; set; }

        [FirestoreProperty]
        public int AddressType { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((PersonAddress)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PersonId, AddressId, AddressType);
        }

        private bool Equals(PersonAddress other)
        {
            return PersonId == other.PersonId &&
                   AddressId == other.AddressId &&
                   AddressType == other.AddressType;
        }
    }
}