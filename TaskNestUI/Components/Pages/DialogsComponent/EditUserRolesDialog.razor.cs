namespace TaskNestUI.Components.Pages.DialogsComponent
{
    public partial class EditUserRolesDialog
    {

        #region Dependencies

        private DialogService _dialogService { get; set; } = null!;

        #endregion

        #region Parameters

        [Parameter]
        public UserManagementDtos? User { get; set; }

        #endregion

        #region Fields

        private List<string> availableRoles = new() { "Admin", "User", "Manager" };

        private Dictionary<string, bool> selectedRoles = new();

        #endregion


        #region LifeCycle Methods
        protected override void OnInitialized()
        {
            if (User != null)
            {
                foreach (var role in availableRoles)
                {
                    selectedRoles[role] = User.Roles.Contains(role);
                }
            }
        }

        #endregion


        #region Cancel

        private void Cancel()
        {
            _dialogService.Close();
        }

        #endregion

        #region Save

        private void Save()
        {
            var newRoles = selectedRoles.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();

            if (!newRoles.Any())
            {
                newRoles.Add("User");
            }

            _dialogService.Close(newRoles);
        }

        #endregion

    }
}