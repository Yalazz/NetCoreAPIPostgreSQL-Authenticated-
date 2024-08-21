using NetCoreAPIPostgreSQL.Model.Models;
using System.Threading.Tasks;

namespace NetCoreAPIPostgreSQL.Data.Repositories
{
    public interface IUserRepository
    {
        Task<User> Authenticate(string username, string password);
        Task<User> Register(User user);
        Task<bool> UserExists(string username);
    }
}
