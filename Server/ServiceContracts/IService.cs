namespace ServiceContracts;

public interface IService<TDto, TUpdate>
{
    Task<TDto> UpdateAsync(int id, TUpdate request);
    Task DeleteAsync(int id);
    Task<TDto> GetSingleAsync(int id);
}
