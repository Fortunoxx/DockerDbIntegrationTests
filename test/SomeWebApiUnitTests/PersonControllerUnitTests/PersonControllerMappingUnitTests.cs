namespace SomeWebApiUnitTests.ProcessControllerUnitTests;

using AutoMapper;
using SomeWebApi.Mappings;
using Xunit;

public sealed class PersonControllerMappingUnitTests
{
    [Fact]
    internal void ServiceMappings_Should_Succed()
    {
        // Arrange
        var mappingConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<ServiceMappings>());

        // Act & Assert
        mappingConfiguration.AssertConfigurationIsValid();
    }
}
