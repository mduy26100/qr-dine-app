namespace SharedKernel.Infrastructure.Persistence.Interceptors
{
    public class EfSlowQueryInterceptor : DbCommandInterceptor
    {
        private const int SlowQueryThresholdMs = 200;
        private readonly ILogger<EfSlowQueryInterceptor> _logger;

        public EfSlowQueryInterceptor(ILogger<EfSlowQueryInterceptor> logger)
        {
            _logger = logger;
        }

        public override ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
        {
            LogQuery(command, eventData.Duration.TotalMilliseconds);
            return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override ValueTask<int> NonQueryExecutedAsync(
            DbCommand command, CommandExecutedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            LogQuery(command, eventData.Duration.TotalMilliseconds);
            return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override ValueTask<object?> ScalarExecutedAsync(
            DbCommand command, CommandExecutedEventData eventData, object? result, CancellationToken cancellationToken = default)
        {
            LogQuery(command, eventData.Duration.TotalMilliseconds);
            return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override DbDataReader ReaderExecuted(
            DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
        {
            LogQuery(command, eventData.Duration.TotalMilliseconds);
            return base.ReaderExecuted(command, eventData, result);
        }

        public override int NonQueryExecuted(
            DbCommand command, CommandExecutedEventData eventData, int result)
        {
            LogQuery(command, eventData.Duration.TotalMilliseconds);
            return base.NonQueryExecuted(command, eventData, result);
        }

        public override object? ScalarExecuted(
            DbCommand command, CommandExecutedEventData eventData, object? result)
        {
            LogQuery(command, eventData.Duration.TotalMilliseconds);
            return base.ScalarExecuted(command, eventData, result);
        }


        private void LogQuery(DbCommand command, double elapsedMs)
        {
            if (elapsedMs > SlowQueryThresholdMs)
            {
                _logger.LogWarning("Slow Database Query ({Elapsed:0.00} ms): {Query}", elapsedMs, command.CommandText);
            }
            else
            {
                _logger.LogDebug("Database Query ({Elapsed:0.00} ms): {Query}", elapsedMs, command.CommandText);
            }
        }
    }
}
