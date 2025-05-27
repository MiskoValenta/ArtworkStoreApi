using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using ArtworkStoreApi.DTOs;
using ArtworkStoreApi.Repositories;


namespace ArtworkStoreApi.Services;

public class GenericService<T, TDto, TCreateDto> : IGenericService<T, TDto, TCreateDto> 
    where T : class
{
    protected readonly IGenericRepository<T> _repository;
    protected readonly IMapper _mapper;

    public GenericService(IGenericRepository<T> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public virtual async Task<ResultDto<IEnumerable<TDto>>> GetAllAsync()
    {
        try
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<TDto>>(entities);
            return new ResultDto<IEnumerable<TDto>>(true, "Success", dtos);
        }
        catch (Exception ex)
        {
            return new ResultDto<IEnumerable<TDto>>(false, ex.Message, null);
        }
    }

    public virtual async Task<ResultDto<TDto>> GetByIdAsync(int id)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return new ResultDto<TDto>(false, "Entity not found", default);

            var dto = _mapper.Map<TDto>(entity);
            return new ResultDto<TDto>(true, "Success", dto);
        }
        catch (Exception ex)
        {
            return new ResultDto<TDto>(false, ex.Message, default);
        }
    }

    public virtual async Task<ResultDto<TDto>> CreateAsync(TCreateDto createDto)
    {
        try
        {
            var entity = _mapper.Map<T>(createDto);
            var createdEntity = await _repository.AddAsync(entity);
            var dto = _mapper.Map<TDto>(createdEntity);
            return new ResultDto<TDto>(true, "Created successfully", dto);
        }
        catch (Exception ex)
        {
            return new ResultDto<TDto>(false, ex.Message, default);
        }
    }

    public virtual async Task<ResultDto<TDto>> UpdateAsync(int id, TCreateDto updateDto)
    {
        try
        {
            var existingEntity = await _repository.GetByIdAsync(id);
            if (existingEntity == null)
                return new ResultDto<TDto>(false, "Entity not found", default);

            _mapper.Map(updateDto, existingEntity);
            await _repository.UpdateAsync(existingEntity);
            var dto = _mapper.Map<TDto>(existingEntity);
            return new ResultDto<TDto>(true, "Updated successfully", dto);
        }
        catch (Exception ex)
        {
            return new ResultDto<TDto>(false, ex.Message, default);
        }
    }

    public virtual async Task<ResultDto<bool>> DeleteAsync(int id)
    {
        try
        {
            var exists = await _repository.ExistsAsync(id);
            if (!exists)
                return new ResultDto<bool>(false, "Entity not found", false);

            await _repository.DeleteAsync(id);
            return new ResultDto<bool>(true, "Deleted successfully", true);
        }
        catch (Exception ex)
        {
            return new ResultDto<bool>(false, ex.Message, false);
        }
    }
}
