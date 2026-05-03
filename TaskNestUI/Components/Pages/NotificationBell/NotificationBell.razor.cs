namespace TaskNestUI.Components.Pages.NotificationBell
{
    public partial class NotificationBell
    {
        #region Dependencies

        [Inject]
        private SignalRService SignalRService { get; set; } = null!;

        [Inject]
        private NotificationService NotificationService { get; set; } = null!;

        [Inject]
        private ILogger<NotificationBell> Logger { get; set; } = null!;

        #endregion


        #region Fields and Properties

        private List<NotificationItem> notifications = new();
        private int unreadCount = 0;
        private bool showNotifications = false;

        #endregion


        #region LifCycle Methods

        protected override async Task OnInitializedAsync()
        {
            SignalRService.OnNotificationReceived += HandleNotification;
            await SignalRService.StartConnectionAsync();
        }

        #endregion

        #region Notification

        private void HandleNotification(object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                var notificationData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

                if (notificationData != null)
                {
                    var notification = new NotificationItem
                    {
                        Id = Guid.NewGuid(),
                        Type = notificationData.GetValueOrDefault("Type").GetString() ?? "Unknown",
                        Message = notificationData.GetValueOrDefault("Message").GetString() ?? "New notification",
                        Timestamp = notificationData.GetValueOrDefault("Timestamp").GetDateTime(),
                        IsRead = false
                    };

                    notifications.Add(notification);
                    unreadCount = notifications.Count(n => !n.IsRead);

                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Info,
                        Summary = GetNotificationTitle(notification.Type),
                        Detail = notification.Message,
                        Duration = 4000
                    });

                    InvokeAsync(StateHasChanged);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($": {ex.Message}");
                //Console.WriteLine($"Error handling notification: {ex.Message}");
            }
        }

        #endregion


        #region ToggleNotifications

        private void ToggleNotifications()
        {
            showNotifications = !showNotifications;
            StateHasChanged();
        }
        #endregion

        #region MarkAsRead

        private void MarkAsRead(NotificationItem notification)
        {
            if (!notification.IsRead)
            {
                notification.IsRead = true;
                unreadCount = notifications.Count(n => !n.IsRead);
                StateHasChanged();
            }
        }

        #endregion

        #region ClearAll
        private void ClearAll()
        {
            notifications.Clear();
            unreadCount = 0;
            showNotifications = false;
            StateHasChanged();
        }

        #endregion

        #region GetNotificationIcon

        private string GetNotificationIcon(string type) => type switch
        {
            "TaskCreated" => "add_circle",
            "TaskUpdated" => "edit",
            "TaskMoved" => "swap_horiz",
            "TaskDeleted" => "delete",
            "CommentAdded" => "comment",
            _ => "notifications"
        };

        #endregion

        #region GetNotificationColor

        private string GetNotificationColor(string type) => type switch
        {
            "TaskCreated" => "#4caf50",
            "TaskUpdated" => "#2196f3",
            "TaskMoved" => "#ff9800",
            "TaskDeleted" => "#f44336",
            "CommentAdded" => "#9c27b0",
            _ => "#666"
        };

        #endregion

        #region GetNotificationTitle

        private string GetNotificationTitle(string type) => type switch
        {
            "TaskCreated" => "Task Created",
            "TaskUpdated" => "Task Updated",
            "TaskMoved" => "Task Moved",
            "TaskDeleted" => "Task Deleted",
            "CommentAdded" => "New Comment",
            _ => "Notification"
        };

        #endregion

        #region GetRelativeTime

        private string GetRelativeTime(DateTime timestamp)
        {
            var span = DateTime.UtcNow - timestamp;
            if (span.TotalMinutes < 1) return "Just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
            return $"{(int)span.TotalDays}d ago";
        }

        #endregion

        #region DisposeAsync

        public async ValueTask DisposeAsync()
        {
            SignalRService.OnNotificationReceived -= HandleNotification;
            await SignalRService.StopConnectionAsync();
        }
        #endregion
    }
}
