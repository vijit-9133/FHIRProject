using FhirProject.Api.Data;
using FhirProject.Api.Models.entities;
using FhirProject.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FhirProject.Api.Repositories.Implementations
{
    public class ExternalSystemRepository : IExternalSystemRepository
    {
        private readonly AppDbContext _context;

        public ExternalSystemRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ExternalSystem?> GetByClientIdAsync(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                return null;

            return await _context.ExternalSystems
                .Include(x => x.ApprovedByUser)
                .FirstOrDefaultAsync(x => x.ClientId == clientId);
        }

        public async Task<ExternalSystem?> GetByIdAsync(int id)
        {
            return await _context.ExternalSystems
                .Include(x => x.ApprovedByUser)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task AddAsync(ExternalSystem system)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));

            _context.ExternalSystems.Add(system);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ExternalSystem system)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));

            _context.ExternalSystems.Update(system);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ExternalSystem>> GetAllAsync()
        {
            return await _context.ExternalSystems
                .Include(x => x.ApprovedByUser)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var system = await _context.ExternalSystems.FindAsync(id);
            if (system != null)
            {
                _context.ExternalSystems.Remove(system);
                await _context.SaveChangesAsync();
            }
        }
    }
}
