using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Exceptions;
using System.Net;
using System.Text.Json;

namespace FresherMisa2026.WebAPI.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
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

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var statusCode = (int)HttpStatusCode.InternalServerError;
            var userMessage = "Có lỗi xảy ra vui lòng liên hệ Misa!";
            var devMessage = exception.Message;

            switch (exception)
            {
                case KeyNotFoundException:
                    statusCode = (int)HttpStatusCode.NotFound;
                    userMessage = exception.Message;
                    break;
                case ArgumentException:
                case InvalidOperationException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    userMessage = exception.Message;
                    break;
                case DuplicateEntityException:
                    statusCode = (int)HttpStatusCode.Conflict;
                    userMessage = exception.Message;
                    devMessage = exception.Message;
                    break;
            }

            context.Response.StatusCode = statusCode;

            var response = new ServiceResponse
            {
                IsSuccess = false,
                Code = statusCode,
                UserMessage = userMessage,
                DevMessage = devMessage
            };

            var jsonResponse = JsonSerializer.Serialize(response, _jsonOptions);

            return context.Response.WriteAsync(jsonResponse);
        }

    }
}

