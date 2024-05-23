namespace CRA.FactorsListener.Cdc.Models.Accounts
{
    public class Account : ICdcMessage
    {
        public string AccountId { get; set; }

        public string AccountStatusId { get; set; }
    }
}