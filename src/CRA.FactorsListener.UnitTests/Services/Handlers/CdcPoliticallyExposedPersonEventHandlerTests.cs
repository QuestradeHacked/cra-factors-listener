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

public class CdcPoliticallyExposedPersonEventHandlerTests
{
    private const string FoundUserId = "100";
    private const string PersonIdDefault = "1";
    private const long TimestampDefault = 1000000;
        
    private readonly Mock<ILogger<CdcPoliticallyExposedPersonEventHandler>> _loggerMock = new();
    private readonly Mock<IPublisherService<PoliticallyExposedPersonChanged>> _pubSubPublisherMock = new();
    private readonly Mock<IUserMappingsService> _userMappingsServiceMock = new();
        
    private CdcPoliticallyExposedPersonEventHandler CdcPoliticallyExposedPersonEventHandler => new(
        _userMappingsServiceMock.Object,
        _pubSubPublisherMock.Object, 
        _loggerMock.Object);

    public CdcPoliticallyExposedPersonEventHandlerTests()
    {
        Setup();
    }
        
    private void Setup()
    {
        _userMappingsServiceMock
            .Setup(x => x.GetUserByPersonAsync(PersonIdDefault))
            .ReturnsAsync(FoundUserId);
    }
        
    [Fact]
    public async Task ProcessAsync_WhenAddPoliticallyExposedPerson_ShouldSendToPubSub()
    {
        // Arrange
        var message = new CdcEvent<PoliticallyExposedPerson>
        {
            After = new PoliticallyExposedPerson
            {
                PersonId = PersonIdDefault,
                IsPoliticallyExposed = 1
            },
            Before = null,
            Operation = OperationType.Create,
            TransactionTimestamp = TimestampDefault
        };

        // Act
        await CdcPoliticallyExposedPersonEventHandler.ProcessAsync(message);

        // Assert
        _pubSubPublisherMock
            .Verify(x => x.PublishMessageAsync(
                    It.Is<PoliticallyExposedPersonChanged>(politicallyExposedPersonChanged => 
                        politicallyExposedPersonChanged.Data.IsPoliticallyExposed == true &&
                        politicallyExposedPersonChanged.Data.Timestamp == TimestampDefault &&
                        politicallyExposedPersonChanged.Data.CustomerId == FoundUserId),
                    null), 
                Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_WhenChangeToPoliticallyExposedPerson_ShouldSendToPubSub()
    {
        // Arrange
        var message = new CdcEvent<PoliticallyExposedPerson>
        {
            After = new PoliticallyExposedPerson
            {
                IsPoliticallyExposed = 1,
                PersonId = PersonIdDefault
            },
            Before = new PoliticallyExposedPerson
            {
                IsPoliticallyExposed = 0,
                PersonId = PersonIdDefault
            },
            Operation = OperationType.Update,
            TransactionTimestamp = TimestampDefault
        };

        // Act
        await CdcPoliticallyExposedPersonEventHandler.ProcessAsync(message);

        // Assert
        _pubSubPublisherMock
            .Verify(x => x.PublishMessageAsync(
                    It.Is<PoliticallyExposedPersonChanged>(politicallyExposedPersonChanged => 
                        politicallyExposedPersonChanged.Data.IsPoliticallyExposed == true &&
                        politicallyExposedPersonChanged.Data.Timestamp == TimestampDefault &&
                        politicallyExposedPersonChanged.Data.CustomerId == FoundUserId),
                    null), 
                Times.Once);
    }
        
    [Fact]
    public async Task ProcessAsync_WhenChangeToNotPoliticallyExposedPerson_ShouldSendToPubSub()
    {
        // Arrange
        var message = new CdcEvent<PoliticallyExposedPerson>
        {
            After = new PoliticallyExposedPerson
            {
                IsPoliticallyExposed = 0,
                PersonId = PersonIdDefault
            },
            Before = new PoliticallyExposedPerson
            {
                IsPoliticallyExposed = 1,
                PersonId = PersonIdDefault
            },
            Operation = OperationType.Update,
            TransactionTimestamp = TimestampDefault
        };

        // Act
        await CdcPoliticallyExposedPersonEventHandler.ProcessAsync(message);

        // Assert
        _pubSubPublisherMock
            .Verify(x => x.PublishMessageAsync(
                    It.Is<PoliticallyExposedPersonChanged>(politicallyExposedPersonChanged => 
                        politicallyExposedPersonChanged.Data.IsPoliticallyExposed == false &&
                        politicallyExposedPersonChanged.Data.Timestamp == TimestampDefault &&
                        politicallyExposedPersonChanged.Data.CustomerId == FoundUserId),
                    null), 
                Times.Once);
    }
        
    [Fact]
    public async Task ProcessAsync_WhenUserNotFound_ShouldIgnoreMessage()
    {
        // Arrange
        _userMappingsServiceMock
            .Setup(x => x.GetUserByPersonAsync(PersonIdDefault))
            .ReturnsAsync((string)null);
            
        var message = new CdcEvent<PoliticallyExposedPerson>
        {
            After = new PoliticallyExposedPerson
            {
                IsPoliticallyExposed = 1,
                PersonId = PersonIdDefault
            },
            Before = null,
            Operation = OperationType.Create,
            TransactionTimestamp = TimestampDefault
        };

        // Act
        await CdcPoliticallyExposedPersonEventHandler.ProcessAsync(message);

        // Assert
        _pubSubPublisherMock.VerifyNoOtherCalls();
    }
}
