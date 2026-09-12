using System.ComponentModel.DataAnnotations;

namespace Kinesis.Poc.Api.Dtos;

/// <summary>Datos para crear o modificar un paciente. El estado lo administra la API.</summary>
public sealed class PatientRequest
{
    /// <summary>Nombre obligatorio, de hasta 100 caracteres.</summary>
    [Required]
    [MaxLength(100)]
    public string? FirstName { get; set => field = Normalize(value); }

    /// <summary>Apellido obligatorio, de hasta 100 caracteres.</summary>
    [Required]
    [MaxLength(100)]
    public string? LastName { get; set => field = Normalize(value); }

    /// <summary>Tipo de documento opcional, de hasta 30 caracteres; requiere número.</summary>
    [MaxLength(30)]
    public string? DocumentType { get; set => field = Normalize(value); }

    /// <summary>Número de documento opcional, de hasta 30 caracteres; requiere tipo.</summary>
    [MaxLength(30)]
    public string? DocumentNumber { get; set => field = Normalize(value); }

    /// <summary>Fecha de nacimiento obligatoria y anterior a hoy, en formato YYYY-MM-DD.</summary>
    [Required]
    public DateOnly? BirthDate { get; set; }

    /// <summary>Email opcional con formato válido, de hasta 150 caracteres.</summary>
    [MaxLength(150)]
    [EmailAddress]
    public string? Email { get; set => field = Normalize(value); }

    /// <summary>Teléfono opcional, de hasta 30 caracteres.</summary>
    [MaxLength(30)]
    public string? Phone { get; set => field = Normalize(value); }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
