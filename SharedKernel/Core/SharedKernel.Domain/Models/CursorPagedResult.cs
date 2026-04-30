namespace SharedKernel.Domain.Models
{
    public class CursorPagedResult<T>
    {
        public IReadOnlyList<T> Data { get; set; }
        public bool HasNextPage { get; set; }
        public DateTime? NextCursorCreatedAt { get; set; }
        public Guid? NextCursorId { get; set; }

        public CursorPagedResult(IReadOnlyList<T> data, bool hasNextPage, DateTime? nextCursorCreatedAt, Guid? nextCursorId)
        {
            Data = data;
            HasNextPage = hasNextPage;
            NextCursorCreatedAt = nextCursorCreatedAt;
            NextCursorId = nextCursorId;
        }
    }
}
