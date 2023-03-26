namespace SomeWebApiUnitTests.ProcessControllerUnitTests;

using System.Threading.Tasks;
using AutoMapper;
using SomeWebApi.Mappings;
using Xunit;

public sealed class PersonControllerMappingUnitTests
{
    [Fact]
    internal async Task ServiceMappings_Should_Succed_Async()
    {
        // Arrange
        var mappingConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<ServiceMappings>());

        // Act & Assert
        mappingConfiguration.AssertConfigurationIsValid();
    }
}
