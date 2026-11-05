namespace ServiceContracts;

public interface ICreatableService<TDto, TCreate, TUpdate> : IService<TDto, TUpdate>
{
    Task<TDto> CreateAsync(TCreate request);
}
