using FinalLab.Application.Services;
using FinalLab.Domain.Entities;
using FinalLab.Persistence.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Localization;

namespace FinalLab.API.Controllers;

[ApiController]
[Route("transaction-logs")]
public class TransactionController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IStringLocalizer<SharedResource> _sharedLocalizer;


    public TransactionController(ITransactionService transactionService, ITransactionRepository transactionRepository, IStringLocalizer<SharedResource> sharedLocalizer)
    {
        _transactionService = transactionService;
        _transactionRepository = transactionRepository;
        _sharedLocalizer = sharedLocalizer;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransactionAsync([FromBody] Transaction transaction)
    {
        var result = await _transactionService.CreateTransactionAsync(transaction);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransactionsAsync([FromRoute] int id)
    {
        var transactions = await _transactionRepository.GetAllAsync();
        return Ok(transactions.FirstOrDefault(x => x.Id == id));
    }
    
    [HttpGet]
    [EnableQuery]
    public async Task<IQueryable<Transaction>> Get()
    {
        var transactions = await _transactionRepository.GetAllAsync();
        return transactions.AsQueryable();
    }

    [HttpPost("/transactions/notify")]
    public async Task<IActionResult> Notify([FromBody] string message)
    {
        Transaction transaction = new Transaction()
        {
            Status = "notified",
            Details = _sharedLocalizer[message],
        };
        return Ok(_transactionService.CreateTransactionAsync(transaction));
    }
    
    
   
}