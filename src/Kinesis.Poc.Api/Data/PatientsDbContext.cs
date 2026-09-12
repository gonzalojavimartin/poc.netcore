using Kinesis.Poc.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Kinesis.Poc.Api.Data;

public sealed class PatientsDbContext(DbContextOptions<PatientsDbContext> options)
    : DbContext(options)
{
    public DbSet<Patient> Patients => Set<Patient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PatientConfiguration());
    }
}
