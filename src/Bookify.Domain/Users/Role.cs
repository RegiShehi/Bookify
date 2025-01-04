namespace Bookify.Domain.Users;

public sealed class Role
{
    public static readonly Role Registered = new(1, "Registered");

    private readonly List<User> _users = [];
    private readonly List<Permission> _permissions = [];
    public int Id { get; init; }
    public string Name { get; set; }

    public Role(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public IReadOnlyCollection<User> Users => _users.ToList();
    public IReadOnlyCollection<Permission> Permissions => _permissions.ToList();
}