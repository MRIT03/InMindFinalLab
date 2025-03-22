using FinalLab.Common;
using FinalLab.Domain.Entities;
using MediatR;

namespace FinalLab.Application.Commands.ChangeAccountStatus;

public class ChangeAccountStatusCommand : IRequest<Result<Account>>
{
    public long accountId;
    public string newStatus;
}