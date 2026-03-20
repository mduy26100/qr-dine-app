namespace QRDine.Application.Tests.Common.Builders.Catalog
{
    public class TableBuilder
    {
        private Guid _id = Guid.NewGuid();
        private Guid _merchantId = Guid.NewGuid();
        private string _name = "Table 1";
        private bool _isOccupied = false;
        private string? _qrCodeToken = Guid.NewGuid().ToString("N");
        private string? _qrCodeImageUrl = "https://example.com/qr.png";
        private Guid? _currentSessionId = null;
        private bool _isDeleted = false;

        public TableBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public TableBuilder WithMerchantId(Guid merchantId)
        {
            _merchantId = merchantId;
            return this;
        }

        public TableBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public TableBuilder WithIsOccupied(bool isOccupied)
        {
            _isOccupied = isOccupied;
            return this;
        }

        public TableBuilder WithQrCodeToken(string? token)
        {
            _qrCodeToken = token;
            return this;
        }

        public TableBuilder WithQrCodeImageUrl(string? url)
        {
            _qrCodeImageUrl = url;
            return this;
        }

        public TableBuilder WithCurrentSessionId(Guid? sessionId)
        {
            _currentSessionId = sessionId;
            return this;
        }

        public TableBuilder WithIsDeleted(bool isDeleted)
        {
            _isDeleted = isDeleted;
            return this;
        }

        public Table Build()
        {
            return new Table
            {
                Id = _id,
                MerchantId = _merchantId,
                Name = _name,
                IsOccupied = _isOccupied,
                QrCodeToken = _qrCodeToken,
                QrCodeImageUrl = _qrCodeImageUrl,
                CurrentSessionId = _currentSessionId,
                IsDeleted = _isDeleted,
                RowVersion = new byte[] { }
            };
        }
    }
}
