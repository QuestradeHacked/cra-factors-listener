using CRA.FactorsListener.Cdc.Models.Persons;
using CRA.FactorsListener.Domain.Entities;

namespace CRA.FactorsListener.UnitTests.Faker;

public abstract class PersonFaker
{
    private static readonly Bogus.Faker Faker = new();
    
    public static CrmPerson GenerateFakeCrmPerson => new()
    {
        PersonId = Faker.Random.Number().ToString(),
        AddressId = Faker.Random.Number(),
        AddressTypeId = Faker.Random.Number()
    };

    public static PersonAddress GenerateFakePersonAddress => new()
    {
        PersonId = Faker.Random.Number().ToString(),
        AddressId = Faker.Random.Number().ToString(),
        AddressType = Faker.Random.Number()
    };
}
