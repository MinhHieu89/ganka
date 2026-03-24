using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Billing.Application.Features;
using Billing.Application.Features.ServiceCatalog;
using Billing.Application.Interfaces;
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
        MapServiceCatalogEndpoints(group);
        MapPrintEndpoints(group);
        MapExportEndpoints(group);

        return app;
    }

    private static void MapInvoiceEndpoints(RouteGroupBuilder group)
    {
        // POST /api/billing/invoices -- create a new draft invoice
        group.MapPost("/invoices", async (CreateInvoiceCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<InvoiceDto>>(command, ct);
            return result.ToCreatedHttpResult("/api/billing/invoices");
        }).RequirePermissions(Permissions.Billing.Create);

        // GET /api/billing/invoices -- get all invoices with optional filters
        group.MapGet("/invoices", async (int? status, string? search, int? page, int? pageSize,
            IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<PaginatedInvoicesResult>>(
                new GetAllInvoicesQuery(status, search, page ?? 1, pageSize ?? 20), ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Billing.View);

        // GET /api/billing/invoices/{invoiceId} -- get invoice by ID with full details
        group.MapGet("/invoices/{invoiceId:guid}",
            async (Guid invoiceId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<InvoiceDto>>(
                new GetInvoiceByIdQuery(invoiceId), ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Billing.View);

        // GET /api/billing/invoices/by-visit/{visitId} -- get all invoices for a visit (summary list)
        group.MapGet("/invoices/by-visit/{visitId:guid}",
            async (Guid visitId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<List<InvoiceSummaryDto>>>(
                new GetInvoicesByVisitQuery(visitId), ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Billing.View);

        // GET /api/billing/invoices/visit/{visitId} -- get single invoice by visit (cross-module)
        group.MapGet("/invoices/visit/{visitId:guid}",
            async (Guid visitId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<InvoiceDto>>(
                new GetVisitInvoiceQuery(visitId), ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Billing.View);

        // GET /api/billing/invoices/pending -- get pending invoices for cashier dashboard
        group.MapGet("/invoices/pending",
            async (Guid? cashierShiftId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<List<InvoiceDto>>>(
                new GetPendingInvoicesQuery(cashierShiftId), ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Billing.View);

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
        }).RequirePermissions(Permissions.Billing.Create);

        // DELETE /api/billing/invoices/{invoiceId}/line-items/{lineItemId} -- remove line item from draft invoice
        group.MapDelete("/invoices/{invoiceId:guid}/line-items/{lineItemId:guid}",
            async (Guid invoiceId, Guid lineItemId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<InvoiceDto>>(
                new RemoveInvoiceLineItemCommand(invoiceId, lineItemId), ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Billing.Create);

        // POST /api/billing/invoices/{invoiceId}/finalize -- finalize a paid invoice
        group.MapPost("/invoices/{invoiceId:guid}/finalize",
            async (Guid invoiceId, FinalizeInvoiceCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new FinalizeInvoiceCommand(invoiceId, command.CashierShiftId);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Billing.Create);
    }

    private static void MapPaymentEndpoints(RouteGroupBuilder group)
    {
        // POST /api/billing/payments -- record a payment against an invoice
        group.MapPost("/payments", async (RecordPaymentCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<PaymentDto>>(command, ct);
            return result.ToCreatedHttpResult("/api/billing/payments");
        }).RequirePermissions(Permissions.Billing.Create);

        // GET /api/billing/payments/invoice/{invoiceId} -- get all payments for an invoice
        group.MapGet("/payments/invoice/{invoiceId:guid}",
            async (Guid invoiceId, IMessageBus bus, CancellationToken ct) =>
        {
            var payments = await bus.InvokeAsync<List<PaymentDto>>(
                new GetPaymentsByInvoiceQuery(invoiceId), ct);
            return Results.Ok(payments);
        }).RequirePermissions(Permissions.Billing.View);
    }

    private static void MapDiscountEndpoints(RouteGroupBuilder group)
    {
        // POST /api/billing/discounts -- apply a discount to an invoice
        group.MapPost("/discounts", async (ApplyDiscountCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<DiscountDto>>(command, ct);
            return result.ToCreatedHttpResult("/api/billing/discounts");
        }).RequirePermissions(Permissions.Billing.Create);

        // POST /api/billing/discounts/{discountId}/approve -- approve a pending discount
        group.MapPost("/discounts/{discountId:guid}/approve",
            async (Guid discountId, ApproveDiscountCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new ApproveDiscountCommand(
                command.InvoiceId, discountId, command.ManagerId);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Billing.Manage);

        // POST /api/billing/discounts/{discountId}/reject -- reject a pending discount
        group.MapPost("/discounts/{discountId:guid}/reject",
            async (Guid discountId, RejectDiscountCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new RejectDiscountCommand(
                command.InvoiceId, discountId, command.RejectionReason,
                command.ManagerId);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Billing.Manage);
    }

    private static void MapRefundEndpoints(RouteGroupBuilder group)
    {
        // POST /api/billing/refunds -- request a refund on a finalized invoice
        group.MapPost("/refunds", async (RequestRefundCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<RefundDto>>(command, ct);
            return result.ToCreatedHttpResult("/api/billing/refunds");
        }).RequirePermissions(Permissions.Billing.Create);

        // POST /api/billing/refunds/{refundId}/approve -- approve a requested refund
        group.MapPost("/refunds/{refundId:guid}/approve",
            async (Guid refundId, ApproveRefundCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new ApproveRefundCommand(
                command.InvoiceId, refundId, command.ManagerId);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Billing.Manage);

        // POST /api/billing/refunds/{refundId}/process -- process an approved refund
        group.MapPost("/refunds/{refundId:guid}/process",
            async (Guid refundId, ProcessRefundCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new ProcessRefundCommand(
                command.InvoiceId, refundId, command.RefundMethod, command.Notes);
            var result = await bus.InvokeAsync<Result<RefundDto>>(enriched, ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Billing.Manage);
    }

    private static void MapShiftEndpoints(RouteGroupBuilder group)
    {
        // POST /api/billing/shifts/open -- open a new cashier shift
        group.MapPost("/shifts/open", async (OpenShiftCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<CashierShiftDto>>(command, ct);
            return result.ToCreatedHttpResult("/api/billing/shifts");
        }).RequirePermissions(Permissions.Billing.Create);

        // POST /api/billing/shifts/close -- close the current cashier shift
        group.MapPost("/shifts/close", async (CloseShiftCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<CashierShiftDto>>(command, ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Billing.Create);

        // GET /api/billing/shifts/current -- get the current open shift
        group.MapGet("/shifts/current", async (IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<CashierShiftDto?>>(
                new GetCurrentShiftQuery(), ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Billing.View);

        // GET /api/billing/shifts/{shiftId}/report -- get shift report with revenue breakdown
        group.MapGet("/shifts/{shiftId:guid}/report",
            async (Guid shiftId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<ShiftReportDto>>(
                new GetShiftReportQuery(shiftId), ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Billing.View);

        // GET /api/billing/shifts -- get closed shift history with pagination
        group.MapGet("/shifts", async (int? page, int? pageSize, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<ShiftHistoryResult>>(
                new GetShiftHistoryQuery(page ?? 1, pageSize ?? 20), ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Billing.View);

        // GET /api/billing/shifts/templates -- get active shift templates for current branch
        group.MapGet("/shifts/templates", async (IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<List<ShiftTemplateDto>>>(
                new GetShiftTemplatesQuery(), ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Billing.View);

        // GET /api/billing/shifts/{shiftId}/report/pdf -- print shift report as PDF
        group.MapGet("/shifts/{shiftId:guid}/report/pdf",
            async (Guid shiftId, IBillingDocumentService docs, CancellationToken ct) =>
        {
            var pdf = await docs.GenerateShiftReportPdfAsync(shiftId, ct);
            return Results.File(pdf, "application/pdf", $"shift-report-{shiftId}.pdf");
        }).RequirePermissions(Permissions.Billing.View);
    }

    private static void MapServiceCatalogEndpoints(RouteGroupBuilder group)
    {
        // POST /api/billing/service-catalog -- create a new service catalog item (requires Billing.Manage permission)
        group.MapPost("/service-catalog",
            async (CreateServiceCatalogItemCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<ServiceCatalogItemDto>>(command, ct);
            return result.ToCreatedHttpResult("/api/billing/service-catalog");
        }).RequirePermissions(Permissions.Billing.Manage);

        // PUT /api/billing/service-catalog/{id} -- update a service catalog item (requires Billing.Manage permission)
        group.MapPut("/service-catalog/{id:guid}",
            async (Guid id, UpdateServiceCatalogItemCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new UpdateServiceCatalogItemCommand(
                id, command.Name, command.NameVi, command.Price, command.IsActive, command.Description);
            var result = await bus.InvokeAsync<Result<ServiceCatalogItemDto>>(enriched, ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Billing.Manage);

        // GET /api/billing/service-catalog -- list service catalog items
        group.MapGet("/service-catalog",
            async (bool? includeInactive, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<List<ServiceCatalogItemDto>>>(
                new GetServiceCatalogItemsQuery(includeInactive ?? false), ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Billing.View);

        // GET /api/billing/service-catalog/by-code/{code} -- get service by code
        group.MapGet("/service-catalog/by-code/{code}",
            async (string code, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<ServiceCatalogItemDto?>>(
                new GetServiceCatalogItemByCodeQuery(code), ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Billing.View);
    }

    private static void MapPrintEndpoints(RouteGroupBuilder group)
    {
        // GET /api/billing/print/{invoiceId}/invoice -- print invoice PDF
        group.MapGet("/print/{invoiceId:guid}/invoice",
            async (Guid invoiceId, IBillingDocumentService docs, CancellationToken ct) =>
        {
            var pdf = await docs.GenerateInvoicePdfAsync(invoiceId, ct);
            return Results.File(pdf, "application/pdf", $"invoice-{invoiceId}.pdf");
        }).RequirePermissions(Permissions.Billing.View);

        // GET /api/billing/print/{invoiceId}/receipt -- print receipt PDF
        group.MapGet("/print/{invoiceId:guid}/receipt",
            async (Guid invoiceId, IBillingDocumentService docs, CancellationToken ct) =>
        {
            var pdf = await docs.GenerateReceiptPdfAsync(invoiceId, ct);
            return Results.File(pdf, "application/pdf", $"receipt-{invoiceId}.pdf");
        }).RequirePermissions(Permissions.Billing.View);

        // GET /api/billing/print/{invoiceId}/e-invoice -- print e-invoice PDF
        group.MapGet("/print/{invoiceId:guid}/e-invoice",
            async (Guid invoiceId, IBillingDocumentService docs, CancellationToken ct) =>
        {
            var pdf = await docs.GenerateEInvoicePdfAsync(invoiceId, ct);
            return Results.File(pdf, "application/pdf", $"e-invoice-{invoiceId}.pdf");
        }).RequirePermissions(Permissions.Billing.View);
    }

    private static void MapExportEndpoints(RouteGroupBuilder group)
    {
        // GET /api/billing/export/{invoiceId}/e-invoice/json -- export e-invoice as JSON
        group.MapGet("/export/{invoiceId:guid}/e-invoice/json",
            async (Guid invoiceId, IBillingDocumentService docs, CancellationToken ct) =>
        {
            var json = await docs.ExportEInvoiceJsonAsync(invoiceId, ct);
            return Results.Content(json, "application/json");
        }).RequirePermissions(Permissions.Billing.Export);

        // GET /api/billing/export/{invoiceId}/e-invoice/xml -- export e-invoice as XML
        group.MapGet("/export/{invoiceId:guid}/e-invoice/xml",
            async (Guid invoiceId, IBillingDocumentService docs, CancellationToken ct) =>
        {
            var xml = await docs.ExportEInvoiceXmlAsync(invoiceId, ct);
            return Results.Content(xml, "application/xml");
        }).RequirePermissions(Permissions.Billing.Export);
    }
}
