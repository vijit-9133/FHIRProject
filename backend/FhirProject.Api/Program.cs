using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FhirProject.Api.Data;
using FhirProject.Api.Repositories.Interfaces;
using FhirProject.Api.Repositories.Implementations;
using FhirProject.Api.Services.Interfaces;
using FhirProject.Api.Services.Implementations;
using FhirProject.Api.Services.Ocr;
using FhirProject.Api.Services.Llm;
using FhirProject.Api.Services.Auth;
using FhirProject.Api.Mapping;
using FhirProject.Api.Validation;
using FhirProject.Api.Middleware;
using System.Text.Json;

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
builder.Services.AddScoped<IFhirResourceMapper, PractitionerFhirMapper>();
builder.Services.AddScoped<IFhirResourceMapper, OrganizationFhirMapper>();

// Register FHIR Validators
builder.Services.AddScoped<IFhirValidator, FhirPatientValidator>();
builder.Services.AddScoped<IFhirValidator, FhirPractitionerValidator>();
builder.Services.AddScoped<IFhirValidator, FhirOrganizationValidator>();

// Register Services
builder.Services.AddScoped<IFhirConversionService, FhirConversionService>();

// Register OCR Service
builder.Services.AddScoped<IOcrService, TesseractOcrService>();

// Register Gemini LLM Service
builder.Services.AddScoped<IGeminiExtractionService, GeminiExtractionService>();

// Register JWT Token Service
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Add JWT Authentication (Passive Mode)
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

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

// Enable Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

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

// Use CORS
app.UseCors("AllowAll");

// Add Authentication and Authorization middleware (Passive Mode)
app.UseAuthentication();
app.UseAuthorization();

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
