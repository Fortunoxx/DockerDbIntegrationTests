namespace SomeWebApi.Configuration;

public record ConnectionStrings
{
    public static string ConfigSectionName => "ConnectionStrings";

    public required string SqlServerConnectionString { get; init; }
}