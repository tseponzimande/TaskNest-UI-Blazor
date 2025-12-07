namespace TaskNestUI.Services
{
    public class AuthHttpMessageHandler(ILocalStorageService localStorage, ILogger<AuthHttpMessageHandler> logger) : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage = localStorage;
        private readonly ILogger<AuthHttpMessageHandler> _logger = logger;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    _logger.LogInformation($"Adding auth token to request: {request.RequestUri}");
                }
                else
                {
                    _logger.LogWarning($"No auth token found for request: {request.RequestUri}");
                }
            }
            catch (InvalidOperationException ex)
            {
                // JS interop not available during prerendering
                _logger.LogWarning($"Cannot access localStorage (prerendering?): {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting auth token: {ex.Message}");
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
