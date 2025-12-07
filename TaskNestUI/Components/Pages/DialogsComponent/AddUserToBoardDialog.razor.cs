namespace TaskNestUI.Components.Pages.DialogsComponent
{
    public partial class AddUserToBoardDialog
    {
        #region Dependencies

        [Inject]
        private DialogService DialogService { get; set; } = null!;

        [Inject]
        private IBoardUserService BoardUserService { get; set; } = null!;

        [Inject]
        private NotificationService NotificationService { get; set; } = null!;

        #endregion

        #region Parameter 

        [Parameter]
        public Guid BoardId { get; set; }

        #endregion

        #region Fields and Properties

        private AddBoardUserDto model = new() { Role = BoardUserRole.Editor };
        private bool showEmailError = false;
        private bool isSubmitting = false;
        private string errorMessage = "Email is required.";

        #endregion

        #region Board User Roles
        private List<BoardUserRole> roles = new()
        {
            BoardUserRole.Viewer,
            BoardUserRole.Editor,
            BoardUserRole.Admin
        };

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
            showEmailError = false;
            errorMessage = "Email is required.";

            if (string.IsNullOrWhiteSpace(model.UserEmail))
            {
                showEmailError = true;
                return;
            }

            if (!IsValidEmail(model.UserEmail))
            {
                showEmailError = true;
                errorMessage = "Please enter a valid email address.";
                return;
            }

            isSubmitting = true;

            try
            {
                var result = await BoardUserService.AddUserToBoardAsync(BoardId, model);

                if (result != null)
                {
                    NotificationService.Notify(NotificationSeverity.Success, "Success", $"User {model.UserEmail} added to board");
                    DialogService.Close(result);
                }
                else
                {
                    showEmailError = true;
                    errorMessage = "User not found or already added to board.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding user: {ex.Message}");
                showEmailError = true;
                errorMessage = "An error occurred. Please try again.";
            }
            finally
            {
                isSubmitting = false;
            }
        }

        #endregion

        #region Validations

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
