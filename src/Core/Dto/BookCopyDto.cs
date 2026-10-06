namespace Core.Dto;

public sealed record BookCopyDto(
    string Id,
    string Isbn,
    string Title,
    bool IsIssued);