namespace QRDine.Application.Tests.Common.Builders.Catalog
{
    public class UpdateTableDtoBuilder
    {
        private string _name = "Table 1 Updated";

        public UpdateTableDtoBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public UpdateTableDto Build()
        {
            return new UpdateTableDto
            {
                Name = _name
            };
        }
    }
}
