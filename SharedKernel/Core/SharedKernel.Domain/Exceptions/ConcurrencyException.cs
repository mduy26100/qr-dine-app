namespace SharedKernel.Domain.Exceptions
{
    public class ConcurrencyException : ApplicationExceptionBase
    {
        public ConcurrencyException(string message) : base(message) { }
        public ConcurrencyException(string message, Exception innerException) : base(message, innerException) { }
    }
}
