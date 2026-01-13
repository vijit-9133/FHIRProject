using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using FhirProject.Api.Data;
using FhirProject.Api.Repositories.Interfaces;
using FhirProject.Api.Repositories.Implementations;
using FhirProject.Api.Services.Interfaces;
using FhirProject.Api.Services.Implementations;
using FhirProject.Api.Services.Implementations.Normalization;
using FhirProject.Api.Services.Implementations.Terminology;
using FhirProject.Api.Services.Ocr;
using FhirProject.Api.Services.Llm;
using FhirProject.Api.Services.Auth;
using FhirProject.Api.Mapping;
using FhirProject.Api.Validation;
using FhirProject.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// Render / Docker hosting config
// --------------------
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080);
});

// --------------------
// Database
// --------------------
var connectionString =
    builder.Configuration.GetConnectionString("DbConn")
    ?? Environment.GetEnvironmentVariable("DB_CONN");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// --------------------
// Repositories
// --------------------
builder.Services.AddScoped<IConversionRequestRepository, ConversionRequestRepository>();
builder.Services.AddScoped<IFhirResourceRepository, FhirResourceRepository>();
builder.Services.AddScoped<IExternalResourceMappingRepository, ExternalResourceMappingRepository>();

// --------------------
// FHIR Mappers
// --------------------
builder.Services.AddScoped<IFhirResourceMapper, PatientFhirMapper>();
builder.Services.AddScoped<IFhirResourceMapper, PractitionerFhirMapper>();
builder.Services.AddScoped<IFhirResourceMapper, OrganizationFhirMapper>();

// --------------------
// Validators
// --------------------
builder.Services.AddScoped<IFhirValidator, FhirPatientValidator>();
builder.Services.AddScoped<IFhirValidator, FhirPractitionerValidator>();
builder.Services.AddScoped<IFhirValidator, FhirOrganizationValidator>();

// --------------------
// Core Services
// --------------------
builder.Services.AddScoped<IFhirConversionService, FhirConversionService>();
builder.Services.AddScoped<IIdempotentResourceService, IdempotentResourceService>();
builder.Services.AddScoped<IConversionLifecycleService, ConversionLifecycleService>();
builder.Services.AddScoped<IInboundNormalizationService, InboundNormalizationService>();
builder.Services.AddScoped<ITerminologyMappingService, TerminologyMappingService>();
builder.Services.AddScoped<IClientCredentialsService, ClientCredentialsService>();
builder.Services.AddScoped<IFhirResourceService, FhirResourceService>();

// --------------------
// FHIR Client + Upsert Services
// --------------------
builder.Services.AddScoped<IFhirPatientClientService, FhirPatientClientService>();
builder.Services.AddScoped<IFhirPatientUpsertService, FhirPatientUpsertService>();

builder.Services.AddScoped<IFhirPractitionerClientService, FhirPractitionerClientService>();
builder.Services.AddScoped<IFhirPractitionerUpsertService, FhirPractitionerUpsertService>();

builder.Services.AddScoped<IFhirEncounterClientService, FhirEncounterClientService>();
builder.Services.AddScoped<IFhirEncounterUpsertService, FhirEncounterUpsertService>();

builder.Services.AddScoped<IHealthcareEventIdempotencyService, HealthcareEventIdempotencyService>();

// --------------------
// OCR & LLM
// --------------------
builder.Services.AddScoped<IOcrService, TesseractOcrService>();
builder.Services.AddScoped<IGeminiExtractionService, GeminiExtractionService>();

// --------------------
// JWT Authentication
// --------------------
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// --------------------
// Controllers + JSON
// --------------------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// --------------------
// CORS
// --------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// --------------------
// Swagger
// --------------------
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
// Middleware pipeline
// --------------------
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Swagger ENABLED in Production (important for Render demo)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "FHIR Data Converter API v1");
});

// --------------------
// Endpoints
// --------------------
app.MapControllers();

app.Run();
