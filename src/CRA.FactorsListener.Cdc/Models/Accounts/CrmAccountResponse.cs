using System.Collections.Generic;
using CRA.FactorsListener.Domain.Entities;

namespace CRA.FactorsListener.Cdc.Models.Accounts
{
    public class CrmUserAccountResponse
    {
        public IList<UserAccount> AccountUsers { get; set; }
    }
}