namespace TenantService.Application.DTOs;

public record CreateTenantProfileRequest(
    Guid TenantUserId,
    string FullName,
    string? PhoneNumber,
    string? Email,
    string? NationalIdType,
    string? NationalIdNumber,
    DateOnly? DateOfBirth,
    string? Nationality,
    string? EmploymentStatus,
    string? EmployerName,
    string? JobTitle,
    string? CurrentAddress,
    string? NextOfKinName,
    string? NextOfKinPhone,
    string? Notes
);

public record UpdateTenantProfileRequest(
    string? FullName,
    string? PhoneNumber,
    string? Email,
    string? NationalIdType,
    string? NationalIdNumber,
    DateOnly? DateOfBirth,
    string? Nationality,
    string? EmploymentStatus,
    string? EmployerName,
    string? JobTitle,
    string? CurrentAddress,
    string? NextOfKinName,
    string? NextOfKinPhone,
    string? Notes
);

public record TenantProfileResponse(
    Guid Id,
    Guid TenantUserId,
    string FullName,
    string? PhoneNumber,
    string? Email,
    string? NationalIdType,
    string? NationalIdNumber,
    DateOnly? DateOfBirth,
    string? Nationality,
    string? EmploymentStatus,
    string? EmployerName,
    string? JobTitle,
    string? CurrentAddress,
    string? NextOfKinName,
    string? NextOfKinPhone,
    string? Notes,
    DateTime CreatedAt
);
