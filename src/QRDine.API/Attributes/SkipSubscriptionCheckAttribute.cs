namespace QRDine.API.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SkipSubscriptionCheckAttribute : Attribute
    {
    }
}
