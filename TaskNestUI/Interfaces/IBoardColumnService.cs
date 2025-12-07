namespace TaskNestUI.Interfaces
{
    public interface IBoardColumnService
    {
        Task<IEnumerable<BoardColumnDto>> GetColumnsByBoardIdAsync(Guid boardId);
        Task<IEnumerable<BoardColumnDto>> GetColumnByIdAsync(Guid columnId);
        Task<IEnumerable<BoardColumnDto>> CreateColumnAsync(BoardColumnDto dto);
        Task<IEnumerable<BoardColumnDto>> UpdateColumnAsync(BoardColumnDto dto);
        Task<bool> DeleteColumnAsync(Guid id);
        Task<bool> ReorderColumnsAsync(List<ColumnOrderDto> columnOrders);
    }
}