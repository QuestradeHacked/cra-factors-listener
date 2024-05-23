namespace CRA.FactorsListener.Cdc.Configs;

public class Resilience
{
    public int ConsecutiveExceptionsAllowedBeforeBreaking { get; set; } = 5;
    public int DurationOfBreakInSeconds { get; set; } = 15;
    public int RetryCount { get; } = 3;
}
