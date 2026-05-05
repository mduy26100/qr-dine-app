namespace QRDine.Infrastructure.Persistence.Interceptors
{
    public class EfSlowQueryInterceptor : DbCommandInterceptor
    {
        private const int SlowQueryThresholdMs = 100;
        private readonly ILogger<EfSlowQueryInterceptor> _logger;

        public EfSlowQueryInterceptor(ILogger<EfSlowQueryInterceptor> logger)
        {
            _logger = logger;
        }

        public override ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
        {
            LogQuery(command, eventData.Duration.TotalMilliseconds);
            return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override ValueTask<int> NonQueryExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            LogQuery(command, eventData.Duration.TotalMilliseconds);
            return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override ValueTask<object?> ScalarExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            object? result,
            CancellationToken cancellationToken = default)
        {
            LogQuery(command, eventData.Duration.TotalMilliseconds);
            return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
        }

        private void LogQuery(DbCommand command, double elapsedMs)
        {
            if (elapsedMs > SlowQueryThresholdMs)
            {
                _logger.LogWarning("Slow SQL ({Elapsed:0.00} ms): {Query}", elapsedMs, command.CommandText);
            }
            else
            {
                _logger.LogInformation("SQL ({Elapsed:0.00} ms): {Query}", elapsedMs, command.CommandText);
            }
        }
    }
}
