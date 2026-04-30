namespace SharedKernel.Application.Interfaces.BackgroundJobs
{
    public interface IBackgroundJobService
    {
        string Enqueue(Expression<Action> methodCall);

        string Enqueue(Expression<Func<Task>> methodCall);

        string Schedule(Expression<Action> methodCall, TimeSpan delay);
        string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay);
    }
}
