namespace TaskNestUI.Components.Pages.DialogsComponent
{
    public partial class EditBoardDialog
    {
        #region Dependencies

        [Inject]
        private DialogService _dialogService { get; set; } = null!;

        #endregion

        #region Fields and Properties

        [Parameter]
        public BoardDto? Board { get; set; }

        public BoardDto model = new();
        //public BoardDto model = new BoardDto();

        public bool showNameError = false;

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
            _dialogService.Close();
        }
        #endregion

        #region Submit
        private void Submit()
        {
            try
            {
                if (string.IsNullOrEmpty(model.Name))
                {
                    showNameError = true;
                    return;
                }
                _dialogService.Close();
            }
            catch (Exception ex)
            {
                throw new Exception($": {ex.Message}");
            }
        }

        #endregion
    }
}