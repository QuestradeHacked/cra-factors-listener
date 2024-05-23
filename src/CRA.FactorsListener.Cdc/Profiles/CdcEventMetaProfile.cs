using AutoMapper;
using CRA.FactorsListener.Cdc.Models;
using io.confluent.connect.avro;
using io.debezium.connector.sqlserver;

namespace CRA.FactorsListener.Cdc.Profiles
{
    public class CdcEventMetaProfile : Profile
    {
        public CdcEventMetaProfile()
        {
            CreateMap<ConnectDefault, CdcTransactionInfo>()
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.id))
                .ForMember(dst => dst.TotalOrder, opt => opt.MapFrom(src => src.total_order))
                .ForMember(dst => dst.DataCollectionOrder, opt => opt.MapFrom(src => src.data_collection_order));

            CreateMap<Source, CdcSource>()
                .ForMember(dst => dst.ChangeLsn, opt => opt.MapFrom(src => src.change_lsn))
                .ForMember(dst => dst.Connector, opt => opt.MapFrom(src => src.connector))
                .ForMember(dst => dst.CommitLst, opt => opt.MapFrom(src => src.commit_lsn))
                .ForMember(dst => dst.Database, opt => opt.MapFrom(src => src.db))
                .ForMember(dst => dst.EventSerialNumber, opt => opt.MapFrom(src => src.event_serial_no))
                .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.name))
                .ForMember(dst => dst.Snapshot, opt => opt.MapFrom(src => src.snapshot))
                .ForMember(dst => dst.Table, opt => opt.MapFrom(src => src.table))
                .ForMember(dst => dst.TransactionTimestamp, opt => opt.MapFrom(src => src.ts_ms))
                .ForMember(dst => dst.Version, opt => opt.MapFrom(src => src.version));
        }
    }
}