using System.Collections.Generic;
using System.Linq;

namespace CRA.FactorsListener.Cdc.Extensions;

public static class EnumerableExtensions
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> self)
    {
        return self == null || !self.Any();
    }
}