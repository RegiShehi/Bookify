namespace Bookify.Domain.Users;

public interface IUserRepository
{
    void AddUser(User user);

    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}