using Microsoft.AspNetCore.Mvc;
using WineApi.Exceptions;

namespace WineApi.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
                await HandleExceptionAsync(context, ex, _logger);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex, ILogger logger)
        {
            var statusCode = ex switch
            {
                ConflictException => StatusCodes.Status409Conflict,
                NotFoundException => StatusCodes.Status404NotFound,
                InvalidRequestException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };
            
            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                logger.LogError(ex, "Internal Server Error");
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = ex.GetType().Name,
                Detail = ex.Message,
                Instance = context.Request.Path
            };

            return context.Response.WriteAsJsonAsync(problem);
        }
    }
}
