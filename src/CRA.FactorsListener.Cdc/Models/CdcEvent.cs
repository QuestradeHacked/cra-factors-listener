#nullable enable
using CRA.FactorsListener.Cdc.Models.Enums;

namespace CRA.FactorsListener.Cdc.Models
{
    public class CdcEvent<TModel>
    {
        public TModel? Before { get; set; }

        public TModel? After { get; set; }

        public CdcSource? Source { get; set; }

        public OperationType Operation { get; set; }

        public long? TransactionTimestamp { get; set; }

        public CdcTransactionInfo? TransactionInfo { get; set; }

        public TModel GetLastSnapshot()
        {
            return After != null ? After : Before!;
        }

        public bool IsNewRecord()
        {
            return Operation == OperationType.Read || Operation == OperationType.Create;
        }

        public bool IsUpdated()
        {
            return Operation == OperationType.Update;
        }

        public bool IsDeleted()
        {
            return Operation == OperationType.Delete;
        }
    }
}
