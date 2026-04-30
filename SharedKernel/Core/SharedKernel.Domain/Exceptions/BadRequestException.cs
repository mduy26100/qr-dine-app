namespace SharedKernel.Domain.Exceptions
{
    public class BadRequestException : ApplicationExceptionBase
    {
        public BadRequestException(string message) : base(message)
        {
        }
    }
}
