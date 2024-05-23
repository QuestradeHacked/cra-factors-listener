using System.Collections.Generic;
using System.Threading.Tasks;
using CRA.FactorsListener.Cdc.Extensions;

namespace CRA.FactorsListener.Cdc.Services.UserMapping
{
    public class UserMappingsService : IUserMappingsService
    {
        private readonly IUserAccountService _userAccountService;
        private readonly IPersonAddressService _personAddressService;
        private readonly IUserService _userService;

        public UserMappingsService(
            IUserAccountService userAccountService,
            IPersonAddressService personAddressService,
            IUserService userService)
        {
            _userAccountService = userAccountService;
            _personAddressService = personAddressService;
            _userService = userService;
        }

        public async Task<string> GetUserByAccountAsync(string accountId)
        {
            var userAccount = await _userAccountService.GetByAccountId(accountId);

            return userAccount?.UserId;
        }

        public async Task<ICollection<string>> GetUsersByAddressAsync(string addressId)
        {
            var personAddresses = await _personAddressService.GetPrimaryAddressesByAddressId(addressId);

            if (personAddresses.IsNullOrEmpty())
            {
                return null;
            }

            var userIds = new HashSet<string>();

            foreach (var address in personAddresses)
            {
                var user = await _userService.GetByPersonId(address.PersonId);

                if (user != null)
                {
                    userIds.Add(user.UserId);
                }
            }

            return userIds;
        }

        /// <inheritdoc cref="IUserMappingsService.GetUserByPersonAsync"/>
        public async Task<string> GetUserByPersonAsync(string personId)
        {
            var user = await _userService.GetByPersonId(personId);

            return user?.UserId;
        }

        public async Task<bool> IsAddressPrimaryResidenceAsync(string addressId)
        {
            var personAddress = await _personAddressService.GetPrimaryAddressesByAddressId(addressId);

            return !personAddress.IsNullOrEmpty();
        }
    }
}