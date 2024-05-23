using System;
using CRA.FactorsListener.Cdc.Models;
using CRA.FactorsListener.Cdc.Models.Enums;
using CRA.FactorsListener.Domain.Entities;

namespace CRA.FactorsListener.Cdc.Extensions
{
    public static class PersonAddressExtensions
    {
        public static bool IsValueUpdated(this CdcEvent<PersonAddress> self)
        {
            if (self.Operation != OperationType.Update)
            {
                throw new InvalidOperationException("This method compares values for update messages only");
            }

            var before = self.Before;
            var after = self.After;

            return before.PersonId != after.PersonId ||
                   before.AddressId != after.AddressId ||
                   before.AddressType != after.AddressType;
        }
    }
}