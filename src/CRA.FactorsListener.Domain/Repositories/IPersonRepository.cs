using System.Threading;
using System.Threading.Tasks;
using CRA.FactorsListener.Domain.Entities;

namespace CRA.FactorsListener.Domain.Repositories
{
    public interface IPersonRepository : IBaseRepository<Person>
    {
        Task<Person> GetByPersonId(string personId, CancellationToken token = default);
    }
}