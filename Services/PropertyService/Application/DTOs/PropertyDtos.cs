using PropertyService.Domain.Enums;

namespace PropertyService.Application.DTOs;

public record CreatePropertyRequest(
    string Name,
    PropertyType Type,
    Guid? OwnerId,
    string? AddressLine,
    string? Area,
    string? City,
    string? Region,
    string? Landmark,
    decimal? GpsLatitude,
    decimal? GpsLongitude,
    string? Notes
);

public record PropertyResponse(
    Guid Id,
    string Name,
    PropertyType Type,
    PropertyStatus Status,
    Guid? OwnerId,
    string? AddressLine,
    string? Area,
    string? City,
    string? Region,
    string? Landmark,
    decimal? GpsLatitude,
    decimal? GpsLongitude,
    DateTime CreatedAt,
     int UnitCount
);