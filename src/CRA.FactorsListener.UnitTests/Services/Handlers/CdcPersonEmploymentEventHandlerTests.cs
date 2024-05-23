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

public class CdcPersonEmploymentEventHandlerTests
{
    private const string FoundUserId = "100";
    private const string PersonIdDefault = "1";
    private const int TimestampDefault = 1000000;
        
    private readonly Mock<ILogger<CdcPersonEmploymentEventHandler>> _loggerMock = new();
    private readonly Mock<IPublisherService<PersonEmploymentChanged>> _pubSubPublisherMock = new();
    private readonly Mock<IUserMappingsService> _userMappingsServiceMock = new();
        
    private CdcPersonEmploymentEventHandler CdcPersonEmploymentEventHandler => new(
        _userMappingsServiceMock.Object,
        _pubSubPublisherMock.Object, 
        _loggerMock.Object);

    public CdcPersonEmploymentEventHandlerTests()
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
    public async Task ProcessAsync_WhenAddAPersonEmployment_ShouldSendToPubSub()
    {
        // Arrange
        var message = new CdcEvent<PersonEmployment>
        {
            After = new PersonEmployment
            {
                IsJobTitleVerified = true,
                JobTitle = "JobTitle",
                PersonId = PersonIdDefault
            },
            Before = null,
            Operation = OperationType.Create,
            TransactionTimestamp = TimestampDefault
        };

        // Act
        await CdcPersonEmploymentEventHandler.ProcessAsync(message);

        // Assert
        _pubSubPublisherMock
            .Verify(x => x.PublishMessageAsync(
                    It.Is<PersonEmploymentChanged>(personEmploymentChanged => 
                        personEmploymentChanged.Data.Occupation == "JobTitle" &&
                        personEmploymentChanged.Data.IsVerified == true &&
                        personEmploymentChanged.Data.Timestamp == TimestampDefault &&
                        personEmploymentChanged.Data.CustomerId == FoundUserId),
                    null), 
                Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_WhenChangePersonEmployment_ShouldSendToPubSub()
    {
        // Arrange
        var message = new CdcEvent<PersonEmployment>
        {
            After = new PersonEmployment
            {
                IsJobTitleVerified = false,
                JobTitle = "JobTitleUpdated",
                PersonId = PersonIdDefault
            },
            Before = new PersonEmployment
            {
                IsJobTitleVerified = true,
                JobTitle = "JobTitle",
                PersonId = PersonIdDefault
            },
            Operation = OperationType.Update,
            TransactionTimestamp = TimestampDefault
        };

        // Act
        await CdcPersonEmploymentEventHandler.ProcessAsync(message);

        // Assert
        _pubSubPublisherMock
            .Verify(x => x.PublishMessageAsync(
                    It.Is<PersonEmploymentChanged>(personEmploymentChanged => 
                        personEmploymentChanged.Data.Occupation == "JobTitleUpdated" &&
                        personEmploymentChanged.Data.IsVerified == false &&
                        personEmploymentChanged.Data.Timestamp == TimestampDefault &&
                        personEmploymentChanged.Data.CustomerId == FoundUserId),
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
            
        var message = new CdcEvent<PersonEmployment>
        {
            After = new PersonEmployment
            {
                IsJobTitleVerified = true,
                JobTitle = "JobTitle",
                PersonId = PersonIdDefault
            },
            Before = null,
            Operation = OperationType.Create,
            TransactionTimestamp = TimestampDefault
        };

        // Act
        await CdcPersonEmploymentEventHandler.ProcessAsync(message);

        // Assert
        _pubSubPublisherMock.VerifyNoOtherCalls();
    }
}
