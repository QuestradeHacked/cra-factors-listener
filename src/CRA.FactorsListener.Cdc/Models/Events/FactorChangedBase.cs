using System;

namespace CRA.FactorsListener.Cdc.Models.Events
{
    public abstract class FactorChangedBase
    {
        public FactorChangedBase(string customerId, long? timestamp)
        {
            CustomerId = customerId;
            Timestamp = timestamp ?? DateTime.UtcNow.Ticks;
        }

        public string CustomerId { get; }

        public long Timestamp { get; set; }
    }
}