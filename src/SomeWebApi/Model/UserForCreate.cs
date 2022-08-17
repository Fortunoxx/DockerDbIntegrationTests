namespace SomeWebApi.Model;

public record UserForCreate
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
}