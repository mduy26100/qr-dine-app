namespace QRDine.Application.Features.Reports.DTOs
{
    public class TrafficHeatmapDto
    {
        public string DayOfWeek { get; set; } = default!;
        public int Hour { get; set; }
        public int OrderCount { get; set; }
    }
}
