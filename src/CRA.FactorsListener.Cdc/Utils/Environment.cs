﻿namespace CRA.FactorsListener.Cdc.Utils
{
    public static class Environment
    {
        public static string Name { get; set; } = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    }
}