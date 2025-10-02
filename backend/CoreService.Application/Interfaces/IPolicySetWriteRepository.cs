using CoreService.Domain.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CoreService.Application.Interfaces;

public interface IPolicySetWriteRepository
{
    Task AddAsync(ForumPolicySet forumPolicySet, CancellationToken cancellationToken);
}