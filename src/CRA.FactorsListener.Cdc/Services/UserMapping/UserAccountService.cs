using System.Threading;
using System.Threading.Tasks;
using CRA.FactorsListener.Cdc.Providers;
using CRA.FactorsListener.Domain.Entities;
using CRA.FactorsListener.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace CRA.FactorsListener.Cdc.Services.UserMapping
{
    public class UserAccountService : IUserAccountService
    {
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly ICustomerMasterDataProvider _customerMasterDataProvider;
        private readonly ILogger<UserAccountService> _logger;

        public UserAccountService(
            IUserAccountRepository userAccountRepository,
            ICustomerMasterDataProvider customerMasterDataProvider,
            ILogger<UserAccountService> logger)
        {
            _userAccountRepository = userAccountRepository;
            _customerMasterDataProvider = customerMasterDataProvider;
            _logger = logger;
        }

        public async Task<UserAccount> GetByAccountId(string accountId, CancellationToken token = default)
        {
            var userAccount = await _userAccountRepository.GetByAccountId(accountId, token);

            if (userAccount != null)
            {
                return userAccount;
            }

            userAccount = await _customerMasterDataProvider.GetUserAccountByAccountIdAsync(accountId, token);

            if (userAccount == null)
            {
                return null;
            }

            await _userAccountRepository.UpsertAsync(userAccount, token);

            return userAccount;
        }
    }
}