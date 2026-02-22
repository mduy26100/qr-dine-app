namespace QRDine.Application.Common.Exceptions
{
    public class ValidationException : ApplicationExceptionBase
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException(IDictionary<string, string[]> errors)
            : base("One or more validation failures have occurred.")
        {
            Errors = errors;
        }
    }
}
