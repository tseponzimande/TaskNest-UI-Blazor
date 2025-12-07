namespace TaskNestUI.Interfaces
{
    public interface IBoardService
    {
        Task<IEnumerable<BoardDto>> GetBoardAsyc();
        Task<BoardDto?> GetBoardByIdAsync(Guid Id);
        Task<BoardDto?> CreateBoardAsync(BoardDto dto);
        Task<BoardDto?> UpdateBoardAsync(BoardDto dto);
        Task<bool> DeleteBoardAsync(Guid Id);
    }
}