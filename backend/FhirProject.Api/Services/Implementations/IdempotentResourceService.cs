using FhirProject.Api.Models.entities;
using FhirProject.Api.Models.enums;
using FhirProject.Api.Repositories.Interfaces;
using FhirProject.Api.Services.Interfaces;
using System.Collections.Concurrent;

namespace FhirProject.Api.Services.Implementations;

public class IdempotentResourceService : IIdempotentResourceService
{
    private readonly IExternalResourceMappingRepository _mappingRepository;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();

    public IdempotentResourceService(IExternalResourceMappingRepository mappingRepository)
    {
        _mappingRepository = mappingRepository;
    }

    public async Task<string> GetOrCreateResourceIdAsync(string sourceSystem, string externalId, FhirResourceType resourceType, Func<Task<string>> createResourceFunc)
    {
        var lockKey = $"{sourceSystem}:{externalId}";
        var semaphore = _semaphores.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();
        try
        {
            // Check if mapping already exists
            var existingMapping = await _mappingRepository.GetMappingAsync(sourceSystem, externalId);
            if (existingMapping != null)
            {
                return existingMapping.InternalResourceId;
            }

            // Create new resource
            var newResourceId = await createResourceFunc();

            // Save mapping
            var mapping = new ExternalResourceMapping
            {
                SourceSystem = sourceSystem,
                ExternalId = externalId,
                ResourceType = resourceType,
                InternalResourceId = newResourceId
            };

            await _mappingRepository.CreateMappingAsync(mapping);
            return newResourceId;
        }
        finally
        {
            semaphore.Release();
            
            // Clean up semaphore if no other threads are waiting
            if (semaphore.CurrentCount == 1)
            {
                _semaphores.TryRemove(lockKey, out _);
                semaphore.Dispose();
            }
        }
    }
}