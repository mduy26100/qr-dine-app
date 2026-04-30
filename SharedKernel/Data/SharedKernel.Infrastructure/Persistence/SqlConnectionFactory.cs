using SharedKernel.Application.Interfaces.Persistence;

namespace SharedKernel.Infrastructure.Persistence
{
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly DbContext _dbContext;

        public SqlConnectionFactory(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IDbConnection GetConnection()
        {
            var connection = _dbContext.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            return connection;
        }
    }
}
