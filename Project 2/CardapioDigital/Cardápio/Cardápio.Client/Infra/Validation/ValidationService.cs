using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;

namespace Cardápio.Client.Infra.Validation
{
    public class ValidationResult
    {
        public bool IsValid { get; set; } = true;
        public Dictionary<string, string> Errors { get; set; } = new();
    }

    public class ValidationService<T>
    {
        public ValidationResult Validate(T entity, params string[] requiredFields)
        {
            ValidationResult validationResult = new ValidationResult();

            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                if (requiredFields.Contains(property.Name))
                {
                    ValidateRequired(property, entity, validationResult);
                    ValidateStringLength(property, entity, validationResult);
                }
            }

            return validationResult;
        }

        private void ValidateRequired(PropertyInfo property, T entity, ValidationResult validationResult)
        {
            object value = property.GetValue(entity);

            if (value == null || value is string str && string.IsNullOrWhiteSpace(str))
            {
                validationResult.IsValid = false;
                validationResult.Errors.Add(property.Name, $"O {property.Name} é obrigatório.");
            }

            if (value is int intValue && intValue == 0)
            {
                validationResult.IsValid = false;
                validationResult.Errors.Add(property.Name, $"A {property.Name} é obrigatória.");
            }
        }

        private void ValidateStringLength(PropertyInfo property, T entity, ValidationResult validationResult)
        {
            if (validationResult.Errors.ContainsKey(property.Name))
            {
                return;
            }

            object value = property.GetValue(entity);

            if (value is string strValue)
            {
                var stringLengthAttribute = property.GetCustomAttribute<StringLengthAttribute>();
                if (stringLengthAttribute != null)
                {
                    ValidateMinLength(strValue, property, stringLengthAttribute, validationResult);
                    ValidateMaxLength(strValue, property, stringLengthAttribute, validationResult);
                }
            }
        }

        private void ValidateMinLength(string value, PropertyInfo property, StringLengthAttribute stringLengthAttribute, ValidationResult validationResult)
        {
            if (value.Length < stringLengthAttribute.MinimumLength)
            {
                validationResult.IsValid = false;
                validationResult.Errors.Add(property.Name, $"O {property.Name} deve ter pelo menos {stringLengthAttribute.MinimumLength} caracteres.");
            }
        }

        private void ValidateMaxLength(string value, PropertyInfo property, StringLengthAttribute stringLengthAttribute, ValidationResult validationResult)
        {
            if (value.Length > stringLengthAttribute.MaximumLength)
            {
                validationResult.IsValid = false;
                validationResult.Errors.Add(property.Name, $"O {property.Name} deve ter no máximo {stringLengthAttribute.MaximumLength} caracteres.");
            }
        }
    }

    public class ValidationErrorResponseApi
    {
        public int Status { get; set; }
        public Dictionary<string, List<string>> Errors { get; set; }

        public static ValidationErrorResponseApi ConvertJsonToData(string ErrorContent)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            ValidationErrorResponseApi errorDetails = JsonSerializer.Deserialize<ValidationErrorResponseApi>(ErrorContent, options);
            return errorDetails;
        }
    }
}