using QRDine.API.Filters;
using QRDine.Domain.Enums;

namespace QRDine.API.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class CheckFeatureLimitAttribute : TypeFilterAttribute
    {
        public CheckFeatureLimitAttribute(FeatureType featureType) : base(typeof(FeatureLimitFilter))
        {
            Arguments = new object[] { featureType };
        }
    }
}
