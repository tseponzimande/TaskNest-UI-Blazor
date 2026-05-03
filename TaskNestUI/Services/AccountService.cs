namespace TaskNestUI.Services
{
    public class AccountService(HttpClient httpClient, NavigationManager navigationManager, ILogger<AccountService> logger, ILocalStorageService localStorage,
        AuthenticationStateProvider authStateProvider) : IAccountService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly NavigationManager _nav = navigationManager;
        private readonly ILogger<AccountService> _logger = logger;
        private readonly ILocalStorageService _localStorage = localStorage;
        private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

        public async Task<string?> LoginAsync(LoginDto model)
        {
            try
            {
                _logger.LogInformation($"Attempting login for: {model.Email}");

                var response = await _httpClient.PostAsJsonAsync("api/account/login", model);
                _logger.LogInformation($"Login response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                    if (result != null && result.TryGetValue("token", out var token) && !string.IsNullOrEmpty(token))
                    {
                        _logger.LogInformation("Token received successfully");

                        await _localStorage.SetItemAsync("authToken", token);
                        if (_authStateProvider is JwtAuthenticationStateProvider jwtProvider)
                        {
                            await jwtProvider.Login(token);
                        }

                        return token;
                    }
                    else
                    {
                        _logger.LogWarning("Token not found in response body");
                        var responseBody = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning($"Response body: {responseBody}");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Login failed: {response.StatusCode} - {errorContent}");
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during login: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<bool> RegisterAsync(RegisterDto model)
        {
            try
            {
                _logger.LogInformation($"Attempting to register user: {model.Email}");
                _logger.LogInformation($"API Base: {_httpClient.BaseAddress}");

                var response = await _httpClient.PostAsJsonAsync("api/account/register", model);
                _logger.LogInformation($"Registration response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Registration successful");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Registration failed: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during registration: {ex.Message}");
                _logger.LogError($"Full exception: {ex}");
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordDto model)
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                var request = new HttpRequestMessage(HttpMethod.Post, "api/account/change-password")
                {
                    Content = JsonContent.Create(model)
                };

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occured: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/account/confirm-email?userId={userId}&token={token}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occured :{ex.Message}");
                return false;
            }
        }
    }
}
