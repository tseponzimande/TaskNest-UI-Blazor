namespace TaskNestUI.Components.Pages.DialogsComponent
{
    public partial class EditColumnDialog
    {
        #region Dependencies
        [Inject]
        private IBoardColumnService BoardColumnService { get; set; } = null!;

        [Inject]
        private DialogService DialogService { get; set; } = null!;

        [Inject]
        private ILogger<EditColumnDialog> _Logger { get; set; } = null!;

        #endregion

        #region Parameters

        [Parameter]
        public BoardColumnDto Column { get; set; } = null!;

        #endregion


        #region Fields and Propertie

        private BoardColumnDto model = new();

        private bool showNameError = false;

        #endregion

        #region LifeCycle Methods

        protected override void OnInitialized()
        {
            if (Column != null)
            {
                model = new BoardColumnDto
                {
                    Id = Column.Id,
                    BoardId = Column.BoardId,
                    Name = Column.Name,
                    Order = Column.Order
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
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    showNameError = true;
                    return;
                }

                var updated = await BoardColumnService.UpdateColumnAsync(model);

                if (updated != null)
                {
                    DialogService.Close(updated);
                }
                else
                {
                    showNameError = true;
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError($": {ex.Message}");
                //Console.WriteLine($"Error updating column: {ex.Message}");
                showNameError = true;
            }
        }
        #endregion
    }
}
