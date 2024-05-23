using System;

namespace CRA.FactorsListener.Cdc.Configs
{
    public class CustomerMasterDataProviderConfig
    {
        public string Token { get; set; }
        public Uri BaseUrl { get; set; }
        public Uri Endpoint => new($"{BaseUrl}graphql");
        public Resilience Resilience { get; set; }
    }
}