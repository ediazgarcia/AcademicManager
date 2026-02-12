namespace AcademicManager.Web.Services
{
    public interface INotificationService
    {
        event Action<Notification>? OnNotificationAdded;
        event Action? OnNotificationsCleared;
        
        void ShowSuccess(string message, string? title = null, int duration = 5000);
        void ShowError(string message, string? title = null, int duration = 8000);
        void ShowWarning(string message, string? title = null, int duration = 6000);
        void ShowInfo(string message, string? title = null, int duration = 4000);
        void Show(Notification notification);
        void ClearAll();
        void ClearExpired();
        List<Notification> GetActiveNotifications();
        void MarkAsRead(Guid id);
        void Remove(Guid id);
    }

    public class Notification
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int Duration { get; set; } = 5000; // in milliseconds
        public bool IsRead { get; set; }
        public bool AutoClose { get; set; } = true;
        public Dictionary<string, object> Metadata { get; set; } = new();

        public bool IsExpired => AutoClose && DateTime.UtcNow > CreatedAt.AddMilliseconds(Duration);
        
        public string GetIcon() => Type switch
        {
            NotificationType.Success => "✅",
            NotificationType.Error => "❌",
            NotificationType.Warning => "⚠️",
            NotificationType.Info => "ℹ️",
            _ => "📢"
        };

        public string GetCssClass() => Type switch
        {
            NotificationType.Success => "notification-success",
            NotificationType.Error => "notification-error",
            NotificationType.Warning => "notification-warning",
            NotificationType.Info => "notification-info",
            _ => "notification-default"
        };
    }

    public enum NotificationType
    {
        Success,
        Error,
        Warning,
        Info
    }

    public class NotificationService : INotificationService
    {
        private readonly List<Notification> notifications = new();
        private readonly Timer cleanupTimer;

        public event Action<Notification>? OnNotificationAdded;
        public event Action? OnNotificationsCleared;

        public NotificationService()
        {
            // Set up a timer to clean up expired notifications every second
            cleanupTimer = new Timer(CleanupNotifications, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        public void ShowSuccess(string message, string? title = null, int duration = 5000)
        {
            Show(new Notification
            {
                Title = title ?? "Éxito",
                Message = message,
                Type = NotificationType.Success,
                Duration = duration
            });
        }

        public void ShowError(string message, string? title = null, int duration = 8000)
        {
            Show(new Notification
            {
                Title = title ?? "Error",
                Message = message,
                Type = NotificationType.Error,
                Duration = duration
            });
        }

        public void ShowWarning(string message, string? title = null, int duration = 6000)
        {
            Show(new Notification
            {
                Title = title ?? "Advertencia",
                Message = message,
                Type = NotificationType.Warning,
                Duration = duration
            });
        }

        public void ShowInfo(string message, string? title = null, int duration = 4000)
        {
            Show(new Notification
            {
                Title = title ?? "Información",
                Message = message,
                Type = NotificationType.Info,
                Duration = duration
            });
        }

        public void Show(Notification notification)
        {
            lock (notifications)
            {
                notifications.Add(notification);
            }

            OnNotificationAdded?.Invoke(notification);
        }

        public void ClearAll()
        {
            lock (notifications)
            {
                notifications.Clear();
            }

            OnNotificationsCleared?.Invoke();
        }

        public void ClearExpired()
        {
            lock (notifications)
            {
                var expired = notifications.Where(n => n.IsExpired).ToList();
                foreach (var notification in expired)
                {
                    notifications.Remove(notification);
                }
            }
        }

        public List<Notification> GetActiveNotifications()
        {
            lock (notifications)
            {
                return notifications.Where(n => !n.IsExpired).ToList();
            }
        }

        public void MarkAsRead(Guid id)
        {
            lock (notifications)
            {
                var notification = notifications.FirstOrDefault(n => n.Id == id);
                if (notification != null)
                {
                    notification.IsRead = true;
                }
            }
        }

        public void Remove(Guid id)
        {
            lock (notifications)
            {
                notifications.RemoveAll(n => n.Id == id);
            }
        }

        private void CleanupNotifications(object? state)
        {
            ClearExpired();
        }

        public void Dispose()
        {
            cleanupTimer?.Dispose();
        }
    }

    public static class NotificationExtensions
    {
        public static Notification WithMetadata(this Notification notification, string key, object value)
        {
            notification.Metadata[key] = value;
            return notification;
        }

        public static Notification WithAutoClose(this Notification notification, bool autoClose = true)
        {
            notification.AutoClose = autoClose;
            return notification;
        }

        public static Notification WithDuration(this Notification notification, int durationMs)
        {
            notification.Duration = durationMs;
            return notification;
        }

        public static Notification WithTitle(this Notification notification, string title)
        {
            notification.Title = title;
            return notification;
        }
    }
}