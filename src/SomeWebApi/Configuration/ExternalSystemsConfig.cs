namespace SomeWebApi.Configuration;

public record ExternalSystemsConfig
{
    public static string ConfigSectionName => "ExternalSystemsConfig";
    
    public required string DocumentServiceUri { get; init; }
}
