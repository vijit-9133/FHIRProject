using FhirProject.Api.Data;
using FhirProject.Api.Models.entities;
using FhirProject.Api.Models.enums;
using FhirProject.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FhirProject.Api.Repositories.Implementations;

public class ExternalResourceMappingRepository : IExternalResourceMappingRepository
{
    private readonly AppDbContext _context;

    public ExternalResourceMappingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ExternalResourceMapping?> GetMappingAsync(string sourceSystem, string externalId)
    {
        return await _context.ExternalResourceMappings
            .FirstOrDefaultAsync(m => m.SourceSystem == sourceSystem && m.ExternalId == externalId);
    }

    public async Task<ExternalResourceMapping?> GetMappingAsync(string sourceSystem, string externalId, FhirResourceType resourceType)
    {
        return await _context.ExternalResourceMappings
            .FirstOrDefaultAsync(m => m.SourceSystem == sourceSystem && m.ExternalId == externalId && m.ResourceType == resourceType);
    }

    public async Task<ExternalResourceMapping> CreateMappingAsync(ExternalResourceMapping mapping)
    {
        mapping.Id = Guid.NewGuid();
        mapping.CreatedAt = DateTime.UtcNow;
        
        _context.ExternalResourceMappings.Add(mapping);
        await _context.SaveChangesAsync();
        
        return mapping;
    }

    public async Task UpdateMappingAsync(ExternalResourceMapping mapping)
    {
        _context.ExternalResourceMappings.Update(mapping);
        await _context.SaveChangesAsync();
    }
}