using Core.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace Articles.Api.Middleware
{
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var errorResponse = new ErrorResponse();

            switch (exception)
            {
                case ArticleNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.Message = exception.Message;
                    errorResponse.ErrorType = "ArticleNotFound";
                    break;

                case NewspaperNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.Message = exception.Message;
                    errorResponse.ErrorType = "NewspaperNotFound";
                    break;

                case DuplicateEntityException:
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    errorResponse.Message = exception.Message;
                    errorResponse.ErrorType = "DuplicateEntity";
                    break;

                case S3ServiceException:
                    response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    errorResponse.Message = "There is a problem connecting to the S3 service. Please try again later.";
                    errorResponse.ErrorType = "S3ServiceError";
                    break;

                case ValidationException validationEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = "Validation failed.";
                    errorResponse.ErrorType = "ValidationError";
                    errorResponse.ValidationErrors = validationEx.Errors.Select(e => e.ErrorMessage ?? "Validation error").ToList();
                    break;

                case InvalidOperationException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = "The request could not be processed.";
                    errorResponse.ErrorType = "InvalidOperation";
                    break;

                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse.Message = "Unauthorized access.";
                    errorResponse.ErrorType = "Unauthorized";
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.Message = "An unexpected error occurred.";
                    errorResponse.ErrorType = "InternalServerError";
                    
                    // Log the full exception for debugging
                    _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
                    break;
            }

            // Log the error
            _logger.LogWarning("Exception handled: {ErrorType} - {Message}", errorResponse.ErrorType, errorResponse.Message);

            var result = JsonSerializer.Serialize(errorResponse);
            await response.WriteAsync(result);
        }
    }

    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public string ErrorType { get; set; } = string.Empty;
        public string? Details { get; set; }
        public List<string>? ValidationErrors { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
} 