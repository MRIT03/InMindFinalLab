using FinalLab.Common;
using MediatR;

namespace FinalLab.Application.Queries;

public class GetBalanceQuery : IRequest<Result<decimal>>
{
    public long accountId;
}