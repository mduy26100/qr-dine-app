namespace SharedKernel.Domain.Exceptions
{
    public class ConflictException : ApplicationExceptionBase
    {
        public ConflictException(string message) : base(message) { }
        public ConflictException(string message, Exception innerException) : base(message, innerException) { }
    }
}
