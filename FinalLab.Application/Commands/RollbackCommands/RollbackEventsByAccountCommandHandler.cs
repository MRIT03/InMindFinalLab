namespace FinalLab.Application.Commands.RollbackCommands;

using FinalLab.Common;
using FinalLab.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


public class RollbackEventsByAccountCommandHandler : IRequestHandler<RollbackEventsByAccountCommand, Result<string>>
{
    private readonly ApplicationDbContext _context;

    public RollbackEventsByAccountCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(RollbackEventsByAccountCommand request, CancellationToken cancellationToken)
    {
        // Retrieve only AccountUpdateEvents that haven't been reverted.
        var eventsToRollback = await _context.Events
            .OfType<FinalLab.Domain.Entities.Events.UpdateEvents.AccountUpdateEvent>()
            .Where(e => e.AccountId == request.AccountId && !e.IsRevert)
            .ToListAsync(cancellationToken);

        foreach (var ev in eventsToRollback)
        {
            ev.IsRevert = true;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success("Rollback by account completed.");
    }
}