using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SomeWebApi.Model;

[Table("Users")]
public record User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreationDate { get; init; }

    public string? DisplayName { get; init; }

    [DefaultValue(0)]
    public int DownVotes { get; init; }

    [DefaultValue(0)]
    public int UpVotes { get; init; }

    [DefaultValue(0)]
    public int Reputation { get; init; }

    [DefaultValue(0)]
    public int Views { get; init; }
}
