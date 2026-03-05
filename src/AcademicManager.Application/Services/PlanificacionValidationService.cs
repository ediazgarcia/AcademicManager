using AcademicManager.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AcademicManager.Application.Services;

/// <summary>
/// Resultado de validación de estructura MINERD
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Servicio de validación de Planificaciones según estándares MINERD
/// Asegura que las planificaciones cumplan con los requisitos del ministerio
/// </summary>
public class PlanificacionValidationService
{
    private readonly ILogger<PlanificacionValidationService> _logger;

    public PlanificacionValidationService(ILogger<PlanificacionValidationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Valida que una planificación cumpla con estructura MINERD
    /// </summary>
    public ValidationResult ValidarEstructuraMinerd(Planificacion planificacion)
    {
        var resultado = new ValidationResult { IsValid = true };

        // VALIDACIONES OBLIGATORIAS
        var errores = new List<string>();

        // 1. Información General
        if (string.IsNullOrWhiteSpace(planificacion.Titulo))
            errores.Add("El título de la planificación es obligatorio");

        // En Plan Anual, TituloUnidad y Mes son opcionales
        // if (string.IsNullOrWhiteSpace(planificacion.TituloUnidad))
        //    errores.Add("El título de la unidad es obligatorio");

        if (planificacion.AnoAcademico <= 0)
            errores.Add("El año académico debe ser válido");

        // if (string.IsNullOrWhiteSpace(planificacion.Mes))
        //    errores.Add("El mes debe especificarse");

        // 2. Situación de Aprendizaje (Núcleo MINERD)
        if (string.IsNullOrWhiteSpace(planificacion.SituacionAprendizaje))
            errores.Add("La situación de aprendizaje es obligatoria (contexto que motiva el aprendizaje)");

        if (planificacion.SituacionAprendizaje?.Length < 50)
            resultado.Warnings.Add("La situación de aprendizaje podría ser más descriptiva (mínimo 50 caracteres)");

        // 3. Competencias MINERD (Eje central)
        if (string.IsNullOrWhiteSpace(planificacion.CompetenciasFundamentales))
            errores.Add("Las competencias fundamentales son obligatorias (Saber ser, Saber pensar, Saber hacer)");

        if (string.IsNullOrWhiteSpace(planificacion.CompetenciasEspecificas))
            errores.Add("Las competencias específicas son obligatorias");

        // Validar que haya competencias específicas
        var compEspecCount = planificacion.CompetenciasEspecificas?
            .Split(new[] { '\n', ';', '-' }, StringSplitOptions.RemoveEmptyEntries)
            .Length ?? 0;

        if (compEspecCount == 0)
            resultado.Warnings.Add("Defina al menos una competencia específica");

        // 4. Contenidos - Triangulación (Requisito fundamental MINERD)
        if (string.IsNullOrWhiteSpace(planificacion.ContenidosConceptuales))
            errores.Add("Los contenidos conceptuales son obligatorios");

        if (string.IsNullOrWhiteSpace(planificacion.ContenidosProcedimentales))
            errores.Add("Los contenidos procedimentales son obligatorios");

        if (string.IsNullOrWhiteSpace(planificacion.ContenidosActitudinales))
            errores.Add("Los contenidos actitudinales son obligatorios");

        // Validar que todos los tipos de contenido estén presentes
        var tiposContenidoPresentes = new[]
        {
            !string.IsNullOrEmpty(planificacion.ContenidosConceptuales),
            !string.IsNullOrEmpty(planificacion.ContenidosProcedimentales),
            !string.IsNullOrEmpty(planificacion.ContenidosActitudinales)
        };

        if (tiposContenidoPresentes.Count(x => x) < 3)
            resultado.Warnings.Add("La triangulación de aprendizajes debe incluir los tres tipos de contenido");

        // 5. Indicadores de Logro (Observable y Medible)
        if (string.IsNullOrWhiteSpace(planificacion.IndicadoresLogro))
            errores.Add("Los indicadores de logro son obligatorios");

        if (!string.IsNullOrEmpty(planificacion.IndicadoresLogro))
        {
            if (planificacion.IndicadoresLogro.Length < 100)
                resultado.Warnings.Add("Los indicadores de logro deben ser suficientemente descriptivos");

            // Verificar que sean observables y medibles
            var palabrasObservables = new[] { "demuestra", "muestra", "realiza", "ejecuta", "construye", "identifica", "analiza" };
            var esObservable = palabrasObservables.Any(p => planificacion.IndicadoresLogro.ToLower().Contains(p));
            if (!esObservable)
                resultado.Warnings.Add("Los indicadores deben ser conductas observables (usar verbos como demostrar, realizar, ejecutar)");
        }

        // 6. Estrategias de Enseñanza
        if (string.IsNullOrWhiteSpace(planificacion.EstrategiasEnsenanza))
            errores.Add("Las estrategias de enseñanza son obligatorias");

        // 7. Recursos Didácticos
        if (string.IsNullOrWhiteSpace(planificacion.RecursosDidacticos) && string.IsNullOrWhiteSpace(planificacion.Recursos))
            errores.Add("Debe especificar recursos didácticos");

        // 8. Evaluación
        if (string.IsNullOrWhiteSpace(planificacion.ActividadesEvaluacion))
            resultado.Warnings.Add("Las actividades de evaluación deberían estar definidas");

        // 9. Validaciones de Integridad
        if (string.IsNullOrWhiteSpace(planificacion.CriteriosEvaluacion))
            resultado.Warnings.Add("Defina criterios de evaluación para mayor claridad");

        // 10. Relaciones con otros registros
        if (planificacion.DocenteId <= 0)
            errores.Add("Debe asignarse un docente a la planificación");

        if (planificacion.CursoId <= 0)
            errores.Add("Debe asignarse un curso a la planificación");

        if (planificacion.PeriodoAcademicoId <= 0)
            errores.Add("Debe asignarse un período académico");

        if (planificacion.SeccionId <= 0)
            errores.Add("Debe asignarse una sección");

        // Compilar resultado
        if (errores.Count > 0)
        {
            resultado.IsValid = false;
            resultado.ErrorMessage = string.Join(" | ", errores);
        }

        if (resultado.Warnings.Count > 0)
        {
            _logger.LogWarning($"Advertencias de validación MINERD: {string.Join(", ", resultado.Warnings)}");
        }

        return resultado;
    }

    /// <summary>
    /// Valida contenidos para asegurar que sean específicos y medibles
    /// </summary>
    public ValidationResult ValidarContenidos(string? conceptuales, string? procedimentales, string? actitudinales)
    {
        var resultado = new ValidationResult { IsValid = true };

        if (string.IsNullOrEmpty(conceptuales) || string.IsNullOrEmpty(procedimentales) || string.IsNullOrEmpty(actitudinales))
        {
            resultado.IsValid = false;
            resultado.ErrorMessage = "Los tres tipos de contenido (conceptuales, procedimentales, actitudinales) son obligatorios";
            return resultado;
        }

        // Validar que los contenidos sean específicos
        if (conceptuales.Length < 20)
            resultado.Warnings.Add("Contenidos conceptuales demasiado breves");

        if (procedimentales.Length < 20)
            resultado.Warnings.Add("Contenidos procedimentales demasiado breves");

        if (actitudinales.Length < 20)
            resultado.Warnings.Add("Contenidos actitudinales demasiado breves");

        return resultado;
    }

    /// <summary>
    /// Valida que las competencias sean medibles y observables
    /// </summary>
    public ValidationResult ValidarCompetencias(string? competencias)
    {
        var resultado = new ValidationResult { IsValid = true };

        if (string.IsNullOrWhiteSpace(competencias))
        {
            resultado.IsValid = false;
            resultado.ErrorMessage = "Las competencias no pueden estar vacías";
            return resultado;
        }

        // Verificar estructura mínima
        if (competencias.Length < 50)
            resultado.Warnings.Add("Las competencias deben estar suficientemente desarrolladas");

        return resultado;
    }

    /// <summary>
    /// Genera un informe de integridad de la planificación
    /// </summary>
    public Dictionary<string, object> GenerarInformeIntegridad(Planificacion planificacion)
    {
        var informe = new Dictionary<string, object>
        {
            ["Titulo"] = new { Estado = !string.IsNullOrEmpty(planificacion.Titulo) ? "✅" : "❌", Completo = !string.IsNullOrEmpty(planificacion.Titulo) },
            ["TituloUnidad"] = new { Estado = !string.IsNullOrEmpty(planificacion.TituloUnidad) ? "✅" : "❌", Completo = !string.IsNullOrEmpty(planificacion.TituloUnidad) },
            ["SituacionAprendizaje"] = new { Estado = !string.IsNullOrEmpty(planificacion.SituacionAprendizaje) ? "✅" : "❌", Completo = !string.IsNullOrEmpty(planificacion.SituacionAprendizaje), Caracteres = planificacion.SituacionAprendizaje?.Length ?? 0 },
            ["CompetenciasFundamentales"] = new { Estado = !string.IsNullOrEmpty(planificacion.CompetenciasFundamentales) ? "✅" : "❌", Completo = !string.IsNullOrEmpty(planificacion.CompetenciasFundamentales) },
            ["CompetenciasEspecificas"] = new { Estado = !string.IsNullOrEmpty(planificacion.CompetenciasEspecificas) ? "✅" : "❌", Completo = !string.IsNullOrEmpty(planificacion.CompetenciasEspecificas) },
            ["ContenidosConceptuales"] = new { Estado = !string.IsNullOrEmpty(planificacion.ContenidosConceptuales) ? "✅" : "❌", Completo = !string.IsNullOrEmpty(planificacion.ContenidosConceptuales) },
            ["ContenidosProcedimentales"] = new { Estado = !string.IsNullOrEmpty(planificacion.ContenidosProcedimentales) ? "✅" : "❌", Completo = !string.IsNullOrEmpty(planificacion.ContenidosProcedimentales) },
            ["ContenidosActitudinales"] = new { Estado = !string.IsNullOrEmpty(planificacion.ContenidosActitudinales) ? "✅" : "❌", Completo = !string.IsNullOrEmpty(planificacion.ContenidosActitudinales) },
            ["IndicadoresLogro"] = new { Estado = !string.IsNullOrEmpty(planificacion.IndicadoresLogro) ? "✅" : "❌", Completo = !string.IsNullOrEmpty(planificacion.IndicadoresLogro), Caracteres = planificacion.IndicadoresLogro?.Length ?? 0 },
            ["EstrategiasEnsenanza"] = new { Estado = !string.IsNullOrEmpty(planificacion.EstrategiasEnsenanza) ? "✅" : "❌", Completo = !string.IsNullOrEmpty(planificacion.EstrategiasEnsenanza) },
            ["RecursosDidacticos"] = new { Estado = !string.IsNullOrEmpty(planificacion.RecursosDidacticos) ? "✅" : "❌", Completo = !string.IsNullOrEmpty(planificacion.RecursosDidacticos) },
            ["ActividadesEvaluacion"] = new { Estado = !string.IsNullOrEmpty(planificacion.ActividadesEvaluacion) ? "✅" : "❌", Completo = !string.IsNullOrEmpty(planificacion.ActividadesEvaluacion) },
            ["CriteriosEvaluacion"] = new { Estado = !string.IsNullOrEmpty(planificacion.CriteriosEvaluacion) ? "✅" : "❌", Completo = !string.IsNullOrEmpty(planificacion.CriteriosEvaluacion) },
        };

        var completoCount = informe.Values.Cast<dynamic>().Count(v => v.Completo == true);
        var totalCampos = informe.Count;
        var porcentaje = (completoCount * 100) / totalCampos;

        informe["Resumen"] = new
        {
            CamposCompletos = completoCount,
            CamposTotales = totalCampos,
            Porcentaje = porcentaje,
            Completa = porcentaje >= 80
        };

        return informe;
    }
}
