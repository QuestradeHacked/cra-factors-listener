
using Questrade.Library.PubSubClientHelper.Subscriber.Default.Models;

namespace CRA.FactorsListener.Cdc.Models.Events
{
    public class AccountChanged : MessageWithMetadata<AccountChangedEnvelope>
    {
        public AccountChanged()
        {
        }

        public AccountChanged(AccountChangedEnvelope data) : base(data)
        {
        }
    }
}