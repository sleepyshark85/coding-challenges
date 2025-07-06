using Elsa.QuizAPI.Data;

namespace Elsa.QuizAPI.Infrastructure;

public interface IUserContext
{
    bool IsAuthenticated();
    UserInfo GetCurrentUser();
}

public class DumpUserContext : IUserContext
{
    private readonly QuizDbContext _context;

    public DumpUserContext(QuizDbContext context)
    {
        _context = context;
    }

    public bool IsAuthenticated() => true;

    public UserInfo GetCurrentUser()
    {
        var user = _context.Users.First();
        return new UserInfo
        {
            UserId = user.UserId,
            Username = user.Username
        };
    }
}

public class UserInfo
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
}