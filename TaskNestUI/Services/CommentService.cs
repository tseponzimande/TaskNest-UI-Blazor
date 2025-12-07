namespace TaskNestUI.Services
{
    public class CommentService(HttpClient httpClient, ILogger<CommentService> logger) : ICommentService
    {
        public readonly HttpClient _httpClient = httpClient;
        public readonly ILogger<CommentService> _logger = logger;

        public async Task<IEnumerable<CommentDto>> GetCommentsByTaskIdAsync(Guid taskId)
        {
            try
            {
                var comments = await _httpClient.GetFromJsonAsync<List<CommentDto>>($"api/tasks/{taskId}/comments");
                return comments ?? Enumerable.Empty<CommentDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occured : {ex.Message}");
                //Console.WriteLine($"Error fetching comments: {ex.Message}");
                return Enumerable.Empty<CommentDto>();
            }
        }

        public async Task<CommentDto?> CreateCommentAsync(Guid taskId, CreateCommentDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/tasks/{taskId}/comments", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error creating comment: {response.StatusCode} - {error}");
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<CommentDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occured : {ex.Message}");
                return null;
            }

        }

        public async Task<bool> DeleteCommentAsync(Guid taskId, Guid commentId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/tasks/{taskId}/comments/{commentId}");

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error deleting comment: {response.StatusCode} - {error}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occured : {ex.Message}");
                //Console.WriteLine($"Exception deleting comment: {ex.Message}");
                return false;
            }

        }
    }
}
