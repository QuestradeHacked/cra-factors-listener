namespace CRA.FactorsListener.Cdc.Models
{
    public class InternationalAddress : ICdcMessage
    {
        public string InternationalAddressId { get; set; }

        public int CountryId { get; set; }
    }
}