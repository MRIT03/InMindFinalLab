using MediatR;
using System.Collections.Generic;
using FinalLab.Application.Dtos;

namespace FinalLab.Application.Queries;

public class GetBalanceSummaryQuery : IRequest<List<AccountBalanceSummaryDto>>
{
        
}
