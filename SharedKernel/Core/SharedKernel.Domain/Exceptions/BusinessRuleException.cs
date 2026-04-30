namespace SharedKernel.Domain.Exceptions
{
    public class BusinessRuleException : ApplicationExceptionBase
    {
        public BusinessRuleException(string message) : base(message) { }
        public BusinessRuleException(string message, Exception innerException) : base(message, innerException) { }
    }
}
