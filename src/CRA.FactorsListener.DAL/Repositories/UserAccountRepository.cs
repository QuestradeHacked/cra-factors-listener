using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRA.FactorsListener.DAL.Clients;
using CRA.FactorsListener.Domain.Entities;
using CRA.FactorsListener.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace CRA.FactorsListener.DAL.Repositories
{
    public class UserAccountRepository : BaseRepository<UserAccount>, IUserAccountRepository
    {
        private readonly ILogger<UserAccountRepository> _logger;

        public UserAccountRepository(FirestoreClientFactory<UserAccount> firestoreClientFactory, ILogger<UserAccountRepository> logger)
            : base(firestoreClientFactory, "UserAccounts")
        {
            _logger = logger;
        }

        public async Task<UserAccount> GetByAccountId(string accountId, CancellationToken token = default)
        {
            return await GetSingleByAsync(account => account.AccountId, accountId, token);
        }

        public override async Task<string> UpsertAsync(UserAccount entity, CancellationToken cancellationToken = default)
        {
            var userAccounts = await GetByAsync(e => e.AccountId, entity.AccountId, cancellationToken);

            var storedUserAccount = userAccounts.FirstOrDefault(account => account.AccountId == entity.AccountId);

            if (!entity.Equals(storedUserAccount))
            {
                return await base.UpsertAsync(entity, cancellationToken);
            }

            _logger.LogInformation("UserAccount #{AccountId} is already saved", entity.AccountId);
            return storedUserAccount?.Id;
        }
    }
}
