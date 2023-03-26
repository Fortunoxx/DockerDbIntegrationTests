namespace SomeWebApi.Configuration;

public record WaitAndRetryConfig
{
    public static string ConfigSectionName => "WaitAndRetryConfig";

    public int Retry { get; set; }
    public int Wait { get; set; }
    public int Timeout { get; set; }
}
