namespace TaskNestUI.Components.Pages.DialogsComponent
{
    public partial class CreateColumnDialog
    {
        #region Dependencies
        [Inject]
        private DialogService dialogService { get; set; } = null!;

        [Inject]
        private IBoardColumnService boardColumnService { get; set; } = null!;

        //[Inject]
        //private BoardColumnService boardColumnService { get; set; } = null!;

        [Inject]
        private NotificationService _notificationService { get; set; } = null!;

        [Inject]
        private ILogger<CreateColumnDialog> _logger { get; set; } = null!;

        [Inject]
        private AuthenticationStateProvider _authenticationStateProvider { get; set; } = null!;

        #endregion

        #region Fields and Properties

        [Parameter]
        public Guid boardId { get; set; }

        private BoardColumnDto column = new();

        //private BoardColumnDto column = new BoardColumnDto();

        private bool showNameError = false;

        #endregion


        #region LifeCycle Methods

        protected override void OnInitialized()
        {
            column.BoardId = boardId;
        }
        #endregion


        #region Cancel Methods
        private void Cancel()
        {
            dialogService.Close();
        }
        #endregion

        #region Submit Methods

        private async Task Submit()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(column.Name))
                {
                    showNameError = true;
                    return;
                }

                var created = await boardColumnService.CreateColumnAsync(column);

                if (created != null)
                {
                    dialogService.Close(created);
                }
                else
                {
                    showNameError = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($": {ex.Message}");
                _logger.LogError($"Error creating column: {ex.Message}");
                showNameError = true;
            }
        }

        #endregion
    }
}