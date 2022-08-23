namespace SomeWebApi.Contracts;

public interface IStartProcessCommand
{
    int Id { get; }
    string ProcessName { get; }
}