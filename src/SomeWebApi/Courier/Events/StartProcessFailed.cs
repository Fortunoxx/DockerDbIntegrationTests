namespace SomeWebApi.Courier.Events;

using MassTransit;

public record StartProcessFailed
{
    public int Id { get; init; }

    public required string ProcessName { get; init; }
    
    public ExceptionInfo? ExceptionInfo { get; init; }
}