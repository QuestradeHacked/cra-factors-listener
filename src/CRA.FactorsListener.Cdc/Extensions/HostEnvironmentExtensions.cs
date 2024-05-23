using System;
using Microsoft.Extensions.Hosting;

namespace CRA.FactorsListener.Cdc.Extensions
{
    public static class HostEnvironmentExtensions
    {
        public static bool IsSit(this IHostEnvironment environment)
            => environment.EnvironmentName.Equals("SIT", StringComparison.OrdinalIgnoreCase);

        public static bool IsUat(this IHostEnvironment environment)
            => environment.EnvironmentName.Equals("UAT", StringComparison.OrdinalIgnoreCase);

        public static bool IsProd(this IHostEnvironment environment)
            => environment.EnvironmentName.Equals("PROD", StringComparison.OrdinalIgnoreCase);
    }
}