namespace Blazor_Application.Services;

public interface IHttpService<TDto, TCreate, TUpdate>
{
    Task<TDto> AddAsync(TCreate request);
    Task<TDto> UpdateAsync(int id, TUpdate request);
    Task DeleteAsync(int id);
    Task<TDto> GetSingleAsync(int id);
}
