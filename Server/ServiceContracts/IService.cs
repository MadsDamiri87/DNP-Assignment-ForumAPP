namespace ServiceContracts;

public interface IService<TDto, TCreate, TUpdate>
{
    Task<TDto> CreateAsync(TCreate request);
    Task<TDto> UpdateAsync(int id, TUpdate request);
    Task DeleteAsync(int id);
    Task<TDto> GetSingleAsync(int id);
}
