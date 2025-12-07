namespace TaskNestUI.Services
{
    public class LocalStorageServiceWrapper(ILocalStorageService localStorage)
    {
        private readonly ILocalStorageService _localStorage = localStorage;

        public async Task<string?> GetItemAsync(string key)
        {
            try
            {
                return await _localStorage.GetItemAsync<string>(key);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            catch (JSException)
            {
                return null;
            }
        }

        public async Task SetItemAsync(string key, string value)
        {
            try
            {
                await _localStorage.SetItemAsync(key, value);
            }
            catch (InvalidOperationException)
            {
                // JS interop not available during prerendering
            }
            catch (JSException)
            {
                // JavaScript error
            }
        }

        public async Task RemoveItemAsync(string key)
        {
            try
            {
                await _localStorage.RemoveItemAsync(key);
            }
            catch (InvalidOperationException)
            {
                // JS interop not available during prerendering
            }
            catch (JSException)
            {
                // JavaScript error
            }
        }
    }
}
