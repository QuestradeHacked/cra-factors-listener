
using Questrade.Library.PubSubClientHelper.Subscriber.Default.Models;

namespace CRA.FactorsListener.Cdc.Models.Events
{
    public class PoliticallyExposedPersonChanged : MessageWithMetadata<PoliticallyExposedPersonChangedEnvelope>
    {
        public PoliticallyExposedPersonChanged()
        {
        }
        
        public PoliticallyExposedPersonChanged(PoliticallyExposedPersonChangedEnvelope data) : base(data)
        {
        }
    }
}