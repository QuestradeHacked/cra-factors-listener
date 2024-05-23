using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CRA.FactorsListener.Domain.Entities;

namespace CRA.FactorsListener.Domain.Repositories
{
    public interface IBaseRepository<TEntity> where TEntity : BaseEntity
    {
        Task<TEntity> GetSingleByAsync(
            Expression<Func<TEntity, object>> property,
            object value,
            CancellationToken cancellationToken = default);

        Task<List<TEntity>> GetByAsync(
            Expression<Func<TEntity, object>> property,
            object value,
            CancellationToken cancellationToken = default);

        Task<string> UpsertAsync(TEntity entity, CancellationToken cancellationToken = default);
    }
}