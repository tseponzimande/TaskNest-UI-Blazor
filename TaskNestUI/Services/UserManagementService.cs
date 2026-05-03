namespace TaskNestUI.Services
{
    public class UserManagementService(HttpClient httpClient, ILogger<UserManagementService> logger) : IUserManagementService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<UserManagementService> _logger = logger;

        public async Task<IEnumerable<UserManagementDtos>> GetAllUsersAsync()
        {
            try
            {
                var users = await _httpClient.GetFromJsonAsync<List<UserManagementDtos>>("api/usermanagement");
                return users ?? Enumerable.Empty<UserManagementDtos>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching users: {ex.Message}");
                return Enumerable.Empty<UserManagementDtos>();
            }
        }

        public async Task<UserManagementDtos?> GetUserByIdAsync(string userId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<UserManagementDtos>($"api/usermanagement/{userId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> ToggleUserStatusAsync(string userId, bool isEnabled)
        {
            try
            {
                var dto = new ToggleUserStatusDto { IsEnabled = isEnabled };
                var response = await _httpClient.PutAsJsonAsync($"api/usermanagement/{userId}/toggle-status", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error toggling user status: {response.StatusCode} - {error}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception toggling user status: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateUserRolesAsync(string userId, List<string> roles)
        {
            try
            {
                var dto = new UpdateUserRolesDto { Roles = roles };
                var response = await _httpClient.PutAsJsonAsync($"api/usermanagement/{userId}/roles", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error updating user roles: {response.StatusCode} - {error}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception updating user roles: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/usermanagement/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error deleting user: {response.StatusCode} - {error}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception deleting user: {ex.Message}");
                return false;
            }
        }
    }
}
