using CRA.FactorsListener.Cdc.Models.Users;
using CRA.FactorsListener.Domain.Entities;

namespace CRA.FactorsListener.UnitTests.Faker;

public class UserFaker
{
    private static readonly Bogus.Faker Faker = new();
    
    public static User GenerateFakeUser => new()
    {
        UserId = Faker.Random.Number().ToString(),
        PersonId = Faker.Random.Number().ToString()
    };

    public static UserAccount GenerateFakeUserAccount => new() {
        UserId = Faker.Random.Number().ToString(),
        AccountId = Faker.Random.Number().ToString()
    };

    public static CrmUserPerson GenerateCrmUserPerson => new() {
        UserId = Faker.Random.Number().ToString(),
        PersonId = Faker.Random.Number()
    };
}
