using FinalLab.Domain.Entities.Events.UpdateEvents;

namespace FinalLab.Persistence.Repositories;

public interface IEventRepository : IRepository<UpdateEvent>
{
    
}