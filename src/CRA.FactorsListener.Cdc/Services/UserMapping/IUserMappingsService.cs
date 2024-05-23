using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRA.FactorsListener.Cdc.Services.UserMapping
{
    public interface IUserMappingsService
    {
        Task<string> GetUserByAccountAsync(string accountId);
        
        /// <summary>
        /// One personAddress might be linked to different persons
        /// </summary>
        /// <param name="addressId">address id</param>
        /// <returns></returns>
        Task<ICollection<string>> GetUsersByAddressAsync(string addressId);
        
        /// <summary>
        /// According to business rules and the CRM scheme,
        /// it is expected that a person may not have a corresponding user record, for example, for spouses
        /// </summary>
        /// <param name="personId">Person id</param>
        /// <returns>User id if it's found or null otherwise</returns>
        Task<string> GetUserByPersonAsync(string personId);

        Task<bool> IsAddressPrimaryResidenceAsync(string addressId);
    }
}