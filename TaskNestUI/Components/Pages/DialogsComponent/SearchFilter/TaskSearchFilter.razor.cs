namespace TaskNestUI.Components.Pages.DialogsComponent.SearchFilter
{
    public partial class TaskSearchFilter
    {
        #region Parameters

        [Parameter]
        public IEnumerable<BoardColumnDto> Columns { get; set; } = Enumerable.Empty<BoardColumnDto>();

        [Parameter]
        public EventCallback<FilterCriteria> OnFilterChange { get; set; }

        [Parameter]
        public int FilteredCount { get; set; }

        #endregion

        #region Fields and Properties

        private string? SearchText { get; set; }
        private Guid? SelectedColumnId { get; set; }
        private string? SelectedDueDateFilter { get; set; }

        private List<string> dueDateFilters = new()
        {
            "Overdue",
            "Today",
            "This Week",
            "This Month",
            "No Due Date"
        };

        private bool HasActiveFilters => !string.IsNullOrWhiteSpace(SearchText) || SelectedColumnId.HasValue || !string.IsNullOrWhiteSpace(SelectedDueDateFilter);


        #endregion

        #region Methods

        private async Task OnSearchChanged(string? value)
        {
            SearchText = value;
            await NotifyFilterChange();
        }

        private async Task OnFilterChanged()
        {
            await NotifyFilterChange();
        }

        private async Task NotifyFilterChange()
        {
            var criteria = new FilterCriteria
            {
                SearchText = SearchText,
                ColumnId = SelectedColumnId,
                DueDateFilter = SelectedDueDateFilter
            };

            await OnFilterChange.InvokeAsync(criteria);
        }

        private async Task ClearFilters()
        {
            SearchText = null;
            SelectedColumnId = null;
            SelectedDueDateFilter = null;
            await NotifyFilterChange();
        }

        #endregion
    }
}