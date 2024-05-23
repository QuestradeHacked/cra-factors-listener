using System;
using System.Collections.Generic;

namespace CRA.FactorsListener.Cdc.Services.Metrics
{
    public interface IMetricService
    {
        void Increment(string statName, List<string> tags);
        
        IDisposable StartTimer(string statName, List<string> tags = null);
    }
}