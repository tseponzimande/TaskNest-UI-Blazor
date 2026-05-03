namespace TaskNestUI.Services
{
    public class TaskAttachmentService(
        HttpClient httpClient,
        ILogger<TaskAttachmentService> logger,
        IJSRuntime jsRuntime) : ITaskAttachmentService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<TaskAttachmentService> _logger = logger;
        private readonly IJSRuntime _jsRuntime = jsRuntime;

        
        private const long MaxFileSize = 10 * 1024 * 1024;

        public async Task<IEnumerable<TaskAttachmentDto>> GetAttachmentsByTaskIdAsync(Guid taskId)
        {
            try
            {
                var attachments = await _httpClient.GetFromJsonAsync<List<TaskAttachmentDto>>($"api/tasks/{taskId}/attachments");
                return attachments ?? Enumerable.Empty<TaskAttachmentDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching attachments for task {taskId}");
                return Enumerable.Empty<TaskAttachmentDto>();
            }
        }

        public async Task<TaskAttachmentDto?> UploadAttachmentAsync(Guid taskId, IBrowserFile file)
        {
            if (file == null)
            {
                _logger.LogWarning("No file provided for upload.");
                return null;
            }

            if (file.Size > MaxFileSize)
            {
                _logger.LogWarning($"File {file.Name} exceeds maximum allowed size of {MaxFileSize / 1024 / 1024} MB.");
                return null;
            }

            try
            {
                using var content = new MultipartFormDataContent();

                var fileContent = new StreamContent(file.OpenReadStream(maxAllowedSize: MaxFileSize));
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                content.Add(fileContent, "file", file.Name);

                var response = await _httpClient.PostAsync($"api/tasks/{taskId}/attachments", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error uploading attachment: {response.StatusCode} - {error}");
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<TaskAttachmentDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading attachment for task {taskId}");
                return null;
            }
        }

        public async Task<bool> DeleteAttachmentAsync(Guid taskId, Guid attachmentId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/tasks/{taskId}/attachments/{attachmentId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting attachment {attachmentId}");
                return false;
            }
        }

        public async Task<bool> DownloadAttachmentAsync(Guid taskId, Guid attachmentId, string fileName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/tasks/{taskId}/attachments/{attachmentId}/download");

                if (!response.IsSuccessStatusCode)
                    return false;

                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                var fileStream = new MemoryStream(fileBytes);

                using var streamRef = new DotNetStreamReference(stream: fileStream);
                await _jsRuntime.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading attachment {attachmentId}");
                return false;
            }
        }
    }
}
