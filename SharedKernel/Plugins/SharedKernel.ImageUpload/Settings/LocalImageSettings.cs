namespace SharedKernel.ImageUpload.Settings
{
    public class LocalImageSettings
    {
        public const string SectionName = "LocalImageSettings";

        public string PhysicalPath { get; set; } = "wwwroot/uploads/images";

        public string BaseUrl { get; set; } = string.Empty;
    }
}
