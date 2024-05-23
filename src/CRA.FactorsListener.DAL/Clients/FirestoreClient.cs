using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CRA.FactorsListener.Domain.Entities;
using Google.Cloud.Firestore;

namespace CRA.FactorsListener.DAL.Clients
{
    public sealed class FirestoreClient<T> where T : BaseEntity
    {
        private readonly CollectionReference _workingCollection;

        public FirestoreClient(FirestoreDb firestoreDb, string collectionName)
        {
            _workingCollection = firestoreDb?.Collection(collectionName);
        }

        public async Task<T> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var querySnapshot = await _workingCollection.Document(id).GetSnapshotAsync(cancellationToken);

            return querySnapshot.Exists ? querySnapshot.ConvertTo<T>() : default;
        }

        public async Task<List<T>> GetByAsync(Expression<Func<T, object>> property, object value,
            CancellationToken cancellationToken = default)
        {
            _ = value ?? throw new ArgumentNullException(nameof(value));

            var propertyName = ((MemberExpression)property.Body).Member.Name;

            var querySnapshot = await _workingCollection.WhereEqualTo(propertyName, value)
                .GetSnapshotAsync(cancellationToken);

            var result = querySnapshot.Documents.Select(x => x.ConvertTo<T>()).ToList();
            return await ClearDuplicates(result);
        }

        public async Task<T> UpsertAsync(T entity, CancellationToken cancellationToken = default)
        {
            DocumentReference document;

            if (!string.IsNullOrWhiteSpace(entity.Id))
            {
                // Can also create document with a custom Id here.
                document = _workingCollection.Document(entity.Id);
                await document.SetAsync(entity, SetOptions.Overwrite, cancellationToken);
            }
            else
            {
                // Auto-generates Id
                document = await _workingCollection.AddAsync(entity, cancellationToken);
            }

            var entitySnapshot = await document.GetSnapshotAsync(cancellationToken);

            return entitySnapshot.ConvertTo<T>();
        }

        private async Task<List<T>> ClearDuplicates(IEnumerable<T> source)
        {
            var distinctItems = new List<T>();
            var tasks = new List<Task>();

            foreach (var pair in source.GroupBy(e => e))
            {
                distinctItems.Add(pair.Key);
                foreach (var duplicate in pair)
                {
                    if (!duplicate.Equals(pair.Key))
                    {
                        tasks.Add(DeleteAsync(duplicate.Id));
                    }
                }
            }

            await Task.WhenAll(tasks);
            return distinctItems;
        }

        private async Task DeleteAsync(string id)
        {
            var document = _workingCollection.Document(id);
            await document.DeleteAsync();
        }
    }
}
