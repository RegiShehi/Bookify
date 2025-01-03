namespace Bookify.Domain.Users;

public sealed class Role
{
    public static readonly Role Registered = new(1, "Registered");

    private readonly List<User> _users = [];
    public int Id { get; init; }
    public string Name { get; set; }

    public Role(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public IReadOnlyCollection<User> Users => _users.ToList();
}