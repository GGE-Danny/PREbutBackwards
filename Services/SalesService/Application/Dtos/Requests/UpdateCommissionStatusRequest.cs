using SalesService.Domain.Enums;

namespace SalesService.Application.Dtos.Requests;

public record UpdateCommissionStatusRequest(
    CommissionStatus Status,
    string? Notes
);
