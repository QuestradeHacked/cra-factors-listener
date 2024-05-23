using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CRA.FactorsListener.Cdc.Models;
using CRA.FactorsListener.Cdc.Models.Enums;
using CRA.FactorsListener.Cdc.Models.Events;
using CRA.FactorsListener.Cdc.Services.EventHandlers;
using CRA.FactorsListener.Cdc.Services.UserMapping;
using Microsoft.Extensions.Logging;
using Moq;
using Questrade.Library.PubSubClientHelper.Primitives;
using Xunit;

namespace CRA.FactorsListener.UnitTests.Services.Handlers;

public class CdcInternationalAddressEventHandlerTests
{
    private const int CountryIdDefault = 5;
    private const string FoundUserId = "100";
    private const string NotPrimaryResidenceAddressId = "11";
    private const string PrimaryResidenceAddressId = "10";
    private const long TimestampDefault = 1000000;
        
    private readonly Mock<ILogger<CdcInternationalAddressEventHandler>> _loggerMock = new();
    private readonly Mock<IPublisherService<CountryChanged>> _pubSubPublisherMock = new();
    private readonly Mock<IUserMappingsService> _userMappingsServiceMock = new();
        
    private CdcInternationalAddressEventHandler CdcInternationalAddressEventHandler =>
        new(
            _userMappingsServiceMock.Object,
            _pubSubPublisherMock.Object,
            _loggerMock.Object);

    public CdcInternationalAddressEventHandlerTests()
    {
        Setup();
    }

    private void Setup()
    { 
        _userMappingsServiceMock
            .Setup(x => x.GetUsersByAddressAsync(PrimaryResidenceAddressId))
            .ReturnsAsync(new List<string> { FoundUserId });
        _userMappingsServiceMock
            .Setup(x => x.IsAddressPrimaryResidenceAsync(PrimaryResidenceAddressId))
            .ReturnsAsync(true);
        _userMappingsServiceMock
            .Setup(x => x.IsAddressPrimaryResidenceAsync(NotPrimaryResidenceAddressId))
            .ReturnsAsync(false);
    }

    [Fact]
    public async Task ProcessAsync_WhenCreateAPrimaryResidenceAddress_ShouldSendToPubSub()
    {
        // Arrange
        var message = new CdcEvent<InternationalAddress>
        {
            After = new InternationalAddress
            {
                CountryId = CountryIdDefault,
                InternationalAddressId = PrimaryResidenceAddressId
            },
            Before = null,
            Operation = OperationType.Create,
            TransactionTimestamp = TimestampDefault
        };

        // Act
        await CdcInternationalAddressEventHandler.ProcessAsync(message);

        // Assert
        _pubSubPublisherMock
            .Verify(x => x.PublishMessageAsync(
                    It.Is<CountryChanged>(countryChanged =>
                        countryChanged.Data.CountryId == CountryIdDefault &&
                        countryChanged.Data.Timestamp == TimestampDefault &&
                        countryChanged.Data.CustomerId == FoundUserId),
                    null),
                Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_WhenCreateNotPrimaryResidenceAddress_ShouldNotSendToPubSub()
    {
        // Arrange
        var message = new CdcEvent<InternationalAddress>
        {
            After = new InternationalAddress
            {
                InternationalAddressId = NotPrimaryResidenceAddressId
            },
            Before = null,
            Operation = OperationType.Create,
            TransactionTimestamp = TimestampDefault
        };

        // Act
        await CdcInternationalAddressEventHandler.ProcessAsync(message);

        // Assert
        _pubSubPublisherMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProcessAsync_WhenUpdateToPrimaryResidenceAddress_ShouldSendToPubSub()
    {
        // Arrange
        var message = new CdcEvent<InternationalAddress>
        {
            After = new InternationalAddress
            {
                CountryId = CountryIdDefault,
                InternationalAddressId = PrimaryResidenceAddressId
            },
            Before = new InternationalAddress
            {
                CountryId = CountryIdDefault,
                InternationalAddressId = NotPrimaryResidenceAddressId
            },
            Operation = OperationType.Create,
            TransactionTimestamp = TimestampDefault
        };

        // Act
        await CdcInternationalAddressEventHandler.ProcessAsync(message);

        // Assert
        _pubSubPublisherMock
            .Verify(x => x.PublishMessageAsync(
                    It.Is<CountryChanged>(countryChanged =>
                        countryChanged.Data.CountryId == CountryIdDefault &&
                        countryChanged.Data.Timestamp == TimestampDefault &&
                        countryChanged.Data.CustomerId == FoundUserId),
                    null),
                Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_WhenUpdateToNotPrimaryResidenceAddress_ShouldNotSendToPubSub()
    {
        // Arrange
        var message = new CdcEvent<InternationalAddress>
        {
            After = new InternationalAddress
            {
                InternationalAddressId = NotPrimaryResidenceAddressId
            },
            Before = new InternationalAddress
            {
                InternationalAddressId = PrimaryResidenceAddressId
            },
            Operation = OperationType.Create,
            TransactionTimestamp = TimestampDefault
        };

        // Act
        await CdcInternationalAddressEventHandler.ProcessAsync(message);

        // Assert
        _pubSubPublisherMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProcessAsync_WhenUserNotFound_ShouldIgnoreMessage()
    {
        // Arrange
        _userMappingsServiceMock
            .Setup(x => x.GetUsersByAddressAsync(PrimaryResidenceAddressId))
            .ReturnsAsync((Collection<string>)null);

        var message = new CdcEvent<InternationalAddress>
        {
            After = new InternationalAddress
            {
                InternationalAddressId = PrimaryResidenceAddressId
            },
            Before = null,
            Operation = OperationType.Create,
            TransactionTimestamp = TimestampDefault
        };

        // Act
        await CdcInternationalAddressEventHandler.ProcessAsync(message);

        // Assert
        _pubSubPublisherMock.VerifyNoOtherCalls();
    }
}
