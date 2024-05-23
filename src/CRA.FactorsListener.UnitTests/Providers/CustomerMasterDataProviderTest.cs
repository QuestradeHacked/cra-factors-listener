using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CRA.FactorsListener.Cdc.Configs;
using CRA.FactorsListener.Cdc.Extensions;
using CRA.FactorsListener.Cdc.Models.Accounts;
using CRA.FactorsListener.Cdc.Models.Persons;
using CRA.FactorsListener.Cdc.Models.Users;
using CRA.FactorsListener.Cdc.Providers;
using CRA.FactorsListener.Cdc.Services.Metrics;
using CRA.FactorsListener.Domain.Entities;
using CRA.FactorsListener.UnitTests.Faker;
using GraphQL;
using GraphQL.Client.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRA.FactorsListener.UnitTests.Providers;

public class CustomerMasterDataProviderTest
{
    private readonly Mock<IGraphQLClient> _graphQlClientMock = new();
    private readonly Mock<ILogger<CustomerMasterDataProvider>> _loggerMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IMetricService> _metricService = new();

    private GraphQLResponse<CrmPersonDataResponse> _crmPersonDataResponse;
    private GraphQLResponse<CrmUserAccountResponse> _crmUserAccountResponse;
    private GraphQLResponse<CrmUserDataResponse> _crmUserDataResponse;

    private readonly CustomerMasterDataProviderConfig _customerMasterDataProviderConfig =
        new()
        {
            Resilience = new Resilience()
        };

    private CustomerMasterDataProvider CustomerMasterDataProvider => new(
        _customerMasterDataProviderConfig,
        _graphQlClientMock.Object,
        _loggerMock.Object,
        _mapperMock.Object,
        _metricService.Object);

    private void Setup()
    {
        _crmUserAccountResponse = new GraphQLResponse<CrmUserAccountResponse>();
        _crmUserDataResponse = new GraphQLResponse<CrmUserDataResponse>();
        _crmPersonDataResponse = new GraphQLResponse<CrmPersonDataResponse>();
    }

    public CustomerMasterDataProviderTest()
    {
        Setup();
    }

    [Fact]
    public async Task GetUserAccountByAccountIdAsync_WhenThereIsNoAccountId_ShouldThrowException()
    {
        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => CustomerMasterDataProvider.GetUserAccountByAccountIdAsync(string.Empty));
    }

    [Fact]
    public async Task GetUserAccountByAccountIdAsync_WhenReceiveValidData_ShouldReturnUserAccount()
    {
        // Arrange
        var generatedFakeUserAccount = UserFaker.GenerateFakeUserAccount;
        
        _crmUserAccountResponse.Data = new CrmUserAccountResponse()
        {
            AccountUsers = new List<UserAccount> { generatedFakeUserAccount }
        };

        _graphQlClientMock
            .Setup(x => x.SendQueryAsync<CrmUserAccountResponse>(It.IsAny<GraphQLRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_crmUserAccountResponse);

        // Act
        var result =
            await CustomerMasterDataProvider.GetUserAccountByAccountIdAsync(UserFaker.GenerateFakeUserAccount
                .AccountId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(result, generatedFakeUserAccount);
    }

    [Fact]
    public async Task GetUserAccountByAccountIdAsync_WhenTheQueryContainsError_ShouldThrowException()
    {
        // Arrange
        _crmUserAccountResponse.Errors = new[]
        {
            new GraphQLError { Message = "Test error message" }
        };

        _graphQlClientMock
            .Setup(x => x.SendQueryAsync<CrmUserAccountResponse>(It.IsAny<GraphQLRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_crmUserAccountResponse);

        // Assert
        await Assert.ThrowsAsync<CrmQueryException>(
            () => CustomerMasterDataProvider.GetUserAccountByAccountIdAsync(UserFaker.GenerateFakeUserAccount
                .AccountId));
    }

    [Fact]
    public async Task GetUserAccountByAccountIdAsync_WhenMoreThanOneUserAccountReturned_ShouldNotThrowException()
    {
        // Arrange
        _crmUserAccountResponse.Data = new CrmUserAccountResponse
        {
            AccountUsers = new List<UserAccount>
                { UserFaker.GenerateFakeUserAccount, UserFaker.GenerateFakeUserAccount }
        };

        _graphQlClientMock
            .Setup(x => x.SendQueryAsync<CrmUserAccountResponse>(It.IsAny<GraphQLRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_crmUserAccountResponse);

        // Act
        var result =
            await CustomerMasterDataProvider.GetUserAccountByAccountIdAsync(UserFaker.GenerateFakeUserAccount
                .AccountId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserAccountByAccountIdAsync_WhenThereIsNoAccountUsers_ShouldReturnNull()
    {
        // Arrange
        _crmUserAccountResponse.Data = new CrmUserAccountResponse
        {
            AccountUsers = null
        };

        _graphQlClientMock
            .Setup(x => x.SendQueryAsync<CrmUserAccountResponse>(It.IsAny<GraphQLRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_crmUserAccountResponse);

        // Act
        var result =
            await CustomerMasterDataProvider.GetUserAccountByAccountIdAsync(UserFaker.GenerateFakeUserAccount
                .AccountId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByPersonIdAsync_WhenThereIsNoPersonId_ShouldThrowException()
    {
        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => CustomerMasterDataProvider.GetUserByPersonIdAsync(string.Empty));
    }

    [Fact]
    public async Task GetUserByPersonIdAsync_WhenReceiveValidData_ShouldReturnUser()
    {
        // Arrange
        var generatedFakeUser = UserFaker.GenerateFakeUser;
        
        _crmUserDataResponse.Data = new CrmUserDataResponse
        {
            UserPerson = new List<CrmUserPerson> { UserFaker.GenerateCrmUserPerson }
        };

        _graphQlClientMock
            .Setup(x => x.SendQueryAsync<CrmUserDataResponse>(It.IsAny<GraphQLRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_crmUserDataResponse);

        _mapperMock.Setup(x => x.Map<CrmUserPerson, User>(It.IsAny<CrmUserPerson>()))
            .Returns(generatedFakeUser);

        // Act
        var result =
            await CustomerMasterDataProvider.GetUserByPersonIdAsync(UserFaker.GenerateCrmUserPerson.PersonId
                .ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(result, generatedFakeUser);
    }

    [Fact]
    public async Task GetUserByPersonIdAsync_WhenTheQueryContainsError_ShouldThrowException()
    {
        // Arrange
        _crmUserDataResponse.Errors = new[]
        {
            new GraphQLError { Message = "Test error message" }
        };

        _graphQlClientMock
            .Setup(x => x.SendQueryAsync<CrmUserDataResponse>(It.IsAny<GraphQLRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_crmUserDataResponse);

        // Assert
        await Assert.ThrowsAsync<CrmQueryException>(
            () => CustomerMasterDataProvider.GetUserByPersonIdAsync(UserFaker.GenerateCrmUserPerson.PersonId
                .ToString()));
    }

    [Fact]
    public async Task GetUserByPersonIdAsync_WhenMoreThanOneUserReturned_ShouldNotThrowException()
    {
        // Arrange
        _crmUserDataResponse.Data = new CrmUserDataResponse
        {
            UserPerson = new List<CrmUserPerson> { UserFaker.GenerateCrmUserPerson, UserFaker.GenerateCrmUserPerson }
        };

        _graphQlClientMock
            .Setup(x => x.SendQueryAsync<CrmUserDataResponse>(It.IsAny<GraphQLRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_crmUserDataResponse);

        // Act
        var result =
            await CustomerMasterDataProvider.GetUserByPersonIdAsync(UserFaker.GenerateCrmUserPerson.PersonId
                .ToString());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByPersonIdAsync_WhenThereIsNoUserPerson_ShouldReturnNull()
    {
        // Arrange
        _crmUserDataResponse.Data = new CrmUserDataResponse()
        {
            UserPerson = null
        };

        _graphQlClientMock
            .Setup(x => x.SendQueryAsync<CrmUserDataResponse>(It.IsAny<GraphQLRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_crmUserDataResponse);

        // Act
        var result =
            await CustomerMasterDataProvider.GetUserByPersonIdAsync(UserFaker.GenerateCrmUserPerson.PersonId
                .ToString());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPersonByAddressIdAsync_WhenThereIsNoAddressId_ShouldThrowException()
    {
        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => CustomerMasterDataProvider.GetPersonsByAddressIdAsync(string.Empty));
    }

    [Fact]
    public async Task GetPersonByAddressIdAsync_WhenReceiveValidData_ShouldReturnPerson()
    {
        // Arrange
        var generatedFakePersonAddress = PersonFaker.GenerateFakePersonAddress;
        
        _crmPersonDataResponse.Data = new CrmPersonDataResponse
        {
            Person = new List<CrmPerson> { PersonFaker.GenerateFakeCrmPerson }
        };

        _graphQlClientMock
            .Setup(x => x.SendQueryAsync<CrmPersonDataResponse>(It.IsAny<GraphQLRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_crmPersonDataResponse);

        _mapperMock.Setup(x => x.Map<IList<PersonAddress>>(It.IsAny<IList<CrmPerson>>()))
            .Returns(new List<PersonAddress> { generatedFakePersonAddress });

        // Act
        var result =
            await CustomerMasterDataProvider.GetPersonsByAddressIdAsync(UserFaker.GenerateCrmUserPerson.PersonId
                .ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(result.Single(), generatedFakePersonAddress);
    }

    [Fact]
    public async Task GetPersonByAddressIdAsync_WhenTheQueryContainsError_ShouldThrowException()
    {
        // Arrange
        _crmPersonDataResponse.Errors = new[]
        {
            new GraphQLError { Message = "Test error message" }
        };

        _graphQlClientMock
            .Setup(x => x.SendQueryAsync<CrmPersonDataResponse>(It.IsAny<GraphQLRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_crmPersonDataResponse);

        // Assert
        await Assert.ThrowsAsync<CrmQueryException>(
            () => CustomerMasterDataProvider.GetPersonsByAddressIdAsync(UserFaker.GenerateCrmUserPerson.PersonId
                .ToString()));
    }

    [Fact]
    public async Task GetPersonByAddressIdAsync_WhenMoreThanOnePersonReturned_ShouldNotThrowException()
    {
        // Arrange
        _crmPersonDataResponse.Data = new CrmPersonDataResponse()
        {
            Person = new List<CrmPerson> { PersonFaker.GenerateFakeCrmPerson, PersonFaker.GenerateFakeCrmPerson }
        };

        _graphQlClientMock
            .Setup(x => x.SendQueryAsync<CrmPersonDataResponse>(It.IsAny<GraphQLRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_crmPersonDataResponse);

        // Act
        var result =
            await CustomerMasterDataProvider.GetPersonsByAddressIdAsync(UserFaker.GenerateCrmUserPerson.PersonId
                .ToString());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPersonByAddressIdAsync_WhenThereIsNoPerson_ShouldReturnNull()
    {
        // Arrange
        _crmPersonDataResponse.Data = new CrmPersonDataResponse()
        {
            Person = null
        };

        _graphQlClientMock
            .Setup(x => x.SendQueryAsync<CrmPersonDataResponse>(It.IsAny<GraphQLRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_crmPersonDataResponse);

        _mapperMock.Setup(x => x.Map<CrmPerson, PersonAddress>(It.IsAny<CrmPerson>()))
            .Returns(PersonFaker.GenerateFakePersonAddress);

        // Act
        var result =
            await CustomerMasterDataProvider.GetPersonsByAddressIdAsync(UserFaker.GenerateCrmUserPerson.PersonId
                .ToString());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByPersonIdAsync_WhenRetriesCall_ShouldThrowException()
    {
        // Arrange
        _crmUserDataResponse.Data = new CrmUserDataResponse();
        _graphQlClientMock
            .SetupSequence(x =>
                x.SendQueryAsync<CrmUserDataResponse>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .Throws<Exception>()
            .ReturnsAsync(_crmUserDataResponse);

        // Act
        await CustomerMasterDataProvider.GetUserByPersonIdAsync(UserFaker.GenerateCrmUserPerson.UserId);

        // Assert
        _graphQlClientMock.Verify(
            client => client.SendQueryAsync<CrmUserDataResponse>(It.IsAny<GraphQLRequest>(),
                It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetUserAccountByAccountIdAsync_WhenRetriesCall_ShouldThrowException()
    {
        // Arrange
        _crmUserAccountResponse.Data = new CrmUserAccountResponse();
        _graphQlClientMock
            .SetupSequence(x =>
                x.SendQueryAsync<CrmUserAccountResponse>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .Throws<Exception>()
            .ReturnsAsync(_crmUserAccountResponse);

        // Act
        await CustomerMasterDataProvider.GetUserAccountByAccountIdAsync(UserFaker.GenerateCrmUserPerson.UserId);

        // Assert
        _graphQlClientMock.Verify(
            client => client.SendQueryAsync<CrmUserAccountResponse>(It.IsAny<GraphQLRequest>(),
                It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
