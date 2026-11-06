namespace ServiceContracts;

public interface IService<TDto, TUpdate>
{
    Task<TDto> UpdateAsync(int id, TUpdate request, Caller caller);
    Task DeleteAsync(int id, Caller caller);
    Task<TDto> GetSingleAsync(int id);
}
