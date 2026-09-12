using System.ComponentModel.DataAnnotations;
using Kinesis.Poc.Api.Dtos;

namespace Kinesis.Poc.Api.Validation;

/// <summary>Normaliza la entrada y verifica las reglas que no requieren persistencia.</summary>
public sealed class PatientRequestValidator(TimeProvider timeProvider)
{
    private static readonly EmailAddressAttribute EmailValidator = new();

    /// <summary>Recorta los textos y devuelve los errores por campo. Usa la fecha local del servidor.</summary>
    public Dictionary<string, string[]> ValidateAndNormalize(PatientRequest request)
    {
        request.FirstName = Normalize(request.FirstName);
        request.LastName = Normalize(request.LastName);
        request.DocumentType = Normalize(request.DocumentType);
        request.DocumentNumber = Normalize(request.DocumentNumber);
        request.Email = Normalize(request.Email);
        request.Phone = Normalize(request.Phone);

        var errors = new Dictionary<string, List<string>>();

        ValidateText(nameof(request.FirstName), request.FirstName, 100, required: true);
        ValidateText(nameof(request.LastName), request.LastName, 100, required: true);
        ValidateText(nameof(request.DocumentType), request.DocumentType, 30);
        ValidateText(nameof(request.DocumentNumber), request.DocumentNumber, 30);
        ValidateText(nameof(request.Email), request.Email, 150);
        ValidateText(nameof(request.Phone), request.Phone, 30);

        if (request.BirthDate is null)
        {
            AddError(nameof(request.BirthDate), "La fecha de nacimiento es obligatoria.");
        }
        else if (request.BirthDate.Value >= DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime))
        {
            AddError(nameof(request.BirthDate), "La fecha de nacimiento debe ser anterior a la fecha actual.");
        }

        if (request.Email is not null && !EmailValidator.IsValid(request.Email))
        {
            AddError(nameof(request.Email), "El email debe tener un formato válido.");
        }

        if ((request.DocumentType is null) != (request.DocumentNumber is null))
        {
            AddError(nameof(request.DocumentType), "El tipo y el número de documento deben informarse conjuntamente.");
            AddError(nameof(request.DocumentNumber), "El tipo y el número de documento deben informarse conjuntamente.");
        }

        return errors.ToDictionary(entry => entry.Key, entry => entry.Value.ToArray());

        void ValidateText(string field, string? value, int maximumLength, bool required = false)
        {
            if (required && value is null)
            {
                AddError(field, "El campo es obligatorio.");
            }

            if (value is not null && value.Length > maximumLength)
            {
                AddError(field, $"El campo admite como máximo {maximumLength} caracteres.");
            }
        }

        void AddError(string field, string message)
        {
            if (!errors.TryGetValue(field, out var messages))
            {
                messages = [];
                errors[field] = messages;
            }

            messages.Add(message);
        }
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
