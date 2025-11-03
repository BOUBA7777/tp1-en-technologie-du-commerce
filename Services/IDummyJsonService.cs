using TP1.Models;

namespace TP1.Services
{
    public interface IDummyJsonService
    {
        Task<List<DummyJsonUser>> GetUsersAsync(int limit = 30);
        Task<DummyJsonUser?> GetUserByIdAsync(int id);
    }
}






