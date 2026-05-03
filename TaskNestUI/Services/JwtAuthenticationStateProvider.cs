namespace TaskNestUI.Services
{
    public class JwtAuthenticationStateProvider(ILocalStorageService localStorage, HttpClient httpClient, ILogger<JwtAuthenticationStateProvider> logger) : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage = localStorage;
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<JwtAuthenticationStateProvider> _logger = logger;
        private bool _isInitialized = false;
        private AuthenticationState _currentState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (!_isInitialized)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogInformation("No token found");
                    _currentState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                    return _currentState;
                }

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    _logger.LogWarning("Token has expired");
                    await _localStorage.RemoveItemAsync("authToken");
                    _currentState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                    return _currentState;
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var claims = ParseClaimsFromJwt(token);
                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);

                _logger.LogInformation("User authenticated: {User}", user.Identity?.Name);
                _currentState = new AuthenticationState(user);
                return _currentState;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting authentication state");
                _currentState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                return _currentState;
            }
        }

        public async Task InitializeAsync()
        {
            _logger.LogInformation("Initializing JWT Authentication Provider");
            _isInitialized = true;
            var state = await GetAuthenticationStateAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(state));
        }

        public async Task Login(string token)
        {
            _logger.LogInformation("Login called, storing token");
            await _localStorage.SetItemAsync("authToken", token);

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            _isInitialized = true;

            var authState = await GetAuthenticationStateAsync();

            NotifyAuthenticationStateChanged(Task.FromResult(authState));

            _logger.LogInformation("Login completed, state notified");
        }

        public async Task Logout()
        {
            _logger.LogInformation("Logout called");
            await _localStorage.RemoveItemAsync("authToken");
            _httpClient.DefaultRequestHeaders.Authorization = null;

            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = new AuthenticationState(anonymousUser);
            _currentState = authState;

            NotifyAuthenticationStateChanged(Task.FromResult(authState));

            _logger.LogInformation("Logout completed, state notified");
        }


        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            try
            {
                var payload = jwt.Split('.')[1];
                var jsonBytes = ParseBase64WithoutPadding(payload);
                var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

                var claims = keyValuePairs?.Select(kvp => new Claim(kvp.Key, kvp.Value?.ToString() ?? string.Empty))
                    ?? Enumerable.Empty<Claim>();

                _logger.LogInformation("Parsed {Count} claims from JWT", claims.Count());
                return claims;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing JWT claims");
                return Enumerable.Empty<Claim>();
            }
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }

    }
}

