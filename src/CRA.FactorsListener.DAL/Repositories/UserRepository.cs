using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRA.FactorsListener.DAL.Clients;
using CRA.FactorsListener.Domain.Entities;
using CRA.FactorsListener.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace CRA.FactorsListener.DAL.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(FirestoreClientFactory<User> firestoreClientFactory, ILogger<UserRepository> logger)
            : base(firestoreClientFactory, "Users")
        {
            _logger = logger;
        }

        public async Task<User> GetByPersonId(string personId, CancellationToken token = default)
        {
            var persons = await GetByAsync(p => p.PersonId, personId, token);

            if (persons?.Count > 1)
            {
                _logger.LogWarning("Found more than 1 person by person id: {Id}", personId);
            }

            return persons?.FirstOrDefault();
        }

        public override async Task<string> UpsertAsync(User entity, CancellationToken cancellationToken = default)
        {
            var storedUser = await GetSingleByAsync(e => e.UserId, entity.UserId, cancellationToken);

            if (!entity.Equals(storedUser))
            {
                return await base.UpsertAsync(entity, cancellationToken);
            }

            _logger.LogInformation("User #{UserId} is already saved", entity.UserId);
            return storedUser.Id;
        }
    }
}
