using AcademicManager.Application.DTOs;
using AcademicManager.Domain.Entities;
using AutoMapper;

namespace AcademicManager.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Alumno, AlumnoDto>()
            .ForMember(dest => dest.GradoNombre, opt => opt.MapFrom(src => src.Grado != null ? src.Grado.Nombre : null))
            .ForMember(dest => dest.SeccionNombre, opt => opt.MapFrom(src => src.Seccion != null ? src.Seccion.Nombre : null))
            .ForMember(dest => dest.NombreCompleto, opt => opt.MapFrom(src => $"{src.Nombres} {src.Apellidos}"));

        CreateMap<CreateAlumnoDto, Alumno>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FechaRegistro, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.Grado, opt => opt.Ignore())
            .ForMember(dest => dest.Seccion, opt => opt.Ignore());

        CreateMap<UpdateAlumnoDto, Alumno>()
            .ForMember(dest => dest.FechaRegistro, opt => opt.Ignore())
            .ForMember(dest => dest.Grado, opt => opt.Ignore())
            .ForMember(dest => dest.Seccion, opt => opt.Ignore());

        CreateMap<Docente, DocenteDto>()
            .ForMember(dest => dest.NombreCompleto, opt => opt.MapFrom(src => $"{src.Nombres} {src.Apellidos}"));

        CreateMap<UpdateDocenteDto, Docente>();

        CreateMap<Curso, CursoDto>()
            .ForMember(dest => dest.GradoNombre, opt => opt.MapFrom(src => src.Grado != null ? src.Grado.Nombre : null));

        CreateMap<Grado, GradoDto>();
        CreateMap<Seccion, SeccionDto>();
        CreateMap<PeriodoAcademico, PeriodoAcademicoDto>();
        CreateMap<Horario, HorarioDto>();
        CreateMap<Planificacion, PlanificacionDto>();
        CreateMap<Evaluacion, EvaluacionDto>();
        CreateMap<Calificacion, CalificacionDto>();
        CreateMap<Asistencia, AsistenciaDto>();

        // Mapeos para Tareas
        CreateMap<Tarea, TareaDto>()
            .ForMember(dest => dest.CursoNombre, opt => opt.MapFrom(src => src.Curso != null ? src.Curso.Nombre : null))
            .ForMember(dest => dest.PeriodoNombre, opt => opt.MapFrom(src => src.PeriodoAcademico != null ? src.PeriodoAcademico.Nombre : null))
            .ForMember(dest => dest.DocenteNombre, opt => opt.MapFrom(src => src.Docente != null ? $"{src.Docente.Nombres} {src.Docente.Apellidos}" : null));

        CreateMap<CreateTareaDto, Tarea>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FechaPublicacion, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.FechaActualizacion, opt => opt.Ignore())
            .ForMember(dest => dest.Activa, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.Curso, opt => opt.Ignore())
            .ForMember(dest => dest.PeriodoAcademico, opt => opt.Ignore())
            .ForMember(dest => dest.Docente, opt => opt.Ignore())
            .ForMember(dest => dest.Planificacion, opt => opt.Ignore());

        CreateMap<UpdateTareaDto, Tarea>()
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.FechaActualizacion, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.CursoId, opt => opt.Ignore())
            .ForMember(dest => dest.PeriodoAcademicoId, opt => opt.Ignore())
            .ForMember(dest => dest.DocenteId, opt => opt.Ignore())
            .ForMember(dest => dest.PlanificacionId, opt => opt.Ignore())
            .ForMember(dest => dest.TipoArchivoPermitido, opt => opt.Ignore())
            .ForMember(dest => dest.TamanoMaximoArchivo, opt => opt.Ignore())
            .ForMember(dest => dest.FechaPublicacion, opt => opt.Ignore())
            .ForMember(dest => dest.Curso, opt => opt.Ignore())
            .ForMember(dest => dest.PeriodoAcademico, opt => opt.Ignore())
            .ForMember(dest => dest.Docente, opt => opt.Ignore())
            .ForMember(dest => dest.Planificacion, opt => opt.Ignore());

        // Mapeos para Entregas de Tareas
        CreateMap<EntregaTarea, EntregaTareaDto>()
            .ForMember(dest => dest.TareaTitulo, opt => opt.MapFrom(src => src.Tarea != null ? src.Tarea.Titulo : null))
            .ForMember(dest => dest.AlumnoNombre, opt => opt.MapFrom(src => src.Alumno != null ? $"{src.Alumno.Nombres} {src.Alumno.Apellidos}" : null));

        CreateMap<CreateEntregaTareaDto, EntregaTarea>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FechaEntrega, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.FechaActualizacion, opt => opt.Ignore())
            .ForMember(dest => dest.EsTardia, opt => opt.MapFrom(_ => false))
            .ForMember(dest => dest.FechaCalificacion, opt => opt.Ignore())
            .ForMember(dest => dest.Puntos, opt => opt.Ignore())
            .ForMember(dest => dest.Retroalimentacion, opt => opt.Ignore())
            .ForMember(dest => dest.Tarea, opt => opt.Ignore())
            .ForMember(dest => dest.Alumno, opt => opt.Ignore());

        CreateMap<Usuario, UsuarioDto>()
            .ForMember(dest => dest.NombreCompleto, opt => opt.MapFrom(src => src.NombreUsuario));
    }
}
