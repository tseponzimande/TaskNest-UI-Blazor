namespace TaskNestUI.Components.Pages.DialogsComponent
{
    public partial class CreateTaskDialog
    {
        #region Dependencies

        [Inject]
        private DialogService DialogService { get; set; } = null!;

        [Inject]
        private ITaskItemService TaskItemService { get; set; } = null!;

        [Inject]
        private ILogger<CreateTaskDialog> _logger { get; set; } = null!;

        #endregion

        #region Fields and Properties

        [Parameter]
        public Guid BoardId { get; set; }

        [Parameter]
        public Guid ColumnId { get; set; }

        private TaskItemDto model = new();
        private bool showTitleError = false;

        #endregion

        #region LifeCycle Methods

        protected override void OnInitialized()
        {
            model.BoardId = BoardId;
            // convert empty Guid to null
            model.ColumnId = ColumnId == Guid.Empty ? null : ColumnId;
            model.CreatedAt = DateTime.UtcNow;
        }

        #endregion

        #region Methods

        private void Cancel()
        {
            DialogService.Close();
        }

        private async Task Submit()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Title))
                {
                    showTitleError = true;
                    return;
                }

                var created = await TaskItemService.CreateTaskAsync(model);

                if (created != null)
                {
                    DialogService.Close(created);
                }
                else
                {
                    showTitleError = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                //Console.WriteLine($"Error creating task: {ex.Message}");
                showTitleError = true;
            }
        }

        #endregion
    }
}