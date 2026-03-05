using AcademicManager.Application.DTOs;
using FluentValidation;

namespace AcademicManager.Application.Validators;

public class CreateAlumnoValidator : AbstractValidator<CreateAlumnoDto>
{
    public CreateAlumnoValidator()
    {
        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("El código es requerido")
            .MaximumLength(20).WithMessage("El código no puede exceder 20 caracteres");

        RuleFor(x => x.Nombres)
            .NotEmpty().WithMessage("Los nombres son requeridos")
            .MaximumLength(100).WithMessage("Los nombres no pueden exceder 100 caracteres");

        RuleFor(x => x.Apellidos)
            .NotEmpty().WithMessage("Los apellidos son requeridos")
            .MaximumLength(100).WithMessage("Los apellidos no pueden exceder 100 caracteres");

        RuleFor(x => x.FechaNacimiento)
            .NotEmpty().WithMessage("La fecha de nacimiento es requerida")
            .LessThan(DateTime.Now).WithMessage("La fecha de nacimiento debe ser anterior a hoy")
            .GreaterThan(DateTime.Now.AddYears(-100)).WithMessage("La fecha de nacimiento no puede ser mayor a 100 años");

        RuleFor(x => x.Genero)
            .NotEmpty().WithMessage("El género es requerido")
            .MaximumLength(20).WithMessage("El género no puede exceder 20 caracteres");

        RuleFor(x => x.Direccion)
            .MaximumLength(200).WithMessage("La dirección no puede exceder 200 caracteres");

        RuleFor(x => x.Telefono)
            .MaximumLength(20).WithMessage("El teléfono no puede exceder 20 caracteres");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El email debe ser válido")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.NombreApoderado)
            .MaximumLength(100).WithMessage("El nombre del apoderado no puede exceder 100 caracteres");

        RuleFor(x => x.TelefonoApoderado)
            .MaximumLength(20).WithMessage("El teléfono del apoderado no puede exceder 20 caracteres");

        RuleFor(x => x.GradoId)
            .GreaterThan(0).WithMessage("Debe seleccionar un grado válido")
            .When(x => x.GradoId.HasValue);

        RuleFor(x => x.SeccionId)
            .GreaterThan(0).WithMessage("Debe seleccionar una sección válida")
            .When(x => x.SeccionId.HasValue);
    }
}

public class UpdateAlumnoValidator : AbstractValidator<UpdateAlumnoDto>
{
    public UpdateAlumnoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("ID inválido");

        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("El código es requerido")
            .MaximumLength(20).WithMessage("El código no puede exceder 20 caracteres");

        RuleFor(x => x.Nombres)
            .NotEmpty().WithMessage("Los nombres son requeridos")
            .MaximumLength(100).WithMessage("Los nombres no pueden exceder 100 caracteres");

        RuleFor(x => x.Apellidos)
            .NotEmpty().WithMessage("Los apellidos son requeridos")
            .MaximumLength(100).WithMessage("Los apellidos no pueden exceder 100 caracteres");

        RuleFor(x => x.FechaNacimiento)
            .NotEmpty().WithMessage("La fecha de nacimiento es requerida")
            .LessThan(DateTime.Now).WithMessage("La fecha de nacimiento debe ser anterior a hoy")
            .GreaterThan(DateTime.Now.AddYears(-100)).WithMessage("La fecha de nacimiento no puede ser mayor a 100 años");

        RuleFor(x => x.Genero)
            .NotEmpty().WithMessage("El género es requerido")
            .MaximumLength(20).WithMessage("El género no puede exceder 20 caracteres");

        RuleFor(x => x.Direccion)
            .MaximumLength(200).WithMessage("La dirección no puede exceder 200 caracteres");

        RuleFor(x => x.Telefono)
            .MaximumLength(20).WithMessage("El teléfono no puede exceder 20 caracteres");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El email debe ser válido")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.NombreApoderado)
            .MaximumLength(100).WithMessage("El nombre del apoderado no puede exceder 100 caracteres");

        RuleFor(x => x.TelefonoApoderado)
            .MaximumLength(20).WithMessage("El teléfono del apoderado no puede exceder 20 caracteres");

        RuleFor(x => x.GradoId)
            .GreaterThan(0).WithMessage("Debe seleccionar un grado válido")
            .When(x => x.GradoId.HasValue);

        RuleFor(x => x.SeccionId)
            .GreaterThan(0).WithMessage("Debe seleccionar una sección válida")
            .When(x => x.SeccionId.HasValue);
    }
}
