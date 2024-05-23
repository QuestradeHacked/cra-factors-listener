using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CRA.FactorsListener.Cdc.Models;
using CRA.FactorsListener.Cdc.Models.Enums;
using CRA.FactorsListener.Cdc.Models.Events;
using CRA.FactorsListener.Cdc.Options;
using CRA.FactorsListener.Cdc.Services.EventHandlers;
using CRA.FactorsListener.Cdc.Services.UserMapping;
using Microsoft.Extensions.Logging;
using Moq;
using Questrade.Library.PubSubClientHelper.Primitives;
using Xunit;

namespace CRA.FactorsListener.UnitTests.Services.Handlers;

public class CdcDomesticAddressEventHandlerTests
{
    private const int DomesticCountryId = 16;
    private const string FoundUserId = "100";
    private const string NotPrimaryResidenceAddressId = "11";
    private const string PrimaryResidenceAddressId = "10";
    private const int PrimaryResidenceType = 1;
    private const int TimestampDefault = 1000000;
        
    private readonly Mock<ILogger<CdcDomesticAddressEventHandler>> _loggerMock = new();
    private readonly Mock<IUserMappingsService> _userMappingsServiceMock = new();
    private readonly Mock<IPublisherService<CountryChanged>> _pubSubPublisherMock = new();
        
    private static RiskFactorsOptions RiskFactorsOptions => new()
    {
        PrimaryResidenceType = PrimaryResidenceType,
        DomesticCountryId = DomesticCountryId
    };
        
    private CdcDomesticAddressEventHandler CdcDomesticAddressEventHandler => new(
        _userMappingsServiceMock.Object,
        RiskFactorsOptions,
        _pubSubPublisherMock.Object,
        _loggerMock.Object);

    public CdcDomesticAddressEventHandlerTests()
    {
        Setup();
    }

    private void Setup()
    {
        _userMappingsServiceMock
            .Setup(x => x.GetUsersByAddressAsync(PrimaryResidenceAddressId))
            .ReturnsAsync(new Collection<string>{FoundUserId});
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
        var message = new CdcEvent<DomesticAddress>
        {
            After = new DomesticAddress
            {
                DomesticAddressId = PrimaryResidenceAddressId
            },
            Before = null,
            Operation = OperationType.Create,
            TransactionTimestamp = TimestampDefault
        };

        // Act
        await CdcDomesticAddressEventHandler.ProcessAsync(message);

        // Assert
        _pubSubPublisherMock
            .Verify(x => x.PublishMessageAsync(
                    It.Is<CountryChanged>(countryChanged =>
                        countryChanged.Data.CountryId == DomesticCountryId &&
                        countryChanged.Data.Timestamp == TimestampDefault &&
                        countryChanged.Data.CustomerId == FoundUserId),
                    null),
                Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_WhenCreateNotPrimaryResidenceAddress_ShouldNotSendToPubSub()
    {
        // Arrange
        var message = new CdcEvent<DomesticAddress>
        {
            After = new DomesticAddress
            {
                DomesticAddressId = NotPrimaryResidenceAddressId
            },
            Before = null,
            Operation = OperationType.Create,
            TransactionTimestamp = TimestampDefault
        };

        // Act
        await CdcDomesticAddressEventHandler.ProcessAsync(message);

        // Assert
        _pubSubPublisherMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProcessAsync_WhenChangeToPrimaryResidenceAddress_ShouldSendToPubSub()
    {
        // Arrange
        var message = new CdcEvent<DomesticAddress>
        {
            After = new DomesticAddress
            {
                DomesticAddressId = PrimaryResidenceAddressId
            },
            Before = new DomesticAddress
            {
                DomesticAddressId = NotPrimaryResidenceAddressId
            },
            Operation = OperationType.Update,
            TransactionTimestamp = TimestampDefault
        };

        // Act
        await CdcDomesticAddressEventHandler.ProcessAsync(message);

        // Assert
        _pubSubPublisherMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProcessAsync_WhenChangeToNotPrimaryResidenceAddress_ShouldNotSendToPubSub()
    {
        // Arrange
        var message = new CdcEvent<DomesticAddress>
        {
            After = new DomesticAddress
            {
                DomesticAddressId = NotPrimaryResidenceAddressId
            },
            Before = new DomesticAddress
            {
                DomesticAddressId = PrimaryResidenceAddressId
            },
            Operation = OperationType.Update,
            TransactionTimestamp = TimestampDefault
        };

        // Act
        await CdcDomesticAddressEventHandler.ProcessAsync(message);

        // Assert
        _pubSubPublisherMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProcessAsync_WhenUserNotFound_ShouldIgnoreMessage()
    {
        // Arrange
        _userMappingsServiceMock
            .Setup(x => x.GetUsersByAddressAsync(PrimaryResidenceAddressId))
            .ReturnsAsync((Collection<string>) null);

        var message = new CdcEvent<DomesticAddress>
        {
            After = new DomesticAddress
            {
                DomesticAddressId = PrimaryResidenceAddressId
            },
            Before = null,
            Operation = OperationType.Create,
            TransactionTimestamp = TimestampDefault
        };

        // Act
        await CdcDomesticAddressEventHandler.ProcessAsync(message);

        // Assert
        _pubSubPublisherMock.VerifyNoOtherCalls();
    }
}
