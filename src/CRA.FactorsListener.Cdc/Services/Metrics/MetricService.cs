using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using StatsdClient;

namespace CRA.FactorsListener.Cdc.Services.Metrics
{
    public class MetricService : IMetricService
    {
        private readonly ILogger<MetricService> _logger;

        private readonly IDogStatsd _dogStatsd;

        public MetricService(IDogStatsd dogStatsd, ILogger<MetricService> logger)
        {
            _dogStatsd = dogStatsd;
            _logger = logger;
        }

        public void Increment(string statName, List<string> tags)
        {
            try
            {
                tags ??= new List<string>();

                _dogStatsd.Increment(statName, tags: tags.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to increment metric, {@Error}, {@StatName} {@Tags}", ex.Message, statName, tags);
            }
        }

        public IDisposable StartTimer(string statName, List<string> tags = null)
        {
            tags ??= new List<string>();
            return _dogStatsd.StartTimer(statName, tags: tags.ToArray());
        }
    }
}