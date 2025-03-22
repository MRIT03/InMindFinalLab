using FinalLab.Common;
using FinalLab.Domain.Entities;
using MediatR;

namespace FinalLab.Application.Commands.CreateAccount;

public class CreateAccountCommand : IRequest<Result<Account>>
{
    public decimal initialBalance;
    
}