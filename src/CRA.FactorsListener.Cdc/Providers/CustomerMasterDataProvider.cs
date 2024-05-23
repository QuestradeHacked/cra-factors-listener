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
using CRA.FactorsListener.Cdc.Services.Metrics;
using CRA.FactorsListener.Cdc.Utils;
using CRA.FactorsListener.Domain.Entities;
using GraphQL;
using GraphQL.Client.Abstractions;
using Microsoft.Extensions.Logging;

namespace CRA.FactorsListener.Cdc.Providers;

public class CustomerMasterDataProvider : ICustomerMasterDataProvider
{
    private readonly CustomerMasterDataProviderConfig _customerMasterDataProviderConfig;
    private readonly IGraphQLClient _graphQlClient;
    private readonly ILogger<CustomerMasterDataProvider> _logger;
    private readonly IMapper _mapper;
    private readonly IMetricService _metricService;

    private const string StatCustomerMasterData = "customer_master_data";

    public CustomerMasterDataProvider(
        CustomerMasterDataProviderConfig customerMasterDataProviderConfig, 
        IGraphQLClient graphQlClient,
        ILogger<CustomerMasterDataProvider> logger,
        IMapper mapper,
        IMetricService metricService)
    {
        _customerMasterDataProviderConfig = customerMasterDataProviderConfig;
        _graphQlClient = graphQlClient;
        _logger = logger;
        _mapper = mapper;
        _metricService = metricService;
    }

    public async Task<UserAccount> GetUserAccountByAccountIdAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(accountId))
        {
            throw new ArgumentNullException(nameof(accountId));
        }

        var request = new GraphQLRequest
        {
            OperationName = "Account",
            Query = @"
                query Account($accountId: [ID]){
                    accountUsers(accountIds: $accountId) {
                        userId
                        accountId
                    }
                }",
            Variables = new
            {
                accountId
            }
        };

        var response = await SendQueryWithResilienceAsync<CrmUserAccountResponse>(request, cancellationToken);

        if (response.Errors?.Any() == true)
        {
            var exceptions = response.Errors.Select(error => new CrmQueryException(error.Message));
            var aggregateException = new AggregateException("GraphQL query error", exceptions);
            
            _logger.LogError(aggregateException, "GraphQL query error");
            
            _metricService.Increment(StatCustomerMasterData,
                new List<string> {"status:error", "message_type:account_users"});

            throw new CrmQueryException($"Invalid Account Id: {accountId}", aggregateException);
        }

        var count = response.Data.AccountUsers?.Count ?? 0;

        if (count > 1)
        {
            _logger.LogWarning(
                "GraphQL query resulted in {Count} of userAccounts for Account Id: {AccountId}",
                count,
                accountId);

            _metricService.Increment(StatCustomerMasterData,
                new List<string> {"status:error", "message_type:account_users"});

            return null;
        }

        _metricService.Increment(StatCustomerMasterData,
            new List<string> {"status:success", "message_type:account_users"});

        return response.Data.AccountUsers?.Single();
    }

    public async Task<User> GetUserByPersonIdAsync(
        string personId, 
        CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(personId, out var personIdAsInt))
        {
            throw new ArgumentNullException(nameof(personId));
        }

        var request = new GraphQLRequest
        {
            OperationName = "UserPerson",
            Query = @"
                query UserPerson($personId: Int) {
                    userPerson(userPersonQueryInput: {personID: $personId }) {
                        userId
                        personId
                    }
                }",
            Variables = new
            {
                personId = personIdAsInt
            }
        };

        var response = await SendQueryWithResilienceAsync<CrmUserDataResponse>(request, cancellationToken);

        if (response.Errors?.Any() == true)
        {
            var exceptions = response.Errors.Select(error => new CrmQueryException(error.Message));
            var aggregateException = new AggregateException("GraphQL query error", exceptions);
            
            _logger.LogError(aggregateException, "GraphQL query error");
            
            _metricService.Increment(StatCustomerMasterData,
                new List<string> {"status:error", "message_type:user_person"});

            throw new CrmQueryException($"Invalid Person Id: {personId}", aggregateException);
        }

        var count = response.Data.UserPerson?.Count ?? 0;

        if (count > 1)
        {
            _logger.LogWarning("GraphQL query resulted in {Count} of users for Person Id: {PersonId}",
                count,
                personId);

            _metricService.Increment(StatCustomerMasterData,
                new List<string> {"status:error", "message_type:user_person"});

            return null;
        }

        _metricService.Increment(StatCustomerMasterData,
            new List<string> {"status:success", "message_type:user_person"});

        return _mapper.Map<CrmUserPerson, User>(response.Data.UserPerson?.Single());
    }

    public async Task<IList<PersonAddress>> GetPersonsByAddressIdAsync(
        string addressId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(addressId))
        {
            throw new ArgumentNullException(nameof(addressId));
        }

        var request = new GraphQLRequest
        {
            OperationName = "Person",
            Query = @"
                query Person($addressId: ID){
                    person(addressId: $addressId){
                        personId
                        addressId,
                        addressTypeId
                    }
                }",
            Variables = new
            {
                addressId
            }
        };

        var response = await SendQueryWithResilienceAsync<CrmPersonDataResponse>(request, cancellationToken);
        
        _logger.BeginScope("{AddressId}", addressId);

        if (response.Errors?.Any() != true)
        {
            return _mapper.Map<IList<PersonAddress>>(response.Data.Person);
        }

        var exceptions = response.Errors.Select(error => new CrmQueryException(error.Message));
        var aggregateException = new AggregateException("GraphQL query error", exceptions);
        
        _logger.LogError(aggregateException, "GraphQL query error");
        
        _metricService.Increment(StatCustomerMasterData,
            new List<string> {"status:error", "message_type:person"});

        throw new CrmQueryException($"Invalid Address Id: {addressId}", aggregateException);
    }

    private async Task<GraphQLResponse<T>> SendQueryWithResilienceAsync<T>(
        GraphQLRequest request,
        CancellationToken cancellationToken)
    {
        return await GraphQlPolicies<GraphQLResponse<T>>
            .GetAllResiliencePolicies(_logger, _customerMasterDataProviderConfig.Resilience)
            .ExecuteAsync(async () => await _graphQlClient.SendQueryAsync<T>(request, cancellationToken));
    }
}
