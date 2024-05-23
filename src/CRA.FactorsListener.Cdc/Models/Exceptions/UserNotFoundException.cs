using System;
using System.Runtime.Serialization;
using CRA.FactorsListener.Cdc.Models.Accounts;

namespace CRA.FactorsListener.Cdc.Extensions;

[Serializable]
public class UserNotFoundException : Exception
{
    public UserNotFoundException(Account account)
    : base($"User not found. AccountId: {account.AccountId}")
    {

    }

    protected UserNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
