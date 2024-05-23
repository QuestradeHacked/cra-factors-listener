using System.Threading.Tasks;
using CRA.FactorsListener.Cdc.Providers;
using CRA.FactorsListener.Cdc.Services.UserMapping;
using CRA.FactorsListener.Domain.Entities;
using CRA.FactorsListener.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRA.FactorsListener.UnitTests.Services.UserMapping;

public class UserServiceTests
{
    private readonly Mock<ICustomerMasterDataProvider> _customerMasterDataProviderMock = new();
    private readonly Mock<ILogger<UserService>> _loggerMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    
    private UserService UserService => new(
        _userRepositoryMock.Object,
        _customerMasterDataProviderMock.Object,
        _loggerMock.Object);

    [Fact]
    public async Task GetByPersonId_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        const string inputPersonId = "1";
        const string expectedUser = "2";
        
        _userRepositoryMock
            .Setup(x => x.GetByPersonId(inputPersonId, default))
            .ReturnsAsync(new User { UserId = expectedUser });

        // Act
        var actualUser = await UserService.GetByPersonId(inputPersonId);

        // Assert
        Assert.Equal(expectedUser, actualUser.UserId);
    }

    [Fact]
    public async Task GetByPersonId_WhenUserNotExists_ShouldReturnNull()
    {
        // Arrange
        const string inputPersonId = "1";
        
        _userRepositoryMock
            .Setup(x => x.GetByPersonId(inputPersonId, default))
            .ReturnsAsync((User)null);

        // Act
        var actualUser = await UserService.GetByPersonId(inputPersonId);

        // Assert
        Assert.Null(actualUser);
    }

    [Fact]
    public async Task GetByPersonId_WhenUserIsNotFound_ShouldGetFromCustomerMasterData()
    {
        // Arrange
        const string inputPersonId = "1";
        var expectedUser = new User { UserId = "2" };

        _customerMasterDataProviderMock
            .Setup(x => x.GetUserByPersonIdAsync(inputPersonId, default))
            .ReturnsAsync(expectedUser);

        // Act
        var actualUser = await UserService.GetByPersonId(inputPersonId);

        // Assert
        Assert.Equal(expectedUser, actualUser);
        _userRepositoryMock.Verify(userRepository => userRepository.UpsertAsync(expectedUser, default), Times.Once);
    }
}
