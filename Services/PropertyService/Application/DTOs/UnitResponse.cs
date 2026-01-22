using PropertyService.Domain.Enums;

namespace PropertyService.Application.DTOs;

public record UnitResponse(
    Guid Id,
    Guid PropertyId,
    string UnitNumber,
    UnitStatus Status,
    string? Floor,
    int? Bedrooms,
    int? Bathrooms,
    DateTime CreatedAt
);
