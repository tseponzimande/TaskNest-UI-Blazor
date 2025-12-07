namespace TaskNestUI.Services
{
    public class BoardUserService(HttpClient httpClient, ILogger<BoardUserService> logger) : IBoardUserService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<BoardUserService> _logger = logger;

        public async Task<IEnumerable<BoardUserDto>> GetBoardUsersAsync(Guid boardId)
        {
            try
            {
                var users = await _httpClient.GetFromJsonAsync<List<BoardUserDto>>($"api/boards/{boardId}/users");
                return users ?? Enumerable.Empty<BoardUserDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occured : {ex.Message}");
                //Console.WriteLine($"Error fetching board users: {ex.Message}");
                return Enumerable.Empty<BoardUserDto>();
            }
        }

        public async Task<BoardUserDto?> AddUserToBoardAsync(Guid boardId, AddBoardUserDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/boards/{boardId}/users", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error adding user to board: {response.StatusCode} - {error}");
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<BoardUserDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occured : {ex.Message}");
                //Console.WriteLine($"Exception adding user to board: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateUserRoleAsync(Guid boardId, string userId, BoardUserRole role)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/boards/{boardId}/users/{userId}", role);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error updating user role: {response.StatusCode} - {error}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occured : {ex.Message}");
                //Console.WriteLine($"Exception updating user role: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveUserFromBoardAsync(Guid boardId, string userId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/boards/{boardId}/users/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error removing user from board: {response.StatusCode} - {error}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occured : {ex.Message}");
                //Console.WriteLine($"Exception removing user from board: {ex.Message}");
                return false;
            }
        }
    }
}
