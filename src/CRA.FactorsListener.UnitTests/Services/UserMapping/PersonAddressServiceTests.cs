using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRA.FactorsListener.Cdc.Options;
using CRA.FactorsListener.Cdc.Providers;
using CRA.FactorsListener.Cdc.Services.UserMapping;
using CRA.FactorsListener.Domain.Entities;
using CRA.FactorsListener.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRA.FactorsListener.UnitTests.Services.UserMapping;

public class PersonAddressServiceTests
{
    private const int PrimaryResidenceType = 1;

    private readonly Mock<ICustomerMasterDataProvider> _customerMasterDataProviderMock = new();
    private readonly Mock<ILogger<PersonAddressService>> _loggerMock = new();
    private readonly Mock<IPersonAddressRepository> _personAddressRepositoryMock = new();

    private static RiskFactorsOptions RiskFactorsOptions => new()
    {
        PrimaryResidenceType = PrimaryResidenceType
    };
    
    private PersonAddressService PersonAddressService => new(
        _customerMasterDataProviderMock.Object,
        _loggerMock.Object,
        _personAddressRepositoryMock.Object,
        RiskFactorsOptions);

    [Fact]
    public async Task GetPrimaryAddressesByAddressId_WhenPersonAddressExists_ShouldReturnPersonAddress()
    {
        // Arrange
        const string addressId = "1";
        var expectedPersonAddress = new List<PersonAddress>{ new() { PersonId = "2" } };

        _personAddressRepositoryMock
            .Setup(x => x.GetByAddressIdAndType(addressId, 1, default))
            .ReturnsAsync(expectedPersonAddress);

        // Act
        var actualPersonAddress = await PersonAddressService.GetPrimaryAddressesByAddressId(addressId);

        // Assert
        Assert.Equal(expectedPersonAddress, actualPersonAddress);
    }

    [Fact]
    public async Task GetPrimaryAddressesByAddressId_WhenPersonAddressIsNotFound_ShouldGetFromCustomerMasterData()
    {
        // Arrange
        const string addressId = "1";
        PersonAddress personAddress = new()
        {
            PersonId = "2",
            AddressType = PrimaryResidenceType
        };
        var expectedPersonAddress = new List<PersonAddress> {personAddress};

        _customerMasterDataProviderMock
            .Setup(x => x.GetPersonsByAddressIdAsync(addressId, default))
            .ReturnsAsync(expectedPersonAddress);

        // Act
        var actualPersonAddress = await PersonAddressService.GetPrimaryAddressesByAddressId(addressId);

        // Assert
        Assert.Equal(personAddress, actualPersonAddress.Single());
        _personAddressRepositoryMock.Verify(repository => repository.UpsertAsync(personAddress, default), Times.Once);
    }

    [Fact]
    public async Task GetPrimaryAddressesByAddressId_WhenPersonAddressNotExists_ShouldReturnNull()
    {
        // Arrange
        const string addressId = "1";

        // Act
        var personAddress = await PersonAddressService.GetPrimaryAddressesByAddressId(addressId);

        // Assert
        Assert.Null(personAddress);
    }
}
