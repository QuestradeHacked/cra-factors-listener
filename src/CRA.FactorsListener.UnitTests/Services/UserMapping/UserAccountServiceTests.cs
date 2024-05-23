using System.Threading.Tasks;
using CRA.FactorsListener.Cdc.Providers;
using CRA.FactorsListener.Cdc.Services.UserMapping;
using CRA.FactorsListener.Domain.Entities;
using CRA.FactorsListener.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRA.FactorsListener.UnitTests.Services.UserMapping;

public class UserAccountServiceTests
{
    private readonly Mock<ICustomerMasterDataProvider> _customerMasterDataProviderMock = new();
    private readonly Mock<ILogger<UserAccountService>> _loggerMock = new();
    private readonly Mock<IUserAccountRepository> _userAccountRepositoryMock = new();
    
    private UserAccountService UserAccountService => new(
        _userAccountRepositoryMock.Object,
        _customerMasterDataProviderMock.Object,
        _loggerMock.Object);

    [Fact]
    public async Task GetByAccountId_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        const string inputAccountId = "1";
        const string expectedUserId = "2";
        _userAccountRepositoryMock
            .Setup(x => x.GetByAccountId(inputAccountId, default))
            .ReturnsAsync(new UserAccount { UserId = expectedUserId });

        // Act
        var actualUser = await UserAccountService.GetByAccountId(inputAccountId);

        // Assert
        Assert.Equal(expectedUserId, actualUser.UserId);
    }

    [Fact]
    public async Task GetByAccountId_WhenUserIsNotFound_ShouldGetFromCustomerMasterData()
    {
        // Arrange
        const string inputAccountId = "1";
        var expectedUser = new UserAccount { UserId = "2" };

        _customerMasterDataProviderMock
            .Setup(x => x.GetUserAccountByAccountIdAsync(inputAccountId, default))
            .ReturnsAsync(expectedUser);

        // Act
        var actualUser = await UserAccountService.GetByAccountId(inputAccountId);

        // Assert
        Assert.Equal(expectedUser, actualUser);
        _userAccountRepositoryMock.Verify(repository => repository.UpsertAsync(expectedUser, default), Times.Once);
    }

    [Fact]
    public async Task GetByAccountId_WhenUserNotExists_ShouldReturnNull()
    {
        // Arrange
        const string inputAccountId = "1";

        // Act
        var actualUser = await UserAccountService.GetByAccountId(inputAccountId);

        // Assert
        Assert.Null(actualUser);
    }
}
