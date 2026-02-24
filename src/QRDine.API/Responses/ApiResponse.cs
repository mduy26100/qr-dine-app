namespace QRDine.API.Responses
{
    public class ApiResponse
    {
        public object? Data { get; set; }
        public object? Meta { get; set; } = new Meta();
        public ApiError? Error { get; set; }

        public static ApiResponse Success(object? data, object? meta = null)
            => new ApiResponse
            {
                Data = data,
                Meta = meta ?? new Meta(),
                Error = null
            };

        public static ApiResponse Fail(ApiError error)
            => new ApiResponse
            {
                Data = null,
                Meta = new Meta(),
                Error = error,
            };
    }
}
