using FinalLab.Domain.Entities;
using FinalLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinalLab.Persistence.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly ApplicationDbContext _db;

    public AccountRepository(ApplicationDbContext db)
    {
        _db = db;
        Query = _db.Accounts;
    }

    public Task<List<Account>> GetAllAsync()
    {
        return _db.Accounts.ToListAsync();
    }

    public IQueryable<Account> Query { get; }
    public async Task AddAsync(Account entity)
    {
        await _db.Accounts.AddAsync(entity);
    }

    public void DeleteAsync(Account entity)
    {
        _db.Accounts.Remove(entity);
    }

    public Task SaveAsync()
    {
        return _db.SaveChangesAsync();
    }
}