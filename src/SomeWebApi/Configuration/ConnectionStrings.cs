namespace SomeWebApi.Configuration;

public record ConnectionStrings
{
    public static string ConfigSectionName => "ConnectionStrings";

    public string SqlServerConnectionString { get; init; }
}