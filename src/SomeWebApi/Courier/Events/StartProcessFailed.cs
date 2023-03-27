namespace SomeWebApi.Courier.Events;

using MassTransit;

public record StartProcessFailed
{
    int Id { get; }
    string ProcessName { get; }
    public ExceptionInfo ExceptionInfo { get; init; }
}