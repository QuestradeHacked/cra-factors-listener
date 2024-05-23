using CRA.FactorsListener.Domain.Entities;
using Google.Cloud.Firestore;

namespace CRA.FactorsListener.DAL.Clients
{
    public class FirestoreClientFactory<TEntity> where TEntity : BaseEntity
    {
        private readonly FirestoreDb _firestoreDb;

        public FirestoreClientFactory(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public FirestoreClient<TEntity> Create(string collectionName)
        {
            var client = new FirestoreClient<TEntity>(_firestoreDb, collectionName);

            return client;
        }
    }
}