namespace BookingService.Domain.Enums;

public enum BookingLogEventType
{
    Created = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3,
    NotesUpdated = 4
}
