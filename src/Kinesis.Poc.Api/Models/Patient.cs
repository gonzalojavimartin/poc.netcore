namespace Kinesis.Poc.Api.Models;

public sealed class Patient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? DocumentType { get; set; }
    public string? DocumentNumber { get; set; }
    public DateOnly BirthDate { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public PatientStatus Status { get; set; } = PatientStatus.Active;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
