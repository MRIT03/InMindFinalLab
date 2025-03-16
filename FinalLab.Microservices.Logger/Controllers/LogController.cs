using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using FinalLab.Domain.Entities;
using FinalLab.Microservices.Logger.Contexts;
using Newtonsoft.Json;

[ApiController]
[Route("api/logs")]
public class LogsController : ControllerBase
{
    private readonly AppDbContext _context;

    public LogsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs(
        [FromQuery] Guid? requestId, 
        [FromQuery] string? routeUrl, 
        [FromQuery] DateTime? startDate, 
        [FromQuery] DateTime? endDate, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10)
    {
        var query = _context.Logs.AsQueryable();

        if (requestId.HasValue)
            query = query.Where(l => l.RequestId == requestId.Value);

        if (!string.IsNullOrEmpty(routeUrl))
            query = query.Where(l => l.RouteURL == routeUrl);

        if (startDate.HasValue)
            query = query.Where(l => l.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(l => l.Timestamp <= endDate.Value);

        var totalLogs = await query.CountAsync();
        var logs = await query
            .OrderByDescending(l => l.Timestamp) // this will get me the most recent ones first.
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            TotalLogs = totalLogs,
            Page = page,
            PageSize = pageSize,
            Data = logs
        });
    }
    [HttpPost]
    [HttpPost]
    public async Task<IActionResult> ReceiveLog([FromBody] LogEntry logEntry)
    {
        if (logEntry == null)
        {
            return BadRequest("Invalid log entry.");
        }

        if (string.IsNullOrEmpty(logEntry.RequestObject) && logEntry.RequestData != null)
        {
            logEntry.RequestObject = JsonConvert.SerializeObject(logEntry.RequestData);
        }

        _context.Logs.Add(logEntry);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Log stored successfully." });
    }
}