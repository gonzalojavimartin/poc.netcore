namespace Kinesis.Poc.Api.Dtos;

/// <summary>Datos para crear o modificar un paciente. El estado lo administra la API.</summary>
public sealed class PatientRequest
{
    /// <summary>Nombre obligatorio, de hasta 100 caracteres.</summary>
    public string? FirstName { get; set; }

    /// <summary>Apellido obligatorio, de hasta 100 caracteres.</summary>
    public string? LastName { get; set; }

    /// <summary>Tipo de documento opcional, de hasta 30 caracteres; requiere número.</summary>
    public string? DocumentType { get; set; }

    /// <summary>Número de documento opcional, de hasta 30 caracteres; requiere tipo.</summary>
    public string? DocumentNumber { get; set; }

    /// <summary>Fecha de nacimiento obligatoria y anterior a hoy, en formato YYYY-MM-DD.</summary>
    public DateOnly? BirthDate { get; set; }

    /// <summary>Email opcional con formato válido, de hasta 150 caracteres.</summary>
    public string? Email { get; set; }

    /// <summary>Teléfono opcional, de hasta 30 caracteres.</summary>
    public string? Phone { get; set; }
}
