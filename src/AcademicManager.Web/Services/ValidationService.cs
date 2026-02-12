using AcademicManager.Web.Constants;

namespace AcademicManager.Web.Services
{
    public interface IValidationService
    {
        ValidationResult ValidateEmail(string email);
        ValidationResult ValidatePhone(string phone);
        ValidationResult ValidatePassword(string password);
        ValidationResult ValidateRequired(string value, string fieldName);
        ValidationResult ValidateLength(string value, string fieldName, int minLength, int maxLength);
        ValidationResult ValidateRange(decimal value, string fieldName, decimal min, decimal max);
        ValidationResult ValidateDate(DateTime date, string fieldName, DateTime? minDate = null, DateTime? maxDate = null);
        ValidationResult ValidateFile(string fileName, long fileSize, string[] allowedExtensions, long maxFileSize);
        ValidationResult ValidateDni(string dni);
        ValidationResult ValidateAlumnoCodigo(string codigo);
        ValidationResult Combine(params ValidationResult[] results);
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;

        public static ValidationResult Success() => new() { IsValid = true };

        public static ValidationResult Error(string errorMessage, string fieldName = "")
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = errorMessage,
                FieldName = fieldName
            };
        }
    }

    public class ValidationService : IValidationService
    {
        public ValidationResult ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return ValidationResult.Success(); // Email is optional by default

            if (string.IsNullOrWhiteSpace(email))
                return ValidationResult.Error(FormValidationMessages.RequiredField, "Email");

            if (!IsValidEmailFormat(email))
                return ValidationResult.Error(FormValidationMessages.InvalidEmail, "Email");

            if (email.Length > UIConstants.MaxTextInputLength)
                return ValidationResult.Error(FormValidationMessages.MaxLength.Replace("{0}", UIConstants.MaxTextInputLength.ToString()), "Email");

            return ValidationResult.Success();
        }

        public ValidationResult ValidatePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return ValidationResult.Success(); // Phone is optional by default

            if (!IsValidPhoneFormat(phone))
                return ValidationResult.Error(FormValidationMessages.InvalidPhone, "Teléfono");

            if (phone.Length > 20)
                return ValidationResult.Error(FormValidationMessages.MaxLength.Replace("{0}", "20"), "Teléfono");

            return ValidationResult.Success();
        }

        public ValidationResult ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return ValidationResult.Error(FormValidationMessages.RequiredField, "Contraseña");

            if (password.Length < UIConstants.MinPasswordLength)
                return ValidationResult.Error(FormValidationMessages.MinLength.Replace("{0}", UIConstants.MinPasswordLength.ToString()), "Contraseña");

            if (!HasRequiredPasswordComplexity(password))
                return ValidationResult.Error("La contraseña debe contener al menos una letra mayúscula, una letra minúscula y un número.", "Contraseña");

            return ValidationResult.Success();
        }

        public ValidationResult ValidateRequired(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                return ValidationResult.Error(FormValidationMessages.RequiredField, fieldName);

            return ValidationResult.Success();
        }

        public ValidationResult ValidateLength(string value, string fieldName, int minLength, int maxLength)
        {
            if (value == null) return ValidationResult.Error(FormValidationMessages.RequiredField, fieldName);

            if (value.Length < minLength)
                return ValidationResult.Error(FormValidationMessages.MinLength.Replace("{0}", minLength.ToString()), fieldName);

            if (value.Length > maxLength)
                return ValidationResult.Error(FormValidationMessages.MaxLength.Replace("{0}", maxLength.ToString()), fieldName);

            return ValidationResult.Success();
        }

        public ValidationResult ValidateRange(decimal value, string fieldName, decimal min, decimal max)
        {
            if (value < min || value > max)
                return ValidationResult.Error(FormValidationMessages.RangeValue.Replace("{0}", min.ToString()).Replace("{1}", max.ToString()), fieldName);

            return ValidationResult.Success();
        }

        public ValidationResult ValidateDate(DateTime date, string fieldName, DateTime? minDate = null, DateTime? maxDate = null)
        {
            if (minDate.HasValue && date < minDate.Value)
                return ValidationResult.Error($"La fecha debe ser igual o posterior a {minDate.Value:dd/MM/yyyy}", fieldName);

            if (maxDate.HasValue && date > maxDate.Value)
                return ValidationResult.Error($"La fecha debe ser igual o anterior a {maxDate.Value:dd/MM/yyyy}", fieldName);

            return ValidationResult.Success();
        }

        public ValidationResult ValidateFile(string fileName, long fileSize, string[] allowedExtensions, long maxFileSize)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return ValidationResult.Error(FormValidationMessages.RequiredField, "Archivo");

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return ValidationResult.Error($"Tipo de archivo no permitido. Extensiones permitidas: {string.Join(", ", allowedExtensions)}", "Archivo");

            if (fileSize > maxFileSize)
                return ValidationResult.Error($"El archivo excede el tamaño máximo permitido de {maxFileSize / (1024 * 1024)}MB", "Archivo");

            return ValidationResult.Success();
        }

        public ValidationResult ValidateDni(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni))
                return ValidationResult.Error(FormValidationMessages.RequiredField, "DNI");

            dni = dni.Replace(" ", "").Replace("-", "").Replace("_", "");

            if (dni.Length != 8 || !long.TryParse(dni, out _))
                return ValidationResult.Error("El DNI debe tener 8 dígitos numéricos", "DNI");

            return ValidationResult.Success();
        }

        public ValidationResult ValidateAlumnoCodigo(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return ValidationResult.Error(FormValidationMessages.RequiredField, "Código");

            if (!codigo.StartsWith("ALU-"))
                return ValidationResult.Error("El código debe comenzar con 'ALU-'", "Código");

            if (codigo.Length < 5)
                return ValidationResult.Error("El código debe tener al menos 5 caracteres", "Código");

            if (codigo.Length > 20)
                return ValidationResult.Error(FormValidationMessages.MaxLength.Replace("{0}", "20"), "Código");

            return ValidationResult.Success();
        }

        public ValidationResult Combine(params ValidationResult[] results)
        {
            foreach (var result in results)
            {
                if (!result.IsValid)
                    return result;
            }

            return ValidationResult.Success();
        }

        private static bool IsValidEmailFormat(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValidPhoneFormat(string phone)
        {
            var cleanPhone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+", "");
            
            // Allow phone numbers with 6-15 digits
            return cleanPhone.Length >= 6 && cleanPhone.Length <= 15 && cleanPhone.All(char.IsDigit);
        }

        private static bool HasRequiredPasswordComplexity(string password)
        {
            return password.Any(char.IsUpper) && 
                   password.Any(char.IsLower) && 
                   password.Any(char.IsDigit);
        }
    }

    public class FluentValidator
    {
        private readonly IValidationService validationService;
        private readonly List<ValidationResult> results = new();

        public FluentValidator(IValidationService validationService)
        {
            this.validationService = validationService;
        }

        public FluentValidator Required(string value, string fieldName)
        {
            results.Add(validationService.ValidateRequired(value, fieldName));
            return this;
        }

        public FluentValidator Email(string email)
        {
            results.Add(validationService.ValidateEmail(email));
            return this;
        }

        public FluentValidator Phone(string phone)
        {
            results.Add(validationService.ValidatePhone(phone));
            return this;
        }

        public FluentValidator Password(string password)
        {
            results.Add(validationService.ValidatePassword(password));
            return this;
        }

        public FluentValidator Length(string value, string fieldName, int minLength, int maxLength)
        {
            results.Add(validationService.ValidateLength(value, fieldName, minLength, maxLength));
            return this;
        }

        public FluentValidator Range(decimal value, string fieldName, decimal min, decimal max)
        {
            results.Add(validationService.ValidateRange(value, fieldName, min, max));
            return this;
        }

        public FluentValidator Date(DateTime date, string fieldName, DateTime? minDate = null, DateTime? maxDate = null)
        {
            results.Add(validationService.ValidateDate(date, fieldName, minDate, maxDate));
            return this;
        }

        public FluentValidator Dni(string dni)
        {
            results.Add(validationService.ValidateDni(dni));
            return this;
        }

        public FluentValidator AlumnoCodigo(string codigo)
        {
            results.Add(validationService.ValidateAlumnoCodigo(codigo));
            return this;
        }

        public ValidationResult Validate()
        {
            return validationService.Combine(results.ToArray());
        }

        public ValidationResult ValidateAndThrow()
        {
            var result = Validate();
            if (!result.IsValid)
            {
                throw new ValidationException(result.ErrorMessage);
            }
            return result;
        }
    }

    public class ValidationException : Exception
    {
        public string FieldName { get; }

        public ValidationException(string message, string fieldName = "") : base(message)
        {
            FieldName = fieldName;
        }
    }
}