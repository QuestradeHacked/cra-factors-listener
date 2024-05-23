using System;
using Google.Cloud.Firestore;

namespace CRA.FactorsListener.Domain.Entities
{
    [FirestoreData]
    public class Person : BaseEntity
    {
        [FirestoreProperty]
        public string PersonId { get; set; }

        [FirestoreProperty]
        public string CustomerId { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Person)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PersonId, CustomerId);
        }

        private bool Equals(Person other)
        {
            return PersonId == other.PersonId && CustomerId == other.CustomerId;
        }
    }
}