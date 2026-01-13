using FhirProject.Api.Models.entities;
using FhirProject.Api.Models.enums;
using FhirProject.Api.Models.Normalized;
using FhirProject.Api.Repositories.Interfaces;
using FhirProject.Api.Services.Interfaces;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using System.Collections.Concurrent;

namespace FhirProject.Api.Services.Implementations;

public class FhirPractitionerUpsertService : IFhirPractitionerUpsertService
{
    private readonly IExternalResourceMappingRepository _mappingRepository;
    private readonly IFhirPractitionerClientService _fhirPractitionerClient;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();

    public FhirPractitionerUpsertService(
        IExternalResourceMappingRepository mappingRepository,
        IFhirPractitionerClientService fhirPractitionerClient)
    {
        _mappingRepository = mappingRepository;
        _fhirPractitionerClient = fhirPractitionerClient;
    }

    public async Task<string> UpsertPractitionerAsync(string sourceSystem, string externalPractitionerId, NormalizedPractitioner normalizedPractitioner)
    {
        var lockKey = $"{sourceSystem}:{externalPractitionerId}:Practitioner";
        var semaphore = _semaphores.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();
        try
        {
            // Query ExternalResourceMappings for existing Practitioner
            var existingMapping = await _mappingRepository.GetMappingAsync(
                sourceSystem, 
                externalPractitionerId, 
                FhirResourceType.Practitioner);

            if (existingMapping != null)
            {
                // UPDATE path: Practitioner mapping exists, verify FHIR resource exists
                try
                {
                    var existingPractitioner = await _fhirPractitionerClient.GetPractitionerAsync(existingMapping.InternalResourceId);
                    if (existingPractitioner != null)
                    {
                        // Update existing Practitioner with normalized data
                        var updatedPractitioner = MapNormalizedToFhirPractitioner(normalizedPractitioner, existingPractitioner);
                        await _fhirPractitionerClient.UpdatePractitionerAsync(updatedPractitioner);
                        
                        return existingMapping.InternalResourceId;
                    }
                }
                catch (FhirOperationException ex) when (ex.Status == System.Net.HttpStatusCode.NotFound)
                {
                    // Mapping exists but FHIR resource is missing - treat as stale mapping
                    // Fall through to CREATE path and update existing mapping
                }
            }

            // CREATE path: Practitioner does not exist or mapping is stale
            var newPractitioner = MapNormalizedToFhirPractitioner(normalizedPractitioner);
            var createdPractitioner = await _fhirPractitionerClient.CreatePractitionerAsync(newPractitioner);

            if (existingMapping != null)
            {
                // Update existing mapping with new FHIR ID
                existingMapping.InternalResourceId = createdPractitioner.Id;
                await _mappingRepository.UpdateMappingAsync(existingMapping);
            }
            else
            {
                // Insert new ExternalResourceMapping
                var mapping = new ExternalResourceMapping
                {
                    SourceSystem = sourceSystem,
                    ExternalId = externalPractitionerId,
                    ResourceType = FhirResourceType.Practitioner,
                    InternalResourceId = createdPractitioner.Id
                };
                await _mappingRepository.CreateMappingAsync(mapping);
            }
            
            return createdPractitioner.Id;
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

    private Practitioner MapNormalizedToFhirPractitioner(NormalizedPractitioner normalized, Practitioner? existingPractitioner = null)
    {
        var practitioner = existingPractitioner ?? new Practitioner();

        // Update name
        practitioner.Name.Clear();
        if (!string.IsNullOrEmpty(normalized.FirstName) || !string.IsNullOrEmpty(normalized.LastName))
        {
            practitioner.Name.Add(new HumanName
            {
                Use = HumanName.NameUse.Official,
                Family = normalized.LastName,
                Given = !string.IsNullOrEmpty(normalized.FirstName) ? new[] { normalized.FirstName } : null
            });
        }

        // Update telecom
        practitioner.Telecom.Clear();
        if (!string.IsNullOrEmpty(normalized.PhoneNumber))
        {
            practitioner.Telecom.Add(new ContactPoint
            {
                System = ContactPoint.ContactPointSystem.Phone,
                Value = normalized.PhoneNumber,
                Use = ContactPoint.ContactPointUse.Work
            });
        }

        if (!string.IsNullOrEmpty(normalized.Email))
        {
            practitioner.Telecom.Add(new ContactPoint
            {
                System = ContactPoint.ContactPointSystem.Email,
                Value = normalized.Email,
                Use = ContactPoint.ContactPointUse.Work
            });
        }

        // Update qualification
        practitioner.Qualification.Clear();
        if (!string.IsNullOrEmpty(normalized.Qualification))
        {
            practitioner.Qualification.Add(new Practitioner.QualificationComponent
            {
                Code = new CodeableConcept
                {
                    Text = normalized.Qualification
                }
            });
        }

        // Update specialty (using PractitionerRole would be more appropriate in production)
        if (!string.IsNullOrEmpty(normalized.Specialty))
        {
            // Add specialty as extension since Practitioner doesn't have direct specialty field
            practitioner.Extension.RemoveAll(e => e.Url == "http://hl7.org/fhir/StructureDefinition/practitioner-specialty");
            practitioner.Extension.Add(new Extension
            {
                Url = "http://hl7.org/fhir/StructureDefinition/practitioner-specialty",
                Value = new CodeableConcept { Text = normalized.Specialty }
            });
        }

        // Update address
        practitioner.Address.Clear();
        if (normalized.Address != null && !string.IsNullOrEmpty(normalized.Address.Line1))
        {
            practitioner.Address.Add(new Address
            {
                Use = Address.AddressUse.Work,
                Line = new[] { normalized.Address.Line1 },
                City = normalized.Address.City,
                State = normalized.Address.State,
                PostalCode = normalized.Address.PostalCode,
                Country = normalized.Address.Country
            });
        }

        return practitioner;
    }
}