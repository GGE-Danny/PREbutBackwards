using SalesService.Domain.Enums;

namespace SalesService.Application.Dtos.Requests;

public record UpdateLeadStatusRequest(
    LeadStatus Status,
    string? Notes
);
