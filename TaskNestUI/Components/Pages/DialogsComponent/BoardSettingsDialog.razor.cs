namespace TaskNestUI.Components.Pages.DialogsComponent
{
    public partial class BoardSettingsDialog
    {
        #region Dependencies

        [Inject]
        private DialogService DialogService { get; set; } = null!;

        [Inject]
        private IBoardService BoardService { get; set; } = null!;

        [Inject]
        private NotificationService NotificationService { get; set; } = null!;

        [Inject]
        private ILogger<BoardSettingsDialog> Logger { get; set; } = null!;

        #endregion

        #region Parameters
        [Parameter]
        public BoardDto? Board { get; set; }


        #endregion

        #region Fields

        private BoardDto model = new();
        private bool showNameError = false;
        private string errorMessage = string.Empty;

        #endregion


        #region LifeCycle Methods

        protected override void OnInitialized()
        {
            if (Board != null)
            {
                model = new BoardDto
                {
                    Id = Board.Id,
                    Name = Board.Name,
                    Description = Board.Description
                };
            }
        }

        #endregion

        #region Cancel

        private void Cancel()
        {
            DialogService.Close();
        }

        #endregion

        #region Submit

        private async Task Submit()
        {
            try
            {
                errorMessage = string.Empty;
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    showNameError = true;
                    return;
                }

                var updated = await BoardService.UpdateBoardAsync(model);
                if (updated != null)
                {
                    NotificationService.Notify(NotificationSeverity.Success, "Success", "Board updated successfully");
                    DialogService.Close(updated);
                }
                else
                {
                    errorMessage = "Failed to update board. Please try again.";
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating board");
                errorMessage = ex.Message;
            }
        }

        #endregion
    }
}
