namespace SharedKernel.BackgroundJobs.Services
{
    public class HangfireBackgroundJobService : IBackgroundJobService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;

        public HangfireBackgroundJobService(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
        }

        public string Enqueue(Expression<Action> methodCall)
        {
            return _backgroundJobClient.Enqueue(methodCall);
        }

        public string Enqueue(Expression<Func<Task>> methodCall)
        {
            return _backgroundJobClient.Enqueue(methodCall);
        }

        public string Schedule(Expression<Action> methodCall, TimeSpan delay)
        {
            return _backgroundJobClient.Schedule(methodCall, delay);
        }

        public string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay)
        {
            return _backgroundJobClient.Schedule(methodCall, delay);
        }
    }
}
