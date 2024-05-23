using System.Threading.Tasks;
using CRA.FactorsListener.Cdc.Extensions;
using CRA.FactorsListener.Cdc.Models;
using CRA.FactorsListener.Cdc.Models.Accounts;
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

public class CdcAccountEventHandlerTests
{
    private const string ActiveAccountStatusId = "1";
    private const string AccountIdDefault = "10";
    private const string FoundUserId = "100";
    private const string InactiveAccountStatusId = "4";
    private const long TimestampDefault = 1000000;
        
    private readonly Mock<ILogger<CdcAccountEventHandler>> _loggerMock = new();
    private readonly Mock<IPublisherService<AccountChanged>> _pubSubPublisherMock = new();
    private readonly RiskFactorsOptions _riskFactorsOptions = new()
    {
        ActiveAccountStatusIds = "1,2,3"
    };
    private readonly Mock<IUserMappingsService> _userMappingsServiceMock = new();
        
    private CdcAccountEventHandler CdcAccountEventHandler => new(
        _userMappingsServiceMock.Object,
        _riskFactorsOptions,
        _pubSubPublisherMock.Object,
        _loggerMock.Object);
        
    public CdcAccountEventHandlerTests()
    {
        Setup();
    }

    private void Setup()
    {
        _userMappingsServiceMock
            .Setup(x => x.GetUserByAccountAsync(AccountIdDefault))
            .ReturnsAsync(FoundUserId);
    }

    [Fact]
    public async Task ProcessAsync_WhenReceivesAnActiveAccount_ShouldSendToPubSub()
    {
        // Arrange
        var message = new CdcEvent<Account>
        {
            After = new Account
            {
                AccountStatusId = ActiveAccountStatusId,
                AccountId = AccountIdDefault
            },
            Before = null,
            Operation = OperationType.Create,
            TransactionTimestamp = TimestampDefault
        };

        // Act
        await CdcAccountEventHandler.ProcessAsync(message);

        // Assert
        _pubSubPublisherMock
            .Verify(x => x.PublishMessageAsync(
                    It.Is<AccountChanged>(accountChanged =>
                        accountChanged.Data.AccountStatus == AccountStatus.Active.ToString() &&
                        accountChanged.Data.Timestamp == TimestampDefault &&
                        accountChanged.Data.CustomerId == FoundUserId), null),
                Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_WhenReceivesAnInactiveAccount_ShouldNotSendToPubSub()
    {
        // Arrange
        var message = new CdcEvent<Account>
        {
            After = new Account
            {
                AccountStatusId = InactiveAccountStatusId,
                AccountId = AccountIdDefault
            },
            Before = null,
            Operation = OperationType.Create
        };

        // Act
        await CdcAccountEventHandler.ProcessAsync(message);

        // Assert
        _pubSubPublisherMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProcessAsync_WhenAccountChangeToActive_ShouldSendToPubSub()
    {
        // Arrange
        var message = new CdcEvent<Account>
        {
            After = new Account
            {
                AccountId = AccountIdDefault,
                AccountStatusId = ActiveAccountStatusId
            },
            Before = new Account
            {
                AccountId = AccountIdDefault,
                AccountStatusId = InactiveAccountStatusId
            },
            Operation = OperationType.Update,
            TransactionTimestamp = TimestampDefault
        };

        // Act
        await CdcAccountEventHandler.ProcessAsync(message);

        // Assert
        _pubSubPublisherMock
            .Verify(x => x.PublishMessageAsync(
                    It.Is<AccountChanged>(accountChanged =>
                        accountChanged.Data.AccountStatus == AccountStatus.Active.ToString() &&
                        accountChanged.Data.Timestamp == TimestampDefault &&
                        accountChanged.Data.CustomerId == FoundUserId),
                    null),
                Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_WhenAccountChangeToInactive_ShouldSendToPubSub()
    {
        // Arrange
        var message = new CdcEvent<Account>
        {
            After = new Account
            {
                AccountId = AccountIdDefault,
                AccountStatusId = InactiveAccountStatusId
            },
            Before = new Account
            {
                AccountId = AccountIdDefault,
                AccountStatusId = ActiveAccountStatusId
            },
            Operation = OperationType.Update,
            TransactionTimestamp = TimestampDefault
        };

        // Act
        await CdcAccountEventHandler.ProcessAsync(message);

        // Assert
        _pubSubPublisherMock
            .Verify(x => x.PublishMessageAsync(
                    It.Is<AccountChanged>(accountChanged =>
                        accountChanged.Data.AccountStatus == AccountStatus.Inactive.ToString() &&
                        accountChanged.Data.Timestamp == TimestampDefault &&
                        accountChanged.Data.CustomerId == FoundUserId),
                    null),
                Times.Once);
    }

    [Theory]
    [InlineData("1", "1")]
    [InlineData("4", "4")]
    public async Task ProcessAsync_WhenReceiveSameAccountStatus_ShouldNotSendToPubSub(string accountStatusBefore, string accountStatusAfter)
    {
        // Arrange
        var message = new CdcEvent<Account>
        {
            After = new Account
            {
                AccountStatusId = accountStatusAfter,
                AccountId = AccountIdDefault
            },
            Before = new Account
            {
                AccountStatusId = accountStatusBefore,
                AccountId = AccountIdDefault
            },
            Operation = OperationType.Update,
            TransactionTimestamp = TimestampDefault
        };

        // Act
        await CdcAccountEventHandler.ProcessAsync(message);

        // Assert
        _pubSubPublisherMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProcessAsync_WhenUserNotFound_ShouldThrowException()
    {
        // Arrange
        _userMappingsServiceMock
            .Setup(x => x.GetUserByAccountAsync(AccountIdDefault))
            .ReturnsAsync((string)null);

        var message = new CdcEvent<Account>
        {
            After = new Account
            {
                AccountStatusId = ActiveAccountStatusId,
                AccountId = AccountIdDefault
            },
            Before = null,
            Operation = OperationType.Create,
            TransactionTimestamp = TimestampDefault
        };

        // Assert
        await Assert.ThrowsAsync<UserNotFoundException>(() => CdcAccountEventHandler.ProcessAsync(message));
        _pubSubPublisherMock.VerifyNoOtherCalls();
    }
}
