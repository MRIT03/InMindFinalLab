namespace FinalLab.Application.Commands.RollbackCommands;

using FinalLab.Common;
using FinalLab.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


public class RollbackEventsByDayCommandHandler : IRequestHandler<RollbackEventsByDayCommand, Result<string>>
{
    private readonly ApplicationDbContext _context;

    public RollbackEventsByDayCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(RollbackEventsByDayCommand request, CancellationToken cancellationToken)
    {
        var dayStart = request.Day.Date;
        var dayEnd = dayStart.AddDays(1);
        // Retrieve all update events on the specified day that are not already reverted.
        var eventsToRollback = await _context.Events
            .Where(e => e.Timestamp >= dayStart && e.Timestamp < dayEnd && !e.IsRevert)
            .ToListAsync(cancellationToken);

        foreach (var ev in eventsToRollback)
        {
            ev.IsRevert = true;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success("Rollback by day completed.");
    }
}