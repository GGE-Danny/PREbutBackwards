using PropertyService.Domain.Enums;

namespace PropertyService.Application.DTOs;

public record CreateUnitRequest(
    string UnitNumber,
    string? Floor,
    int? Bedrooms,
    int? Bathrooms
);
