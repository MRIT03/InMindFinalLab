using FinalLab.Domain.Events;
using FinalLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLab.Domain.Entities.Events.UpdateEvents;

namespace FinalLab.Persistence.Repositories
{
    
    public class EventRepository : IEventRepository
    {
        private readonly ApplicationDbContext _db;

        public EventRepository(ApplicationDbContext db)
        {
            _db = db;
            Query = _db.Events;
        }

        public IQueryable<UpdateEvent> Query { get; }

        public async Task<List<UpdateEvent>> GetAllAsync()
        {
            return await _db.Events.ToListAsync();
        }

        public async Task AddAsync(UpdateEvent entity)
        {
            await _db.Events.AddAsync(entity);
        }

        public void DeleteAsync(UpdateEvent entity)
        {
            _db.Events.Remove(entity);
        }

        public Task SaveAsync()
        {
            return _db.SaveChangesAsync();
        }
    }
}