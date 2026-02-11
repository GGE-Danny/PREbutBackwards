namespace SalesService.Application.Dtos.Requests;

public record AssignLeadRequest(
    Guid AssignedToUserId
);
