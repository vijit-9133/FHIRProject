using FhirProject.Api.Models.entities;
using FhirProject.Api.Models.enums;
using FhirProject.Api.Models.Normalized;
using FhirProject.Api.Repositories.Interfaces;
using FhirProject.Api.Services.Interfaces;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using System.Collections.Concurrent;

namespace FhirProject.Api.Services.Implementations;

public class FhirEncounterUpsertService : IFhirEncounterUpsertService
{
    private readonly IExternalResourceMappingRepository _mappingRepository;
    private readonly IFhirEncounterClientService _fhirEncounterClient;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();

    public FhirEncounterUpsertService(
        IExternalResourceMappingRepository mappingRepository,
        IFhirEncounterClientService fhirEncounterClient)
    {
        _mappingRepository = mappingRepository;
        _fhirEncounterClient = fhirEncounterClient;
    }

    public async Task<string> UpsertEncounterAsync(
        string sourceSystem, 
        string externalEncounterId, 
        NormalizedEncounter normalizedEncounter,
        string internalPatientFhirId,
        string internalPractitionerFhirId)
    {
        var lockKey = $"{sourceSystem}:{externalEncounterId}:Encounter";
        var semaphore = _semaphores.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();
        try
        {
            // Query ExternalResourceMappings for existing Encounter
            var existingMapping = await _mappingRepository.GetMappingAsync(
                sourceSystem, 
                externalEncounterId, 
                FhirResourceType.Encounter);

            if (existingMapping != null)
            {
                // UPDATE path: Encounter mapping exists, verify FHIR resource exists
                try
                {
                    var existingEncounter = await _fhirEncounterClient.GetEncounterAsync(existingMapping.InternalResourceId);
                    if (existingEncounter != null)
                    {
                        // Update existing Encounter with normalized data
                        var updatedEncounter = MapNormalizedToFhirEncounter(
                            normalizedEncounter, 
                            internalPatientFhirId, 
                            internalPractitionerFhirId, 
                            existingEncounter);
                        await _fhirEncounterClient.UpdateEncounterAsync(updatedEncounter);
                        
                        return existingMapping.InternalResourceId;
                    }
                }
                catch (FhirOperationException ex) when (ex.Status == System.Net.HttpStatusCode.NotFound)
                {
                    // Mapping exists but FHIR resource is missing - treat as stale mapping
                    // Fall through to CREATE path and update existing mapping
                }
            }

            // CREATE path: Encounter does not exist or mapping is stale
            var newEncounter = MapNormalizedToFhirEncounter(
                normalizedEncounter, 
                internalPatientFhirId, 
                internalPractitionerFhirId);
            var createdEncounter = await _fhirEncounterClient.CreateEncounterAsync(newEncounter);

            if (existingMapping != null)
            {
                // Update existing mapping with new FHIR ID
                existingMapping.InternalResourceId = createdEncounter.Id;
                await _mappingRepository.UpdateMappingAsync(existingMapping);
            }
            else
            {
                // Insert new ExternalResourceMapping
                var mapping = new ExternalResourceMapping
                {
                    SourceSystem = sourceSystem,
                    ExternalId = externalEncounterId,
                    ResourceType = FhirResourceType.Encounter,
                    InternalResourceId = createdEncounter.Id
                };
                await _mappingRepository.CreateMappingAsync(mapping);
            }
            
            return createdEncounter.Id;
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

    private Encounter MapNormalizedToFhirEncounter(
        NormalizedEncounter normalized, 
        string internalPatientFhirId, 
        string internalPractitionerFhirId, 
        Encounter? existingEncounter = null)
    {
        var encounter = existingEncounter ?? new Encounter();

        // Update status
        if (!string.IsNullOrEmpty(normalized.Status))
        {
            encounter.Status = normalized.Status.ToLower() switch
            {
                "planned" => Encounter.EncounterStatus.Planned,
                "arrived" => Encounter.EncounterStatus.Arrived,
                "triaged" => Encounter.EncounterStatus.Triaged,
                "in-progress" => Encounter.EncounterStatus.InProgress,
                "onleave" => Encounter.EncounterStatus.Onleave,
                "finished" => Encounter.EncounterStatus.Finished,
                "cancelled" => Encounter.EncounterStatus.Cancelled,
                _ => Encounter.EncounterStatus.Unknown
            };
        }

        // Update class
        if (!string.IsNullOrEmpty(normalized.EncounterType))
        {
            encounter.Class = new Coding
            {
                System = "http://terminology.hl7.org/CodeSystem/v3-ActCode",
                Code = normalized.EncounterType.ToUpper() switch
                {
                    "AMBULATORY" => "AMB",
                    "EMERGENCY" => "EMER",
                    "INPATIENT" => "IMP",
                    "OUTPATIENT" => "AMB",
                    _ => "AMB"
                },
                Display = normalized.EncounterType.ToLower()
            };
        }

        // Update subject (Patient reference)
        encounter.Subject = new ResourceReference($"Patient/{internalPatientFhirId}");

        // Update participant (Practitioner reference)
        encounter.Participant.Clear();
        encounter.Participant.Add(new Encounter.ParticipantComponent
        {
            Individual = new ResourceReference($"Practitioner/{internalPractitionerFhirId}"),
            Type = new List<CodeableConcept>
            {
                new CodeableConcept
                {
                    Coding = new List<Coding>
                    {
                        new Coding
                        {
                            System = "http://terminology.hl7.org/CodeSystem/v3-ParticipationType",
                            Code = "ATND",
                            Display = "attender"
                        }
                    }
                }
            }
        });

        // Update period
        if (normalized.StartDateTime.HasValue || normalized.EndDateTime.HasValue)
        {
            encounter.Period = new Period();
            if (normalized.StartDateTime.HasValue)
            {
                encounter.Period.Start = normalized.StartDateTime.Value.ToString("yyyy-MM-ddTHH:mm:ssZ");
            }
            if (normalized.EndDateTime.HasValue)
            {
                encounter.Period.End = normalized.EndDateTime.Value.ToString("yyyy-MM-ddTHH:mm:ssZ");
            }
        }

        // Update reason code
        if (!string.IsNullOrEmpty(normalized.ReasonCode) || !string.IsNullOrEmpty(normalized.ReasonDisplay))
        {
            encounter.ReasonCode.Clear();
            encounter.ReasonCode.Add(new CodeableConcept
            {
                Coding = new List<Coding>
                {
                    new Coding
                    {
                        Code = normalized.ReasonCode,
                        Display = normalized.ReasonDisplay
                    }
                }
            });
        }

        // Update location
        if (!string.IsNullOrEmpty(normalized.Location))
        {
            encounter.Location.Clear();
            encounter.Location.Add(new Encounter.LocationComponent
            {
                Location = new ResourceReference
                {
                    Display = normalized.Location
                }
            });
        }

        return encounter;
    }
}