using FinalLab.Common;
using MediatR;

namespace FinalLab.Application.Commands.RollbackCommands;
public class RollbackEventsByDayCommand : IRequest<Result<string>>
{
    public DateTime Day { get; set; }
}