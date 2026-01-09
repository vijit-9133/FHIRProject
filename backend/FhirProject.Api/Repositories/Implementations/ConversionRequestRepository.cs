using FhirProject.Api.Data;
using FhirProject.Api.Models.entities;
using FhirProject.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FhirProject.Api.Repositories.Implementations
{
    public class ConversionRequestRepository : IConversionRequestRepository
    {
        private readonly AppDbContext _context;

        public ConversionRequestRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ConversionRequestEntity> CreateAsync(ConversionRequestEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.ConversionRequests.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<ConversionRequestEntity?> GetByIdAsync(int id)
        {
            return await _context.ConversionRequests.FindAsync(id);
        }

        public async Task<ConversionRequestEntity?> GetByIdAsync(int id, int? userId)
        {
            if (userId == null)
            {
                return null; // No authentication provided
            }

            // Strict filtering: UserId == currentUserId only
            return await _context.ConversionRequests
                .Where(x => x.Id == id && x.UserId == userId)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ConversionRequestEntity>> GetAllAsync()
        {
            return await _context.ConversionRequests.ToListAsync();
        }

        public async Task<IEnumerable<ConversionRequestEntity>> GetAllAsync(int? userId)
        {
            if (userId == null)
            {
                return Enumerable.Empty<ConversionRequestEntity>(); // No authentication provided
            }

            // Strict filtering: UserId == currentUserId only
            return await _context.ConversionRequests
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ConversionRequestEntity>> GetByResourceTypeAsync(string resourceType)
        {
            if (string.IsNullOrWhiteSpace(resourceType))
                return Enumerable.Empty<ConversionRequestEntity>();

            return await _context.ConversionRequests
                .Where(x => x.ResourceType == resourceType)
                .ToListAsync();
        }

        public async Task<ConversionRequestEntity> UpdateAsync(ConversionRequestEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.ConversionRequests.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
    }
}