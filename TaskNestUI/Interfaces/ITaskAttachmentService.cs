namespace TaskNestUI.Interfaces
{
    public interface ITaskAttachmentService
    {
        Task<IEnumerable<TaskAttachmentDto>> GetAttachmentsByTaskIdAsync(Guid taskId);
        Task<TaskAttachmentDto?> UploadAttachmentAsync(Guid taskId, IBrowserFile file);
        Task<bool> DeleteAttachmentAsync(Guid taskId, Guid attachmentId);
        Task<bool> DownloadAttachmentAsync(Guid taskId, Guid attachmentId, string fileName);
    }
}
