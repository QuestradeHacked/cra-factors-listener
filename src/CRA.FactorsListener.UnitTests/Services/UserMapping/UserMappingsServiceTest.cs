using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRA.FactorsListener.Cdc.Services.UserMapping;
using CRA.FactorsListener.Domain.Entities;
using CRA.FactorsListener.Domain.Repositories;
using Moq;
using Xunit;

namespace CRA.FactorsListener.UnitTests.Services.UserMapping;

public class UserMappingsServiceTest
{
    private readonly Mock<IPersonAddressRepository> _personAddressRepositoryMock = new();
    private readonly Mock<IPersonAddressService> _personAddressServiceMock = new();
    private readonly Mock<IUserAccountService> _userAccountServiceMock = new();
    private readonly Mock<IUserService> _userServiceMock = new();
        
    private UserMappingsService UserMappingsService => new(
        _userAccountServiceMock.Object,
        _personAddressServiceMock.Object,
        _userServiceMock.Object);

    [Fact]
    public async Task GetUserByAccountAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        const string inputAccountId = "1";
        const string expectedUserId = "2";
        
        _userAccountServiceMock
            .Setup(x => x.GetByAccountId(inputAccountId, default))
            .ReturnsAsync(new UserAccount { UserId = expectedUserId });

        // Act
        var actualUserId = await UserMappingsService.GetUserByAccountAsync(inputAccountId);

        // Assert
        Assert.Equal(expectedUserId, actualUserId);
    }

    [Fact]
    public async Task GetUserByAccountAsync_WhenUserNotExists_ShouldReturnNull()
    {
        // Arrange
        const string inputAccountId = "1";
        
        _userAccountServiceMock
            .Setup(x => x.GetByAccountId(inputAccountId, default))
            .ReturnsAsync((UserAccount) null);

        // Act
        var actualUserId = await UserMappingsService.GetUserByAccountAsync(inputAccountId);

        // Assert
        Assert.Null(actualUserId);
    }

    [Fact]
    public async Task GetUsersByAddressAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        const string inputAddressId = "2";
        const string foundPersonId = "3";
        const string expectedUserId = "4";
        
        _personAddressServiceMock
            .Setup(x => x.GetPrimaryAddressesByAddressId(inputAddressId, default))
            .ReturnsAsync(new List<PersonAddress>{ new() { PersonId = foundPersonId } });
        _userServiceMock
            .Setup(x => x.GetByPersonId(foundPersonId, default))
            .ReturnsAsync(new User { UserId = expectedUserId });

        // Act
        var actualUsers = await UserMappingsService.GetUsersByAddressAsync(inputAddressId);

        // Assert
        Assert.Equal(expectedUserId, actualUsers.Single());
    }

    [Fact]
    public async Task GetUsersByAddressAsync_WhenUserNotExists_ShouldReturnNull()
    {
        // Arrange
        const string inputAddressId = "2";
        const string foundPersonId = "3";
        
        _personAddressServiceMock
            .Setup(x => x.GetPrimaryAddressesByAddressId(inputAddressId, default))
            .ReturnsAsync(new List<PersonAddress>{ new() { PersonId = foundPersonId } });

        _userServiceMock
            .Setup(x => x.GetByPersonId(foundPersonId, default))
            .ReturnsAsync((User) null);

        // Act
        var actualUserId = await UserMappingsService.GetUsersByAddressAsync(inputAddressId);

        // Assert
        Assert.Empty(actualUserId);
    }

    [Fact]
    public async Task GetUserByPersonAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        const string inputPersonId = "1";
        const string expectedUserId = "2";
        
        _userServiceMock
            .Setup(x => x.GetByPersonId(inputPersonId, default))
            .ReturnsAsync(new User {UserId = expectedUserId});

        // Act
        var actualUserId = await UserMappingsService.GetUserByPersonAsync(inputPersonId);

        // Assert
        Assert.Equal(expectedUserId, actualUserId);
    }

    [Fact]
    public async Task GetUserByPersonAsync_WhenUserNotExists_ShouldReturnNull()
    {
        // Arrange
        const string inputPersonId = "1";
        
        _userServiceMock
            .Setup(x => x.GetByPersonId(inputPersonId, default))
            .ReturnsAsync((User) null);

        // Act
        var actualUserId = await UserMappingsService.GetUserByPersonAsync(inputPersonId);

        // Assert
        Assert.Null(actualUserId);
    }
    
    [Fact]
    public async Task IsAddressPrimaryResidenceAsync_WhenPersonAddressExists_ShouldReturnTrue()
    {
        // Arrange
        const string addressId = "1";
        var expectedPersonAddress = new List<PersonAddress>{ new() { PersonId = "2" } };

        _personAddressServiceMock
            .Setup(x => x.GetPrimaryAddressesByAddressId(addressId, default))
            .ReturnsAsync(expectedPersonAddress);

        // Act
        var isAddressPrimaryResidence = await UserMappingsService.IsAddressPrimaryResidenceAsync(addressId);

        // Assert
        Assert.True(isAddressPrimaryResidence);
    }
    
    [Fact]
    public async Task IsAddressPrimaryResidenceAsync_WhenPersonAddressNotExists_ShouldReturnFalse()
    {
        // Arrange
        const string addressId = "1";

        _personAddressServiceMock
            .Setup(x => x.GetPrimaryAddressesByAddressId(addressId, default))
            .ReturnsAsync(new List<PersonAddress>());

        // Act
        var isAddressPrimaryResidence = await UserMappingsService.IsAddressPrimaryResidenceAsync(addressId);

        // Assert
        Assert.False(isAddressPrimaryResidence);
    }
}
