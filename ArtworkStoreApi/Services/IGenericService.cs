using ArtworkStoreApi.DTOs;

namespace ArtworkStoreApi.Services;

public interface IGenericService<T, TDto, TCreateDto> where T : class
{
    Task<ResultDto<IEnumerable<TDto>>> GetAllAsync();
    Task<ResultDto<TDto>> GetByIdAsync(int id);
    Task<ResultDto<TDto>> CreateAsync(TCreateDto createDto);
    Task<ResultDto<TDto>> UpdateAsync(int id, TCreateDto updateDto);
    Task<ResultDto<bool>> DeleteAsync(int id);
}