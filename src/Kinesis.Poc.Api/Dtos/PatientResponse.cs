using Kinesis.Poc.Api.Models;

namespace Kinesis.Poc.Api.Dtos;

/// <summary>Datos públicos del paciente, incluido su estado y las fechas de auditoría.</summary>
public sealed record PatientResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string? DocumentType,
    string? DocumentNumber,
    DateOnly BirthDate,
    string? Email,
    string? Phone,
    PatientStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
