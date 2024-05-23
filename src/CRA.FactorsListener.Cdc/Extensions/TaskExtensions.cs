using System.Threading.Tasks;

namespace CRA.FactorsListener.Cdc.Extensions
{
    public static class TaskExtensions
    {
        public static void Await(this Task task) => task.ConfigureAwait(false).GetAwaiter().GetResult();
    }
}