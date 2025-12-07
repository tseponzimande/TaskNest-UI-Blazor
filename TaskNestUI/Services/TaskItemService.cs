namespace TaskNestUI.Services
{
    public class TaskItemService(HttpClient httpClient, ILogger<TaskItemService> logger) : ITaskItemService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<TaskItemService> _logger = logger;

        public async Task<IEnumerable<TaskItemDto>> GetTasksByBoardIdAsync(Guid boardId)
        {
            try
            {
                var allTasks = await _httpClient.GetFromJsonAsync<List<TaskItemDto>>("api/taskitems");
                return allTasks?.Where(t => t.BoardId == boardId) ?? Enumerable.Empty<TaskItemDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occured : {ex.Message}");
                //Console.WriteLine($"Error fetching tasks: {ex.Message}");
                return Enumerable.Empty<TaskItemDto>();
            }
        }

        public async Task<IEnumerable<TaskItemDto>> GetTasksByColumnIdAsync(Guid columnId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<TaskItemDto>>($"api/taskitems/byColumn/{columnId}")
                    ?? Enumerable.Empty<TaskItemDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occured : {ex.Message}");
                //Console.WriteLine($"Error fetching tasks by column: {ex.Message}");
                return Enumerable.Empty<TaskItemDto>();
            }
        }

        public async Task<TaskItemDto?> GetTaskByIdAsync(Guid id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<TaskItemDto>($"api/taskitems/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occured : {ex.Message}");
                //Console.WriteLine($"Error fetching task: {ex.Message}");
                return null;
            }
        }

        public async Task<TaskItemDto?> CreateTaskAsync(TaskItemDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/taskitems", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error creating task: {response.StatusCode} - {error}");
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<TaskItemDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occured : {ex.Message}");
                //Console.WriteLine($"Exception creating task: {ex.Message}");
                return null;
            }
        }

        public async Task<TaskItemDto?> UpdateTaskAsync(TaskItemDto dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/taskitems/{dto.Id}", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error updating task: {response.StatusCode} - {error}");
                    return null;
                }

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occured : {ex.Message}");
                //Console.WriteLine($"Exception updating task: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteTaskAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/taskitems/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error deleting task: {response.StatusCode} - {error}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occured : {ex.Message}");
                //Console.WriteLine($"Exception deleting task: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> MoveTaskAsync(Guid taskId, Guid newColumnId, int newPosition)
        {
            try
            {
                var moveDto = new { ColumnId = newColumnId, Position = newPosition };
                var response = await _httpClient.PutAsJsonAsync($"api/taskitems/{taskId}/move", moveDto);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error moving task: {response.StatusCode} - {error}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occured : {ex.Message}");
                //Console.WriteLine($"Exception moving task: {ex.Message}");
                return false;
            }
        }
    }
}
