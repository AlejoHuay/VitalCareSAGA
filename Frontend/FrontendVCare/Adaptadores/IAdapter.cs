namespace FrontendVCare.Adaptadores
{
    public interface IAdapter<T>
    {
        Task<List<T>> GetListAsync(string url);
        Task<T?> GetAsync(string url);

        Task<bool> PostAsync(string url, T data);
        Task<(bool Success, string? Message)> PostWithMessageAsync(string url, T data);

        Task<bool> PutAsync(string url, T data);
        Task<(bool Success, string? Message)> PutWithMessageAsync(string url, T data);

        Task<bool> DeleteAsync(string url);
    }
}