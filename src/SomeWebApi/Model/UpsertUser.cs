namespace SomeWebApi.Model;

public record UpsertUser
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
}