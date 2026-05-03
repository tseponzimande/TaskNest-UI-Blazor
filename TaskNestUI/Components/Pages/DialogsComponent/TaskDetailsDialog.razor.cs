namespace TaskNestUI.Components.Pages.DialogsComponent
{
    public partial class TaskDetailsDialog
    {
        #region Dependencies

        [Inject]
        private DialogService DialogService { get; set; } = null!;

        [Inject]
        private ITaskItemService TaskItemService { get; set; } = null!;

        [Inject]
        private ICommentService CommentService { get; set; } = null!;

        [Inject]
        private NotificationService NotificationService { get; set; } = null!;

        [Inject]
        private ITaskAttachmentService TaskAttachmentService { get; set; } = null!;

        #endregion

        #region Parameters

        [Parameter]
        public TaskItemDto? Task { get; set; }

        [Parameter] public IEnumerable<BoardColumnDto> Columns { get; set; } = Enumerable.Empty<BoardColumnDto>();

        [Parameter] public string? CurrentUserId { get; set; }

        #endregion

        #region Fields and Properties

        private TaskItemDto editModel = new();
        private bool isEditing = false;
        private bool showTitleError = false;

        private List<CommentDto> comments = new();
        private string newCommentContent = string.Empty;
        private bool isLoadingComments = false;

        private List<TaskAttachmentDto> attachments = new();
        private bool isLoadingAttachments = false;
        private IBrowserFile? selectedFile;
        private bool isUploadingFile = false;

        #endregion

        #region LifeCycle Methods

        protected override async Task OnInitializedAsync()
        {
            if (Task != null)
            {
                editModel = new TaskItemDto
                {
                    Id = Task.Id,
                    BoardId = Task.BoardId,
                    ColumnId = Task.ColumnId,
                    Title = Task.Title,
                    Description = Task.Description,
                    CreatedAt = Task.CreatedAt,
                    DueDate = Task.DueDate,
                    Position = Task.Position
                };

                await LoadComments();
                await LoadAttachments();
            }
        }

        #endregion

        #region Comment Methods

        private async Task LoadComments()
        {
            if (Task == null) return;

            isLoadingComments = true;
            try
            {
                var result = await CommentService.GetCommentsByTaskIdAsync(Task.Id);
                comments = result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading comments: {ex.Message}");
            }
            finally
            {
                isLoadingComments = false;
                StateHasChanged();
            }
        }

        private async Task AddComment()
        {
            if (Task == null || string.IsNullOrWhiteSpace(newCommentContent)) return;

            try
            {
                var dto = new CreateCommentDto { Content = newCommentContent };
                var created = await CommentService.CreateCommentAsync(Task.Id, dto);

                if (created != null)
                {
                    comments.Add(created);
                    newCommentContent = string.Empty;
                    NotificationService.Notify(NotificationSeverity.Success, "Success", "Comment added");
                    StateHasChanged();
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to add comment");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding comment: {ex.Message}");
                NotificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }

        private bool CanDeleteComment(CommentDto comment) =>
            !string.IsNullOrEmpty(CurrentUserId) && comment.ApplicationUserId == CurrentUserId;

        private async Task ConfirmDeleteComment(CommentDto comment)
        {
            var confirmed = await DialogService.Confirm(
                "Delete this comment?",
                "Confirm Delete",
                new ConfirmOptions() { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

            if (confirmed == true) await DeleteComment(comment);
        }

        private async Task DeleteComment(CommentDto comment)
        {
            if (Task == null) return;

            try
            {
                var success = await CommentService.DeleteCommentAsync(Task.Id, comment.Id);

                if (success)
                {
                    comments.Remove(comment);
                    NotificationService.Notify(NotificationSeverity.Success, "Success", "Comment deleted");
                    StateHasChanged();
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to delete comment");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting comment: {ex.Message}");
                NotificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }

        private void OnCommentContentChanged(object args)
        {
            newCommentContent = args?.ToString() ?? string.Empty;
            StateHasChanged();
        }

        #endregion

        #region Task Methods

        private void StartEdit() => isEditing = true;

        private void CancelEdit()
        {
            isEditing = false;
            showTitleError = false;

            if (Task != null)
            {
                editModel = new TaskItemDto
                {
                    Id = Task.Id,
                    BoardId = Task.BoardId,
                    ColumnId = Task.ColumnId,
                    Title = Task.Title,
                    Description = Task.Description,
                    CreatedAt = Task.CreatedAt,
                    DueDate = Task.DueDate,
                    Position = Task.Position
                };
            }
        }

        private async Task SaveChanges()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(editModel.Title))
                {
                    showTitleError = true;
                    return;
                }

                var updated = await TaskItemService.UpdateTaskAsync(editModel);

                if (updated != null)
                {
                    NotificationService.Notify(NotificationSeverity.Success, "Success", "Task updated successfully");
                    DialogService.Close(updated);
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to update task");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating task: {ex.Message}");
                NotificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }

        private async Task ConfirmDelete()
        {
            var confirmed = await DialogService.Confirm(
                $"Are you sure you want to delete this task?",
                "Confirm Delete",
                new ConfirmOptions() { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

            if (confirmed == true) await DeleteTask();
        }

        private async Task DeleteTask()
        {
            try
            {
                if (Task == null) return;

                var success = await TaskItemService.DeleteTaskAsync(Task.Id);

                if (success)
                {
                    NotificationService.Notify(NotificationSeverity.Success, "Success", "Task deleted successfully");
                    DialogService.Close(true);
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to delete task");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting task: {ex.Message}");
                NotificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }

        private void Close() => DialogService.Close();

        #endregion

        #region Attachment Methods

        private async Task LoadAttachments()
        {
            if (Task == null) return;

            isLoadingAttachments = true;
            try
            {
                var result = await TaskAttachmentService.GetAttachmentsByTaskIdAsync(Task.Id);
                attachments = result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading attachments: {ex.Message}");
            }
            finally
            {
                isLoadingAttachments = false;
                StateHasChanged();
            }
        }

        private void HandleFileSelected(InputFileChangeEventArgs e)
        {
            selectedFile = e.File;
            StateHasChanged();
        }

        private void ClearFileSelection()
        {
            selectedFile = null;
            StateHasChanged();
        }

        private async Task UploadAttachment()
        {
            if (Task == null || selectedFile == null) return;

            isUploadingFile = true;
            try
            {
                var uploaded = await TaskAttachmentService.UploadAttachmentAsync(Task.Id, selectedFile);

                if (uploaded != null)
                {
                    attachments.Add(uploaded);
                    selectedFile = null;
                    NotificationService.Notify(NotificationSeverity.Success, "Success", "File uploaded successfully");
                    StateHasChanged();
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to upload file");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading attachment: {ex.Message}");
                NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to upload file: " + ex.Message);
            }
            finally
            {
                isUploadingFile = false;
                StateHasChanged();
            }
        }

        private async Task DownloadAttachment(TaskAttachmentDto attachment)
        {
            if (Task == null) return;

            try
            {
                var success = await TaskAttachmentService.DownloadAttachmentAsync(Task.Id, attachment.Id, attachment.OriginalFileName);

                if (!success)
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to download file");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading attachment: {ex.Message}");
                NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to download file");
            }
        }

        private async Task ConfirmDeleteAttachment(TaskAttachmentDto attachment)
        {
            var confirmed = await DialogService.Confirm(
                "Delete this attachment?",
                "Confirm Delete",
                new ConfirmOptions() { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

            if (confirmed == true)
            {
                await DeleteAttachment(attachment);
            }
        }

        private async Task DeleteAttachment(TaskAttachmentDto attachment)
        {
            if (Task == null) return;

            try
            {
                var success = await TaskAttachmentService.DeleteAttachmentAsync(Task.Id, attachment.Id);

                if (success)
                {
                    attachments.Remove(attachment);
                    NotificationService.Notify(NotificationSeverity.Success, "Success", "Attachment deleted");
                    StateHasChanged();
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to delete attachment");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting attachment: {ex.Message}");
                NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to delete attachment");
            }
        }

        private bool CanDeleteAttachment(TaskAttachmentDto attachment) =>
            !string.IsNullOrEmpty(CurrentUserId) && attachment.ApplicationUserId == CurrentUserId;

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private string GetFileIcon(string contentType)
        {
            return contentType.ToLower() switch
            {
                var ct when ct.Contains("pdf") => "picture_as_pdf",
                var ct when ct.Contains("word") || ct.Contains("document") => "description",
                var ct when ct.Contains("excel") || ct.Contains("spreadsheet") => "grid_on",
                var ct when ct.Contains("image") => "image",
                var ct when ct.Contains("zip") || ct.Contains("rar") || ct.Contains("compressed") => "folder_zip",
                var ct when ct.Contains("text") => "text_snippet",
                _ => "attach_file"
            };
        }

        #endregion
    }
}
