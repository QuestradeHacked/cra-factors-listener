using System;
using Google.Cloud.Firestore;

namespace CRA.FactorsListener.Domain.Entities
{
    public class BaseEntity
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreDocumentCreateTimestamp]
        public DateTime Created { get; set; }

        [FirestoreDocumentUpdateTimestamp]
        public DateTime Modified { get; set; }
    }
}