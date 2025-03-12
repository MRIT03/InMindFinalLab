using FinalLab.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinalLab.Application.Services;

public interface ITransactionService
{
    public Task<Transaction> CreateTransactionAsync(Transaction transaction);
}