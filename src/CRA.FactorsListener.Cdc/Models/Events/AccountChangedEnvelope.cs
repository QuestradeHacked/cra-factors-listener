namespace CRA.FactorsListener.Cdc.Models.Events
{
    public class AccountChangedEnvelope : FactorChangedBase
    {
        public AccountChangedEnvelope(string customerId, string accountId, long? timestamp) : base(customerId, timestamp)
        {
            AccountId = accountId;
        }

        public string AccountId { get; }

        public string AccountStatus { get; set; }
    }
}