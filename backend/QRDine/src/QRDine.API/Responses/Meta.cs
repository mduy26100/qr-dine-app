namespace QRDine.API.Responses
{
    public class Meta
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Path { get; set; } = default!;
        public string Method { get; set; } = default!;
        public int StatusCode { get; set; }

        public string TraceId { get; set; } = default!;
        public string? RequestId { get; set; }

        public string? ClientIp { get; set; }
    }
}
