namespace TaskNestUI.Services
{
    public class BoardColumnService : IBoardColumnService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BoardColumnService> _logger;

        public BoardColumnService(HttpClient httpClient, ILogger<BoardColumnService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<BoardColumnDto>> GetColumnByIdAsync(Guid columnId)
        {
            try
            {
                var column = await _httpClient.GetFromJsonAsync<BoardColumnDto>($"api/boardcolumns/{columnId}");
                return column != null ? new[] { column } : Enumerable.Empty<BoardColumnDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching column: {ex.Message}");
                return Enumerable.Empty<BoardColumnDto>();
            }
        }

        public async Task<IEnumerable<BoardColumnDto>> GetColumnsByBoardIdAsync(Guid boardId)
        {
            try
            {
                var columns = await _httpClient.GetFromJsonAsync<List<BoardColumnDto>>($"api/boardcolumns/byBoard/{boardId}");
                return columns ?? Enumerable.Empty<BoardColumnDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching columns: {ex.Message}");
                return Enumerable.Empty<BoardColumnDto>();
            }
        }

        public async Task<IEnumerable<BoardColumnDto>> CreateColumnAsync(BoardColumnDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/boardcolumns", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error creating column: {response.StatusCode} - {error}");
                    return Enumerable.Empty<BoardColumnDto>();
                }

                var created = await response.Content.ReadFromJsonAsync<BoardColumnDto>();
                return created is not null ? new List<BoardColumnDto> { created } : Enumerable.Empty<BoardColumnDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception creating column: {ex.Message}");
                return Enumerable.Empty<BoardColumnDto>();
            }
        }

        public async Task<bool> DeleteColumnAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/boardcolumns/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error deleting column: {response.StatusCode} - {error}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception deleting column: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<BoardColumnDto>> UpdateColumnAsync(BoardColumnDto dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/boardcolumns/{dto.Id}", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error updating column: {response.StatusCode} - {error}");
                    return Enumerable.Empty<BoardColumnDto>();
                }

                return new[] { dto };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception updating column: {ex.Message}");
                return Enumerable.Empty<BoardColumnDto>();
            }
        }

        public async Task<bool> ReorderColumnsAsync(List<ColumnOrderDto> columnOrders)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync("api/boardcolumns/reorder", columnOrders);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error reordering columns: {response.StatusCode} - {error}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception reordering columns: {ex.Message}");
                return false;
            }
        }
    }
}
