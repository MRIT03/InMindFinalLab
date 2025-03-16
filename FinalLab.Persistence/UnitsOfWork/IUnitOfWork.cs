using FinalLab.Persistence.Repositories;

namespace FinalLab.Persistence.UnitsOfWork;

public interface IUnitOfWork
{

    Task commit();
}