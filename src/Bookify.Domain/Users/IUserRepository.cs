namespace Bookify.Domain.Users;

public interface IUserRepository
{
    void AddUser(User user);

    Task<User?> GetUserById(Guid userId, CancellationToken cancellationToken = default);
}