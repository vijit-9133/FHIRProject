using FhirProject.Api.Models.entities;
using FhirProject.Api.Models.enums;
using FhirProject.Api.Models.Normalized;
using FhirProject.Api.Repositories.Interfaces;
using FhirProject.Api.Services.Interfaces;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using System.Collections.Concurrent;

namespace FhirProject.Api.Services.Implementations;

public class FhirPatientUpsertService : IFhirPatientUpsertService
{
    private readonly IExternalResourceMappingRepository _mappingRepository;
    private readonly IFhirPatientClientService _fhirPatientClient;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();

    public FhirPatientUpsertService(
        IExternalResourceMappingRepository mappingRepository,
        IFhirPatientClientService fhirPatientClient)
    {
        _mappingRepository = mappingRepository;
        _fhirPatientClient = fhirPatientClient;
    }

    public async Task<string> UpsertPatientAsync(string sourceSystem, string externalPatientId, NormalizedPatient normalizedPatient)
    {
        var lockKey = $"{sourceSystem}:{externalPatientId}:Patient";
        var semaphore = _semaphores.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();
        try
        {
            // Query ExternalResourceMappings for existing Patient
            var existingMapping = await _mappingRepository.GetMappingAsync(
                sourceSystem, 
                externalPatientId, 
                FhirResourceType.Patient);

            if (existingMapping != null)
            {
                // UPDATE path: Patient mapping exists, verify FHIR resource exists
                try
                {
                    var existingPatient = await _fhirPatientClient.GetPatientAsync(existingMapping.InternalResourceId);
                    if (existingPatient != null)
                    {
                        // Update existing Patient with normalized data
                        var updatedPatient = MapNormalizedToFhirPatient(normalizedPatient, existingPatient);
                        await _fhirPatientClient.UpdatePatientAsync(existingMapping.InternalResourceId, updatedPatient);
                        
                        return existingMapping.InternalResourceId;
                    }
                }
                catch (FhirOperationException ex) when (ex.Status == System.Net.HttpStatusCode.NotFound)
                {
                    // Mapping exists but FHIR resource is missing - treat as stale mapping
                    // Fall through to CREATE path and update existing mapping
                }
            }

            // CREATE path: Patient does not exist or mapping is stale
            var newPatient = MapNormalizedToFhirPatient(normalizedPatient);
            var createdPatient = await _fhirPatientClient.CreatePatientAsync(newPatient);

            if (existingMapping != null)
            {
                // Update existing mapping with new FHIR ID
                existingMapping.InternalResourceId = createdPatient.Id;
                await _mappingRepository.UpdateMappingAsync(existingMapping);
            }
            else
            {
                // Insert new ExternalResourceMapping
                var mapping = new ExternalResourceMapping
                {
                    SourceSystem = sourceSystem,
                    ExternalId = externalPatientId,
                    ResourceType = FhirResourceType.Patient,
                    InternalResourceId = createdPatient.Id
                };
                await _mappingRepository.CreateMappingAsync(mapping);
            }
            
            return createdPatient.Id;
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

    private Patient MapNormalizedToFhirPatient(NormalizedPatient normalized, Patient? existingPatient = null)
    {
        var patient = existingPatient ?? new Patient();

        // Update name
        patient.Name.Clear();
        if (!string.IsNullOrEmpty(normalized.FirstName) || !string.IsNullOrEmpty(normalized.LastName))
        {
            patient.Name.Add(new HumanName
            {
                Use = HumanName.NameUse.Official,
                Family = normalized.LastName,
                Given = !string.IsNullOrEmpty(normalized.FirstName) ? new[] { normalized.FirstName } : null
            });
        }

        // Update gender
        if (!string.IsNullOrEmpty(normalized.Gender))
        {
            patient.Gender = normalized.Gender.ToLower() switch
            {
                "male" => AdministrativeGender.Male,
                "female" => AdministrativeGender.Female,
                "other" => AdministrativeGender.Other,
                _ => AdministrativeGender.Unknown
            };
        }

        // Update birth date
        if (normalized.DateOfBirth.HasValue)
        {
            patient.BirthDate = normalized.DateOfBirth.Value.ToString("yyyy-MM-dd");
        }

        // Update telecom
        patient.Telecom.Clear();
        if (!string.IsNullOrEmpty(normalized.PhoneNumber))
        {
            patient.Telecom.Add(new ContactPoint
            {
                System = ContactPoint.ContactPointSystem.Phone,
                Value = normalized.PhoneNumber,
                Use = ContactPoint.ContactPointUse.Home
            });
        }

        if (!string.IsNullOrEmpty(normalized.Email))
        {
            patient.Telecom.Add(new ContactPoint
            {
                System = ContactPoint.ContactPointSystem.Email,
                Value = normalized.Email,
                Use = ContactPoint.ContactPointUse.Home
            });
        }

        // Update address
        patient.Address.Clear();
        if (normalized.Address != null && !string.IsNullOrEmpty(normalized.Address.Line1))
        {
            patient.Address.Add(new Address
            {
                Use = Address.AddressUse.Home,
                Line = new[] { normalized.Address.Line1 },
                City = normalized.Address.City,
                State = normalized.Address.State,
                PostalCode = normalized.Address.PostalCode,
                Country = normalized.Address.Country
            });
        }

        return patient;
    }
}