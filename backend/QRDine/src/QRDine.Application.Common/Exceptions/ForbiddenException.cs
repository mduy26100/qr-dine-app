namespace QRDine.Application.Common.Exceptions
{
    public class ForbiddenException : ApplicationExceptionBase
    {
        public ForbiddenException(string message) : base(message) { }
    }
}
