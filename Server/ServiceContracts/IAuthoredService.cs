namespace ServiceContracts;

public interface IAuthoredService<TDto, TCreate, TUpdate> : IService<TDto, TUpdate>
{
    Task<TDto> CreateAsync(TCreate request, int userId);
}
