using FhirProject.Api.Data;
using FhirProject.Api.Models.entities;
using FhirProject.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FhirProject.Api.Repositories.Implementations
{
    public class FhirResourceRepository : IFhirResourceRepository
    {
        private readonly AppDbContext _context;

        public FhirResourceRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<FhirResourceEntity> SaveAsync(FhirResourceEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.FhirResources.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<FhirResourceEntity?> GetByConversionRequestIdAsync(int conversionRequestId)
        {
            return await _context.FhirResources
                .FirstOrDefaultAsync(x => x.ConversionRequestId == conversionRequestId);
        }

        public async Task<FhirResourceEntity?> GetByIdAsync(int id)
        {
            return await _context.FhirResources.FindAsync(id);
        }

        public async Task<IEnumerable<FhirResourceEntity>> GetAllAsync()
        {
            return await _context.FhirResources.ToListAsync();
        }
    }
}