namespace Bookify.Domain.Users;

public interface IUserRepository
{
    void Add(User user);

    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}