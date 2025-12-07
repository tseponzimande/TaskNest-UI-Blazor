namespace TaskNestUI.Components.Pages.DialogsComponent
{
    public partial class CreateBoardDialog
    {
        #region Dependencies

        [Inject]
        private DialogService DialogService { get; set; } = null!;

        [Inject]
        private ILogger<CreateBoardDialog> _logger { get; set; } = null!;

        [Inject]
        private NotificationService _notificationService { get; set; } = null!;
        #endregion

        #region Fields and Properties

        [Parameter]
        public BoardDto? Model { get; set; }

        private BoardDto model = new();

        private bool showNameError = false;

        #endregion

        #region LifeCycle Methods

        protected override void OnInitialized()
        {
            if (Model != null)
            {
                model = Model;
            }
        }
        #endregion

        #region Methods

        private void Cancel()
        {
            DialogService.Close();
        }

        #endregion

        #region Submit Methods

        private void Submit()
        {
            try
            {
                if (string.IsNullOrEmpty(model.Name))
                {
                    showNameError = true;
                    return;
                }
                DialogService.Close(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($": {ex.Message}");
                _notificationService.Notify(NotificationSeverity.Error, "Error", "Error Occured", duration: 4000);
            }
        }

        #endregion
    }
}