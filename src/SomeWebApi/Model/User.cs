namespace SomeWebApi.Model;

public record User
{
    public int Id { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
}
