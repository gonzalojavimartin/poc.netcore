using Kinesis.Poc.Api.Data;
using Kinesis.Poc.Api.Dtos;
using Kinesis.Poc.Api.Models;
using Kinesis.Poc.Api.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Kinesis.Poc.Api.Controllers;

[ApiController]
[Route("api/patients")]
[Produces("application/json")]
public sealed class PatientsController(
    PatientsDbContext context,
    PatientRequestValidator validator,
    TimeProvider timeProvider) : ControllerBase
{
    /// <summary>Crea un paciente con estado Active.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PatientResponse>> Create(
        [FromBody] PatientRequest request, CancellationToken cancellationToken)
    {
        var errors = validator.ValidateAndNormalize(request);
        if (errors.Count > 0)
        {
            return InvalidRequest(errors);
        }

        if (await DocumentExists(request, null, cancellationToken))
        {
            return DuplicateDocument();
        }

        var now = timeProvider.GetUtcNow();
        var patient = new Patient
        {
            FirstName = request.FirstName!,
            LastName = request.LastName!,
            BirthDate = request.BirthDate!.Value,
            Status = PatientStatus.Active,
            CreatedAt = now,
            UpdatedAt = now
        };
        ApplyData(patient, request);
        context.Patients.Add(patient);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            return DuplicateDocument();
        }

        return CreatedAtAction(nameof(GetById), new { id = patient.Id }, ToResponse(patient));
    }

    /// <summary>Lista pacientes activos e inactivos, con filtro opcional de estado.</summary>
    /// <param name="status">Active o Inactive. Sin filtro se incluyen ambos estados.</param>
    /// <param name="cancellationToken">Cancelación de la solicitud.</param>
    [HttpGet]
    [ProducesResponseType(typeof(PatientResponse[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PatientResponse[]>> List(
        [FromQuery] string? status, CancellationToken cancellationToken)
    {
        var query = context.Patients.AsNoTracking();
        if (status is not null)
        {
            PatientStatus parsedStatus;
            switch (status.Trim())
            {
                case "Active":
                    parsedStatus = PatientStatus.Active;
                    break;
                case "Inactive":
                    parsedStatus = PatientStatus.Inactive;
                    break;
                default:
                    return InvalidRequest(new Dictionary<string, string[]>
                    {
                        [nameof(status)] = ["El estado debe ser Active o Inactive."]
                    });
            }

            query = query.Where(patient => patient.Status == parsedStatus);
        }

        var patients = await query.OrderBy(patient => patient.CreatedAt)
            .ThenBy(patient => patient.Id).ToListAsync(cancellationToken);
        return Ok(patients.Select(ToResponse).ToArray());
    }

    /// <summary>Consulta un paciente por UUID, incluido si está inactivo.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientResponse>> GetById(
        Guid id, CancellationToken cancellationToken)
    {
        var patient = await context.Patients.AsNoTracking()
            .SingleOrDefaultAsync(patient => patient.Id == id, cancellationToken);
        if (patient is null)
        {
            return MissingPatient();
        }

        return Ok(ToResponse(patient));
    }

    /// <summary>Modifica los datos del paciente conservando su estado y fecha de creación.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PatientResponse>> Update(
        Guid id, [FromBody] PatientRequest request, CancellationToken cancellationToken)
    {
        var errors = validator.ValidateAndNormalize(request);
        if (errors.Count > 0)
        {
            return InvalidRequest(errors);
        }

        var patient = await context.Patients
            .SingleOrDefaultAsync(patient => patient.Id == id, cancellationToken);
        if (patient is null)
        {
            return MissingPatient();
        }

        if (await DocumentExists(request, id, cancellationToken))
        {
            return DuplicateDocument();
        }

        ApplyData(patient, request);
        patient.UpdatedAt = timeProvider.GetUtcNow();

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            return DuplicateDocument();
        }

        return Ok(ToResponse(patient));
    }

    /// <summary>Realiza la baja lógica. Repetirla sobre un paciente inactivo devuelve 204.</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var patient = await context.Patients
            .SingleOrDefaultAsync(patient => patient.Id == id, cancellationToken);
        if (patient is null)
        {
            return MissingPatient();
        }

        if (patient.Status != PatientStatus.Inactive)
        {
            patient.Status = PatientStatus.Inactive;
            patient.UpdatedAt = timeProvider.GetUtcNow();
            await context.SaveChangesAsync(cancellationToken);
        }

        return NoContent();
    }

    private Task<bool> DocumentExists(PatientRequest request, Guid? excludedId,
        CancellationToken cancellationToken)
    {
        if (request.DocumentType is null)
        {
            return Task.FromResult(false);
        }

        return context.Patients.AsNoTracking().AnyAsync(patient =>
            patient.DocumentType == request.DocumentType &&
            patient.DocumentNumber == request.DocumentNumber &&
            (!excludedId.HasValue || patient.Id != excludedId.Value), cancellationToken);
    }

    private static void ApplyData(Patient patient, PatientRequest request)
    {
        patient.FirstName = request.FirstName!;
        patient.LastName = request.LastName!;
        patient.DocumentType = request.DocumentType;
        patient.DocumentNumber = request.DocumentNumber;
        patient.BirthDate = request.BirthDate!.Value;
        patient.Email = request.Email;
        patient.Phone = request.Phone;
    }

    private static PatientResponse ToResponse(Patient patient) => new(
        patient.Id, patient.FirstName, patient.LastName, patient.DocumentType,
        patient.DocumentNumber, patient.BirthDate, patient.Email, patient.Phone,
        patient.Status, patient.CreatedAt, patient.UpdatedAt);

    private ObjectResult InvalidRequest(Dictionary<string, string[]> errors) =>
        ValidationProblem(new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Los datos de la solicitud no son válidos."
        });

    private ObjectResult MissingPatient() => Problem(
        statusCode: StatusCodes.Status404NotFound,
        title: "Paciente no encontrado.");

    private ObjectResult DuplicateDocument() => Problem(
        statusCode: StatusCodes.Status409Conflict,
        title: "Documento duplicado.",
        detail: "Ya existe un paciente con ese tipo y número de documento.");

    // El índice único cubre también solicitudes concurrentes que superen la consulta previa.
    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException sqlException &&
        sqlException.Errors.Cast<SqlError>().Any(error => error.Number is 2601 or 2627);
}
