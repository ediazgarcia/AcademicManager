using AcademicManager.Application.Common.Errors;
using AcademicManager.Application.DTOs;
using AcademicManager.Application.Interfaces;
using MediatR;
using AutoMapper;

namespace AcademicManager.Application.Features.Docentes.Queries;

public record GetAllDocentesQuery : IRequest<Result<IEnumerable<DocenteDto>>>;

public class GetAllDocentesHandler : IRequestHandler<GetAllDocentesQuery, Result<IEnumerable<DocenteDto>>>
{
    private readonly IDocenteRepository _repository;
    private readonly IMapper _mapper;

    public GetAllDocentesHandler(IDocenteRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<DocenteDto>>> Handle(GetAllDocentesQuery request, CancellationToken cancellationToken)
    {
        var docentes = await _repository.GetAllAsync();
        var dto = _mapper.Map<IEnumerable<DocenteDto>>(docentes);
        return Result.Success(dto);
    }
}

public record GetDocenteByIdQuery(int Id) : IRequest<Result<DocenteDto>>;

public class GetDocenteByIdHandler : IRequestHandler<GetDocenteByIdQuery, Result<DocenteDto>>
{
    private readonly IDocenteRepository _repository;
    private readonly IMapper _mapper;

    public GetDocenteByIdHandler(IDocenteRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<DocenteDto>> Handle(GetDocenteByIdQuery request, CancellationToken cancellationToken)
    {
        var docente = await _repository.GetByIdAsync(request.Id);
        if (docente == null)
        {
            return Result.Failure<DocenteDto>(Error.NotFound("DOCENTE_NOT_FOUND", $"Docente con ID {request.Id} no encontrado"));
        }

        var dto = _mapper.Map<DocenteDto>(docente);
        return Result.Success(dto);
    }
}
