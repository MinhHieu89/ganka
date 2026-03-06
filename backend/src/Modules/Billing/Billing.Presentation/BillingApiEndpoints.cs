using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Billing.Application.Features;
using Billing.Contracts.Dtos;
using Billing.Contracts.Queries;
using Shared.Domain;
using Shared.Presentation;
using Wolverine;

namespace Billing.Presentation;

/// <summary>
/// Billing API endpoints covering invoices, payments, discounts, refunds, and cashier shifts.
/// All endpoints require authorization and are grouped under /api/billing.
/// </summary>
public static class BillingApiEndpoints
{
    public static IEndpointRouteBuilder MapBillingApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/billing").RequireAuthorization();

        MapInvoiceEndpoints(group);
        MapPaymentEndpoints(group);
        MapDiscountEndpoints(group);
        MapRefundEndpoints(group);
        MapShiftEndpoints(group);

        return app;
    }

    private static void MapInvoiceEndpoints(RouteGroupBuilder group)
    {
        // POST /api/billing/invoices -- create a new draft invoice
        group.MapPost("/invoices", async (CreateInvoiceCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<InvoiceDto>>(command, ct);
            return result.ToCreatedHttpResult("/api/billing/invoices");
        });

        // POST /api/billing/invoices/{invoiceId}/line-items -- add line item to invoice
        group.MapPost("/invoices/{invoiceId:guid}/line-items",
            async (Guid invoiceId, AddInvoiceLineItemCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new AddInvoiceLineItemCommand(
                invoiceId, command.Description, command.DescriptionVi,
                command.UnitPrice, command.Quantity, command.Department,
                command.SourceId, command.SourceType);
            var result = await bus.InvokeAsync<Result<InvoiceDto>>(enriched, ct);
            return result.ToHttpResult();
        });

        // POST /api/billing/invoices/{invoiceId}/finalize -- finalize a paid invoice
        group.MapPost("/invoices/{invoiceId:guid}/finalize",
            async (Guid invoiceId, FinalizeInvoiceCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new FinalizeInvoiceCommand(invoiceId, command.CashierShiftId);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });

        // GET /api/billing/invoices/visit/{visitId} -- get invoice by visit
        group.MapGet("/invoices/visit/{visitId:guid}",
            async (Guid visitId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<InvoiceDto>>(
                new GetVisitInvoiceQuery(visitId), ct);
            return result.ToHttpResult();
        });

        // GET /api/billing/invoices/pending -- get pending invoices for cashier dashboard
        group.MapGet("/invoices/pending",
            async (Guid? cashierShiftId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<List<InvoiceDto>>>(
                new GetPendingInvoicesQuery(cashierShiftId), ct);
            return result.ToHttpResult();
        });
    }

    private static void MapPaymentEndpoints(RouteGroupBuilder group)
    {
        // POST /api/billing/payments -- record a payment against an invoice
        group.MapPost("/payments", async (RecordPaymentCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<PaymentDto>>(command, ct);
            return result.ToCreatedHttpResult("/api/billing/payments");
        });

        // GET /api/billing/payments/invoice/{invoiceId} -- get all payments for an invoice
        group.MapGet("/payments/invoice/{invoiceId:guid}",
            async (Guid invoiceId, IMessageBus bus, CancellationToken ct) =>
        {
            var payments = await bus.InvokeAsync<List<PaymentDto>>(
                new GetPaymentsByInvoiceQuery(invoiceId), ct);
            return Results.Ok(payments);
        });
    }

    private static void MapDiscountEndpoints(RouteGroupBuilder group)
    {
        // POST /api/billing/discounts -- apply a discount to an invoice
        group.MapPost("/discounts", async (ApplyDiscountCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<DiscountDto>>(command, ct);
            return result.ToCreatedHttpResult("/api/billing/discounts");
        });

        // POST /api/billing/discounts/{discountId}/approve -- approve a pending discount
        group.MapPost("/discounts/{discountId:guid}/approve",
            async (Guid discountId, ApproveDiscountCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new ApproveDiscountCommand(
                command.InvoiceId, discountId, command.ManagerId, command.ManagerPin);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });
    }

    private static void MapRefundEndpoints(RouteGroupBuilder group)
    {
        // POST /api/billing/refunds -- request a refund on a finalized invoice
        group.MapPost("/refunds", async (RequestRefundCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<RefundDto>>(command, ct);
            return result.ToCreatedHttpResult("/api/billing/refunds");
        });
    }

    private static void MapShiftEndpoints(RouteGroupBuilder group)
    {
        // POST /api/billing/shifts/open -- open a new cashier shift
        group.MapPost("/shifts/open", async (OpenShiftCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<CashierShiftDto>>(command, ct);
            return result.ToCreatedHttpResult("/api/billing/shifts");
        });

        // POST /api/billing/shifts/close -- close the current cashier shift
        group.MapPost("/shifts/close", async (CloseShiftCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<CashierShiftDto>>(command, ct);
            return result.ToHttpResult();
        });

        // GET /api/billing/shifts/current -- get the current open shift
        group.MapGet("/shifts/current", async (IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<CashierShiftDto?>>(
                new GetCurrentShiftQuery(), ct);
            return result.ToHttpResult();
        });

        // GET /api/billing/shifts/{shiftId}/report -- get shift report with revenue breakdown
        group.MapGet("/shifts/{shiftId:guid}/report",
            async (Guid shiftId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<ShiftReportDto>>(
                new GetShiftReportQuery(shiftId), ct);
            return result.ToHttpResult();
        });
    }
}
