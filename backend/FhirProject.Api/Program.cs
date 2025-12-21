using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using FhirProject.Api.Data;
using FhirProject.Api.Repositories.Interfaces;
using FhirProject.Api.Repositories.Implementations;
using FhirProject.Api.Services.Interfaces;
using FhirProject.Api.Services.Implementations;
using FhirProject.Api.Mapping;
using FhirProject.Api.Validation;
using FhirProject.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// Add services to the container
// --------------------

// Register DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbConn")));

// Register Repositories
builder.Services.AddScoped<IConversionRequestRepository, ConversionRequestRepository>();
builder.Services.AddScoped<IFhirResourceRepository, FhirResourceRepository>();

// Register FHIR Mappers
builder.Services.AddScoped<IFhirResourceMapper, PatientFhirMapper>();

// Register FHIR Validators
builder.Services.AddScoped<IFhirValidator, FhirPatientValidator>();

// Register Services
builder.Services.AddScoped<IFhirConversionService, FhirConversionService>();

// Enable Controllers
builder.Services.AddControllers();

// Enable Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FHIR Data Converter API",
        Version = "v1",
        Description = "API to convert non-FHIR healthcare data into FHIR-compliant resources"
    });
});

var app = builder.Build();

// --------------------
// Configure the HTTP request pipeline
// --------------------

// Add global exception handling middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "FHIR Data Converter API v1");
    });
}


// Map Controllers
app.MapControllers();

app.Run();
