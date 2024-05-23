using System.Text.Json.Serialization;

namespace CRA.FactorsListener.Cdc.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EmploymentType
{
    Unknown = 1,
    Employed,
    SelfEmployed,
    Student,
    Homemaker,
    Unemployed,
    Retired,
}
