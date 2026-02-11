namespace SalesService.Application.Dtos.Requests;

public record CreateLeadRequest(
    string FullName,
    string PhoneNumber,
    string? Email,
    string Source,
    Guid PropertyId,
    Guid? UnitId,
    string? Notes
);
