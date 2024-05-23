using System;
using Google.Cloud.Firestore;

namespace CRA.FactorsListener.Domain.Entities
{
    [FirestoreData]
    public class User : BaseEntity
    {
        [FirestoreProperty]
        public string UserId { get; set; }

        [FirestoreProperty]
        public string PersonId { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((User)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UserId, PersonId);
        }

        private bool Equals(User other)
        {
            return UserId == other.UserId && PersonId == other.PersonId;
        }
    }
}