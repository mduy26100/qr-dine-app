using QRDine.API.Responses;

namespace QRDine.API.Filters
{
    public class ApiResponseFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(
            ResultExecutingContext context,
            ResultExecutionDelegate next)
        {
            if (context.Result is StatusCodeResult statusCodeResult &&
                statusCodeResult.StatusCode == StatusCodes.Status204NoContent)
            {
                await next();
                return;
            }

            if (context.Result is FileResult)
            {
                await next();
                return;
            }

            if (context.Result is not ObjectResult objectResult)
            {
                await next();
                return;
            }

            if (objectResult.Value is ApiResponse ||
                objectResult.Value is ProblemDetails)
            {
                await next();
                return;
            }

            var httpContext = context.HttpContext;
            var statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;

            var meta = new Meta
            {
                Timestamp = DateTime.UtcNow,
                Path = httpContext.Request.Path,
                Method = httpContext.Request.Method,
                StatusCode = statusCode,
                TraceId = httpContext.TraceIdentifier,
                RequestId = httpContext.Items["RequestId"]?.ToString(),
                ClientIp = httpContext.Connection.RemoteIpAddress?.ToString()
            };

            var wrappedResponse = ApiResponse.Success(objectResult.Value, meta);

            context.Result = new ObjectResult(wrappedResponse)
            {
                StatusCode = statusCode,
                DeclaredType = typeof(ApiResponse)
            };

            await next();
        }
    }
}