
using Questrade.Library.PubSubClientHelper.Subscriber.Default.Models;

namespace CRA.FactorsListener.Cdc.Models.Events
{
    public class CountryChanged : MessageWithMetadata<CountryChangedEnvelope>
    {
        public CountryChanged()
        {
        }
        
        public CountryChanged(CountryChangedEnvelope data) : base(data)
        {
        }
    }
}