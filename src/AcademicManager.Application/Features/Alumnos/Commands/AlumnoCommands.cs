using AcademicManager.Application.Common.Errors;
using AcademicManager.Application.DTOs;
using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using MediatR;
using AutoMapper;

namespace AcademicManager.Application.Features.Alumnos.Commands;

public record CreateAlumnoCommand(CreateAlumnoDto Dto) : IRequest<Result<int>>;

public class CreateAlumnoHandler : IRequestHandler<CreateAlumnoCommand, Result<int>>
{
    private readonly IAlumnoRepository _repository;
    private readonly IMapper _mapper;

    public CreateAlumnoHandler(IAlumnoRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<int>> Handle(CreateAlumnoCommand request, CancellationToken cancellationToken)
    {
        var existente = await _repository.GetByCodigoAsync(request.Dto.Codigo);
        if (existente != null)
        {
            return Result.Failure<int>(Error.Conflict("ALUMNO_EXISTS", $"Ya existe un alumno con el código '{request.Dto.Codigo}'"));
        }

        var alumno = _mapper.Map<Alumno>(request.Dto);
        var id = await _repository.CreateAsync(alumno);
        return Result.Success(id);
    }
}

public record UpdateAlumnoCommand(UpdateAlumnoDto Dto) : IRequest<Result<bool>>;

public class UpdateAlumnoHandler : IRequestHandler<UpdateAlumnoCommand, Result<bool>>
{
    private readonly IAlumnoRepository _repository;
    private readonly IMapper _mapper;

    public UpdateAlumnoHandler(IAlumnoRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<bool>> Handle(UpdateAlumnoCommand request, CancellationToken cancellationToken)
    {
        var existente = await _repository.GetByIdAsync(request.Dto.Id);
        if (existente == null)
        {
            return Result.Failure<bool>(Error.NotFound("ALUMNO_NOT_FOUND", $"Alumno con ID {request.Dto.Id} no encontrado"));
        }

        var codigoExistente = await _repository.GetByCodigoAsync(request.Dto.Codigo);
        if (codigoExistente != null && codigoExistente.Id != request.Dto.Id)
        {
            return Result.Failure<bool>(Error.Conflict("ALUMNO_EXISTS", $"Ya existe un alumno con el código '{request.Dto.Codigo}'"));
        }

        _mapper.Map(request.Dto, existente);
        var success = await _repository.UpdateAsync(existente);
        return Result.Success(success);
    }
}

public record DeleteAlumnoCommand(int Id) : IRequest<Result<bool>>;

public class DeleteAlumnoHandler : IRequestHandler<DeleteAlumnoCommand, Result<bool>>
{
    private readonly IAlumnoRepository _repository;

    public DeleteAlumnoHandler(IAlumnoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> Handle(DeleteAlumnoCommand request, CancellationToken cancellationToken)
    {
        var existente = await _repository.GetByIdAsync(request.Id);
        if (existente == null)
        {
            return Result.Failure<bool>(Error.NotFound("ALUMNO_NOT_FOUND", $"Alumno con ID {request.Id} no encontrado"));
        }

        var success = await _repository.DeleteAsync(request.Id);
        return Result.Success(success);
    }
}
