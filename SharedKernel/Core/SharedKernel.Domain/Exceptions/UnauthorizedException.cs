namespace SharedKernel.Domain.Exceptions
{
    public class UnauthorizedException : ApplicationExceptionBase
    {
        public UnauthorizedException(string message) : base(message) { }
    }
}
