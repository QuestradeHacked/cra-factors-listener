namespace CRA.FactorsListener.Cdc.Models
{
    public class CdcSource
    {
        public string Version { get; set; }

        public string Connector { get; set; }

        public string Name { get; set; }

        public long? TransactionTimestamp { get; set; }

        public string Snapshot { get; set; }

        public string Database { get; set; }

        public string Schema { get; set; }

        public string Table { get; set; }

        public string ChangeLsn { get; set; }

        public string CommitLst { get; set; }

        public int EventSerialNumber { get; set; }
    }
}