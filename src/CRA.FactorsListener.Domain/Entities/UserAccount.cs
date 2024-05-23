using System;
using Google.Cloud.Firestore;

namespace CRA.FactorsListener.Domain.Entities
{
    [FirestoreData]
    public class UserAccount : BaseEntity
    {
        [FirestoreProperty]
        public string UserId { get; set; }

        [FirestoreProperty]
        public string AccountId { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((UserAccount)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UserId, AccountId);
        }

        private bool Equals(UserAccount other)
        {
            return UserId == other.UserId && AccountId == other.AccountId;
        }
    }
}