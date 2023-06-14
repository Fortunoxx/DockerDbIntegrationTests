namespace SomeWebApi.Model;

using System.ComponentModel.DataAnnotations;

public record UpsertUser
{
    [StringLength(40)]
    public string DisplayName { get; init; }
    public int DownVotes { get; init; }
    public int UpVotes { get; init; }
    public int Reputation { get; init; }
    public int Views { get; init; }
}