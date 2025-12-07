namespace TaskNestUI.Services
{
    public class BoardService(HttpClient httpClient, ILogger<BoardService> logger) : IBoardService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<BoardService> _logger = logger;

        public async Task<IEnumerable<BoardDto>> GetBoardAsyc()
        {
            try
            {
                var items = await _httpClient.GetFromJsonAsync<List<BoardDto>>("api/boards");
                return items ?? Enumerable.Empty<BoardDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occured : {ex.Message}");
                //Console.WriteLine($"Error fetching boards: {ex.Message}");
                return Enumerable.Empty<BoardDto>();
            }
        }

        public async Task<BoardDto?> GetBoardByIdAsync(Guid Id)
        {
            try
            {
                var item = await _httpClient.GetFromJsonAsync<BoardDto>($"api/boards/{Id}");
                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occured : {ex.Message}");
                //Console.WriteLine($"Error fetching board: {ex.Message}");
                return null;
            }
        }

        public async Task<BoardDto?> CreateBoardAsync(BoardDto dto)
        {
            try
            {
                var res = await _httpClient.PostAsJsonAsync("api/boards", dto);

                if (!res.IsSuccessStatusCode)
                {
                    var errorContent = await res.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error creating board: {res.StatusCode} - {errorContent}");
                    return null;
                }

                return await res.Content.ReadFromJsonAsync<BoardDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occured : {ex.Message}");
                //Console.WriteLine($"Exception creating board: {ex.Message}");
                return null;
            }
        }

        public async Task<BoardDto?> UpdateBoardAsync(BoardDto dto)
        {
            try
            {
                var res = await _httpClient.PutAsJsonAsync($"api/board/{dto.Id}", dto);

                if (!res.IsSuccessStatusCode)
                {
                    var errorContent = await res.Content.ReadAsStringAsync();
                    _logger.LogError($"Error updating board: {res.StatusCode} - {errorContent}");
                    //Console.WriteLine($"Error updating board: {res.StatusCode} - {errorContent}");
                }
                return dto;

            }
            catch (Exception ex)
            {
                _logger.LogError($" Error Occured :{ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteBoardAsync(Guid Id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/boards/{Id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occured : {ex.Message}");
                //Console.WriteLine($"Error deleting board: {ex.Message}");
                return false;
            }
        }
    }
}