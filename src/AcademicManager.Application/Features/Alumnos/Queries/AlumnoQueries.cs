using AcademicManager.Application.Common.Errors;
using AcademicManager.Application.DTOs;
using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using MediatR;
using AutoMapper;

namespace AcademicManager.Application.Features.Alumnos.Queries;

public record GetAllAlumnosQuery : IRequest<Result<IEnumerable<AlumnoDto>>>;

public class GetAllAlumnosHandler : IRequestHandler<GetAllAlumnosQuery, Result<IEnumerable<AlumnoDto>>>
{
    private readonly IAlumnoRepository _repository;
    private readonly IMapper _mapper;

    public GetAllAlumnosHandler(IAlumnoRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<AlumnoDto>>> Handle(GetAllAlumnosQuery request, CancellationToken cancellationToken)
    {
        var alumnos = await _repository.GetAllAsync();
        var dto = _mapper.Map<IEnumerable<AlumnoDto>>(alumnos);
        return Result.Success(dto);
    }
}

public record GetAlumnoByIdQuery(int Id) : IRequest<Result<AlumnoDto>>;

public class GetAlumnoByIdHandler : IRequestHandler<GetAlumnoByIdQuery, Result<AlumnoDto>>
{
    private readonly IAlumnoRepository _repository;
    private readonly IMapper _mapper;

    public GetAlumnoByIdHandler(IAlumnoRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<AlumnoDto>> Handle(GetAlumnoByIdQuery request, CancellationToken cancellationToken)
    {
        var alumno = await _repository.GetByIdAsync(request.Id);
        if (alumno == null)
        {
            return Result.Failure<AlumnoDto>(Error.NotFound("ALUMNO_NOT_FOUND", $"Alumno con ID {request.Id} no encontrado"));
        }

        var dto = _mapper.Map<AlumnoDto>(alumno);
        return Result.Success(dto);
    }
}

public record GetAlumnosByGradoQuery(int GradoId) : IRequest<Result<IEnumerable<AlumnoDto>>>;

public class GetAlumnosByGradoHandler : IRequestHandler<GetAlumnosByGradoQuery, Result<IEnumerable<AlumnoDto>>>
{
    private readonly IAlumnoRepository _repository;
    private readonly IMapper _mapper;

    public GetAlumnosByGradoHandler(IAlumnoRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<AlumnoDto>>> Handle(GetAlumnosByGradoQuery request, CancellationToken cancellationToken)
    {
        var alumnos = await _repository.GetByGradoIdAsync(request.GradoId);
        var dto = _mapper.Map<IEnumerable<AlumnoDto>>(alumnos);
        return Result.Success(dto);
    }
}

public record GetAlumnosBySeccionQuery(int SeccionId) : IRequest<Result<IEnumerable<AlumnoDto>>>;

public class GetAlumnosBySeccionHandler : IRequestHandler<GetAlumnosBySeccionQuery, Result<IEnumerable<AlumnoDto>>>
{
    private readonly IAlumnoRepository _repository;
    private readonly IMapper _mapper;

    public GetAlumnosBySeccionHandler(IAlumnoRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<AlumnoDto>>> Handle(GetAlumnosBySeccionQuery request, CancellationToken cancellationToken)
    {
        var alumnos = await _repository.GetBySeccionIdAsync(request.SeccionId);
        var dto = _mapper.Map<IEnumerable<AlumnoDto>>(alumnos);
        return Result.Success(dto);
    }
}
