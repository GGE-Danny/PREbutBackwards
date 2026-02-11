using AccountingService.Application.Dtos.Requests;
using AccountingService.Application.Dtos.Responses;
using AccountingService.Application.Interfaces;
using AccountingService.Domain.Entities;
using AccountingService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountingService.Api.Controllers;

[Route("api/v1/invoices")]
public sealed class InvoicesController : ApiControllerBase
{
    private readonly IInvoiceRepository _invoices;
    private readonly IPaymentRepository _payments;
    private readonly ILedgerRepository _ledger;
    private readonly IUnitOfWork _uow;

    public InvoicesController(
        IInvoiceRepository invoices,
        IPaymentRepository payments,
        ILedgerRepository ledger,
        IUnitOfWork uow)
    {
        _invoices = invoices;
        _payments = payments;
        _ledger = ledger;
        _uow = uow;
    }

    [HttpPost("pay")]
    [Authorize(Policy = "accounting.write")]
    public async Task<ActionResult<PaymentResponse>> Pay([FromBody] RecordPaymentRequest req, CancellationToken ct)
    {
        if (!TryGetCallerUserId(out var actorId))
            return Unauthorized();

        if (req.Amount <= 0)
            return BadRequest("Amount must be > 0.");

        // Idempotency: same reference should not be posted twice
        if (await _payments.ExistsByReferenceIdAsync(req.ReferenceId, ct))
            return Conflict("Duplicate payment reference.");

        var invoice = await _invoices.GetByIdForUpdateAsync(req.InvoiceId, ct);
        if (invoice is null) return NotFound("Invoice not found.");

        if (invoice.Status == PaymentStatus.Paid)
            return Conflict("Invoice already paid.");

        await _uow.BeginTransactionAsync(ct);

        try
        {
            var payment = new Payment(
                invoiceId: invoice.Id,
                amount: req.Amount,
                paymentMethod: req.PaymentMethod,
                referenceId: req.ReferenceId
            );

            await _payments.AddAsync(payment, ct);

            // Rule: full payment marks paid (MVP). Partial payments can come later.
            if (req.Amount >= invoice.Amount)
            {
                invoice.Status = PaymentStatus.Paid;
                invoice.UpdatedAt = DateTime.UtcNow;
            }

            // Ledger entry (explicit, consistent)
            await _ledger.AddAsync(new LedgerEntry(
                entryType: LedgerEntryType.Payment,
                amount: payment.Amount,
                invoiceId: invoice.Id,
                paymentId: payment.Id,
                expenseId: null,
                ownerDisbursementId: null,
                notes: $"Payment via {payment.PaymentMethod}. Ref:{payment.ReferenceId}. Actor:{actorId}"
            ), ct);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitAsync(ct);

            return Ok(new PaymentResponse(
                payment.Id,
                payment.InvoiceId,
                payment.Amount,
                payment.PaymentMethod,
                payment.ReferenceId,
                payment.CreatedAt
            ));
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
