namespace SharedKernel.Domain.Exceptions
{
    public class ForbiddenException : ApplicationExceptionBase
    {
        public ForbiddenException(string message) : base(message) { }
    }
}
