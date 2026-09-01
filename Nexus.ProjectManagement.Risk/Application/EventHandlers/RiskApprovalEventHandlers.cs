using NexusCore.Application.Approvals;
using NexusCore.SharedKernel.Domain;
using NexusCore.SharedKernel.Interfaces;

namespace Nexus.ProjectManagement.RiskManagement.Application.EventHandlers;

public sealed class RiskApprovalGrantedHandler(IRiskRepository repository, IUnitOfWork unitOfWork)
    : IDomainEventHandler<ApprovalGranted>
{
    public async Task HandleAsync(ApprovalGranted domainEvent, CancellationToken cancellationToken)
    {
        if (domainEvent.SubjectType != "Risk")
        {
            return;
        }

        var risk = await repository.GetByIdAsync(domainEvent.SubjectId, cancellationToken);
        if (risk is null)
        {
            return;
        }

        risk.Approve();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class RiskApprovalRejectedHandler(IRiskRepository repository, IUnitOfWork unitOfWork)
    : IDomainEventHandler<ApprovalRejected>
{
    public async Task HandleAsync(ApprovalRejected domainEvent, CancellationToken cancellationToken)
    {
        if (domainEvent.SubjectType != "Risk")
        {
            return;
        }

        var risk = await repository.GetByIdAsync(domainEvent.SubjectId, cancellationToken);
        if (risk is null)
        {
            return;
        }

        risk.Reject();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
