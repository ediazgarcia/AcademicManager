namespace AcademicManager.Web.Constants
{
    public static class UIConstants
    {
        // Pagination
        public const int DefaultPageSize = 10;
        public static readonly int[] PageSizes = { 5, 10, 25, 50, 100 };
        
        // Animation
        public const string DefaultTransitionDuration = "0.3s";
        public const string FadeInClass = "fade-in";
        public const string SlideUpClass = "slide-up";
        
        // Modal sizes
        public const string ModalSizeSmall = "sm";
        public const string ModalSizeMedium = "md";
        public const string ModalSizeLarge = "lg";
        public const string ModalSizeExtraLarge = "xl";
        public const string ModalSizeFull = "full";
        
        // Form validation
        public const int MaxTextInputLength = 500;
        public const int MaxTextAreaLength = 4000;
        public const int MaxFileNameLength = 255;
        
        // File upload
        public const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB
        public static readonly string[] AllowedFileExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
        
        // Security
        public const int MinPasswordLength = 6;
        public const int MaxLoginAttempts = 5;
        public const int SessionTimeoutMinutes = 30;
        
        // UI Messages
        public const string DefaultSuccessMessage = "Operación realizada con éxito";
        public const string DefaultErrorMessage = "Ha ocurrido un error inesperado";
        public const string DefaultConfirmationMessage = "¿Está seguro de realizar esta acción?";
        public const string DefaultDeleteConfirmation = "¿Está seguro de eliminar este elemento? Esta acción no se puede deshacer.";
    }

    public static class FormValidationMessages
    {
        public const string RequiredField = "Este campo es requerido";
        public const string InvalidEmail = "Por favor ingrese un email válido";
        public const string InvalidPhone = "Por favor ingrese un teléfono válido";
        public const string InvalidDate = "Por favor ingrese una fecha válida";
        public const string MinLength = "El campo debe tener al menos {0} caracteres";
        public const string MaxLength = "El campo no puede exceder {0} caracteres";
        public const string RangeValue = "El valor debe estar entre {0} y {1}";
        public const string NumericRequired = "Este campo debe ser numérico";
        public const string PositiveNumber = "El valor debe ser positivo";
    }

    public static class StatusMessages
    {
        public const string Creating = "Creando...";
        public const string Updating = "Actualizando...";
        public const string Deleting = "Eliminando...";
        public const string Loading = "Cargando...";
        public const string Processing = "Procesando...";
        public const string Saving = "Guardando...";
        public const string Submitting = "Enviando...";
    }
}