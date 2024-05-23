
using Questrade.Library.PubSubClientHelper.Subscriber.Default.Models;

namespace CRA.FactorsListener.Cdc.Models.Events
{
    public class PersonEmploymentChanged : MessageWithMetadata<PersonEmploymentChangedEnvelope>
    {
        public PersonEmploymentChanged()
        {
        }
        
        public PersonEmploymentChanged(PersonEmploymentChangedEnvelope data) : base(data)
        {
        }
    }
}