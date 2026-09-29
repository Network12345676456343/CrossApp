namespace Core.Dto;

public record ReaderDto(
    string Id,
    string FullName,
    string Email,
    string? Phone = null);