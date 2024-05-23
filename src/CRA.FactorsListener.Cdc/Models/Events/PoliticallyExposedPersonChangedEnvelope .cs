namespace CRA.FactorsListener.Cdc.Models.Events
{
    public class PoliticallyExposedPersonChangedEnvelope :  FactorChangedBase
    {
        public PoliticallyExposedPersonChangedEnvelope(string customerId, long? timestamp) : base(customerId, timestamp)
        {
        }

        public bool IsPoliticallyExposed { get; set; }
    }
}