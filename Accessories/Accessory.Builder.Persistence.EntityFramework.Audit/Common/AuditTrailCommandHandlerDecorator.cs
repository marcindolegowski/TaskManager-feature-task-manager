using System;
using System.Threading.Tasks;
using Accessory.Builder.CQRS.Core.Commands;
using Accessory.Builder.Persistence.Core.Common.Logs;
using Accessory.Persistence.EntityFramework.UnitOfWork.Common;

namespace Accessory.Persistence.EntityFramework.Audit.Common;

public class AuditTrailCommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
    where TCommand : class, ICommand
{
    private readonly IAuditTrailProvider _auditTrailProvider;
    private readonly ITransactionalUnitOfWork _unitOfWork;
    private readonly ICommandHandler<TCommand> _decoratedHandler;

    public AuditTrailCommandHandlerDecorator(
        IAuditTrailProvider auditTrailProvider,
        ITransactionalUnitOfWork unitOfWork,
        ICommandHandler<TCommand> decoratedHandler)
    {
        _auditTrailProvider = auditTrailProvider ?? throw new ArgumentNullException(nameof(auditTrailProvider));
        _decoratedHandler = decoratedHandler ?? throw new ArgumentNullException(nameof(decoratedHandler));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task Handle(TCommand command)
    {
        if (_unitOfWork.HasActiveTransaction)
        {
            await HandleAndLog(command);
        }
        else
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            await HandleAndLog(command);
            await _unitOfWork.CommitTransactionAsync(transaction!);
        }
    }

    private async Task HandleAndLog(TCommand command)
    {
        await _decoratedHandler.Handle(command);
        await _auditTrailProvider.LogChangesAsync();
    }
}