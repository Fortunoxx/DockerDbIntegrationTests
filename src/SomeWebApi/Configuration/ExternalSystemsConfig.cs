namespace SomeWebApi.Configuration;

public record ExternalSystemsConfig
{
    public static string ConfigSectionName => "ExternalSystemsConfig";
    public string DocumentServiceUri { get; init; }
}
