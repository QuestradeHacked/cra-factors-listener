namespace CRA.FactorsListener.Cdc.Models.Events
{
    public class CountryChangedEnvelope : FactorChangedBase
    {
        public CountryChangedEnvelope(string customerId, int countryId, long? timestamp) : base(customerId, timestamp)
        {
            CountryId = countryId;
        }

        public int CountryId { get; }
    }
}