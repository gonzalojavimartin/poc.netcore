using System.Reflection;
using System.Text.Json.Serialization;
using Kinesis.Poc.Api.Data;
using Kinesis.Poc.Api.Errors;
using Kinesis.Poc.Api.Validation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
});
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddScoped<PatientRequestValidator>();
builder.Services.AddDbContext<PatientsDbContext>(options =>
{
    var connectionString = builder.Configuration["KINESIS:ConnectionString_PatientsDb"];
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Configure la conexión mediante la variable KINESIS__ConnectionString_PatientsDb.");
    }

    options.UseSqlServer(connectionString);
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = "api/docs";
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Kinesis.Poc.Api v1");
    });
}

app.UseHttpsRedirection();
app.MapControllers();

// Los cambios de esquema se aplican exclusivamente mediante migraciones autorizadas.
app.Run();
