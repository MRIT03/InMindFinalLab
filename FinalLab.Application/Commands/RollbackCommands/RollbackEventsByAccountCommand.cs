using FinalLab.Common;
using MediatR;

namespace FinalLab.Application.Commands.RollbackCommands;

public class RollbackEventsByAccountCommand : IRequest<Result<string>>
{
    public long AccountId { get; set; }
}