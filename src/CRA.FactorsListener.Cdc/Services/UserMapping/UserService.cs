using System.Threading;
using System.Threading.Tasks;
using CRA.FactorsListener.Cdc.Providers;
using CRA.FactorsListener.Domain.Entities;
using CRA.FactorsListener.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace CRA.FactorsListener.Cdc.Services.UserMapping
{
    public class UserService : IUserService
    {
        private readonly ICustomerMasterDataProvider _customerMasterDataProvider;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            ICustomerMasterDataProvider customerMasterDataProvider,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _customerMasterDataProvider = customerMasterDataProvider;
            _logger = logger;
        }

        public async Task<User> GetByPersonId(string personId, CancellationToken token = default)
        {
            var user = await _userRepository.GetByPersonId(personId, token);

            if (user != null)
            {
                return user;
            }

            user = await _customerMasterDataProvider.GetUserByPersonIdAsync(personId, token);

            if (user == null)
            {
                return null;
            }

            await _userRepository.UpsertAsync(user, token);

            return user;
        }
    }
}