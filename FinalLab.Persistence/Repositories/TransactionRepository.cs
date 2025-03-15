using FinalLab.Domain.Entities;
using FinalLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinalLab.Persistence.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _db;

    public TransactionRepository(ApplicationDbContext db, IQueryable<Transaction> query)
    {
        _db = db;
        Query = query;
    }


    public Task<List<Transaction>> GetAllAsync()
    {
        return _db.Transactions.ToListAsync();
    }

    public IQueryable<Transaction> Query { get; }
    public async Task AddAsync(Transaction entity)
    {
        await _db.Transactions.AddAsync(entity);
    }

    public async Task DeleteAsync(Transaction entity)
    {
        _db.Transactions.Remove(entity);
    }

    public Task SaveAsync()
    {
        return _db.SaveChangesAsync();
    }

    
}