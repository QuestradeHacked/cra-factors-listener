using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CRA.FactorsListener.DAL.Clients;
using CRA.FactorsListener.Domain.Entities;
using CRA.FactorsListener.Domain.Repositories;

namespace CRA.FactorsListener.DAL.Repositories
{
    public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity
    {
        protected readonly FirestoreClient<TEntity> FirestoreClient;

        protected BaseRepository(FirestoreClientFactory<TEntity> firestoreClientFactory, string collection)
        {
            FirestoreClient = firestoreClientFactory.Create(collection);
        }

        public async Task<List<TEntity>> GetByAsync(
            Expression<Func<TEntity, object>> property,
            object value,
            CancellationToken cancellationToken = default)
        {
            return await FirestoreClient.GetByAsync(property, value, cancellationToken);
        }

        public async Task<TEntity> GetSingleByAsync(
            Expression<Func<TEntity, object>> property,
            object value,
            CancellationToken cancellationToken = default)
        {
            var entities = await FirestoreClient.GetByAsync(property, value, cancellationToken);
            return entities.SingleOrDefault();
        }

        public virtual async Task<string> UpsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var createdEntity = await FirestoreClient.UpsertAsync(entity, cancellationToken);

            return createdEntity.Id;
        }
    }
}