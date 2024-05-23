using System.Threading.Tasks;
using CRA.FactorsListener.Cdc.Models;

namespace CRA.FactorsListener.Cdc.Services.EventHandlers
{
    public interface ICdcEventHandler<TMessage> where TMessage : ICdcMessage
    {
        Task ProcessAsync(CdcEvent<TMessage> message);
    }
}