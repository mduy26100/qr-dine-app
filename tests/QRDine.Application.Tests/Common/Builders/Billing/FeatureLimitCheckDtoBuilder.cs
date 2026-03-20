namespace QRDine.Application.Tests.Common.Builders.Billing
{
    public class FeatureLimitCheckDtoBuilder
    {
        private int? _maxTables = 10;
        private int? _maxProducts = 100;
        private int? _maxStaffMembers = 5;
        private bool _allowAdvancedReports = true;

        public FeatureLimitCheckDtoBuilder WithMaxTables(int? maxTables)
        {
            _maxTables = maxTables;
            return this;
        }

        public FeatureLimitCheckDtoBuilder WithMaxProducts(int? maxProducts)
        {
            _maxProducts = maxProducts;
            return this;
        }

        public FeatureLimitCheckDtoBuilder WithMaxStaffMembers(int? maxStaffMembers)
        {
            _maxStaffMembers = maxStaffMembers;
            return this;
        }

        public FeatureLimitCheckDtoBuilder WithAllowAdvancedReports(bool allowAdvancedReports)
        {
            _allowAdvancedReports = allowAdvancedReports;
            return this;
        }

        public FeatureLimitCheckDto Build()
        {
            return new FeatureLimitCheckDto
            {
                MaxTables = _maxTables,
                MaxProducts = _maxProducts,
                MaxStaffMembers = _maxStaffMembers,
                AllowAdvancedReports = _allowAdvancedReports
            };
        }
    }
}
