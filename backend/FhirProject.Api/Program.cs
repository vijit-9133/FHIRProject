using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// Add services to the container
// --------------------

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
