using Dapper;
using NetCoreAPIPostgreSQL.Model.Models;
using Npgsql;
using System.Threading.Tasks;

namespace NetCoreAPIPostgreSQL.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly PostgreSQLConfiguration _connectionString;

        public UserRepository(PostgreSQLConfiguration connectionString)
        {
            _connectionString = connectionString;
        }

        protected NpgsqlConnection DbConnection()
        {
            return new NpgsqlConnection(_connectionString.ConnectionString);
        }

        public async Task<User> Authenticate(string username, string password)
        {
            var db = DbConnection();

            var sql = @"
                        SELECT * 
                        FROM Users 
                        WHERE Username = @Username AND Password = @Password";

            return await db.QueryFirstOrDefaultAsync<User>(sql, new { Username = username, Password = password });
        }

        public async Task<User> Register(User user)
        {
            var db = DbConnection();

            var sql = @"
                        INSERT INTO Users (Username, Password) 
                        VALUES (@Username, @Password) 
                        RETURNING Id";

            var id = await db.ExecuteScalarAsync<int>(sql, new { user.Username, user.Password });
            user.Id = id;
            return user;
        }

        public async Task<bool> UserExists(string username)
        {
            var db = DbConnection();

            var sql = @"
                        SELECT COUNT(1) 
                        FROM Users 
                        WHERE Username = @Username";

            return await db.ExecuteScalarAsync<bool>(sql, new { Username = username });
        }
    }
}
