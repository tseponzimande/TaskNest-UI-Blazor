namespace TaskNestUI.Services
{
    public class SignalRService(
        ILocalStorageService localStorage,
        IConfiguration configuration,
        ILogger<SignalRService> logger) : IAsyncDisposable
    {
        private HubConnection? _hubConnection;
        private readonly ILocalStorageService _localStorage = localStorage;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<SignalRService> _logger = logger;

        public event Action<object>? OnNotificationReceived;
        public event Action? OnBoardRefreshRequested;
        public event Action<string>? OnTaskDragStarted;
        public event Action<string>? OnTaskDragEnded;
        public event Action<object>? OnColumnReordered;

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public async Task StartConnectionAsync()
        {
            if (_hubConnection != null && IsConnected)
            {
                _logger.LogInformation("SignalR already connected");
                return;
            }

            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("No auth token found, cannot connect to SignalR");
                    return;
                }

                var hubUrl = _configuration["TaskNestApiBase"]?.TrimEnd('/') + "/notificationHub";
                _logger.LogInformation($"Connecting to SignalR hub at: {hubUrl}");

                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(hubUrl, options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult<string?>(token);
                    })
                    .WithAutomaticReconnect()
                    .ConfigureLogging(logging =>
                    {
                        logging.SetMinimumLevel(LogLevel.Information);
                    })
                    .Build();

               
                _hubConnection.On<object>("ReceiveNotification", (notification) =>
                {
                    _logger.LogInformation($"Received notification: {notification}");
                    OnNotificationReceived?.Invoke(notification);
                });

                
                _hubConnection.On<object>("RefreshBoard", (data) =>
                {
                    _logger.LogInformation("Board refresh requested");
                    OnBoardRefreshRequested?.Invoke();
                });

                
                _hubConnection.On<object>("TaskDragStarted", (data) =>
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(data);
                    var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, System.Text.Json.JsonElement>>(json);
                    var taskId = dict?["TaskId"].GetString();
                    if (!string.IsNullOrEmpty(taskId))
                    {
                        OnTaskDragStarted?.Invoke(taskId);
                    }
                });

                _hubConnection.On<object>("TaskDragEnded", (data) =>
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(data);
                    var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, System.Text.Json.JsonElement>>(json);
                    var taskId = dict?["TaskId"].GetString();
                    if (!string.IsNullOrEmpty(taskId))
                    {
                        OnTaskDragEnded?.Invoke(taskId);
                    }
                });

                _hubConnection.On<object>("ColumnReordered", (data) =>
                {
                    _logger.LogInformation("Column reorder notification received");
                    OnColumnReordered?.Invoke(data);
                });

                _hubConnection.Reconnecting += error =>
                {
                    _logger.LogWarning($"SignalR reconnecting: {error?.Message}");
                    return Task.CompletedTask;
                };

                _hubConnection.Reconnected += connectionId =>
                {
                    _logger.LogInformation($"SignalR reconnected: {connectionId}");
                    return Task.CompletedTask;
                };

                _hubConnection.Closed += error =>
                {
                    _logger.LogWarning($"SignalR connection closed: {error?.Message}");
                    return Task.CompletedTask;
                };

                await _hubConnection.StartAsync();
                _logger.LogInformation($"SignalR connection started successfully. State: {_hubConnection.State}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error starting SignalR connection: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
            }
        }

        public async Task JoinBoardGroupAsync(Guid boardId)
        {
            if (_hubConnection != null && IsConnected)
            {
                try
                {
                    await _hubConnection.InvokeAsync("JoinBoardGroup", boardId.ToString());
                    _logger.LogInformation($"Joined board group: {boardId}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error joining board group: {ex.Message}");
                }
            }
        }

        public async Task LeaveBoardGroupAsync(Guid boardId)
        {
            if (_hubConnection != null && IsConnected)
            {
                try
                {
                    await _hubConnection.InvokeAsync("LeaveBoardGroup", boardId.ToString());
                    _logger.LogInformation($"Left board group: {boardId}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error leaving board group: {ex.Message}");
                }
            }
        }

        public async Task NotifyTaskDragStartAsync(Guid boardId, Guid taskId, string userId)
        {
            if (_hubConnection == null)
            {
                _logger.LogWarning("Cannot notify drag start - hub connection is null");
                return;
            }

            if (!IsConnected)
            {
                _logger.LogWarning($"Cannot notify drag start - not connected. State: {_hubConnection.State}");
                return;
            }

            try
            {
                _logger.LogInformation($"Notifying drag start for task {taskId} on board {boardId}");
                await _hubConnection.InvokeAsync("NotifyTaskDragStart",
                    boardId.ToString(), taskId.ToString(), userId);
                _logger.LogInformation("Drag start notification sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error notifying drag start: {ex.Message}");
            }
        }

        public async Task NotifyTaskDragEndAsync(Guid boardId, Guid taskId)
        {
            if (_hubConnection == null || !IsConnected)
            {
                _logger.LogWarning("Cannot notify drag end - not connected");
                return;
            }

            try
            {
                _logger.LogInformation($"Notifying drag end for task {taskId}");
                await _hubConnection.InvokeAsync("NotifyTaskDragEnd",
                    boardId.ToString(), taskId.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error notifying drag end: {ex.Message}");
            }
        }

        public async Task StopConnectionAsync()
        {
            if (_hubConnection != null)
            {
                try
                {
                    await _hubConnection.StopAsync();
                    _logger.LogInformation("SignalR connection stopped");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error stopping SignalR connection: {ex.Message}");
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}
