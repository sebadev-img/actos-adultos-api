using ActosAdultosAPI.Data;
using ActosAdultosAPI.Models;
using ActosAdultosAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ActosAdultosAPI.Services.Implementation
{
    public class CiudadService: ICiudadService
    {
        private readonly ApplicationDbContext _context;

        public CiudadService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Ciudad>> GetCiudades()
        {
            return await _context.Set<Ciudad>().AsNoTracking().ToListAsync();
        }

        public async Task<bool> ExistsByIdAsync(int ciudadId)
        {
            return await _context.Set<Ciudad>()
                .AsNoTracking()
                .AnyAsync(c => c.Id == ciudadId);
        }
    }
}
