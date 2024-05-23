namespace CRA.FactorsListener.Cdc.Models
{
    public class PoliticallyExposedPerson : ICdcMessage
    {
        public string PoliticallyExposedPersonId { get; set; }
        
        public string PersonId { get; set; }

        public int IsPoliticallyExposed { get; set; }
    }
}