using AcademicManager.Web.Constants;

namespace AcademicManager.Web.Services
{
    public interface IErrorHandler
    {
        Task LogErrorAsync(Exception exception, string? message = null, string? context = null);
        Task LogWarningAsync(string message, string? context = null);
        Task LogInfoAsync(string message, string? context = null);
        void ShowError(Exception exception, string? userMessage = null);
        void ShowSuccess(string message);
        void ShowWarning(string message);
        void ShowInfo(string message);
        void ClearErrors();
        List<Exception> GetErrors();
        event Action<Exception, string?>? OnErrorOccurred;
    }

    public class ErrorHandler : IErrorHandler
    {
        private readonly ILogger<ErrorHandler> logger;
        private readonly List<Exception> errors = new();

        public event Action<Exception, string?>? OnErrorOccurred;

        public ErrorHandler(ILogger<ErrorHandler> logger)
        {
            this.logger = logger;
        }

        public async Task LogErrorAsync(Exception exception, string? message = null, string? context = null)
        {
            var fullMessage = BuildFullMessage(message, context);
            
            logger.LogError(exception, fullMessage);
            
            errors.Add(exception);
            OnErrorOccurred?.Invoke(exception, GetUserFriendlyMessage(exception));
            
            await Task.CompletedTask;
        }

        public async Task LogWarningAsync(string message, string? context = null)
        {
            var fullMessage = BuildFullMessage(message, context);
            
            logger.LogWarning(fullMessage);
            
            await Task.CompletedTask;
        }

        public async Task LogInfoAsync(string message, string? context = null)
        {
            var fullMessage = BuildFullMessage(message, context);
            
            logger.LogInformation(fullMessage);
            
            await Task.CompletedTask;
        }

        public void ShowError(Exception exception, string? userMessage = null)
        {
            errors.Add(exception);
            OnErrorOccurred?.Invoke(exception, userMessage ?? GetUserFriendlyMessage(exception));
        }

        public void ShowSuccess(string message)
        {
            logger.LogInformation("Success: {Message}", message);
        }

        public void ShowWarning(string message)
        {
            logger.LogWarning("Warning: {Message}", message);
        }

        public void ShowInfo(string message)
        {
            logger.LogInformation("Info: {Message}", message);
        }

        public void ClearErrors()
        {
            errors.Clear();
        }

        public List<Exception> GetErrors()
        {
            return new List<Exception>(errors);
        }

        private static string BuildFullMessage(string? message, string? context)
        {
            var fullMessage = string.Empty;
            
            if (!string.IsNullOrEmpty(context))
            {
                fullMessage += $"[{context}] ";
            }
            
            if (!string.IsNullOrEmpty(message))
            {
                fullMessage += message;
            }
            
            return fullMessage;
        }

        private static string GetUserFriendlyMessage(Exception exception)
        {
            return exception switch
            {
                ArgumentException => "Los datos proporcionados no son válidos.",
                UnauthorizedAccessException => "No tiene permisos para realizar esta acción.",
                TimeoutException => "La operación tardó demasiado tiempo. Intente nuevamente.",
                InvalidOperationException => "La operación no es válida en este momento.",
                _ => UIConstants.DefaultErrorMessage
            };
        }
    }

    public static class ExceptionExtensions
    {
        public static string GetFullMessage(this Exception exception)
        {
            var message = exception.Message;
            
            if (exception.InnerException != null)
            {
                message += $" → {GetFullMessage(exception.InnerException)}";
            }
            
            return message;
        }

        public static Dictionary<string, object> GetErrorData(this Exception exception)
        {
            var data = new Dictionary<string, object>
            {
                ["Type"] = exception.GetType().Name,
                ["Message"] = exception.Message,
                ["Timestamp"] = DateTime.UtcNow,
                ["StackTrace"] = exception.StackTrace ?? string.Empty
            };

            foreach (var key in exception.Data.Keys)
            {
                if (key != null)
                {
                    data[key.ToString()!] = exception.Data[key]!;
                }
            }

            return data;
        }

        public static bool IsCritical(this Exception exception)
        {
            return exception is OutOfMemoryException or StackOverflowException or ThreadAbortException;
        }

        public static bool IsTransient(this Exception exception)
        {
            return exception is TimeoutException or 
                   OperationCanceledException or 
                   System.Net.Http.HttpRequestException;
        }
    }
}