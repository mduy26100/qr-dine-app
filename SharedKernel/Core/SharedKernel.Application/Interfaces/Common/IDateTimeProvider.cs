namespace SharedKernel.Application.Interfaces.Common
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }
}
