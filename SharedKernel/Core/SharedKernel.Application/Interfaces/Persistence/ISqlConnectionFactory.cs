namespace SharedKernel.Application.Interfaces.Persistence
{
    public interface ISqlConnectionFactory
    {
        IDbConnection GetConnection();
    }
}
