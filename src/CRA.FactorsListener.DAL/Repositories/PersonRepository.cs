using System.Threading;
using System.Threading.Tasks;
using CRA.FactorsListener.DAL.Clients;
using CRA.FactorsListener.Domain.Entities;
using CRA.FactorsListener.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace CRA.FactorsListener.DAL.Repositories
{
    public class PersonRepository : BaseRepository<Person>, IPersonRepository
    {
        private readonly ILogger<PersonRepository> _logger;

        public PersonRepository(FirestoreClientFactory<Person> firestoreClientFactory, ILogger<PersonRepository> logger)
            : base(firestoreClientFactory, "Persons")
        {
            _logger = logger;
        }

        public async Task<Person> GetByPersonId(string personId, CancellationToken token = default)
        {
            return await GetSingleByAsync(p => p.PersonId, personId, token);
        }

        public override async Task<string> UpsertAsync(Person entity, CancellationToken cancellationToken = default)
        {
            var storedPerson = await GetByPersonId(entity.PersonId, cancellationToken);

            if (!entity.Equals(storedPerson))
            {
                return await base.UpsertAsync(entity, cancellationToken);
            }

            _logger.LogInformation("Person #{PersonId} is already saved", entity.PersonId);
            return storedPerson.Id;
        }
    }
}
