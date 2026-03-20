namespace QRDine.Application.Tests.Common.Builders.Sales
{
    public class ManagementCreateOrderDtoBuilder
    {
        private Guid _tableId = Guid.NewGuid();
        private Guid? _sessionId = null;
        private string? _note = null;
        private List<ManagementCreateOrderItemDto> _items = new();

        public ManagementCreateOrderDtoBuilder WithTableId(Guid tableId)
        {
            _tableId = tableId;
            return this;
        }

        public ManagementCreateOrderDtoBuilder WithSessionId(Guid? sessionId)
        {
            _sessionId = sessionId;
            return this;
        }

        public ManagementCreateOrderDtoBuilder WithNote(string? note)
        {
            _note = note;
            return this;
        }

        public ManagementCreateOrderDtoBuilder WithItems(List<ManagementCreateOrderItemDto> items)
        {
            _items = items;
            return this;
        }

        public ManagementCreateOrderDtoBuilder AddItem(ManagementCreateOrderItemDto item)
        {
            _items.Add(item);
            return this;
        }

        public ManagementCreateOrderDto Build()
        {
            return new ManagementCreateOrderDto
            {
                TableId = _tableId,
                SessionId = _sessionId,
                Note = _note,
                Items = _items.Count > 0 ? _items : new List<ManagementCreateOrderItemDto>
                {
                    new ManagementCreateOrderItemDtoBuilder().Build()
                }
            };
        }
    }
}
